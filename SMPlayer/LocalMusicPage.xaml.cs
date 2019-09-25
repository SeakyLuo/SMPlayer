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
    public sealed partial class LocalMusicPage : Page, SwitchMusicListener, ViewModeChangedListener
    {
        private ObservableCollection<GridMusicView> GridItems = new ObservableCollection<GridMusicView>();
        private ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        private FolderTree Tree;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.SwitchMusicListeners.Add(this as SwitchMusicListener);
            LocalPage.MusicViewModeChangedListener = this as ViewModeChangedListener;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup((FolderTree)e.Parameter);
            base.OnNavigatedTo(e);
            ModeChanged(Settings.settings.LocalMusicGridView);
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
                Songs.Clear();
                foreach (var file in tree.Files)
                {
                    var copy = file.Copy();
                    copy.IsPlaying = copy.Equals(MediaHelper.CurrentMusic);
                    GridMusicView gridItem = new GridMusicView();
                    await gridItem.Init(copy);
                    GridItems.Add(gridItem);
                    Songs.Add(copy);
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
                    if (findCurrent && findNext) break;
                }
                findCurrent = current == null; findNext = next == null;
                foreach (var music in Songs)
                {
                    if (!findCurrent && (findCurrent = music.Equals(current)))
                        music.IsPlaying = false;
                    if (!findNext && (findNext = music.Equals(next)))
                        music.IsPlaying = true;
                    if (findCurrent && findNext) break;
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

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as GridMusicView;
            new MenuFlyoutHelper().GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        public void ModeChanged(bool isGridView)
        {
            if (isGridView)
            {
                LocalMusicGridView.Visibility = Visibility.Visible;
                LocalPlaylist.Visibility = Visibility.Collapsed;
            }
            else
            {
                LocalMusicGridView.Visibility = Visibility.Collapsed;
                LocalPlaylist.Visibility = Visibility.Visible;
            }
        }
    }
}
