using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
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
    public sealed partial class LocalMusicPage : Page, MusicSwitchingListener
    {
        private ObservableCollection<GridMusicView> GridItems = new ObservableCollection<GridMusicView>();
        private FolderTree Tree;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            MediaHelper.MusicSwitchingListeners.Add(this as MusicSwitchingListener);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup((FolderTree)e.Parameter);
            base.OnNavigatedTo(e);
        }

        private void LocalMusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            MediaHelper.SetMusicAndPlay(Tree.Files, item.Source);
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
                    gridItem.Source.IsPlaying = gridItem.Source.Equals(MediaHelper.CurrentMusic);
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

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                bool findCurrent = current == null, findNext = next == null;
                foreach (var item in GridItems)
                {
                    var music = item.Source;
                    if (!findCurrent && (findCurrent = music.Equals(current)))
                        music.IsPlaying = false;
                    if (!findNext && (findNext = music.Equals(next)))
                        music.IsPlaying = true;
                    if (findCurrent && findNext) return;
                }
            });
        }
        private void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }
    }
}
