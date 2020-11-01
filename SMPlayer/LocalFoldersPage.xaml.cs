using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
    public sealed partial class LocalFoldersPage : Page, ILocalPageButtonListener
    {
        public static FolderTree CurrentTree;
        private ObservableCollection<GridFolderView> GridItems = new ObservableCollection<GridFolderView>();
        private string TreePath;
        public static ILocalSetter setter;

        public LocalFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            LocalPage.FolderListener = this;
            Controls.MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
            {
                if (CurrentTree.Contains(before))
                {
                    CurrentTree = Settings.settings.Tree.FindTree(CurrentTree);
                    foreach (var node in LocalFoldersTreeView.RootNodes)
                    {
                        if (node.Content is Music music)
                        {
                            if (music == before)
                            {
                                music.CopyFrom(after);
                                break;
                            }
                        }
                        else if (node.Content is FolderTree tree)
                        {
                            if (tree.FindMusic(before) is Music m)
                            {
                                m.CopyFrom(after);
                                break;
                            }
                        }
                    }
                    if (GridItems.FirstOrDefault(item => item.Tree.Contains(before)) is GridFolderView gridItem)
                        gridItem.Tree.FindMusic(before).CopyFrom(after);
                }
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ModeChanged(Settings.settings.LocalFolderGridView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Setup(CurrentTree = (FolderTree)e.Parameter);
        }

        private void LocalFoldersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var folderView = (GridFolderView)e.ClickedItem;
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
            MenuFlyoutHelper.SetPlaylistMenu(sender);
            FolderTree tree = null;
            if (flyout.Target.DataContext is GridFolderView gridFolderView) tree = gridFolderView.Tree;
            else if (flyout.Target.DataContext is TreeViewNode node) tree = node.Content as FolderTree;
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, Windows.Storage.StorageItemTypes.Folder));
            flyout.Items.Add(MenuFlyoutHelper.GetRefreshDirectoryItem(tree, AfterTreeUpdated));
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
                if (tree.Equals(Settings.settings.Tree)) SetupTreeView(tree);
                else if (FindNode(LocalFoldersTreeView.RootNodes.FirstOrDefault(n => tree.Path.StartsWith((n.Content as FolderTree).Path)), tree) is TreeViewNode node)
                {
                    node.Content = tree;
                    if (!node.HasUnrealizedChildren)
                        FillTreeNode(node);
                }
            }
        }
        private TreeViewNode FindNode(TreeViewNode node, FolderTree tree)
        {
            return node == null || tree.Equals(node.Content) ? node :
                FindNode(node.Children.FirstOrDefault(sub => tree.Path.StartsWith((sub.Content as FolderTree).Path)), tree);
        }
        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
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
            foreach (var node in FillTreeNode(new TreeViewNode() { Content = tree, IsExpanded = true, HasUnrealizedChildren = true }).Children)
                LocalFoldersTreeView.RootNodes.Add(node);
        }
        private TreeViewNode FillTreeNode(TreeViewNode node)
        {
            node.HasUnrealizedChildren = false;
            node.Children.Clear();
            var tree = node.Content as FolderTree;
            foreach (var item in tree.Trees)
                node.Children.Add(new TreeViewNode() { Content = item, HasUnrealizedChildren = true });
            foreach (var item in tree.Files)
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

        private void LocalFoldersTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as TreeViewNode;
            if (node.Content is FolderTree)
            {
                node.IsExpanded = !node.IsExpanded;
            }
            else if (node.Content is Music music)
            {
                MediaHelper.SetMusicAndPlay((node.Parent.Content as FolderTree).Files, music);
            }
        }

        private void FolderTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tree = ((sender as Grid).DataContext as TreeViewNode).Content as FolderTree;
            setter.SetPage(tree);
        }
    }
}
