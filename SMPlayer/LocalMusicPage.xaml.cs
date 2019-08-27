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
    public sealed partial class LocalMusicPage : Page
    {
        private ObservableCollection<GridMusicView> GridItems = new ObservableCollection<GridMusicView>();
        private FolderTree Tree;
        public LocalMusicPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup((FolderTree)e.Parameter);
            base.OnNavigatedTo(e);
        }

        private async void LocalMusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            if (!Tree.Equals(LocalPage.History.Peek()))
                await MediaHelper.SetPlayList(GridItems.Select((m) => m.Source).ToList());
            MainPage.Instance.SetMusicAndPlay(item.Source);
        }

        private async void Setup(FolderTree tree)
        {
            if (Tree == tree) return;
            Tree = tree;
            LocalLoadingControl.Visibility = Visibility.Visible;
            try
            {
                GridItems.Clear();
                foreach (var file in tree.Files)
                {
                    GridMusicView gridItem = new GridMusicView();
                    await gridItem.Init(file);
                    GridItems.Add(gridItem);
                }
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Local Music Page");
            }
            LocalLoadingControl.Visibility = Visibility.Collapsed;
        }
    }
}
