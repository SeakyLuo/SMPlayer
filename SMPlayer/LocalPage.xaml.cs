using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page, LocalSetter, AfterPathSetListener
    {
        public static ViewModeChangedListener MusicViewModeChangedListener;
        public static ViewModeChangedListener FolderViewModeChangedListener;
        public static Stack<FolderTree> History = new Stack<FolderTree>();
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            LocalFoldersPage.setter = this;
            SetPage(Settings.settings.Tree);
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackButtonVisibility();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TreeInfo info = History.Peek().Info;
            MainPage.Instance.SetHeaderText(string.IsNullOrEmpty(info.Directory) ? "No Music" : info.Directory);
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = (NavigationViewItem)LocalNavigationView.SelectedItem;
            bool isFolders = item.Name == "LocalFoldersItem";
            Type page = isFolders ? typeof(LocalFoldersPage) : typeof(LocalMusicPage);
            if (LocalFrame.CurrentSourcePageType != page)
            {
                if (LocalFrame.CanGoBack && LocalFrame.BackStack.Last().SourcePageType == page)
                    LocalFrame.GoBack();
                else
                    LocalFrame.Navigate(page, History.Peek());
                SetLocalGridView(isFolders ? Settings.settings.LocalFolderGridView : Settings.settings.LocalMusicGridView);
            }
        }

        private void SetBackButtonVisibility()
        {
            LocalNavigationView.IsBackButtonVisible = History.Count == 1 ? NavigationViewBackButtonVisible.Collapsed : NavigationViewBackButtonVisible.Visible;
        }

        private void LocalNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (History.Count == 1) return;
            var tree = History.Pop();
            int last;
            PageStackEntry page;
            do
            {
                page = LocalFrame.BackStack[last = LocalFrame.BackStack.Count - 1];
                if (page.Parameter == tree) LocalFrame.BackStack.RemoveAt(last);
                else break;
            } while (true);
            LocalFrame.GoBack();
            var info = History.Peek().Info;
            SetText(info);
            switch (page.SourcePageType.Name)
            {
                case "LocalFoldersPage":
                    LocalFoldersItem.IsSelected = true;
                    break;
                case "LocalMusicPage":
                    LocalSongsItem.IsSelected = true;
                    break;
            }
            SetBackButtonVisibility();
        }

        private void SetLocalGridView(bool isGridView)
        {
            if (isGridView)
            {
                LocalGridViewItem.Visibility = Visibility.Collapsed;
                LocalListViewItem.Visibility = Visibility.Visible;
            }
            else
            {
                LocalGridViewItem.Visibility = Visibility.Visible;
                LocalListViewItem.Visibility = Visibility.Collapsed;
            }
        }

        private void LocalListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool isGridView = false;
            SetLocalGridView(isGridView);
            SetMode(isGridView);
        }

        private void LocalGridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool isGridView = true;
            SetLocalGridView(isGridView);
            SetMode(isGridView);
        }

        public void SetMode(bool isGridView)
        {
            if (LocalFrame.CurrentSourcePageType.Name == "LocalFoldersPage")
            {
                Settings.settings.LocalFolderGridView = isGridView;
                if (FolderViewModeChangedListener != null) FolderViewModeChangedListener.ModeChanged(isGridView);
            }
            else
            {
                Settings.settings.LocalMusicGridView = isGridView;
                if (MusicViewModeChangedListener != null) MusicViewModeChangedListener.ModeChanged(isGridView);
            }
        }

        private void LocalShuffleItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tree = History.Peek();
            MediaHelper.ShuffleAndPlay(LocalFrame.SourcePageType == typeof(LocalMusicPage) ? tree.Files : tree.Flatten());
        }

        private void SetText(TreeInfo info, bool setHeader = true)
        {
            if (setHeader)
                MainPage.Instance.SetHeaderText(string.IsNullOrEmpty(info.Directory) ? "No Music" : info.Directory);
            LocalFoldersItem.Content = $"Folders ({info.Folders})";
            LocalFoldersItem.IsEnabled = info.Folders != 0;
            LocalSongsItem.Content = $"Songs ({info.Songs})";
            LocalSongsItem.IsEnabled = info.Songs != 0;
        }

        public void SetPage(FolderTree tree, bool setHeader = true)
        {
            if (History.Count > 0 && History.Peek() == tree) return;
            History.Push(tree);
            SetBackButtonVisibility();
            TreeInfo info = tree.Info;
            SetText(info, setHeader);
            if (IsBackToMusicPage(info))
            {
                LocalNavigationView.SelectedItem = LocalSongsItem;
                LocalFrame.Navigate(typeof(LocalMusicPage), tree);
                SetLocalGridView(Settings.settings.LocalMusicGridView);
            }
            else if (info.Folders > 0)
            {
                LocalNavigationView.SelectedItem = LocalFoldersItem;
                LocalFrame.Navigate(typeof(LocalFoldersPage), tree);
                SetLocalGridView(Settings.settings.LocalFolderGridView);
            }
        }

        private bool IsBackToMusicPage(TreeInfo info)
        {
            return info.Songs > info.Folders;
        }

        public void PathSet(string path)
        {
            History.Clear();
            SetPage(Settings.settings.Tree, false);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            if (History.Count == 0) return;
            var tree = History.Peek();
            var menu = sender as MenuFlyout;
            var helper = new MenuFlyoutHelper()
            {
                Data = tree.Files,
                DefaultPlaylistName = tree.Directory
            };
            menu.Items.Add(helper.GetAddToMenuFlyoutSubItem());
            menu.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, StorageItemTypes.Folder));
        }
    }
    public interface LocalSetter
    {
        void SetPage(FolderTree tree, bool setHeader = true);
    }

    public interface ViewModeChangedListener
    {
        void ModeChanged(bool isGridView);
    }
}