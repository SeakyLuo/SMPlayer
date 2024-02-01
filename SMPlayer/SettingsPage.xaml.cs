using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.Enums;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;
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
        public static List<NotificationSendMode> NotificationOptions = EnumHelper.Values<NotificationSendMode>(typeof(NotificationSendMode));
        private static readonly int[] LimitedRecentPlayedItems = { -1, 100, 200, 500, 1000 };
        private static readonly List<SupportedLanguage> VoiceAssistantPreferredLanguanges = EnumHelper.Values<SupportedLanguage>(typeof(SupportedLanguage));
        private static readonly List<LyricsSource> LyricsSources = EnumHelper.Values<LyricsSource>(typeof(LyricsSource));
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
            NotificationSendComboBox.SelectedIndex = (int)settings.NotificationSend;
            NotificationModeComboBox.SelectedIndex = (int)settings.NotificationDisplay;
            ThemeColorPicker.Color = settings.ThemeColor;
            ShowCountToggleSwitch.IsOn = settings.ShowCount;
            AutoPlayToggleSwitch.IsOn = settings.AutoPlay;
            AutoLyricsToggleSwitch.IsOn = settings.AutoLyrics;
            SaveProgressToggleSwitch.IsOn = settings.SaveMusicProgress;
            HideMultiSelectCommandBarToggleSwitch.IsOn = settings.HideMultiSelectCommandBarAfterOperation;
            ShowLyricsInNotificationToggleSwitch.IsOn = settings.ShowLyricsInNotification;
            LanguageComboBox.SelectedIndex = (int)settings.VoiceAssistantPreferredLanguage;
            NotificationLyricsSourceComboBox.SelectedIndex = (int)settings.NotificationLyricsSource;
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            StorageFolder folder = await StorageHelper.PickFolder();
            if (folder == null) return;
            if (await UpdateHelper.UpdateMusicLibrary(folder))
            {
                PathBox.Text = folder.Path;
            }
        }

        private void ConfirmColorButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.settings.ThemeColor = ThemeColorPicker.Color;
            ThemeColorButton.Background = new SolidColorBrush(ThemeColorPicker.Color);
            Helper.ShowNotification("NotImplemented");
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
            Helper.ShowNotification("ChangesSaved");
        }

        private async void AddLyrics_Click(object sender, RoutedEventArgs e)
        {
            ++addLyricsClickCounter;
            if (addLyricsClickCounter == 2)
            {
                Helper.ShowNotification("ClickAgainToStopAddingLyrics");
                return;
            }
            else if (addLyricsClickCounter == 3)
            {
                addLyricsClickCounter = 0;
                MainPage.Instance.Loader.ShowIndeterminant("StopAddingLyrics");
                return;
            }
            try
            {
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
                        await Task.Run(async () => await music.FindLyricsIfEmpty());
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
                        await Task.Run(async () => await music.FindLyricsIfEmpty());
                        skipped.Remove(music);
                        button.Content = string.Format(paren, addLyricsContent, ++counter + "/" + count);
                    }
                }
            Done:
                button.Content = addLyricsContent;
                Helper.ShowNotification("SearchLyricsDone");
            }
            catch (Exception ex)
            {
                Log.Warn($"AddLyrics_Click failed {ex}");
                Helper.ShowOperationFailedNotification(ex);
            }
            finally
            {
                addLyricsClickCounter = 0;
            }
        }

        private async void Reauthorize_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageHelper.PickFolder();
            if (folder == null) return;
            if (folder.Path == Settings.settings.RootPath)
            {
                Helper.ShowNotification("AuthorizeSuccessful");
            }
            else
            {
                Helper.ShowNotification("AuthorizeFolderFailed");
            }
        }

        private async void AuthorizeOtherFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await StorageHelper.AuthorizeFolder();
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
            StorageFile dbFile = await StorageHelper.LoadFileAsync(SQLHelper.BuildDBPath());
            try
            {
                await dbFile.CopyAsync(folder);
            }
            catch (Exception ex)
            {
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("OperationFailed", ex), 5000);
            }
            MainPage.Instance.Loader.Hide();
            Helper.ShowNotification("DataExported");
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
                string originalPath = Settings.settings.RootPath;
                if (!await SettingsHelper.InitWithFile(file))
                {
                    MainPage.Instance.Loader.Hide();
                    return;
                }
                if (string.IsNullOrEmpty(originalPath) && SQLHelper.Run(c => c.SelectSettings()) is Settings tempSettings)
                {
                    // 这个场景适用于迁移到新设备时，直接通过导入获取老设备的路径，此时Settings.settings.RootPath在InitWithFile时已经更新为新的路径了，所以得再查一次SQL
                    originalPath = tempSettings.RootPath;
                }
                PathBox.Text = Settings.settings.RootPath;
                bool successful;
                if (originalPath == Settings.settings.RootPath)
                {
                    successful = await UpdateHelper.UpdateMusicLibrary(Helper.MusicFolder);
                }
                else
                {
                    MainPage.Instance.Loader.ShowIndeterminant("DifferentPathDetectedWhenImportingData");
                    SQLHelper.UpdatePath(originalPath, Settings.settings.RootPath);
                    successful = true;
                }
                if (successful)
                {
                    App.Save();
                    FillSettings(Settings.settings);
                }
                MainPage.Instance.Loader.Hide();
                Helper.ShowNotification(successful ? "DataImported" : "ImportDataCancelled");
            }
            catch (Exception ex)
            {
                MainPage.Instance.Loader.Hide();
                Debug.WriteLine($"ImportData_Click {ex}");
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("ImportDataFailed") + ex.Message);
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
            await Helper.SendEmailToDeveloper("ShareFeedBacks", "");
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
                Helper.CopyStringToClipboard(uri);
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

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SupportedLanguage language = VoiceAssistantPreferredLanguanges[(sender as ComboBox).SelectedIndex];
            if (Settings.settings.VoiceAssistantPreferredLanguage == language)
            {
                return;
            }
            Settings.settings.VoiceAssistantPreferredLanguage = language;
            string languageStr;
            if (language == SupportedLanguage.Chinese)
            {
                languageStr = Helper.Language_CN;
            }
            else
            {
                languageStr = Helper.Language_EN;
            }
            ApplicationLanguages.PrimaryLanguageOverride = languageStr;
            string message = Helper.LocalizeMessageWithLanguage("SwitchLanguageSuccessful", languageStr);
            int time = 3000;
            try
            {
                await VoiceAssistantHelper.SetLanguage(language);
            }
            catch (Exception ex)
            {
                message += Helper.LocalizeMessageWithLanguage("VoiceAssistantSetLanguageFailed", languageStr) + ex.Message;
                time += 5000;
            }
            Helper.ShowNotificationRaw(message, time);
        }

        private void PreferenceSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(PreferenceSettingsPage));
        }

        private async void SystemLogButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Logs");
            await Helper.ShowInExplorer(path, StorageItemTypes.Folder);
        }

        private void LoadMusicNameOptionToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.UseFilenameNotMusicName = (sender as ToggleSwitch).IsOn;
        }

        private void NotificationLyricsSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.NotificationLyricsSource = LyricsSources[(sender as ComboBox).SelectedIndex];
        }

        private void SaveLyricsImmediatelyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.settings.SaveLyricsImmediately = (sender as ToggleSwitch).IsOn;
        }
    }
}
