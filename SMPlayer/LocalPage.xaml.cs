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
    public sealed partial class LocalPage : Page
    {
        private ObservableCollection<GridFolderView> GridFolders = new ObservableCollection<GridFolderView>();
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void LocalFolderGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            GridFolderView folderView = (GridFolderView)e.ClickedItem;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GridFolders.Clear();
            LocalProgressRing.IsActive = true;
            LocalProgressRing.Visibility = Visibility.Visible;
            foreach (var tree in Settings.settings.Tree.Trees)
            {
                GridFolderView folder = new GridFolderView();
                await folder.Init(tree);
                GridFolders.Add(folder);
            }
            LocalProgressRing.Visibility = Visibility.Collapsed;
            LocalProgressRing.IsActive = false;
        }
    }


}
