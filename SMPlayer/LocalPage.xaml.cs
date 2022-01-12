using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
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
    public sealed partial class LocalPage : Page, IAfterPathSetListener, IWindowResizeListener, IStorageItemEventListener, IMenuFlyoutItemClickListener, IMenuFlyoutHelperBuildListener, IMultiSelectListener, ISwitchMusicListener
    {
        public static Stack<FolderTree> History = new Stack<FolderTree>();
        private readonly ObservableCollection<GridViewFolder> GridFolders = new ObservableCollection<GridViewFolder>();
        private readonly ObservableCollection<GridViewStorageItem> GridItems = new ObservableCollection<GridViewStorageItem>();
        private bool IsProcessing = false, PathSet = false;
        private FolderTree CurrentFolder { get => History.IsEmpty() ? null : History.Peek(); }
        public List<Music> SelectedItems
        {
            get
            {
                List<Music> list = new List<Music>();
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        foreach (var node in LocalTreeView.SelectedNodes)
                        {
                            if (node.Content is FolderTree tree)
                            {
                                list.AddRange(tree.Flatten());
                            }
                            else if (node.Content is TreeViewFolderFile file)
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

        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            SwitchViewMode(Settings.settings.LocalViewMode);
            UpdateHelper.AddAfterPathSetListener(this);
            MainPage.WindowResizeListeners.Add(this);
            Settings.AddFolderTreeEventListener(this);
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
            else if (e.Parameter is FolderTree)
            {
                folder = (FolderTree)e.Parameter;
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                folder = CurrentFolder;
            }
            else
            {
                folder = Settings.Root;
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
            if (!PathSet)
            {
                if (IsProcessing) return;
                if (CurrentFolder?.Equals(tree) == true) return;
            }
            IsProcessing = true;
            LocalProgressRing.Visibility = Visibility.Visible;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SortFolder(tree, tree.Criterion);
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        SetupTreeView(tree);
                        break;
                    case LocalPageViewMode.Grid:
                        SetupGridView(tree);
                        break;
                }
                History.Push(tree);
                SetHeader(tree);
                SetNavText(tree);
                SetLocalCommandBar(tree);
                SetBackButtonIsEnabled();
                // 不展示的放在后面加载
                switch (Settings.settings.LocalViewMode)
                {
                    case LocalPageViewMode.List:
                        SetupGridView(tree);
                        break;
                    case LocalPageViewMode.Grid:
                        SetupTreeView(tree);
                        break;
                }
            });
            PathSet = false;
            LocalProgressRing.Visibility = Visibility.Collapsed;
            IsProcessing = false;
        }

        private void SetupTreeView(FolderTree tree)
        {
            LocalTreeView.RootNodes.Clear();
            foreach (var item in tree.Trees)
                LocalTreeView.RootNodes.Add(new TreeViewNode() { Content = item });
            foreach (var item in tree.Files)
                LocalTreeView.RootNodes.Add(new TreeViewNode() { Content = new TreeViewFolderFile(item) });
        }

        private void SetupGridView(FolderTree tree)
        {
            GridItems.Clear();
            foreach (var item in tree.Trees)
                GridItems.Add(new GridViewFolder(Settings.FindFolder(item.Id)));
            foreach (var item in tree.Songs)
            {
                GridItems.Add(new GridViewMusic(item)
                {
                    IsPlaying = MusicPlayer.CurrentMusic == item
                });
            }
        }

        private void SetHeader(FolderTree tree)
        {
            MainPage.Instance.SetHeaderText(tree == null || string.IsNullOrEmpty(tree.Info.Directory) ?
                                            Helper.LocalizeMessage("No Music") : tree.Info.Directory);
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
            if (tree != null || !string.IsNullOrEmpty(tree.Path))
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
            if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }
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
                return;
            }
            if (!(node.Parent.Content is FolderTree))
            {
                // 如果移动到非文件夹节点，恢复拖动前的位置
                node.Parent.Children.Remove(node);
                originalParentNode.Children.Insert(originalIndex, node);
                return;
            }
            FolderTree currentFolder = CurrentFolder;
            object content = node.Content;
            string newParent = (node.Parent.Content == null ? currentFolder : node.Parent.Content as FolderTree).Path;
            if (content is FolderTree folder)
            {
                await Settings.settings.MoveFolderAsync(folder, newParent);
            }
            else if (content is TreeViewFolderFile file)
            {
                await Settings.settings.MoveFileAsync(file.ToFolderFile(), newParent);
            }
        }

        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, this, new MenuFlyoutOption
            {
                MultiSelectOption = new MultiSelectCommandBarOption
                {
                    ShowRemove = false,
                    ShowMoveToFolder = true,
                }
            });
        }

        private void OpenPlaylistFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            MenuFlyoutHelper.SetPlaylistMenu(sender, this, this, new MenuFlyoutOption
            {
                MultiSelectOption = new MultiSelectCommandBarOption
                {
                    ShowRemove = false,
                    ShowMoveToFolder = true,
                }
            });
            FolderTree folder = null;
            if (flyout.Target.DataContext is GridViewFolder gridFolderView) folder = gridFolderView.Source;
            else if (flyout.Target.DataContext is TreeViewNode node) folder = node.Content as FolderTree;
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(folder));
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(folder.Path, StorageItemTypes.Folder));
            flyout.Items.Add(MenuFlyoutHelper.GetRefreshDirectoryItem(folder, AfterFolderUpdated));
            flyout.Items.Add(MenuFlyoutHelper.GetDeleteFolderItem(folder, t =>
            {
                GridFolders.RemoveAll(g => g.Source == folder);
                FindNode(folder.ParentPath);
                SetNavText(CurrentFolder);
            }));
            flyout.Items.Add(MenuFlyoutHelper.GetRenameFolderItem(folder,
                async (newName) => await Settings.ValidateFolderName(folder.ParentPath, newName),
                async (newName) =>
                {
                    await Settings.settings.RenameFolder(folder, newName);
                    string newPath = folder.Path;
                    GridFolders.FirstOrDefault(i => i.Source.Equals(folder))?.Rename(newPath);
                    if (FindNode(folder) is TreeViewNode node)
                    {
                        (node.Content as FolderTree).Rename(newPath);
                    }
                }));
            flyout.Items.Add(MenuFlyoutHelper.GetSearchDirectoryItem(folder));
        }

        private TreeViewNode FindNode(FolderTree tree)
        {
            return FindNode(tree.Path);
        }

        private TreeViewNode FindNode(string path)
        {
            return LocalTreeView.RootNodes.FirstOrDefault(node => FindNode(node, path) is TreeViewNode);
        }

        private TreeViewNode FindNode(TreeViewNode node, string tree)
        {
            return node == null || tree == (node.Content as FolderTree).Path ? node :
                   FindNode(node.Children.FirstOrDefault(sub => tree.StartsWith((sub.Content as FolderTree).Path)), tree);
        }

        private void LocalTreeView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (LocalTreeView.SelectionMode == TreeViewSelectionMode.Multiple)
                return;
            object dataContext = (e.OriginalSource as FrameworkElement).DataContext;
            if (dataContext == null)
            {
                // why is null?
                return;
            }
            object content = (dataContext as TreeViewNode).Content;
            if (content is FolderTree tree)
            {
                SetPage(Settings.FindFolder(tree.Id));
            }
            else if (content is TreeViewFolderFile file)
            {
                Music music = new Music() { Id = file.Id };
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
            UpdateHelper.CheckNewMusic(CurrentFolder, AfterFolderUpdated);
        }

        private void AfterFolderUpdated(FolderTree folder)
        {
            if (!CurrentFolder.Equals(folder)) return;
            if (MainPage.Instance.CurrentPage != typeof(LocalPage)) return;
            History.Pop();
            SetPage(folder);
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
            SwitchViewMode(LocalPageViewMode.List);
        }

        private void GridViewFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
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

        private void SortFolder(FolderTree folder, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Title:
                    folder.SortByTitle();
                    break;
                case SortBy.Artist:
                    folder.SortByArtist();
                    break;
                case SortBy.Album:
                    folder.SortByAlbum();
                    break;
            }
        }

        private void SortAppButton_Click(object sender, RoutedEventArgs e)
        {
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
                            SortFolder(CurrentFolder, criterion);
                            Settings.settings.UpdateFolder(CurrentFolder);
                        }
                        SetupGridView(CurrentFolder);
                        SetupTreeView(CurrentFolder);
                    });
                    LocalProgressRing.Visibility = Visibility.Collapsed;
                });
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
                        LocalTreeView.ClearSelections();
                        LocalTreeView.SelectionMode = TreeViewSelectionMode.None;
                        MainPage.Instance.HideMultiSelectCommandBar();
                        return true;
                    }
                    return false;
                case LocalPageViewMode.Grid:
                    if (LocalGridView.SelectionMode == ListViewSelectionMode.Multiple)
                    {
                        LocalGridView.ClearSelections();
                        LocalGridView.SelectionMode = ListViewSelectionMode.None;
                        MainPage.Instance.HideMultiSelectCommandBar();
                        return true;
                    }
                    return false;
            }
            return false;
        }

        async void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    LocalGridView.ClearSelections();
                    LocalGridView.SelectionMode = ListViewSelectionMode.None;
                    LocalTreeView.ClearSelections();
                    LocalTreeView.SelectionMode = TreeViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.Data = SelectedItems;
                    args.FlyoutHelper.DefaultPlaylistName = CurrentFolder.Name;
                    break;
                case MultiSelectEvent.Play:
                    MusicPlayer.SetMusicAndPlay(SelectedItems);
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
                    switch (Settings.settings.LocalViewMode)
                    {
                        case LocalPageViewMode.List:
                            foreach (var node in LocalTreeView.SelectedNodes)
                            {
                                if (node.Content is FolderTree folder)
                                {
                                    await Settings.settings.MoveFolderAsync(folder, args.TargetPath);
                                }
                                else if (node.Content is TreeViewFolderFile file)
                                {
                                    await Settings.settings.MoveFileAsync(file.ToFolderFile(), args.TargetPath);
                                }
                            }
                            break;
                        case LocalPageViewMode.Grid:
                            foreach (var item in LocalGridView.SelectedItems)
                            {
                                if (item is GridViewFolder folder)
                                {
                                    await Settings.settings.MoveFolderAsync(folder.Source, args.TargetPath);
                                }
                                else if (item is GridViewMusic file)
                                {
                                    await Settings.settings.MoveFileAsync(file.Source.ToFolderFile(), args.TargetPath);
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        void IAfterPathSetListener.PathSet(string path)
        {
            if (MainPage.Instance?.CurrentPage != typeof(LocalPage))
            {
                PathSet = true;
                return;
            }
            History.Clear();
            SetPage(Settings.Root);
        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
            FolderTree currentFolder = CurrentFolder;
            if (currentFolder == null) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Move:
                    if (args.Path == currentFolder.Path || file.ParentId == currentFolder.Id)
                        SetNavText(currentFolder);
                    currentFolder.MoveFile(file, args.Path);
                    break;
            }
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            FolderTree currentFolder = CurrentFolder;
            if (currentFolder == null) return;
            if (!folder.Equals(CurrentFolder)) return;
            switch (args.EventType)
            {
                case StorageItemEventType.Add:
                    if (args.Folder.Equals(currentFolder))
                        SetNavText(currentFolder);
                    break;
                case StorageItemEventType.Remove:
                    if (folder.ParentId == currentFolder.Id)
                        SetNavText(currentFolder);
                    break;
                case StorageItemEventType.Move:
                    SetNavText(currentFolder);
                    currentFolder.MoveBranch(folder, args.Path);
                    break;
            }
        }


        void IWindowResizeListener.Resized(WindowSizeChangedEventArgs e)
        {
            SetNavText(CurrentFolder);
        }

        void IMenuFlyoutHelperBuildListener.OnBuild(MenuFlyoutHelper helper)
        {
            helper.DefaultPlaylistName = CurrentFolder.Name;
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
    }
}