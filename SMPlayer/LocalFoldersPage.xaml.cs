using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalFoldersPage : Page, ViewModeChangedListener
    {
        private ObservableCollection<GridFolderView> GridItems = new ObservableCollection<GridFolderView>();
        private FolderTree Tree;
        public static LocalSetter setter;
        
        public LocalFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            LocalPage.FolderViewModeChangedListener = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup(Tree);
            //ModeChanged(Settings.settings.LocalFolderGridView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //base.OnNavigatedTo(e);
            //Tree = (FolderTree)e.Parameter;

            base.OnNavigatedTo(e);
            Setup((FolderTree)e.Parameter);
            ModeChanged(Settings.settings.LocalFolderGridView);
        }

        private void LocalFoldersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var folderView = (GridFolderView)e.ClickedItem;
            setter.SetPage(Tree.Trees[GridItems.IndexOf(folderView)]);
        }

        private async void Setup(FolderTree tree)
        {
            if (Tree == tree) return;
            Tree = tree;
            LocalFolderTreeView.RootNodes.Clear();
            LocalFolderTreeView.RootNodes.Add(FillTreeNode(new TreeViewNode()
            {
                Content = tree,  IsExpanded = true,  HasUnrealizedChildren = true
            }));
            LocalLoadingControl.Visibility = Visibility.Visible;
            GridItems.Clear();
            setter.SetPage(tree);
            foreach (var branch in tree.Trees)
            {
                GridFolderView gridItem = new GridFolderView();
                await gridItem.Init(branch);
                GridItems.Add(gridItem);
            }
            LocalLoadingControl.Visibility = Visibility.Collapsed;
        }

        private void OpenPlaylistFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            MenuFlyoutHelper.SetPlaylistMenu(sender);
            FolderTree tree = null;
            if (flyout.Target.DataContext is GridFolderView gridFolderView) tree = gridFolderView.Tree;
            else if (flyout.Target.DataContext is TreeViewNode node) tree = node.Content as FolderTree;
            flyout.Items.Add(MenuFlyoutHelper.GetShowInExplorerItem(tree.Path, Windows.Storage.StorageItemTypes.Folder));
        }
        private void OpenMusicFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
        }

        private void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as GridFolderView;
            MediaHelper.ShuffleAndPlay(data.Songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as GridFolderView;
            var helper = new MenuFlyoutHelper() { Data = data.Songs, DefaultPlaylistName = data.Name };
            helper.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        public void ModeChanged(bool isGridView)
        {
            if (isGridView)
            {
                LocalFoldersGridView.Visibility = Visibility.Visible;
                LocalFolderTreeView.Visibility = Visibility.Collapsed;
            }
            else
            {
                LocalFoldersGridView.Visibility = Visibility.Collapsed;
                LocalFolderTreeView.Visibility = Visibility.Visible;
            }
        }

        private TreeViewNode FillTreeNode(TreeViewNode node)
        {
            var tree = node.Content as FolderTree;
            foreach (var item in tree.Trees)
                node.Children.Add(FillTreeNode(new TreeViewNode() { Content = item, HasUnrealizedChildren = true }));
            foreach(var item in tree.Files)
                node.Children.Add(new TreeViewNode() { Content = item });
            return node;
        }

        private void LocalFolderTreeView_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        {
            if (args.Node.HasUnrealizedChildren)
            {
                FillTreeNode(args.Node);
            }
        }

        private void LocalFolderTreeView_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
        {
            args.Node.Children.Clear();
            args.Node.HasUnrealizedChildren = true;
        }

        private void LocalFolderTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var node = args.InvokedItem as TreeViewNode;
            if (node.Content is FolderTree)
            {
                node.IsExpanded = !node.IsExpanded;
            }
            else if (node.Content is Music)
            {
                MediaHelper.SetMusicAndPlay((node.Parent.Content as FolderTree).Files, node.Content as Music);
            }
        }

        private void FolderTemplate_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var tree = ((sender as StackPanel).DataContext as TreeViewNode).Content as FolderTree;
            setter.SetPage(tree);
        }
    }
}
