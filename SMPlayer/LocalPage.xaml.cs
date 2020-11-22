using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page, ILocalSetter, IAfterPathSetListener
    {
        public static ILocalPageButtonListener MusicListener, FolderListener;
        public static Stack<FolderTree> History = new Stack<FolderTree>();
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            LocalFoldersPage.setter = this;
            LocalMusicPage.setter = this;
            SetPage(Settings.settings.Tree);
            SettingsPage.AddAfterPathSetListener(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is GridFolderView folder)
            {
                SetPage(folder.Tree);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackButtonVisibility();
            SetHeader(History.Peek().Info);
        }

        private void SetHeader(TreeInfo info)
        {
            MainPage.Instance.SetHeaderText(string.IsNullOrEmpty(info.Directory) ? Helper.LocalizeMessage("No Music") : info.Directory);
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            bool isFolders = LocalFoldersItem.IsSelected;
            Type page = isFolders ? typeof(LocalFoldersPage) : typeof(LocalMusicPage);
            if (LocalFrame.CurrentSourcePageType == page) return;
            PageStackEntry lastPage = LocalFrame.BackStackDepth == 0 ? null : LocalFrame.BackStack.Last();
            if (LocalFrame.CanGoBack && lastPage.SourcePageType == page && lastPage.Parameter == History.Peek())
                LocalFrame.GoBack();
            else
                LocalFrame.Navigate(page, History.Peek());
            SetLocalGridView(isFolders ? Settings.settings.LocalFolderGridView : Settings.settings.LocalMusicGridView);
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
                    SetLocalGridView(Settings.settings.LocalFolderGridView);
                    break;
                case "LocalMusicPage":
                    LocalSongsItem.IsSelected = true;
                    SetLocalGridView(Settings.settings.LocalMusicGridView);
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
            if (LocalFoldersItem.IsSelected)
            {
                Settings.settings.LocalFolderGridView = isGridView;
                FolderListener?.ModeChanged(isGridView);
            }
            else
            {
                Settings.settings.LocalMusicGridView = isGridView;
                MusicListener?.ModeChanged(isGridView);
            }
        }

        private void LocalRefreshItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SettingsPage.CheckNewMusic(History.Peek(), tree =>
            {
                if (LocalFoldersItem.IsSelected)
                {
                    FolderListener.UpdatePage(tree);
                    MusicListener?.UpdatePage(tree);
                }
                else
                {
                    MusicListener.UpdatePage(tree);
                    FolderListener?.UpdatePage(tree);
                }
            });
        }

        private void LocalShuffleItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tree = History.Peek();
            MediaHelper.ShuffleAndPlay(LocalFrame.SourcePageType == typeof(LocalMusicPage) ? tree.Files : tree.Flatten());
        }

        private void SetText(TreeInfo info, bool setHeader = true)
        {
            if (setHeader) SetHeader(info);
            SetNavText(info);
        }
        public void SetNavText(TreeInfo info)
        {
            LocalFoldersItem.Content = Helper.LocalizeMessage("Folders", info.Folders);
            LocalFoldersItem.IsEnabled = info.Folders != 0;
            LocalSongsItem.Content = Helper.LocalizeMessage("Songs", info.Songs);
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
            else
            {
                LocalNavigationView.SelectedItem = null;
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
            if (LocalFoldersItem.IsSelected) FolderListener.UpdatePage(Settings.settings.Tree);
            else MusicListener.UpdatePage(Settings.settings.Tree);
        }

        private void OpenLocalMusicFlyout(object sender, object e)
        {
            if (History.Count == 0) return;
            var tree = History.Peek();
            var flyout = sender as MenuFlyout;
            flyout.Items.Clear();
            var helper = new MenuFlyoutHelper()
            {
                Data = tree.Files,
                DefaultPlaylistName = tree.Directory
            };
            flyout.Items.Add(helper.GetAddToMenuFlyoutSubItem());
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, StorageItemTypes.Folder));
            flyout.Items.Add(MenuFlyoutHelper.GetSortByMenuSubItem(new Dictionary<SortBy, Action>
            {
                { SortBy.Title, () =>
                {
                    if (LocalFrame.Content is LocalMusicPage page) page.SortByTitle();
                    else LocalMusicPage.SortByTitleRequested = !LocalMusicPage.SortByTitleRequested;
                } },
                { SortBy.Artist, () =>
                {
                    if (LocalFrame.Content is LocalMusicPage page) page.SortByArtist();
                    else LocalMusicPage.SortByArtistRequested = !LocalMusicPage.SortByArtistRequested;
                } },
                { SortBy.Album, () =>
                {
                    if (LocalFrame.Content is LocalMusicPage page) page.SortByAlbum();
                    else LocalMusicPage.SortByAlbumRequested = !LocalMusicPage.SortByAlbumRequested;
                } },
            },
            () =>
            {
                if (LocalFrame.Content is LocalMusicPage page) page.Reverse();
                else LocalMusicPage.ReverseRequested = !LocalMusicPage.ReverseRequested;
            }));
        }
    }
    public interface ILocalSetter
    {
        void SetPage(FolderTree tree, bool setHeader = true);
        void SetNavText(TreeInfo treeInfo);
    }

    public interface ILocalPageButtonListener
    {
        void ModeChanged(bool isGridView);
        void UpdatePage(FolderTree tree);
    }
}