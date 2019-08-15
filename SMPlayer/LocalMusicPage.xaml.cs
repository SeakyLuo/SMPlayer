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
        public static InfoSetter infoSetter;
        public LocalMusicPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is InfoSetter)
            {
                infoSetter = (InfoSetter)e.Parameter;
                Setup(Settings.settings.Tree, false);
            }
            else if (e.Parameter is FolderTree)
            {
                Setup(e.Parameter as FolderTree, true);
            }
            base.OnNavigatedTo(e);
        }

        private void LocalMusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            MainPage.Instance.SetMusic(item.Source);
            MainPage.Instance.PlayMusic();
        }

        private async void Setup(FolderTree tree, bool redirect)
        {
            LocalProgressRing.IsActive = true;
            LocalProgressRing.Visibility = Visibility.Visible;
            GridItems.Clear();
            infoSetter.SetInfo(tree, redirect);
            foreach (var file in tree.Files)
            {
                GridMusicView gridItem = new GridMusicView();
                await gridItem.Init(file);
                GridItems.Add(gridItem);
            }
            LocalProgressRing.Visibility = Visibility.Collapsed;
            LocalProgressRing.IsActive = false;
        }
    }
}
