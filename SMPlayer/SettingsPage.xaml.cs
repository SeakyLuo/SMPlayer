using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
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
    public sealed partial class SettingsPage : Page, IAfterPathSetListener
    {
        public static ShowToast[] NotificationOptions = { ShowToast.Always, ShowToast.MusicChanged, ShowToast.Never };
        private static readonly int[] LimitedRecentPlayedItems = { -1, 100, 200, 500, 1000 };
        private static readonly List<IAfterPathSetListener> listeners = new List<IAfterPathSetListener>();
        private static FolderTree loadingTree;
        private volatile int addLyricsClickCounter = 0;
        private readonly string addLyricsContent = Helper.Localize("AddLyrics");

        public SettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            MainPage.Instance.Loader.BreakLoadingListeners.Add(() => loadingTree?.PauseLoading());
            listeners.Add(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PathBox.Text = Settings.settings.RootPath;
            NotificationComboBox.SelectedIndex = (int)Settings.settings.Toast;
            ThemeColorPicker.Color = Settings.settings.ThemeColor;
            ShowCounterCheckBox.IsChecked = Settings.settings.ShowCount;
            KeepRecentComboBox.SelectedIndex = LimitedRecentPlayedItems.FindIndex(num => num == Settings.settings.LimitedRecentPlayedItems);
            AutoPlayCheckBox.IsChecked = Settings.settings.AutoPlay;
            AutoLyricsCheckBox.IsChecked = Settings.settings.AutoLyrics;
            SaveProgressCheckBox.IsChecked = Settings.settings.SaveMusicProgress;
            HideMultiSelectCommandBarCheckBox.IsChecked = Settings.settings.HideMultiSelectCommandBarAfterOperation;
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                await UpdateMusicLibrary(folder);
            }
        }

        public static async Task<bool> UpdateMusicLibrary(string message = null)
        {
            return Helper.CurrentFolder == null || await UpdateMusicLibrary(Helper.CurrentFolder, message);
        }

        public static async Task<bool> UpdateMusicLibrary(StorageFolder folder, string message = null)
        {
            MainPage.Instance.Loader.ShowDeterminant(message ?? "LoadMusicLibrary", true);
            loadingTree = new FolderTree();
            if (!await loadingTree.Init(folder, (treeFolder, file, progress, max) =>
            {
                bool isDeterminant = max != 0;
                if (MainPage.Instance.Loader.IsDeterminant != isDeterminant)
                    MainPage.Instance.Loader.IsDeterminant = isDeterminant;
                if (isDeterminant)
                {
                    MainPage.Instance.Loader.Max = max;
                    MainPage.Instance.Loader.Progress = progress;
                    MainPage.Instance.Loader.Text = message ?? file;
                }
            }))
            {
                return false;
            }
            MainPage.Instance.Loader.SetLocalizedText(message ?? "UpdateMusicLibrary");
            Helper.CurrentFolder = folder;
            await Task.Run(() =>
            {
                loadingTree.MergeFrom(Settings.settings.Tree);
                Settings.settings.Tree = loadingTree;
                Settings.settings.RootPath = folder.Path;
            });
            MusicLibraryPage.SortAndSetAllSongs(await Task.Run(Settings.settings.Tree.Flatten));
            MainPage.Instance.Loader.Progress = 0;
            MainPage.Instance.Loader.Max = listeners.Count;
            for (int i = 0; i < listeners.Count;)
            {
                var listener = listeners[i];
                listener.PathSet(folder.Path);
                MainPage.Instance.Loader.Progress = ++i;
            }
            MediaHelper.RemoveBadMusic();
            App.Save();
            MainPage.Instance.Loader.Hide();
            return true;
        }

        public static void NotifyLibraryChange(string path) { foreach (var listener in listeners) listener.PathSet(path); }

        public static void AddAfterPathSetListener(IAfterPathSetListener listener)
        {
            listeners.Add(listener);
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

        private void NotificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.Toast = NotificationOptions[(sender as ComboBox).SelectedIndex];
        }

        public static async void CheckNewMusic(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
        {
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            var data = new TreeUpdateData();
            if (!await tree.CheckNewFile(data))
            {
                if (!string.IsNullOrEmpty(data.Message))
                    MainPage.Instance.ShowNotification(data.Message);
                MainPage.Instance?.Loader.Hide();
                return;
            }
            if (data.More != 0 || data.Less != 0)
            {
                Settings.settings.Tree.FindTree(tree).CopyFrom(tree);
                MusicLibraryPage.SortAndSetAllSongs(Settings.settings.Tree.Flatten());
                foreach (var listener in listeners)
                    listener.PathSet(tree.Path);
                if (data.Less != 0) MediaHelper.RemoveBadMusic();
                afterTreeUpdated?.Invoke(tree);
                App.Save();
            }
            MainPage.Instance?.Loader.Hide();
            string message;
            if (data.More == 0 && data.Less == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultNoChange");
            else if (data.More == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultRemoved", data.Less);
            else if (data.Less == 0)
                message = Helper.LocalizeMessage("CheckNewMusicResultAdded", data.More);
            else
                message = Helper.LocalizeMessage("CheckNewMusicResultChange", data.More, data.Less);
            Helper.ShowNotificationWithoutLocalization(message);
        }

        private async void UpdateMusicLibrary_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.CurrentFolder == null)
            {
                MainPage.Instance.ShowNotification("SetRootFolder");
            }
            else
            {
                await UpdateMusicLibrary();
            }

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
            int count = MusicLibraryPage.SongCount, counter = 0;
            foreach (Music music in MusicLibraryPage.AllSongs)
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
                    if (music == MediaHelper.CurrentMusic)
                    {
                        skipped.Add(music);
                        continue;
                    }
                    await Task.Run(async () =>
                    {
                        lyrics = await Controls.MusicLyricsControl.SearchLyrics(music);
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
                    if (music == MediaHelper.CurrentMusic && skipped.Count > 1) continue;
                    await Task.Run(async () =>
                    {
                        string lyrics = await Controls.MusicLyricsControl.SearchLyrics(music);
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


        private void AutoPlayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoPlay = true;
        }

        private void AutoPlayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoPlay = false;
        }

        private void SaveProgressCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.SaveMusicProgress = true;
        }

        private void SaveProgressCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.SaveMusicProgress = false;
        }

        private void KeepRecentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.LimitedRecentPlayedItems = LimitedRecentPlayedItems[(sender as ComboBox).SelectedIndex];
        }

        private void AutoLyricsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoLyrics = true;
        }

        private void AutoLyricsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.AutoLyrics = false;
        }

        private async void ExportData_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                JsonFileHelper.SaveAsync(folder, Settings.JsonFilename, Settings.settings);
                MainPage.Instance.Loader.Hide();
                MainPage.Instance.ShowLocalizedNotification("DataExported");
            }
        }

        private async void ImportData_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".json");
            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
                try
                {
                    string json = await FileIO.ReadTextAsync(file);
                    Settings settings = JsonFileHelper.Convert<Settings>(json);
                    bool successful = await UpdateMusicLibrary();
                    MainPage.Instance.Loader.Hide();
                    MainPage.Instance.ShowLocalizedNotification(successful ? "DataImported" : "ImportDataCancelled");
                }
                catch (Exception ex)
                {
                    MainPage.Instance.Loader.Hide();
                    MainPage.Instance.ShowLocalizedNotification("ImportDataFailed" + ex.Message);
                }
            }
        }

        void IAfterPathSetListener.PathSet(string path)
        {
            PathBox.Text = path;
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

        private void HideMultiSelectCommandBarCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.HideMultiSelectCommandBarAfterOperation = true;
        }

        private void HideMultiSelectCommandBarCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.HideMultiSelectCommandBarAfterOperation = false;
        }

        private void ShowCounterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.ShowCount = true;
        }

        private void ShowCounterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.ShowCount = false;
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
    }

    public interface IAfterPathSetListener
    {
        void PathSet(string path);
    }
}
