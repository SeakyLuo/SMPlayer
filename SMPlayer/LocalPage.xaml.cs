using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.VO;
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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page, IWindowResizeListener, IStorageItemEventListener, IMenuFlyoutItemClickListener, IMenuFlyoutHelperBuildListener, IMultiSelectListener, ISwitchMusicListener, IMusicEventListener
    {
        private Stack<FolderTree> History = new Stack<FolderTree>();
        private readonly ObservableCollection<GridViewStorageItem> GridItems = new ObservableCollection<GridViewStorageItem>();
        private readonly ObservableCollection<TreeViewStorageItem> TreeItems = new ObservableCollection<TreeViewStorageItem>();
        private bool IsProcessing = false, folderUpdated = false;
        private FolderTree CurrentFolder { get => History.IsEmpty() ? null : History.Peek(); }
        public List<Music> SelectedSongs
        {
            get
            {
                List<Music> list = new List<Music>();
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        foreach (var node in LocalTreeView.SelectedNodes)
                        {
                            if (node.Content is TreeViewFolder tree)
                            {
                                list.AddRange(tree.Flatten());
                            }
                            else if (node.Content is TreeViewFile file)
                            {
                                list.Add(Settings.FindMusic(file.FileId));
                            }
                        }
                        break;
                    case LocalPageViewMode.Grid:
                        foreach (GridViewStorageItem item in LocalGridView.SelectedItems)
                        {
                            if (item is GridViewFolder folder)
                            {
                                list.AddRange(folder.Songs);
                            }
                            else if (item is GridViewMusic music)
                            {
                                list.Add(music.Source);
                            }
                        }
                        break;
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
            MainPage.WindowResizeListeners.Add(this);
            Settings.AddStorageItemEventListener(this);
            Settings.AddMusicEventListener(this);
            MusicPlayer.AddSwitchMusicListener(this);
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
                folder = History.IsEmpty() ? Settings.Root : History.Peek();
            }
            SetPage(folder);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
            if (History.IsNotEmpty())
            {
                SetHeader(CurrentFolder);
            }
        }

        public async void SetPage(FolderTree tree)
        {
            if (!folderUpdated)
            {
                if (IsProcessing) return;
                if (tree.Equals(CurrentFolder)) return;
            }
            IsProcessing = true;
            LocalProgressRing.Visibility = Visibility.Visible;
            ClearMultiSelectStatus();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tree.SortFiles();
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        SetupTreeView(tree);
                        SetupGridView(tree);
                        break;
                    case LocalPageViewMode.Grid:
                        SetupGridView(tree);
                        SetupTreeView(tree);
                        break;
                }
                //if (GridItems.IsNotEmpty())
                History.Push(tree);
            });
            SetHeader(tree);
            SetNavText(tree);
            SetLocalCommandBar(tree);
            SetBackButtonIsEnabled();
            LocalProgressRing.Visibility = Visibility.Collapsed;
            folderUpdated = false;
            IsProcessing = false;
        }

        private void SetupTreeView(FolderTree tree)
        {
            TreeItems.Clear();
            TreeItems.AddRange(tree.Trees.Select(i => new TreeViewFolder(i)));
            TreeItems.AddRange(tree.Files.Select(i => new TreeViewFile(i)));
        }

        private void SetupGridView(FolderTree tree)
        {
            GridItems.Clear();
            foreach (var item in tree.Trees)
                GridItems.Add(new GridViewFolder(Settings.FindFolder(item.Id)));
            foreach (var item in tree.Songs)
                GridItems.Add(new GridViewMusic(item));
        }

        private void SetHeader(FolderTree tree)
        {
            MainPage.Instance.SetHeaderText(tree == null || string.IsNullOrEmpty(tree.Path) ?
                                            Helper.LocalizeMessage("No Music") : tree.Name);
        }

        private void SetBackButtonIsEnabled()
        {
            BackButton.IsEnabled = History.Count > 1;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClearMultiSelectStatus())
            {
                return;
            }
            History.Pop();
            SetPage(History.Pop());
        }

        private void SetNavText(FolderTree tree)
        {
            if (tree == null) return;
            FolderInfoTextBlock.Text = tree.Info.ToString();
        }

        private void SetLocalCommandBar(FolderTree tree)
        {
            if (tree != null && !string.IsNullOrEmpty(tree.Path))
            {
                NewFolderButton.IsEnabled = true;
                RefreshButton.IsEnabled = true;
                ShuffleButton.IsEnabled = true;
            }
            else
            {
                NewFolderButton.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                ShuffleButton.IsEnabled = false;
            }
        }

        private void LocalGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple) return;
            if (e.ClickedItem is GridViewFolder folder)
            {
                SetPage(Settings.FindFolder(folder.Id));
            }
            else if (e.ClickedItem is GridViewMusic music)
            {
                MusicPlayer.SetMusicAndPlay(CurrentFolder.Songs, music.Source);
            }
        }

        private TreeViewNode originalParentNode;
        private int originalIndex;
        private void LocalTreeView_DragItemsStarting(TreeView sender, TreeViewDragItemsStartingEventArgs args)
        {
            TreeViewNode node = args.Items[0] as TreeViewNode;
            originalParentNode = node.Parent;
            originalIndex = LocalTreeView.RootNodes.IndexOf(node);
        }

        private async void LocalTreeView_DragItemsCompleted(TreeView sender, TreeViewDragItemsCompletedEventArgs args)
        {
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            TreeViewNode node = args.Items[0] as TreeViewNode;
            if (node.Parent == originalParentNode)
            {
                // 如果移动到同一个树下面，恢复拖动前的位置
                IList<TreeViewNode> siblings = LocalTreeView.RootNodes;
                int currentIndex = siblings.IndexOf(node);
                if (currentIndex != originalIndex)
                {
                    siblings.RemoveAt(currentIndex);
                    siblings.Insert(originalIndex, node);
                }
            }
            else if (!(node.Parent.Content is TreeViewFolder))
            {
                // 如果移动到非文件夹节点，恢复拖动前的位置
                MoveBackNode(node);
            }
            else
            {
                FolderTree currentFolder = CurrentFolder;
                object content = node.Content;
                FolderTree newParent = node.Parent.Content == null ? currentFolder : (node.Parent.Content as TreeViewFolder).Source;
                if (content is TreeViewFolder folder)
                {
                    // 文件夹没有移动成功的话，就把节点移动回去
                    if (!await Settings.settings.MoveFolderAsync(folder.Source, newParent))
                    {
                        MoveBackNode(node);
                    }
                }
                else if (content is TreeViewFile file)
                {
                    // 文件没有移动成功的话，就把节点移动回去
                    if (!await Settings.settings.MoveFileAsync(file.ToFolderFile(), newParent))
                    {
                        MoveBackNode(node);
                    }
                }
            }
            MainPage.Instance?.Loader.Hide();
        }

        private void MoveBackNode(TreeViewNode node)
        {
            node.Parent.Children.Remove(node);
            originalParentNode.Children.Insert(originalIndex, node);
        }

        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, this, new MenuFlyoutOption
            {
                MultiSelectOption = MultiSelectOption
            });
        }

        private void OpenPlaylistFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            FolderTree folder = null;
            if (flyout.Target.DataContext is GridViewFolder gridFolder) folder = gridFolder.Source;
            else if (flyout.Target.DataContext is TreeViewFolder treeFolder) folder = treeFolder.Source;
            MenuFlyoutHelper.SetFolderMenu(sender, folder, this, this, new MenuFlyoutOption
            {
                MultiSelectOption = MultiSelectOption
            });
        }

        private TreeViewNode FindNode(string path)
        {
            return FindNode(LocalTreeView.RootNodes, path);
        }

        private TreeViewNode FindNode(IList<TreeViewNode> nodes, string path)
        {
            if (nodes.IsEmpty()) return null;
            foreach (var node in nodes)
            {
                StorageItem item = node.Content as StorageItem;
                if (path == item.Path) return node;
                if (FindNode(node.Children, path) is TreeViewNode target) return target;
            }
            return null;
        }

        private void LocalTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (PleaseExitMultiSelectMode()) return;
            object dataContext = (e.OriginalSource as FrameworkElement).DataContext;
            if (dataContext is TreeViewFolder tree)
            {
                SetPage(Settings.FindFolder(tree.Id));
            }
            else if (dataContext is TreeViewFile file)
            {
                Music music = new Music() { Id = file.FileId };
                MusicPlayer.SetMusicAndPlay(CurrentFolder.Songs, music);
            }
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderTree currentTree = CurrentFolder;
            string path = currentTree.Path;

            string defaultName = Settings.settings.FindNextFolderName(currentTree, Helper.LocalizeText("NewFolderName"));
            RenameDialog renameDialog = new RenameDialog(RenameOption.Create, RenameTarget.Folder, defaultName)
            {
                ValidateAsync = async (newName) => await Settings.ValidateFolderName(path, newName),
                Confirmed = async (newName) =>
                {
                    FolderTree tree = new FolderTree() { Path = Path.Combine(path, newName) };
                    await Settings.settings.AddFolder(tree, currentTree);
                }
            };
            await renameDialog.ShowAsync();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHelper.RefreshFolder(CurrentFolder);
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            List<Music> songs = CurrentFolder.Songs;
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
            switch (Settings.settings.LocalViewMode)
            {
                case LocalPageViewMode.List:
                    LocalTreeView.SelectionMode = TreeViewSelectionMode.Multiple;
                    break;
                case LocalPageViewMode.Grid:
                    LocalGridView.SelectionMode = ListViewSelectionMode.Multiple;
                    break;
            }
            MainPage.Instance.ShowMultiSelectCommandBar(MultiSelectOption);
        }

        private void SortAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (PleaseExitMultiSelectMode()) return;
            SortBy[] criteria = new SortBy[] { SortBy.Reverse, SortBy.Title, SortBy.Artist, SortBy.Album };
            MenuFlyoutHelper.ShowSortByMenu(sender, CurrentFolder.Criterion, criteria,
                async criterion =>
                {
                    LocalProgressRing.Visibility = Visibility.Visible;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (criterion == SortBy.Reverse)
                        {
                            CurrentFolder.Reverse();
                        }
                        else
                        {
                            CurrentFolder.SortFiles(criterion);
                            Settings.settings.UpdateFolder(CurrentFolder);
                        }
                        SetupGridView(CurrentFolder);
                        SetupTreeView(CurrentFolder);
                    });
                    LocalProgressRing.Visibility = Visibility.Collapsed;
                });
        }

        private void CreatorHyperLinkButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewFile file = ((FrameworkElement)sender).DataContext as TreeViewFile;
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage), file.Creator);
        }

        private void CollectionHyperLinkButton_Click(object sender, RoutedEventArgs e)
        {
            TreeViewFile file = ((FrameworkElement)sender).DataContext as TreeViewFile;
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), Settings.FindAlbum(file.Collection, file.Creator));
        }

        private bool PleaseExitMultiSelectMode()
        {
            switch (Settings.settings.LocalViewMode)
            {
                case LocalPageViewMode.List:
                    if (LocalTreeView.SelectionMode == TreeViewSelectionMode.Multiple)
                    {
                        Helper.ShowNotification("PleaseExitMultiSelectMode");
                        return true;
                    }
                    return false;
                case LocalPageViewMode.Grid:
                    if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple)
                    {
                        Helper.ShowNotification("PleaseExitMultiSelectMode");
                        return true;
                    }
                    return false;
            }
            return false;
        }

        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.AddTo:
                    break;
                case MenuFlyoutEvent.Select:
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            LocalTreeView.SelectionMode = TreeViewSelectionMode.Multiple;
                            LocalTreeView.SelectedNodes.Add((TreeViewNode)args.Data);
                            break;
                        case LocalPageViewMode.Grid:
                            LocalGridView.SelectionMode = ListViewSelectionMode.Multiple;
                            LocalGridView.SelectedValue = args.Data;
                            break;
                    }
                    break;
            }
        }

        private bool ClearMultiSelectStatus()
        {
            switch (Settings.settings.LocalViewMode)
            {
                case LocalPageViewMode.List:
                    if (LocalTreeView.SelectionMode == TreeViewSelectionMode.Multiple)
                    {
                        MainPage.Instance.CancelMultiSelectCommandBar();
                        return true;
                    }
                    return false;
                case LocalPageViewMode.Grid:
                    if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple)
                    {
                        MainPage.Instance.CancelMultiSelectCommandBar();
                        return true;
                    }
                    return false;
            }
            return false;
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            LocalTreeView.SelectionMode = TreeViewSelectionMode.None;
                            break;
                        case LocalPageViewMode.Grid:
                            LocalGridView.SelectionMode = ListViewSelectionMode.None;
                            break;
                    }
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedSongs;
                    args.FlyoutHelper.DefaultPlaylistName = CurrentFolder.Name;
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(SelectedSongs);
                    break;
                case MultiSelectEvent.SelectAll:
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            LocalTreeView.SelectAll();
                            break;
                        case LocalPageViewMode.Grid:
                            LocalGridView.SelectAll();
                            break;
                    }
                    break;
                case MultiSelectEvent.ReverseSelections:
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            LocalTreeView.ReverseSelections();
                            break;
                        case LocalPageViewMode.Grid:
                            LocalGridView.ReverseSelections();
                            break;
                    }
                    break;
                case MultiSelectEvent.ClearSelections:
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            LocalTreeView.ClearSelections();
                            break;
                        case LocalPageViewMode.Grid:
                            LocalGridView.ClearSelections();
                            break;
                    }
                    break;
                case MultiSelectEvent.MoveToFolder:
                    List<StorageItem> items = new List<StorageItem>();
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            items.AddRange(LocalTreeView.SelectedNodes.Select(i =>
                            {
                                if (i.Content is TreeViewFile file) return file.ToFolderFile();
                                else return i.Content as StorageItem;
                            }).ToList());
                            break;
                        case LocalPageViewMode.Grid:
                            items.AddRange(LocalGridView.SelectedItems.Select(i => (i as GridViewStorageItem).AsStorageItem()).ToList());
                            break;
                    }
                    args.FlyoutHelper.Data = items;
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
            FolderTree currentFolder = CurrentFolder;
            if (currentFolder == null) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Move:
                    currentFolder.RemoveFile(file.Path);
                    GridItems.RemoveAll(i => i.Path == file.Path);
                    if (GridItems.FirstOrDefault(i => i.Path.Equals(args.Folder.Path)) is GridViewFolder folder)
                    {
                        folder.AddFile(file);
                    }
                    LocalTreeView.RootNodes.RemoveAll(n => (n.Content as StorageItem).Path == file.Path);
                    break;
            }
            SetNavText(currentFolder);
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            FolderTree currentFolder = CurrentFolder;
            if (currentFolder == null) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Add:
                    if (args.Folder.Equals(currentFolder))
                    {
                        currentFolder.AddBranch(folder);
                        GridViewFolder gridViewFolder = new GridViewFolder(folder);
                        int index = GridItems.FindSortedListInsertIndex(gridViewFolder, i => i.Name);
                        GridItems.Insert(index, gridViewFolder);
                        LocalTreeView.RootNodes.Insert(index, new TreeViewNode() { Content = folder });
                    }
                    break;
                case StorageItemEventType.Remove:
                    if (folder.ParentId == currentFolder.Id)
                    {
                        currentFolder.RemoveBranch(folder.Path);
                        GridItems.RemoveAll(i => i.Path == folder.Path);
                        LocalTreeView.RootNodes.RemoveAll(i => i.Content is TreeViewFolder tree && tree.Path == folder.Path);
                    }
                    break;
                case StorageItemEventType.Move:
                    if (folder.State.IsInactive())
                    {
                        currentFolder.MoveBranch(folder, args.Folder);
                        GridItems.RemoveAll(i => i.Path == folder.Path);
                        string newPath = Path.Combine(args.Folder.Path, folder.Name);
                        FindNode(folder.Path).Content = Settings.FindFolderInfo(newPath);
                    }
                    break;
                case StorageItemEventType.Reset:
                    History.Clear();
                    break;
                case StorageItemEventType.Rename:
                    if (GridItems.FirstOrDefault(i => i.Path == folder.Path) is GridViewFolder renameGridItem)
                    {
                        renameGridItem.Rename(args.Path);
                    }
                    if (TreeItems.FirstOrDefault(i => i.Path == folder.Path) is TreeViewFolder renameTreeItem)
                    {
                        renameTreeItem.Rename(args.Path);
                    }
                    break;
                case StorageItemEventType.Update:
                    if (MainPage.Instance?.CurrentPage != typeof(LocalPage))
                    {
                        folderUpdated = true;
                        return;
                    }
                    if (folder.Equals(currentFolder))
                    {
                        History.Pop();
                        SetPage(folder);
                    }
                    else if (currentFolder.FindFolder(folder.Path) != null)
                    {
                        GridViewFolder gridViewFolder = GridItems.FirstOrDefault(i => i.Path == folder.Path) as GridViewFolder;
                        gridViewFolder.Source = folder;
                    }
                    return;
            }
            SetNavText(currentFolder);
        }

        void IWindowResizeListener.Resized(WindowSizeChangedEventArgs e)
        {
            SetNavText(CurrentFolder);
        }

        void IMenuFlyoutHelperBuildListener.OnBuild(MenuFlyoutHelper helper)
        {
            if (helper.SelectedItems is TreeViewStorageItem treeViewItem)
            {
                helper.DefaultPlaylistName = treeViewItem.Name;
            }
            else if (helper.SelectedItems is GridViewFolder gridViewItem)
            {
                helper.DefaultPlaylistName = gridViewItem.Name;
            }
            else
            {
                helper.DefaultPlaylistName = CurrentFolder.Name;
            }
        }

        async void ISwitchMusicListener.MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                foreach (var item in GridItems)
                {
                    if (item is GridViewMusic music)
                    {
                        music.IsPlaying = music.Source == next;
                    }
                }
            });
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            FolderTree currentFolder = CurrentFolder;
            if (currentFolder == null) return;
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    break;
                case MusicEventType.Remove:
                    currentFolder.RemoveFile(music.Path);
                    GridItems.RemoveAll(i => i.Path == music.Path);
                    LocalTreeView.RootNodes.RemoveAll(i => i.Content is TreeViewFile file && file.Path == music.Path);
                    break;
                case MusicEventType.Like:
                    break;
                case MusicEventType.Modify:
                    break;
            }
        }
    }
}