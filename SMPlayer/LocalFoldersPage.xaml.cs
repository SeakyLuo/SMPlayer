using SMPlayer.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public sealed partial class LocalFoldersPage : Page, IMusicEventListener, ILocalPageButtonListener, IMenuFlyoutItemClickListener, IMultiSelectListener, IFolderTreeEventListener
    {
        public static FolderTree CurrentTree;
        private ObservableCollection<GridFolderView> GridItems = new ObservableCollection<GridFolderView>();
        private string TreePath;
        public static ILocalSetter setter;

        public LocalFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.AddFolderTreeEventListener(this);
            LocalPage.FolderListener = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetMultiSelectListener(this);
            ModeChanged(Settings.settings.LocalFolderGridView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Setup(CurrentTree = (FolderTree)e.Parameter);
        }

        private void LocalFoldersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (LocalFoldersGridView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }
            var folderView = (GridFolderView)e.ClickedItem;
            if (folderView.Tree.IsEmpty) return;
            setter.SetPage(CurrentTree.Trees[GridItems.IndexOf(folderView)]);
        }

        private void Setup(FolderTree tree)
        {
            if (TreePath == tree.Path) return;
            LocalLoadingControl.Visibility = Visibility.Visible;
            UpdateTree(tree);
            LocalLoadingControl.Visibility = Visibility.Collapsed;
        }

        private void UpdateTree(FolderTree tree)
        {
            SetupTreeView(tree);
            GridItems.Clear();
            setter.SetPage(tree);
            foreach (var branch in tree.Trees)
                GridItems.Add(new GridFolderView(branch));
            TreePath = tree.Path;
            CurrentTree = tree;
        }

        private void OpenPlaylistFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            MenuFlyoutHelper.SetPlaylistMenu(sender, this, null, new MenuFlyoutOption
            { 
                MultiSelectOption = new MultiSelectCommandBarOption
                {
                    ShowRemove = false,
                    ShowReverseSelection = false
                } 
            });
            FolderTree tree = null;
            if (flyout.Target.DataContext is GridFolderView gridFolderView) tree = gridFolderView.Tree;
            else if (flyout.Target.DataContext is TreeViewNode node) tree = node.Content as FolderTree;
            flyout.Items.Add(MenuFlyoutHelper.GetPreferItem(tree));
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, StorageItemTypes.Folder));
            flyout.Items.Add(MenuFlyoutHelper.GetRefreshDirectoryItem(tree, AfterTreeUpdated));
            flyout.Items.Add(MenuFlyoutHelper.GetDeleteFolderItem(tree, t =>
            {
                GridItems.RemoveAll(g => g.Tree == tree);
                FindNode(tree.ParentPath);
                setter.SetNavText(CurrentTree.Info);
            }));
            flyout.Items.Add(MenuFlyoutHelper.GetRenameFolderItem(tree,
                async (newName) => await Settings.ValidateFolderName(tree.ParentPath, newName),
                async (newName) =>
                {
                    await Settings.settings.RenameFolder(tree, newName);
                    string newPath = tree.Path;
                    GridItems.FirstOrDefault(i => i.Tree.Equals(tree))?.Rename(newPath);
                    if (FindNode(tree) is TreeViewNode node)
                    {
                        (node.Content as FolderTree).Rename(newPath);
                    }
                    App.Save();
                }));
            flyout.Items.Add(MenuFlyoutHelper.GetSearchDirectoryItem(tree));
        }
        private void AfterTreeUpdated(FolderTree tree)
        {
            if (CurrentTree.Equals(tree))
            {
                UpdateTree(tree);
            }
            else
            {
                CurrentTree.FindTree(tree).CopyFrom(tree);
                int index = GridItems.FindIndex(i => i.Tree.Equals(tree));
                if (index > -1)
                {
                    var item = GridItems[index];
                    item.Tree = tree;
                    GridItems[index] = item;
                }
                if (tree.Path == Settings.settings.RootPath) SetupTreeView(tree);
                else if (FindNode(tree) is TreeViewNode node)
                {
                    node.Content = tree;
                    if (!node.HasUnrealizedChildren)
                        FillTreeNode(node);
                }
            }
        }

        private TreeViewNode FindNode(FolderTree tree)
        {
            return FindNode(tree.Path);
        }

        private TreeViewNode FindNode(string path)
        {
            return LocalFoldersTreeView.RootNodes.FirstOrDefault(node => FindNode(node, path) is TreeViewNode);
        }

        private TreeViewNode FindNode(TreeViewNode node, string tree)
        {
            return node == null || tree == (node.Content as FolderTree).Path ? node :
                   FindNode(node.Children.FirstOrDefault(sub => tree.StartsWith((sub.Content as FolderTree).Path)), tree);
        }

        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption
            {
                MultiSelectOption = new MultiSelectCommandBarOption
                {
                    ShowRemove = false,
                    ShowReverseSelection = false
                }
            });
        }
        public void UpdatePage(FolderTree tree)
        {
            AfterTreeUpdated(tree);
        }
        public void ModeChanged(bool isGridView)
        {
            if (isGridView)
            {
                LocalFoldersGridView.Visibility = Visibility.Visible;
                LocalFoldersTreeView.Visibility = Visibility.Collapsed;
            }
            else
            {
                LocalFoldersGridView.Visibility = Visibility.Collapsed;
                LocalFoldersTreeView.Visibility = Visibility.Visible;
            }
        }
        private void SetupTreeView(FolderTree tree)
        {
            LocalFoldersTreeView.RootNodes.Clear();
            foreach (var node in FillTreeNode(new TreeViewNode() { Content = tree, IsExpanded = true, HasUnrealizedChildren = !tree.IsEmpty }).Children)
                LocalFoldersTreeView.RootNodes.Add(node);
        }
        private TreeViewNode FillTreeNode(TreeViewNode node)
        {
            node.HasUnrealizedChildren = false;
            node.Children.Clear();
            var tree = node.Content as FolderTree;
            foreach (var item in tree.Trees)
                node.Children.Add(TreeToNode(item));
            foreach (var item in tree.Songs)
                node.Children.Add(new TreeViewNode() { Content = item });
            return node;
        }

        private void LocalFoldersTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.HasUnrealizedChildren)
            {
                FillTreeNode(args.Node);
            }
        }

        private TreeViewNode lastSelected = null;

        private void LocalFoldersTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as TreeViewNode;
            if (node.Content is FolderTree)
            {
                node.IsExpanded = !node.IsExpanded;
            }
            if (sender.SelectionMode == TreeViewSelectionMode.Multiple)
            {
            }
            else
            {
                if (node == lastSelected)
                {
                    sender.SelectionMode = TreeViewSelectionMode.None;
                }
                else
                {
                    sender.SelectionMode = TreeViewSelectionMode.Single;
                    sender.SelectedNodes.Add(node);
                }
            }
            lastSelected = node;
        }

        private void FolderTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tree = ((sender as Grid).DataContext as TreeViewNode).Content as FolderTree;
            if (tree.IsEmpty) return;
            setter.SetPage(tree);
        }

        void IMenuFlyoutItemClickListener.AddTo(object data, object collection, int index, AddToCollectionType type) { }
        void IMenuFlyoutItemClickListener.Favorite(object data) { }
        void IMenuFlyoutItemClickListener.Delete(Music music) { }
        void IMenuFlyoutItemClickListener.UndoDelete(Music music) { }
        void IMenuFlyoutItemClickListener.Remove(Music music) { }

        void IMenuFlyoutItemClickListener.Select(object data)
        {
            if (Settings.settings.LocalFolderGridView)
            {
                LocalFoldersGridView.SelectionMode = ListViewSelectionMode.Multiple;
                LocalFoldersGridView.SelectedValue = data;
            }
            else
            {
                LocalFoldersTreeView.SelectionMode = TreeViewSelectionMode.Multiple;
                LocalFoldersTreeView.SelectedNodes.Add((TreeViewNode)data);
            }
            MainPage.Instance.ShowMultiSelectCommandBar();
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            LocalFoldersGridView.SelectionMode = ListViewSelectionMode.None;
            LocalFoldersTreeView.SelectionMode = TreeViewSelectionMode.None;
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            helper.Data = GetSelectedSongs();
        }

        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar)
        {
            MusicPlayer.SetMusicAndPlay(GetSelectedSongs());
        }

        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar) { }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            if (Settings.settings.LocalFolderGridView)
            {
                LocalFoldersGridView.SelectAll();
            } 
            else
            {
                LocalFoldersTreeView.SelectAll();
            }
        }

        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar) { }

        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar)
        {
            if (Settings.settings.LocalFolderGridView)
            {
                LocalFoldersGridView.SelectedItems.Clear();
            }
            else
            {
                LocalFoldersTreeView.SelectedNodes.Clear();
            }
        }

        public List<Music> GetSelectedSongs()
        {
            List<Music> list = new List<Music>();
            if (Settings.settings.LocalFolderGridView)
            {
                foreach (GridFolderView item in LocalFoldersGridView.SelectedItems)
                {
                    list.AddRange(item.Songs);
                }
            }
            else
            {
                foreach (var node in LocalFoldersTreeView.SelectedNodes)
                {
                    if (node.Content is FolderTree tree)
                    {
                        list.AddRange(tree.Flatten());
                    }
                    else if (node.Content is FolderFile file)
                    {
                        list.Add(file.ToMusic());
                    }
                }
            }
            return list;
        }

        private async void GridFolderControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext is GridFolderView data)
            {
                Log.Info($"DataContextChanged, name {data.Name}, isLoaded {data.ThumbnailLoaded}, isPartiallyVisible {sender.IsPartiallyVisible(LocalFoldersGridView)}");
                if (!data.ThumbnailLoaded && sender.IsPartiallyVisible(LocalFoldersGridView))
                {
                    await data.LoadThumbnailAsync();
                }
            }
        }

        private TreeViewNode originalParentNode;
        private int originalIndex;

        private void LocalFoldersTreeView_DragItemsStarting(TreeView sender, TreeViewDragItemsStartingEventArgs args)
        {
            TreeViewNode node = args.Items[0] as TreeViewNode;
            originalParentNode = node.Parent;
            originalIndex = GetSiblings(node).IndexOf(node);
        }

        private void LocalFoldersTreeView_DragItemsCompleted(TreeView sender, TreeViewDragItemsCompletedEventArgs args)
        {
            TreeViewNode node = args.Items[0] as TreeViewNode;
            if (node.Parent == originalParentNode)
            {
                // 如果移动到同一个树下面，恢复拖动前的位置
                IList<TreeViewNode> siblings = GetSiblings(node);
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
            object content = node.Content;
            string path = (node.Parent.Content == null ? CurrentTree : node.Parent.Content as FolderTree).Path;
            if (content is FolderTree tree)
            {
                Settings.settings.MoveFolder(tree, path);
                App.Save();
            }
            else if (content is FolderFile file)
            {
                Settings.settings.MoveFile(file, path);
                App.Save();
            }
        }

        private IList<TreeViewNode> GetSiblings(TreeViewNode node)
        {
            return node == null ? LocalFoldersTreeView.RootNodes : node.Parent.Children;
        }

        private void FileTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            TreeViewNode node = (sender as Grid).DataContext as TreeViewNode;
            Music music = Settings.FindMusic(node.Content as FolderFile);
            MusicPlayer.SetMusicAndPlay(GetSiblings(node).Select(n => n.Content)
                                                         .Where(n => n is FolderFile)
                                                         .Select(n => Settings.FindMusic(n as FolderFile))
                                                         .ToList(),
                                        music);
        }

        void IMusicEventListener.Liked(Music music, bool isFavorite) { }

        void IMusicEventListener.Added(Music music) { }

        void IMusicEventListener.Removed(Music music) { }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            if (!CurrentTree.Contains(before.Path))
            {
                return;
            }
            CurrentTree = Settings.FindFolderInfo(CurrentTree.Id);
            foreach (var node in LocalFoldersTreeView.RootNodes)
            {
                if (node.Content is FolderTree tree)
                {
                    if (tree.FindMusic(before) is Music m)
                    {
                        m.CopyFrom(after);
                        break;
                    }
                }
            }
            if (GridItems.FirstOrDefault(item => item.Tree.Contains(before.Path)) is GridFolderView gridItem)
                gridItem.Tree.FindMusic(before).CopyFrom(after);
        }

        void IFolderTreeEventListener.Added(FolderTree folder, FolderTree root)
        {
            FolderTree branch = CurrentTree.FindTree(root);
            if (branch == null)
            {
                return;
            }
            branch.AddTree(folder);
            GridItems.Add(new GridFolderView(folder));
            if (root.Path == TreePath)
            {
                LocalFoldersTreeView.RootNodes.Add(TreeToNode(folder));
            }
        }

        void IFolderTreeEventListener.Renamed(FolderTree folder, string newPath)
        {
            FolderTree branch = CurrentTree.FindTree(folder);
            if (branch == null)
            {
                return;
            }
            branch.Rename(newPath);
            GridItems.FirstOrDefault(i => i.Tree.Equals(folder))?.Rename(newPath);
        }

        void IFolderTreeEventListener.Removed(FolderTree folder)
        {
            CurrentTree.RemoveBranch(folder.Path);
            GridItems.RemoveAll(i => i.Tree.Equals(folder));
        }

        private static TreeViewNode TreeToNode(FolderTree tree)
        {
            return new TreeViewNode() { Content = tree, HasUnrealizedChildren = !tree.IsEmpty };
        }
    }
}
