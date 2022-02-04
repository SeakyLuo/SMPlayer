using SMPlayer.Controls;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
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
    public sealed partial class MusicGridViewControl : UserControl, IMusicEventListener, ISwitchMusicListener, IMenuFlyoutItemClickListener, IMultiSelectListener
    {
        public object Header { get => MusicGridView.Header; set => MusicGridView.Header = value; }
        public ListViewSelectionMode SelectionMode { get => MusicGridView.SelectionMode; set => MusicGridView.SelectionMode = value; }
        public ObservableCollection<GridViewMusic> GridMusicCollection = new ObservableCollection<GridViewMusic>();
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

        public MusicGridViewControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddSwitchMusicListener(this);
            Settings.AddMusicEventListener(this);
        }

        private void MusicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (MusicGridView.SelectionMode != ListViewSelectionMode.None)
            {
                return;
            }
            var item = (GridViewMusic)e.ClickedItem;
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
            GridViewMusic gridMusicView = new GridViewMusic(copy);
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

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, MenuFlyoutHelperBuildListener, MenuFlyoutOpeningOption);
        }

        public void UndoDelete(Music music)
        {
            MusicCollection.Insert(removedItemIndex, music);
            GridMusicCollection.Insert(removedItemIndex, new GridViewMusic(music));
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
            if (sender.DataContext is GridViewMusic data && !data.IsThumbnailLoaded && sender.IsPartiallyVisible(MusicGridView))
            {
                await data.LoadThumbnailAsync();
            }
        }

        private async void UserControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (sender.DataContext is GridViewMusic data && !data.IsThumbnailLoaded && ImageHelper.NeedsLoading(sender, args))
            {
                await data.LoadThumbnailAsync();
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

        async void ISwitchMusicListener.MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MusicPlayer.SetMusicPlaying(MusicCollection, next);
                foreach (var item in GridMusicCollection)
                    item.IsPlaying = item.Source.Equals(next);
            });
        }

        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.Remove:
                case MenuFlyoutEvent.Delete:
                    RemoveMusic(args.Music);
                    break;
                case MenuFlyoutEvent.UndoDelete:
                    UndoDelete(args.Music);
                    break;
                case MenuFlyoutEvent.Select:
                    MusicGridView.SelectionMode = ListViewSelectionMode.Multiple;
                    MusicGridView.SelectedItems.Add(args.Data);
                    break;
            }
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    MusicGridView.SelectionMode = ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    if (SelectedItemsCount == 0) return;
                    args.FlyoutHelper.Data = MusicGridView.SelectedItems.Select(i => (GridViewMusic)i);
                    break;
                case MultiSelectEvent.Play:
                    if (SelectedItemsCount == 0) return;
                    MusicPlayer.SetMusicAndPlay(MusicGridView.SelectedItems.Select(i => (GridViewMusic)i));
                    break;
                case MultiSelectEvent.Remove:
                    break;
                case MultiSelectEvent.SelectAll:
                    MusicGridView.SelectAll();
                    break;
                case MultiSelectEvent.ReverseSelections:
                    MusicGridView.ReverseSelections();
                    break;
                case MultiSelectEvent.ClearSelections:
                    MusicGridView.ClearSelections();
                    break;
                case MultiSelectEvent.MoveToFolder:
                    break;
            }
            MultiSelectListener?.Execute(commandBar, args);
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Modify:
                    int index = MusicCollection.IndexOf(music);
                    if (index > -1)
                    {
                        GridMusicCollection[index].Source = MusicCollection[index] = args.ModifiedMusic;
                    }
                    break;
            }
        }

        private void MusicGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = (sender as ListViewBase);
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
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
            return MusicService.FindMusic(Id);
        }
    }
}
