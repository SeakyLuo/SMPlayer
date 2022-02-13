using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public static NotificationSendMode[] NotificationOptions = { NotificationSendMode.MusicChanged, NotificationSendMode.Never };
        private static readonly int[] LimitedRecentPlayedItems = { -1, 100, 200, 500, 1000 };
        private static readonly VoiceAssistantLanguage[] VoiceAssistantPreferredLanguanges = { VoiceAssistantLanguage.English, VoiceAssistantLanguage.Chinese };
        private volatile int addLyricsClickCounter = 0;
        private readonly string addLyricsContent = Helper.Localize("AddLyrics");

        public SettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FillSettings(Settings.settings);
        }

        private void FillSettings(Settings settings)
        {
            PathBox.Text = settings.RootPath;
            int notificationSend = (int)settings.NotificationSend;
            int notificationDisplay = (int)settings.NotificationDisplay;
            NotificationSendComboBox.SelectedIndex = (int)settings.NotificationSend;
            NotificationModeComboBox.SelectedIndex = (int)settings.NotificationDisplay;
            ThemeColorPicker.Color = settings.ThemeColor;
            ShowCountToggleSwitch.IsOn = settings.ShowCount;
            AutoPlayToggleSwitch.IsOn = settings.AutoPlay;
            AutoLyricsToggleSwitch.IsOn = settings.AutoLyrics;
            SaveProgressToggleSwitch.IsOn = settings.SaveMusicProgress;
            HideMultiSelectCommandBarToggleSwitch.IsOn = settings.HideMultiSelectCommandBarAfterOperation;
            ShowLyricsInNotificationToggleSwitch.IsOn = settings.ShowLyricsInNotification;
            VoiceAssistantLanguageComboBox.SelectedIndex = (int)settings.VoiceAssistantPreferredLanguage;
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            await UpdateHelper.UpdateMusicLibrary(folder);
            PathBox.Text = folder.Path;
        }

        private void ConfirmColorButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.settings.ThemeColor = ThemeColorPicker.Color;
            ThemeColorButton.Background = new SolidColorBrush(ThemeColorPicker.Color);
            MainPage.Instance.ShowLocalizedNotification("NotImplemented");
            ColorPickerFlyout.Hide();
        }

        private void ResetColorButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeColorPicker.Color = ColorHelper.SystemColorHighlightColor;
        }

        private void CancelColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerFlyout.Hide();
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            App.Save();
            MainPage.Instance.ShowLocalizedNotification("ChangesSaved");
        }

        private async void AddLyrics_Click(object sender, RoutedEventArgs e)
        {
            ++addLyricsClickCounter;
            if (addLyricsClickCounter == 2)
            {
                MainPage.Instance.ShowLocalizedNotification("ClickAgainToStopAddingLyrics");
                return;
            }
            else if (addLyricsClickCounter == 3)
            {
                addLyricsClickCounter = 0;
                MainPage.Instance.Loader.ShowIndeterminant("StopAddingLyrics");
                return;
            }
            string paren = Helper.LocalizeMessage("PostParenthesis");
            HyperlinkButton button = (HyperlinkButton)sender;
            List<Music> skipped = new List<Music>();
            int count = MusicService.AllSongs.Count(), counter = 0;
            foreach (Music music in MusicService.AllSongs)
            {
                if (addLyricsClickCounter == 0)
                {
                    Helper.ShowNotification("AddingLyricsStopped");
                    MainPage.Instance.Loader.Hide();
                    goto Done;
                }
                string lyrics = await music.GetLyricsAsync();
                if (string.IsNullOrEmpty(lyrics))
                {
                    if (music == MusicPlayer.CurrentMusic)
                    {
                        skipped.Add(music);
                        continue;
                    }
                    await Task.Run(async () =>
                    {
                        lyrics = await LyricsHelper.SearchLyrics(music);
                        await music.SaveLyricsAsync(lyrics);
                    });
                }
                button.Content = string.Format(paren, addLyricsContent, ++counter + "/" + count);
            }
            while (skipped.Count > 0)
            {
                foreach (Music music in skipped.ToList())
                {
                    if (addLyricsClickCounter == 0)
                    {
                        Helper.ShowNotification("AddingLyricsStopped");
                        MainPage.Instance.Loader.Hide();
                        goto Done;
                    }
                    if (music == MusicPlayer.CurrentMusic && skipped.Count > 1) continue;
                    await Task.Run(async () =>
                    {
                        string lyrics = await LyricsHelper.SearchLyrics(music);
                        await music.SaveLyricsAsync(lyrics);
                    });
                    skipped.Remove(music);
                    button.Content = string.Format(paren, addLyricsContent, ++counter + "/" + count);
                }
            }
            Helper.ShowNotification("SearchLyricsDone");
            Done:
            button.Content = addLyricsContent;
            addLyricsClickCounter = 0;
        }

        private async void Reauthorize_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;
            if (folder.Path == Settings.settings.RootPath)
            {
                MainPage.Instance.ShowLocalizedNotification("AuthorizeSuccessful");
            }
            else
            {
                MainPage.Instance.ShowLocalizedNotification("AuthorizeFolderFailed");
            }
        }

        private void KeepRecentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.LimitedRecentPlayedItems = LimitedRecentPlayedItems[(sender as ComboBox).SelectedIndex];
        }

        private async void ExportData_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null) return;
            MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            StorageFile dbFile = await StorageHelper.LoadFileAsync(Path.Combine(Helper.LocalFolder.Path, SQLHelper.DBFileName));
            await dbFile.CopyAsync(folder);
            MainPage.Instance.Loader.Hide();
            MainPage.Instance.ShowLocalizedNotification("DataExported");
        }

        private async void ImportData_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".db");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;
            MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
            try
            {
                await SettingsHelper.InitWithFile(file);
                bool successful = await UpdateHelper.UpdateMusicLibrary(Helper.CurrentFolder);
                if (successful)
                {
                    App.Save();
                    FillSettings(Settings.settings);
                }
                MainPage.Instance.Loader.Hide();
                MainPage.Instance.ShowLocalizedNotification(successful ? "DataImported" : "ImportDataCancelled");
            }
            catch (Exception ex)
            {
                MainPage.Instance.Loader.Hide();
                MainPage.Instance.ShowLocalizedNotification("ImportDataFailed" + ex.Message);
            }
        }

        private void ReleaseNotesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowReleaseNotes();
        }

        public static async void ShowReleaseNotes()
        {
            var dialog = new ReleaseNotesDialog();
            await dialog.ShowAsync();
        }

        private static async Task ComposeEmail(string receiver, string subject, string messageBody)
        {
            var emailMessage = new EmailMessage
            {
                Subject = subject,
                Body = messageBody
            };

            emailMessage.To.Add(new EmailRecipient(receiver));
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyout flyout = new MenuFlyout();
            var emailItem = new MenuFlyoutItem()
            {
                Text = Helper.LocalizeText("ViaEmail"),
            };
            emailItem.Click += ViaEmailMenuFlyoutItem_Click;
            flyout.Items.Add(emailItem);
            var webBrowserItem = new MenuFlyoutItem()
            {
                Text = Helper.LocalizeText("ViaWebBrowser"),
            };
            webBrowserItem.Click += ViaWebBrowserMenuFlyoutItem_Click;
            flyout.Items.Add(webBrowserItem);
            flyout.ShowAt(sender as FrameworkElement);
        }

        private async void ViaEmailMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await ComposeEmail("luokiss9@qq.com", Helper.LocalizeText("ShareFeedBacks"), "");
        }

        private bool IsProcessing = false;
        private async void ViaWebBrowserMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            string uri = "https://github.com/SeakyLuo/SMPlayer/issues";
            if (await Windows.System.Launcher.LaunchUriAsync(new Uri(uri)))
            {

            }
            else
            {
                DataPackage dataPackage = new DataPackage()
                {
                    RequestedOperation = DataPackageOperation.Copy
                };
                dataPackage.SetText(uri);
                Clipboard.SetContent(dataPackage);
                MainPage.Instance.ShowNotification(Helper.LocalizeMessage("FailToOpenBrowser"));
            }
            IsProcessing = false;
        }

        private async void NotificationModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotificationModeComboBox.SelectedIndex < 0) return;
            Settings.settings.NotificationDisplay = SettingsEnum.NotificationDisplayModes[NotificationModeComboBox.SelectedIndex];
            if (Settings.settings.NotificationDisplay == NotificationDisplayMode.Reminder)
            {
                await ToastHelper.ShowToast(MusicPlayer.CurrentMusic, MusicPlayer.PlaybackState);
            }
        }

        private void NotificationSendComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotificationModeComboBox.SelectedIndex < 0) return;
            Settings.settings.NotificationSend = SettingsEnum.NotificationSendModes[NotificationSendComboBox.SelectedIndex];
        }

        private void AutoPlayToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoPlay = (sender as ToggleSwitch).IsOn;
        }

        private async void ShowLyricsInNotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (Settings.settings.ShowLyricsInNotification = (sender as ToggleSwitch).IsOn)
            {
                await LyricsHelper.SetLyrics();
            }
            else
            {
                LyricsHelper.ClearLyrics();
            }
        }

        private void HideMultiSelectCommandBarToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.HideMultiSelectCommandBarAfterOperation = (sender as ToggleSwitch).IsOn;
        }

        private void ShowCountToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.ShowCount = (sender as ToggleSwitch).IsOn;
        }

        private void AutoLyricsToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoLyrics = (sender as ToggleSwitch).IsOn;
        }

        private void SaveProgressToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.SaveMusicProgress = (sender as ToggleSwitch).IsOn;
        }

        private VoiceAssistantHelpDialog voiceAssistantHelpDialog;
        private async void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (voiceAssistantHelpDialog == null)
            {
                voiceAssistantHelpDialog = new VoiceAssistantHelpDialog();
            }
            await voiceAssistantHelpDialog.ShowAsync();
        }

        private void VoiceAssistantLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VoiceAssistantLanguage language = VoiceAssistantPreferredLanguanges[(sender as ComboBox).SelectedIndex];
            Settings.settings.VoiceAssistantPreferredLanguage = language;
            VoiceAssistantHelper.SetLanguage(language);
        }

        private void PreferenceSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(PreferenceSettingsPage));
        }
    }
}
