using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        public event ItemClickEventHandler GridItemClickedListener;
        public List<IRemoveMusicListener> RemoveListeners = new List<IRemoveMusicListener>();
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
            if (GridItemClickedListener == null)
            {
                MediaHelper.SetMusicAndPlay(MusicCollection, item.Source);
            }
            else
            {
                GridItemClickedListener.Invoke(item, e);
            }
        }

        public void Setup(IEnumerable<string> collection)
        {
            IsProcessing = true;
            Setup(collection.Select(i => new MusicPath(i)));
            IsProcessing = false;
        }

        public async void Setup(IEnumerable<IMusicable> collection)
        {
            IsProcessing = true;
            Clear();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                foreach (var item in collection)
                {
                    Music music = item.ToMusic();
                    AddMusic(music);
                }
            });
            IsProcessing = false;
        }
        public void AddMusic(Music music)
        {
            var copy = music.Copy();
            copy.IsPlaying = copy.Equals(MediaHelper.CurrentMusic);
            MusicCollection.Add(copy);
            GridMusicCollection.Add(new GridMusicView(copy));
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
            new MenuFlyoutHelper() { Data = (fe.DataContext as GridMusicView).Source }.GetAddToMenuFlyout(this).ShowAt(fe);
        }
        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption() { ShowSelect = false });
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

        void IMenuFlyoutItemClickListener.Favorite(object data) { }

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

        void IMenuFlyoutItemClickListener.Select(object data) { }

        public bool RemoveMusic(Music music)
        {
            removedItemIndex = MusicCollection.IndexOf(music);
            MusicCollection.RemoveAt(removedItemIndex);
            GridMusicCollection.RemoveAt(removedItemIndex);
            foreach (var listener in RemoveListeners) listener.MusicRemoved(removedItemIndex, music, MusicCollection);
            return removedItemIndex > -1;
        }
    }

    public class MusicPath : IMusicable
    {
        public string Path { get; set; }
        public MusicPath(string path)
        {
            Path = path;
        }

        Music IMusicable.ToMusic()
        {
            return Settings.FindMusic(Path);
        }
    }
}
