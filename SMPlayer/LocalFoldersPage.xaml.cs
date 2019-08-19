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
    public sealed partial class LocalFoldersPage : Page
    {
        private ObservableCollection<GridFolderView> GridItems = new ObservableCollection<GridFolderView>();
        private FolderTree Tree;
        public static LocalSetter setter;
        
        public LocalFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup((FolderTree)e.Parameter);
            base.OnNavigatedTo(e);
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
    }
}
