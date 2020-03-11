using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page, TreeOperationProgressListener
    {
        public static ShowToast[] NotificationOptions = { ShowToast.Always, ShowToast.MusicChanged, ShowToast.Never };
        private static List<AfterLibraryUpdated> listeners = new List<AfterLibraryUpdated>();
        private FolderTree loadingTree;
        public SettingsPage()
        {
            this.InitializeComponent();
            MainPage.Instance.Loader.BreakLoadingListeners.Add(() => loadingTree.PauseLoading());
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PathBox.Text = Settings.settings.RootPath;
            NotificationComboBox.SelectedIndex = (int)Settings.settings.Toast;
            ThemeColorPicker.Color = Settings.settings.ThemeColor;
            KeepRecentCheckBox.IsChecked = Settings.settings.KeepLimitedRecentPlayedItems;
            AutoPlayCheckBox.IsChecked = Settings.settings.AutoPlay;
            SaveProgressCheckBox.IsChecked = Settings.settings.SaveMusicProgress;
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
                UpdateMusicLibrary(folder);
            }
        }

        public async void UpdateMusicLibrary(StorageFolder folder)
        {
            if (folder == null) return;
            MainPage.Instance.Loader.Show("LoadMusicLibrary", true);
            Helper.CurrentFolder = folder;
            loadingTree = new FolderTree();
            if (!await loadingTree.Init(folder, this)) return;
            MainPage.Instance.Loader.SetLocalizedText("UpdateMusicLibrary");
            loadingTree.MergeFrom(Settings.settings.Tree);
            Settings.settings.Tree = loadingTree;
            Settings.settings.RootPath = folder.Path;
            MusicLibraryPage.SetAllSongs(Settings.settings.Tree.Flatten());
            MainPage.Instance.Loader.Max = listeners.Count;
            for (int i = 0; i < listeners.Count; i++)
            {
                var listener = listeners[i];
                listener.LibraryUpdated(folder.Path);
                MainPage.Instance.Loader.Progress = i;
            }
            MediaHelper.RemoveBadMusic();
            Settings.Save();
            PathBox.Text = folder.Path;
            MainPage.Instance.Loader.Hide();
        }

        public static void NotifyLibraryChange(string path) { foreach (var listener in listeners) listener.LibraryUpdated(path); }

        public static void AddAfterPathSetListener(AfterLibraryUpdated listener)
        {
            listeners.Add(listener);
        }

        private void ConfirmColorButton_Click(object sender, RoutedEventArgs e)
        {
            //Settings.settings.ThemeColor = ThemeColorPicker.Color;
            ColorPickerFlyout.Hide();
        }

        private void CancelColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerFlyout.Hide();
        }

        private void NotificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.Toast = NotificationOptions[(sender as ComboBox).SelectedIndex];
        }

        public void Update(string folder, string file, int progress, int max)
        {
            bool isDeterminant = max != 0;
            if (MainPage.Instance.Loader.IsDeterminant != isDeterminant)
                MainPage.Instance.Loader.IsDeterminant = isDeterminant;
            if (isDeterminant)
            {
                MainPage.Instance.Loader.Max = max;
                MainPage.Instance.Loader.Progress = progress;
                MainPage.Instance.Loader.Text = file;
            }
        }

        private void CheckNewMusicButton_Click(object sender, RoutedEventArgs e)
        {
            CheckNewMusic(Settings.settings.Tree);
        }

        public static async void CheckNewMusic(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
        {
            MainPage.Instance.Loader.Show("ProcessRequest", false);
            var indicator = await tree.CheckNewFile();
            Settings.settings.Tree.FindTree(tree).CopyFrom(tree);
            if (indicator.Progress != 0 || indicator.Max != 0)
            {
                foreach (var listener in listeners)
                    listener.LibraryUpdated(tree.Path);
                if (indicator.Max != 0) MediaHelper.RemoveBadMusic();
            }
            Settings.Save();
            afterTreeUpdated.Invoke(tree);
            MainPage.Instance.Loader.Hide();
            MainPage.Instance.ShowNotification(Helper.LocalizeMessage("CheckNewMusicResult", indicator.Progress, -indicator.Max));
        }

        private void UpdateMusicLibrary_Click(object sender, RoutedEventArgs e)
        {
            UpdateMusicLibrary(Helper.CurrentFolder);
        }

        private async void BugReport_Click(object sender, RoutedEventArgs e)
        {
            if (await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/SeakyLuo/SMPlayer/issues")))
            {

            }
            else
            {
                MainPage.Instance.ShowNotification(Helper.LocalizeMessage("FailToOpenBrowser"));
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            App.Save();
            Helper.ShowNotification("ChangesSaved");
        }

        private void KeepRecentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.KeepLimitedRecentPlayedItems = true;
            while (Settings.settings.Recent.Count > Settings.RecentPlayedLimit)
                Settings.settings.Recent.RemoveAt(Settings.RecentPlayedLimit);
        }

        private void KeepRecentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.KeepLimitedRecentPlayedItems = false;
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
    }

    public interface AfterLibraryUpdated
    {
        void LibraryUpdated(string path);
    }
}
