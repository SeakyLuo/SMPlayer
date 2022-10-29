using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page, IStorageItemEventListener, IMenuFlyoutItemClickListener, IMenuFlyoutHelperBuildListener, IMultiSelectListener, IMusicPlayerEventListener, IMusicEventListener
    {
        private readonly ObservableCollection<FolderChainItem> FolderChain = new ObservableCollection<FolderChainItem>();
        private readonly ObservableCollection<GridViewStorageItem> GridItems = new ObservableCollection<GridViewStorageItem>();
        private bool IsProcessing = false, folderUpdated = false;
        private FolderTree CurrentFolderInfo { get => FolderChain.IsEmpty() ? null : FolderChain.Last().ToFolderTree(); }
        private FolderTree CurrentFolder
        {
            get 
            {
                FolderTree folder = CurrentFolderInfo;
                if (folder == null) return null;
                folder.Files = GridItems.Where(i => i is GridViewMusic)
                                        .Select(i => (i as GridViewMusic).ToFolderFile())
                                        .ToList();
                folder.Trees = GridItems.Where(i => i is GridViewFolder)
                                        .Select(i => (i as GridViewFolder).Source)
                                        .ToList();
                return folder;
            }
        }
        private List<MusicView> CurrentSongs => GridItems.Where(i => i is GridViewMusic)
                                                     .Select(i => (i as GridViewMusic).Source)
                                                     .ToList();

        public ListViewBase CurrentListView
        {
            get
            {
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        return LocalTreeView;
                    case LocalPageViewMode.Grid:
                        return LocalGridView;
                    default:
                        return null;
                }
            }
        }
        public List<Music> SelectedSongs
        {
            get
            {
                List<Music> list = new List<Music>();
                foreach (var item in CurrentListView.SelectedItems)
                {
                    if (item is GridViewFolder folder)
                    {
                        list.AddRange(folder.Songs);
                    }
                    else if (item is GridViewMusic music)
                    {
                        list.Add(music.Source.FromVO());
                    }
                }
                return list;
            }
        }

        private MultiSelectCommandBarOption MultiSelectOption => new MultiSelectCommandBarOption
        {
            ShowRemove = false,
            ShowMoveToFolder = true,
        };

        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            SwitchViewMode(Settings.settings.LocalViewMode);
            StorageService.AddStorageItemEventListener(this);
            MusicService.AddMusicEventListener(this);
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FolderTree folder;
            if (e.Parameter is GridViewFolder gridViewFolder)
            {
                folder = gridViewFolder.Source;
            }
            else if (e.Parameter is FolderTree folderTree)
            {
                folder = folderTree;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                folder = CurrentFolder;
            }
            else
            {
                folder = CurrentFolder ?? StorageService.Root;
            }
            SetPage(folder);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
        }

        public void SetPage(FolderTree tree)
        {
            if (!folderUpdated)
            {
                if (IsProcessing) return;
                if (tree.Equals(CurrentFolderInfo)) return;
            }
            IsProcessing = true;
            LocalProgressRing.Visibility = Visibility.Visible;
            ClearMultiSelectStatus();
            tree.SortFolders();
            tree.SortFiles();
            SetupGridView(tree);
            ResetFolderChain(tree.Path);
            SetNavText(tree);
            LocalCommandBar.IsEnabled = !string.IsNullOrEmpty(tree.Path);
            LocalProgressRing.Visibility = Visibility.Collapsed;
            folderUpdated = false;
            IsProcessing = false;
        }

        // 为了更好的动画效果
        private void ResetFolderChain(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            int index = -1;
            for (int i = 0; i < FolderChain.Count; i++)
            {
                FolderChainItem item = FolderChain[i];
                if (!path.StartsWith(item.Path))
                {
                    index = i;
                    break;
                }
            }
            if (index > -1) FolderChain.RemoveAfter(index);
            string start = FolderChain.LastOrDefault()?.Path ?? StorageHelper.GetParentPath(Settings.settings.RootPath);
            string[] folders = path.Substring(start.Length).Split(Path.DirectorySeparatorChar);
            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder)) continue;
                start = Path.Combine(start, folder);
                FolderChain.Add(new FolderChainItem(StorageService.FindFolder(start)));
            }
            if (FolderChain.IsNotEmpty())
            {
                foreach (var folder in FolderChain)
                {
                    folder.IsLastItem = true;
                }
                FolderChain.Last().IsLastItem = false;
                FolderChainListView.ScrollToIndex(FolderChain.Count - 1);
            }
        }

        private void SetupGridView(FolderTree tree)
        {
            GridItems.Clear();
            foreach (var item in tree.Trees)
                GridItems.Add(new GridViewFolder(StorageService.FindFolder(item.Id)));
            foreach (var item in tree.Files)
                GridItems.Add(new GridViewMusic(item));
        }

        private void SetNavText(FolderTree tree)
        {
            FolderInfoTextBlock.Text = string.IsNullOrEmpty(tree?.Path) ? "" : tree.Info;
        }

        private void LocalGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple) return;
            if (e.ClickedItem is GridViewFolder folder)
            {
                SetPage(StorageService.FindFolder(folder.Id));
            }
            else if (e.ClickedItem is GridViewMusic music)
            {
                MusicPlayer.SetMusicAndPlay(CurrentSongs, music.Source);
            }
        }

        private GridViewStorageItem draggingItem;
        private void LocalListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            draggingItem = e.Items[0] as GridViewStorageItem;
        }

        private void LocalListView_DragEnter(object sender, DragEventArgs e)
        {
            if (draggingItem == null)
            {
                CancelDrop(e);
                return;
            }
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        private async void LocalListViewFolder_Drop(object sender, DragEventArgs e)
        {
            object dataContext = ((FrameworkElement)sender).DataContext;
            FolderTree newParent;
            if (dataContext is GridViewFolder gridViewFolder)
            {
                newParent = gridViewFolder.Source;
            }
            else if (dataContext is FolderChainItem folderChainItem)
            {
                newParent = folderChainItem.ToFolderTree();
            }
            else
            {
                return;
            }
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            if (newParent.IsParentOf(draggingItem.Path) || newParent.Path == draggingItem.Path)
            {
                // 相当于没移动
                CancelDrop(e);
            }
            else if (draggingItem is GridViewFolder folder)
            {
                // 文件夹没有移动成功的话，就把节点移动回去
                if (!await StorageService.MoveFolderAsync(folder.Source, newParent))
                {
                    CancelDrop(e);
                }
            }
            else if (draggingItem is GridViewMusic file)
            {
                // 文件没有移动成功的话，就把节点移动回去
                if (!await StorageService.MoveFileAsync(file.ToFolderFile(), newParent))
                {
                    CancelDrop(e);
                }
            }
            MainPage.Instance?.Loader.Hide();
        }

        private void CancelDrop(DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;
        }


        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, this, new MenuFlyoutOption
            {
                MultiSelectOption = MultiSelectOption,
                ShowMoveToFolder = true,
            });
        }

        private void OpenPlaylistFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            object dataContext = flyout.Target.DataContext;
            if (dataContext is GridViewFolder gridFolder)
            {
                FolderTree folder = gridFolder.Source;
                MenuFlyoutHelper.SetFolderMenu(sender, folder, this, this, new MenuFlyoutOption
                {
                    MultiSelectOption = MultiSelectOption,
                    ShowMoveToFolder = true,
                });
            }
            else if (dataContext is FolderChainItem folderChainItem)
            {
                FolderTree folder = StorageService.FindFolderInfo(folderChainItem.Id);
                MenuFlyoutHelper.SetSimpleFolderMenu(sender, folder, this, this, new MenuFlyoutOption
                {
                    ShowSelect = false,
                    MultiSelectOption = MultiSelectOption,
                });
            }
        }

        private void LocalTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (PleaseExitMultiSelectMode()) return;
            object dataContext = (e.OriginalSource as FrameworkElement).DataContext;
            if (dataContext is GridViewFolder tree)
            {
                SetPage(StorageService.FindFolder(tree.Id));
            }
            else if (dataContext is GridViewMusic file)
            {
                MusicPlayer.SetMusicAndPlay(CurrentSongs, file.Source);
            }
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await StorageHelper.AddFolder(CurrentFolderInfo);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHelper.RefreshFolder(CurrentFolderInfo);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            List<MusicView> songs = CurrentSongs;
            if (songs.IsEmpty())
            {
                Helper.ShowNotification("NoMusicUnderCurrentFolder");
            }
            else
            {
                MusicPlayer.ShuffleAndPlay(songs);
            }
        }

        private void ListViewFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (PleaseExitMultiSelectMode())
            {
                ListViewFlyoutItem.IsChecked = false;
                return;
            }
            SwitchViewMode(LocalPageViewMode.List);
        }

        private void GridViewFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (PleaseExitMultiSelectMode())
            {
                GridViewFlyoutItem.IsChecked = false;
                return;
            }
            SwitchViewMode(LocalPageViewMode.Grid);
        }

        private void SwitchViewMode(LocalPageViewMode mode)
        {
            switch (mode)
            {
                case LocalPageViewMode.Grid:
                    LocalTreeView.Visibility = Visibility.Collapsed;
                    LocalGridView.Visibility = Visibility.Visible;
                    ListViewFlyoutItem.IsChecked = false;
                    GridViewFlyoutItem.IsChecked = true;
                    break;
                case LocalPageViewMode.List:
                    LocalTreeView.Visibility = Visibility.Visible;
                    LocalGridView.Visibility = Visibility.Collapsed;
                    ListViewFlyoutItem.IsChecked = true;
                    GridViewFlyoutItem.IsChecked = false;
                    break;
            }
            Settings.settings.LocalViewMode = mode;
        }

        private void MultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentListView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(MultiSelectOption);
        }

        private void SortAppButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.GetFolderSortByMenu(CurrentFolderInfo, this).ToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        private void LocalListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = (sender as ListViewBase);
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
            }
        }

        private void FolderChainListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FolderChainItemClicked((FolderChainItem)e.ClickedItem);
        }

        private void PathItemButton_Click(object sender, RoutedEventArgs e)
        {
            FolderChainItem folder = (sender as FrameworkElement).DataContext as FolderChainItem;
            FolderChainItemClicked(folder);
        }

        private void FolderChainItemClicked(FolderChainItem folder)
        {
            if (folder.Id == CurrentFolderInfo.Id)
            {
                CurrentListView.ScrollToTop();
                LocalGridView.ScrollToTop();
            }
            else
            {
                SetPage(StorageService.FindFolder(folder.Id));
            }
        }

        private void PathItemDropDownButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPathItemArrowAnimation.Stop();
            Button fe = sender as Button;
            FolderChainItem folderChainItem = fe.DataContext as FolderChainItem;
            Storyboard.SetTarget(FolderPathItemArrowAnimation, fe.Content as FontIcon);
            FolderPathItemArrowAnimation.Begin();
            string path = CurrentFolderInfo.Path;
            foreach (var child in folderChainItem.Children)
            {
                child.IsHighlighted = path.StartsWith(child.Path);
            }
        }

        private void FolderChainItemFlyout_Closed(object sender, object e)
        {
            FolderPathItemArrowResumeAnimation.Stop();
            Flyout flyout = sender as Flyout;
            Storyboard.SetTarget(FolderPathItemArrowResumeAnimation, (flyout.Target as Button).Content as FontIcon);
            FolderPathItemArrowResumeAnimation.Begin();
        }


        private void GoToSettingsHyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            MainPage.Instance.NavigateToPage(typeof(SettingsPage));
        }

        private bool PleaseExitMultiSelectMode()
        {
            if (CurrentListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.ShowNotification("PleaseExitMultiSelectMode");
                return true;
            }
            return false;
        }

        private void FolderChainItemButton_Click(object sender, RoutedEventArgs e)
        {
            FolderChainItem item = (sender as FrameworkElement).DataContext as FolderChainItem;
            SetPage(StorageService.FindFolder(item.Id));
            folderChainItemFlyout?.Hide();
        }
        private Flyout folderChainItemFlyout = null;
        private void FolderChainItemFlyout_Opened(object sender, object e)
        {
            folderChainItemFlyout = sender as Flyout;
        }

        async void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            folderChainItemFlyout?.Hide();
            switch (args.Event)
            {
                case MenuFlyoutEvent.AddTo:
                    break;
                case MenuFlyoutEvent.Select:
                    CurrentListView.SelectionMode = ListViewSelectionMode.Multiple;
                    CurrentListView.SelectedValue = args.Data;
                    break;
                case MenuFlyoutEvent.Sort:
                    if (PleaseExitMultiSelectMode()) return;
                    LocalProgressRing.Visibility = Visibility.Visible;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        FolderTree currentFolder = CurrentFolder;
                        SortBy criterion = (SortBy)args.Data;
                        if (criterion == SortBy.Reverse)
                        {
                            currentFolder.Reverse();
                        }
                        else
                        {
                            currentFolder.SortFiles(criterion);
                            StorageService.UpdateFolder(currentFolder);
                        }
                        SetupGridView(currentFolder);
                    });
                    LocalProgressRing.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private bool ClearMultiSelectStatus()
        {
            if (CurrentListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                MainPage.Instance.CancelMultiSelectCommandBar();
                return true;
            }
            return false;
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    CurrentListView.SelectionMode = CurrentListView is ListView ? ListViewSelectionMode.Single :
                                                                                  ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedSongs;
                    args.FlyoutHelper.DefaultPlaylistName = CurrentFolderInfo.Name;
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(SelectedSongs);
                    break;
                case MultiSelectEvent.SelectAll:
                    CurrentListView.SelectAll();
                    break;
                case MultiSelectEvent.ReverseSelections:
                    CurrentListView.ReverseSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedSongs.Count);
                    break;
                case MultiSelectEvent.ClearSelections:
                    CurrentListView.ClearSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedSongs.Count);
                    break;
                case MultiSelectEvent.MoveToFolder:
                    args.FlyoutHelper.Data = CurrentListView.SelectedItems
                                                            .Select(i => (i as GridViewStorageItem).AsStorageItem())
                                                            .ToList();
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
            if (CurrentFolderInfo == null) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Move:
                    GridItems.RemoveAll(i => i.Path == file.Path);
                    if (GridItems.FirstOrDefault(i => i.Path.Equals(args.Folder.Path)) is GridViewFolder folder)
                    {
                        folder.AddFile(file);
                    }
                    break;
            }
            SetNavText(CurrentFolder);
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            FolderTree currentFolder = CurrentFolderInfo;
            if (currentFolder == null) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Add:
                    if (!args.Folder.Equals(currentFolder)) return;
                    GridViewFolder newFolder = new GridViewFolder(folder);
                    int firstMusic = GridItems.FindIndex(i => i is GridViewMusic);
                    if (firstMusic == -1)
                    {
                        GridItems.Add(newFolder);
                    }
                    else
                    {
                        int index = GridItems.Take(firstMusic).FindSortedListInsertIndex(newFolder, i => i.Name);
                        GridItems.Insert(index, newFolder);
                    }
                    break;
                case StorageItemEventType.Remove:
                    GridItems.RemoveAll(i => i.Path == folder.Path);
                    break;
                case StorageItemEventType.Move:
                    GridItems.RemoveAll(i => i.Path == folder.Path);
                    if (GridItems.FirstOrDefault(i => i.Path.Equals(args.Folder.Path)) is GridViewFolder targetFolder)
                    {
                        targetFolder.AddFolder(folder);
                    }
                    break;
                case StorageItemEventType.Reset:
                    FolderChain.Clear();
                    GridItems.Clear();
                    break;
                case StorageItemEventType.Rename:
                    if (GridItems.FirstOrDefault(i => i.Path == folder.Path) is GridViewFolder renameGridItem)
                    {
                        renameGridItem.Rename(args.Path);
                    }
                    break;
                case StorageItemEventType.Update:
                    folderUpdated = true;
                    if (MainPage.Instance?.CurrentPage != typeof(LocalPage))
                    {
                        return;
                    }
                    if (currentFolder.Equals(folder))
                    {
                        SetPage(folder);
                    }
                    if (GridItems.FirstOrDefault(i => i.Path == folder.Path) is GridViewFolder updateFolder)
                    {
                        updateFolder.Source = folder;
                    }
                    return;
            }
            SetNavText(CurrentFolder);
        }

        void IMenuFlyoutHelperBuildListener.OnBuild(MenuFlyoutHelper helper)
        {
            if (helper.SelectedItems is GridViewFolder gridViewItem)
            {
                helper.DefaultPlaylistName = gridViewItem.Name;
            }
            else
            {
                helper.DefaultPlaylistName = CurrentFolderInfo.Name;
            }
        }
        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var item in GridItems)
                        {
                            if (item is GridViewMusic music)
                            {
                                music.IsPlaying = music.Source.Equals(args.Music);
                            }
                        }
                    });
                    break;
            }
        }

        async void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            if (CurrentFolderInfo == null) return;
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    break;
                case MusicEventType.Remove:
                    GridItems.RemoveAll(i => i.Path == music.Path);
                    break;
                case MusicEventType.Modify:
                    if (GridItems.FirstOrDefault(i => i.Path == music.Path) is GridViewMusic gridViewMusic)
                        gridViewMusic.Source = music.ToVO();
                    break;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetNavText(CurrentFolder);
            });
        }
    }
}