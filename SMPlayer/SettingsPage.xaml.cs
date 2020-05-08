﻿using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page, TreeOperationListener
    {
        public static ShowToast[] NotificationOptions = { ShowToast.Always, ShowToast.MusicChanged, ShowToast.Never };
        private static List<AfterPathSetListener> listeners = new List<AfterPathSetListener>();
        private FolderTree loadingTree;
        private volatile int addLyricsClickCounter = 0;
        private string addLyricsContent = Helper.Localize("AddLyrics");

        public SettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            MainPage.Instance.Loader.BreakLoadingListeners.Add(() => loadingTree?.PauseLoading());
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
            MainPage.Instance.Loader.ShowDeterminant("LoadMusicLibrary", true);
            Helper.CurrentFolder = folder;
            loadingTree = new FolderTree();
            if (!await loadingTree.Init(folder, this)) return;
            MainPage.Instance.Loader.SetLocalizedText("UpdateMusicLibrary");
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
            PathBox.Text = folder.Path;
            MainPage.Instance.Loader.Hide();
        }

        public static void NotifyLibraryChange(string path) { foreach (var listener in listeners) listener.PathSet(path); }

        public static void AddAfterPathSetListener(AfterPathSetListener listener)
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

        public static async void CheckNewMusic(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
        {
            MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest");
            var data = new TreeUpdateData();
            if (!await tree.CheckNewFile(data)) return;
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
            Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("CheckNewMusicResult", data.More, data.Less));
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
                MainPage.Instance.ShowLocalizedNotification("FailToOpenBrowser");
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
            int count = MusicLibraryPage.AllSongs.Count, counter = 0;
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

        private void KeepRecentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.KeepLimitedRecentPlayedItems = true;
            while (Settings.settings.RecentPlayed.Count > Settings.RecentPlayedLimit)
                Settings.settings.RecentPlayed.RemoveAt(Settings.RecentPlayedLimit);
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

    public interface AfterPathSetListener
    {
        void PathSet(string path);
    }
}
