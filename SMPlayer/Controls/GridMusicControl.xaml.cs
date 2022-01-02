using SMPlayer.Controls;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class GridMusicControl : UserControl, IMusicEventListener, ISwitchMusicListener, IMenuFlyoutItemClickListener, IMultiSelectListener
    {
        public object Header { get => MusicGridView.Header; set => MusicGridView.Header = value; }
        public ListViewSelectionMode SelectionMode { get => MusicGridView.SelectionMode; set => MusicGridView.SelectionMode = value; }
        public ObservableCollection<GridMusicView> GridMusicCollection = new ObservableCollection<GridMusicView>();
        public List<Music> MusicCollection = new List<Music>();
        private volatile bool IsProcessing = false;
        public event ItemClickEventHandler GridItemClickedListener;
        public IMultiSelectListener MultiSelectListener { get; set; }
        public IMenuFlyoutHelperBuildListener MenuFlyoutHelperBuildListener { get; set; }
        public List<IRemoveMusicListener> RemoveListeners = new List<IRemoveMusicListener>();
        public MenuFlyoutOption MenuFlyoutOpeningOption { get; set; }
        public event TypedEventHandler<FrameworkElement, EffectiveViewportChangedEventArgs> TopItemEffectiveViewportChanged;

        public IList<object> SelectedItems { get => MusicGridView.SelectedItems; }
        public int SelectedItemsCount { get => SelectedItems.Count; }
        private int removedItemIndex = -1;

        public GridMusicControl()
        {
            this.InitializeComponent();
            MusicPlayer.SwitchMusicListeners.Add(this);
            Settings.AddMusicEventListener(this);
        }

        private void MusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (MusicGridView.SelectionMode != ListViewSelectionMode.None)
            {
                return;
            }
            var item = (GridMusicView)e.ClickedItem;
            if (GridItemClickedListener == null)
            {
                MusicPlayer.SetMusicAndPlay(MusicCollection, item.Source);
            }
            else
            {
                GridItemClickedListener.Invoke(item, e);
            }
        }

        public void Setup(IEnumerable<long> collection)
        {
            Setup(collection.Select(i => new MusicPath(i)));
        }

        public void Setup(IEnumerable<IMusicable> collection)
        {
            IsProcessing = true;
            Clear();
            foreach (var item in collection)
            {
                Music music = item.ToMusic();
                AddMusic(music);
            }      
            IsProcessing = false;
        }
        public void AddMusic(Music music, int index = -1)
        {
            var copy = music.Copy();
            copy.IsPlaying = copy.Equals(MusicPlayer.CurrentMusic);
            GridMusicView gridMusicView = new GridMusicView(copy);
            if (index == -1)
            {
                MusicCollection.Add(copy);
                GridMusicCollection.Add(gridMusicView);
            }
            else
            {
                MusicCollection.Insert(index, copy);
                GridMusicCollection.Insert(index, gridMusicView);
            }
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
            MenuFlyoutHelper.SetMusicMenu(sender, this, MenuFlyoutHelperBuildListener, MenuFlyoutOpeningOption);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MusicPlayer.SetMusicPlaying(MusicCollection, next);
                foreach (var item in GridMusicCollection)
                    item.Source.IsPlaying = item.Source.Equals(next);
            });
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

        public void UndoDelete(Music music)
        {
            MusicCollection.Insert(removedItemIndex, music);
            GridMusicCollection.Insert(removedItemIndex, new GridMusicView(music));
        }

        void IMenuFlyoutItemClickListener.Select(object data) 
        {
            MusicGridView.SelectionMode = ListViewSelectionMode.Multiple;
            MusicGridView.SelectedItems.Add(data);
        }

        public bool RemoveMusic(Music music)
        {
            removedItemIndex = MusicCollection.IndexOf(music);
            if (removedItemIndex > -1)
            {
                MusicCollection.RemoveAt(removedItemIndex);
                GridMusicCollection.RemoveAt(removedItemIndex);
                foreach (var listener in RemoveListeners) listener.MusicRemoved(removedItemIndex, music, MusicCollection);
            }
            return removedItemIndex > -1;
        }

        public void AddOrMoveToTheFirst(Music music)
        {
            int removedItemIndex = MusicCollection.IndexOf(music);
            if (removedItemIndex > -1)
            {
                MusicCollection.RemoveAt(removedItemIndex);
                GridMusicCollection.RemoveAt(removedItemIndex);
            }
            AddMusic(music, 0);
        }

        private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender.DataContext is GridMusicView data && !data.ThumbnailLoaded && sender.IsPartiallyVisible(MusicGridView))
            {
                await data.SetThumbnailAsync();
            }
        }

        private async void UserControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (sender.DataContext is GridMusicView data && !data.ThumbnailLoaded && ImageHelper.NeedsLoading(sender, args))
            {
                await (sender.DataContext as GridMusicView).SetThumbnailAsync();
            }
            // args.EffectiveViewport.X == 0 保证最左
            // args.EffectiveViewport.Top == args.BringIntoViewDistanceY 保证最上
            // sender.ActualHeight > args.BringIntoViewDistanceY 保证在视图内
            if (args.EffectiveViewport.X == 0 && args.EffectiveViewport.Top == args.BringIntoViewDistanceY
                && sender.ActualHeight > args.BringIntoViewDistanceY)
            {
                TopItemEffectiveViewportChanged?.Invoke(sender, args);
            }
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            MusicGridView.SelectionMode = ListViewSelectionMode.Single;
            MultiSelectListener?.Cancel(commandBar);
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            if (SelectedItemsCount == 0) return;
            helper.Data = MusicGridView.SelectedItems.Select(i => (GridMusicView)i);
            MultiSelectListener?.AddTo(commandBar, helper);
        }

        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar)
        {
            if (SelectedItemsCount == 0) return;
            MusicPlayer.SetMusicAndPlay(MusicGridView.SelectedItems.Select(i => (GridMusicView)i));
            MultiSelectListener?.Play(commandBar);
        }

        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar)
        {
            MultiSelectListener?.Remove(commandBar);
        }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            MusicGridView.SelectAll();
            MultiSelectListener?.SelectAll(commandBar);
        }

        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar)
        {
            MusicGridView.ReverseSelections();
            MultiSelectListener?.ReverseSelections(commandBar);
        }

        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar)
        {
            MusicGridView.ClearSelections();
            MultiSelectListener?.ClearSelections(commandBar);
        }

        void IMenuFlyoutItemClickListener.AddTo(object data, object collection, int index, AddToCollectionType type) { }
        void IMenuFlyoutItemClickListener.UndoDelete(Music music) { }

        void IMusicEventListener.Liked(Music music, bool isFavorite)
        {

        }

        void IMusicEventListener.Added(Music music)
        {
        }

        void IMusicEventListener.Removed(Music music)
        {
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            int index = MusicCollection.IndexOf(before);
            if (index > -1)
            {
                GridMusicCollection[index].Source = MusicCollection[index] = after;
            }
        }
    }

    public class MusicPath : IMusicable
    {
        public long Id { get; set; }
        public MusicPath(long Id)
        {
            this.Id = Id;
        }

        Music IMusicable.ToMusic()
        {
            return Settings.FindMusic(Id);
        }
    }
}
