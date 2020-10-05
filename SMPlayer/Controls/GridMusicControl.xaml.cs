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
    public sealed partial class GridMusicControl : UserControl, ISwitchMusicListener, IMenuFlyoutItemClickListener
    {
        public ObservableCollection<GridMusicView> GridMusicCollection = new ObservableCollection<GridMusicView>();
        public List<Music> MusicCollection = new List<Music>();
        private volatile bool IsProcessing = false;
        public List<RemoveMusicListener> RemoveListeners = new List<RemoveMusicListener>();
        private int removedItemIndex = -1;

        public GridMusicControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
            Controls.MusicInfoControl.MusicModifiedListeners.Add((before, after) =>
            {
                int index = MusicCollection.IndexOf(before);
                if (index > -1)
                {
                    GridMusicCollection[index].Source = MusicCollection[index] = after;
                }
            });
        }

        private void MusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (GridMusicView)e.ClickedItem;
            MediaHelper.SetMusicAndPlay(MusicCollection, item.Source);
        }

        public void Setup(ICollection<Music> collection)
        {
            IsProcessing = true;
            MusicCollection = collection.ToList();
            GridMusicCollection.Clear();
            foreach (var music in collection)
            {
                var copy = music.Copy();
                copy.IsPlaying = copy.Equals(MediaHelper.CurrentMusic);
                GridMusicView gridItem = new GridMusicView(copy);
                GridMusicCollection.Add(gridItem);
            }
            IsProcessing = false;
        }
        public void Clear()
        {
            MusicCollection.Clear();
            GridMusicCollection.Clear();
        }
        public void Reverse()
        {
            if (IsProcessing) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection.Reverse();
                GridMusicCollection.SetTo(GridMusicCollection.Reverse());
            }
        }
        public void SortByTitle()
        {
            if (IsProcessing) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy(m => m.Name).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy(m => m.Name));
            }
        }
        public void SortByArtist()
        {
            if (IsProcessing) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy(m => m.Artist).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy(m => m.Artist));
            }
        }
        public void SortByAlbum()
        {
            if (IsProcessing) Helper.ShowNotification("NowLoading");
            else
            {
                MusicCollection = MusicCollection.OrderBy(m => m.Album).ToList();
                GridMusicCollection.SetTo(GridMusicCollection.ToList().OrderBy(m => m.Source.Album));
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
            new MenuFlyoutHelper() { Data = (fe.DataContext as GridMusicView).Source }.GetAddToMenuFlyout("", this).ShowAt(fe);
        }
        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption() { WithSelect = false });
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

        void IMenuFlyoutItemClickListener.Favorite(object data)
        {
            
        }

        void IMenuFlyoutItemClickListener.Delete(Music music)
        {
            RemoveMusic(music);
        }

        void IMenuFlyoutItemClickListener.Remove(Music music)
        {
            RemoveMusic(music);
        }

        void IMenuFlyoutItemClickListener.UndoDelete(Music music)
        {
            MusicCollection.Insert(removedItemIndex, music);
            GridMusicCollection.Insert(removedItemIndex, new GridMusicView(music));
        }

        void IMenuFlyoutItemClickListener.Select(Music music)
        {
            
        }

        private void RemoveMusic(Music music)
        {
            removedItemIndex = MusicCollection.IndexOf(music);
            MusicCollection.RemoveAt(removedItemIndex);
            GridMusicCollection.RemoveAt(removedItemIndex);
            foreach (var listener in RemoveListeners) listener.MusicRemoved(removedItemIndex, music, MusicCollection);
        }
    }
}
