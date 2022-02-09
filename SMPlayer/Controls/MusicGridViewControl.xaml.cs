using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
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
    public sealed partial class MusicGridViewControl : UserControl, IMusicEventListener, IMusicPlayerEventListener, IMenuFlyoutItemClickListener, IMultiSelectListener
    {
        public object Header { get => MusicGridView.Header; set => MusicGridView.Header = value; }
        public ListViewSelectionMode SelectionMode { get => MusicGridView.SelectionMode; set => MusicGridView.SelectionMode = value; }
        public ObservableCollection<GridViewMusic> GridMusicCollection = new ObservableCollection<GridViewMusic>();
        private bool IsProcessing = false;
        public event ItemClickEventHandler GridItemClickedListener;
        public IMultiSelectListener MultiSelectListener { get; set; }
        public IMenuFlyoutHelperBuildListener MenuFlyoutHelperBuildListener { get; set; }
        public MenuFlyoutOption MenuFlyoutOpeningOption { get; set; }
        public event TypedEventHandler<FrameworkElement, EffectiveViewportChangedEventArgs> TopItemEffectiveViewportChanged;

        public IList<object> SelectedItems { get => MusicGridView.SelectedItems; }
        public int SelectedItemsCount { get => SelectedItems.Count; }
        private int removedItemIndex = -1;

        public MusicGridViewControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddMusicPlayerEventListener(this);
            MusicService.AddMusicEventListener(this);
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
                MusicPlayer.SetMusicAndPlay(GridMusicCollection, item.Source);
            }
            else
            {
                GridItemClickedListener.Invoke(item, e);
            }
        }

        public void Setup(IEnumerable<IMusicable> collection)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Clear();
            foreach (var item in collection)
            {
                AddMusic(item.ToMusic().ToVO());
            }      
            IsProcessing = false;
        }
        public void AddMusic(MusicView music, int index = -1)
        {
            var copy = music.Copy();
            copy.IsPlaying = copy.Equals(MusicPlayer.CurrentMusic);
            GridViewMusic gridMusicView = new GridViewMusic(copy);
            if (index == -1)
            {
                GridMusicCollection.Add(gridMusicView);
            }
            else
            {
                GridMusicCollection.Insert(index, gridMusicView);
            }
        }
        public void Clear()
        {
            GridMusicCollection.Clear();
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetMusicMenu(sender, this, MenuFlyoutHelperBuildListener, MenuFlyoutOpeningOption);
        }

        public void UndoDelete(MusicView music)
        {
            GridMusicCollection.Insert(removedItemIndex, new GridViewMusic(music));
        }

        public bool RemoveMusic(MusicView music)
        {
            removedItemIndex = GridMusicCollection.FindIndex(i => i.Source == music);
            if (removedItemIndex > -1)
            {
                GridMusicCollection.RemoveAt(removedItemIndex);
            }
            return removedItemIndex > -1;
        }

        public void AddOrMoveToTheFirst(IMusicable music)
        {
            GridViewMusic gridViewMusic = GridMusicCollection.FirstOrDefault(i => i.Source.Equals(music.ToMusic()));
            if (gridViewMusic == null)
            {
                GridMusicCollection.Insert(0, new GridViewMusic(music.ToMusic().ToVO()));
            }
            else
            {
                GridMusicCollection.AddOrMoveToTheFirst(gridViewMusic);
            }
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

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var item in GridMusicCollection)
                            item.IsPlaying = item.Source.Equals(args.Music);
                    });
                    break;
            }
        }

        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.Remove:
                case MenuFlyoutEvent.Delete:
                    RemoveMusic(args.MusicView);
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
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItemsCount);
                    break;
                case MultiSelectEvent.ReverseSelections:
                    MusicGridView.ReverseSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItemsCount);
                    break;
                case MultiSelectEvent.ClearSelections:
                    MusicGridView.ClearSelections();
                    break;
                case MultiSelectEvent.MoveToFolder:
                    break;
            }
            MultiSelectListener?.Execute(commandBar, args);
        }

        async void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Modify:
                    int index = GridMusicCollection.FindIndex(i => i.Source.Equals(music.ToVO()));
                    if (index > -1)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            GridMusicCollection[index].Source = args.ModifiedMusic.ToVO();
                        });
                    }
                    break;
            }
        }

        private void MusicGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = sender as ListViewBase;
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
            }
        }
    }
}
