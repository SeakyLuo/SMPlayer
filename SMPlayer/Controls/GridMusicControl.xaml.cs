using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class GridMusicControl : UserControl, SwitchMusicListener
    {
        public ObservableCollection<GridMusicView> GridMusicCollection = new ObservableCollection<GridMusicView>();
        public List<Music> MusicCollection = new List<Music>();
        private bool SetupInProgress = false;

        public GridMusicControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
        }

        private void MusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            MediaHelper.SetMusicAndPlay(MusicCollection, item.Source);
        }

        public void Setup(ICollection<Music> collection)
        {
            SetupInProgress = true;
            MusicCollection = collection.ToList();
            GridMusicCollection.Clear();
            foreach (var music in collection)
            {
                var copy = music.Copy();
                copy.IsPlaying = copy.Equals(MediaHelper.CurrentMusic);
                GridMusicView gridItem = new GridMusicView(copy);
                GridMusicCollection.Add(gridItem);
            }
            SetupInProgress = false;
        }
        public void Reverse()
        {
            if (SetupInProgress) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection.Reverse();
                GridMusicCollection.SetTo(GridMusicCollection.Reverse());
            }
        }
        public void SortByTitle()
        {
            if (SetupInProgress) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy((m) => m.Name).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy((m) => m.Name));
            }
        }
        public void SortByArtist()
        {
            if (SetupInProgress) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy((m) => m.Artist).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy((m) => m.Artist));
            }
        }
        public void SortByAlbum()
        {
            if (SetupInProgress) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy((m) => m.Album).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy((m) => m.Source.Album));
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
            var fe = (FrameworkElement)sender;
            new MenuFlyoutHelper() { Data = fe.DataContext }.GetAddToMenuFlyout().ShowAt(fe);
        }
        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.FindMusicAndSetPlaying(MusicCollection, current, next);
                foreach (var item in GridMusicCollection)
                    item.Source.IsPlaying = item.Source.Equals(next);
            });
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (sender.DataContext as GridMusicView)?.SetThumbnail();
        }
    }
}
