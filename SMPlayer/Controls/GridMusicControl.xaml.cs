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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class GridMusicControl : UserControl, SwitchMusicListener
    {
        public ObservableCollection<GridMusicView> GridMusicCollection = new ObservableCollection<GridMusicView>();
        public List<Music> MusicCollection = new List<Music>();

        public GridMusicControl()
        {
            this.InitializeComponent();
        }

        private void MusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            MediaHelper.SetMusicAndPlay(MusicCollection, item.Source);
        }

        public async void Setup(ICollection<Music> collection)
        {
            GridMusicCollection.Clear();
            foreach (var file in collection)
            {
                var copy = file.Copy();
                copy.IsPlaying = copy.Equals(MediaHelper.CurrentMusic);
                GridMusicView gridItem = new GridMusicView();
                await gridItem.Init(copy);
                GridMusicCollection.Add(gridItem);
                MusicCollection.Add(copy);
            }
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
        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                bool findCurrent = current == null, findNext = next == null;
                foreach (var item in GridMusicCollection)
                {
                    var music = item.Source;
                    if (!findCurrent && (findCurrent = music.Equals(current)))
                        music.IsPlaying = false;
                    if (!findNext && (findNext = music.Equals(next)))
                        music.IsPlaying = true;
                    if (findCurrent && findNext) break;
                }
            });
        }
    }
}
