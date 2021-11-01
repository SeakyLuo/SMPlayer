﻿using SMPlayer.Controls;
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
    public sealed partial class LocalFoldersPage : Page, ILocalPageButtonListener, IMenuFlyoutItemClickListener, IMultiSelectListener
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
            MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
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
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, Windows.Storage.StorageItemTypes.Folder));
            flyout.Items.Add(MenuFlyoutHelper.GetRefreshDirectoryItem(tree, AfterTreeUpdated));
            flyout.Items.Add(MenuFlyoutHelper.GetDeleteFolderItem(tree, t =>
            {
                GridItems.RemoveAll(g => g.Tree == tree);
                FindNode(tree.ParentPath);
                setter.SetNavText(CurrentTree.Info);
            }));
            flyout.Items.Add(MenuFlyoutHelper.GetRenameFolderItem(tree,
                async (oldName, newName) =>
                {
                    try
                    {
                        if (await StorageFolder.GetFolderFromPathAsync(tree.ParentPath + "\\" + newName) != null)
                        {
                            return NamingError.Used;
                        }
                    }
                    catch (Exception ex)
                    {
                        Helper.Print(ex.ToString());
                    }
                    return NamingError.Good;
                },
                async (oldName, newName) =>
                {

                    StorageFolder folder = await tree.GetStorageFolderAsync();
                    await folder.RenameAsync(newName);
                    string newPath = tree.Path.Substring(0, tree.Path.LastIndexOf('\\') + 1) + newName;
                    Settings.settings.RenameFolder(tree, newPath);
                    RecentPage.RecentAdded.RenameFolder(tree.Path, newPath);

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
                if (tree.Equals(Settings.settings.Tree)) SetupTreeView(tree);
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
            foreach (var node in FillTreeNode(new TreeViewNode() { Content = tree, IsExpanded = true, HasUnrealizedChildren = true }).Children)
                LocalFoldersTreeView.RootNodes.Add(node);
        }
        private TreeViewNode FillTreeNode(TreeViewNode node)
        {
            node.HasUnrealizedChildren = false;
            node.Children.Clear();
            var tree = node.Content as FolderTree;
            foreach (var item in tree.Trees)
                node.Children.Add(TreeToNode(item));
            foreach (var item in tree.Files)
                node.Children.Add(new TreeViewNode() { Content = item });
            return node;
        }

        public void AddTree(string path, FolderTree tree)
        {
            CurrentTree.Trees.Add(tree);
            GridItems.Add(new GridFolderView(tree));
            TreeViewNode node = TreeToNode(tree);
            if (path == TreePath)
            {
                LocalFoldersTreeView.RootNodes.Add(node);
            }
        }

        private TreeViewNode TreeToNode(FolderTree tree)
        {
            return new TreeViewNode() { Content = tree, HasUnrealizedChildren = true };
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
            MediaHelper.SetMusicAndPlay(GetSelectedSongs());
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
                    else if (node.Content is Music music)
                    {
                        list.Add(music);
                    }
                }
            }
            return list;
        }

        private async void GridFolderControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext is GridFolderView data)
            {
                Helper.Print($"DataContextChanged, name {data.Name}, isLoaded {data.ThumbnailLoaded}, isPartiallyVisible {sender.IsPartiallyVisible(LocalFoldersGridView)}");
                if (!data.ThumbnailLoaded && sender.IsPartiallyVisible(LocalFoldersGridView))
                {
                    await data.SetThumbnailAsync();
                }
            }
        }
    }
}
