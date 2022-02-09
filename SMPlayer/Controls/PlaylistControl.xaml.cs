using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class PlaylistControl : UserControl, IMusicEventListener, IMenuFlyoutItemClickListener, IMultiSelectListener, IMusicPlayerEventListener
    {
        public ObservableCollection<MusicView> CurrentPlaylist
        {
            get => currentPlaylist;
            set => currentPlaylist.SetTo(value);
        }
        private ObservableCollection<MusicView> currentPlaylist = new ObservableCollection<MusicView>();
        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        public IPlaylistScrollListener ScrollListener { get; set; }
        public IMultiSelectListener MultiSelectListener { get; set; }
        public bool AllowReorder
        {
            get => SongsListView.CanReorderItems;
            set => SongsListView.CanReorderItems = SongsListView.AllowDrop = SongsListView.CanDrag = SongsListView.CanDragItems = value;
        }
        public object Header
        {
            get => SongsListView.Header;
            set => SongsListView.Header = value;
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public object Footer
        {
            get => SongsListView.Footer;
            set => SongsListView.Footer = value;
        }
        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register("Footer", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public ObservableCollection<MusicView> ItemsSource
        {
            get => currentPlaylist;
            set => currentPlaylist = value;
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public bool ShowAlbumText { get; set; } = true;
        public bool Removable { get; set; } = true;
        public bool IsNowPlaying { get; set; }
        public bool AlternatingRowColor { get; set; }
        public bool Selectable { get; set; } = true;
        public ScrollViewer ScrollViewer => SongsListView.GetFirstDescendantOfType<ScrollViewer>();
        public ListViewSelectionMode SelectionMode
        {
            get => SongsListView.SelectionMode;
            set => SongsListView.SelectionMode = value;
        }
        public List<MusicView> SelectedItems => SongsListView.SelectedItems.Select(m => (MusicView)m).ToList();
        public int SelectedItemsCount => SongsListView.SelectedItems.Count;
        public IPlaylistEventListener PlaylistEventListener { get; set; }
        private Dialogs.RemoveDialog dialog;
        private int dragIndex, dropIndex, removedMusicIndex = -1;
        private MusicView removedMusic = null;
        private bool doNotListenToMusicPlayer;

        public PlaylistControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsNowPlaying)
            {
                int index = 0;
                CurrentPlaylist.SetTo(MusicPlayer.CurrentPlaylist.Select(i => i.ToVO(index++)));
            }
            Helper.GetMainPageContainer()?.SetMultiSelectListener(this);
        }

        private bool ViewChangedUnadded = true;
        private double ScrollPosition = 0d;
        private void SongsListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewChangedUnadded && ScrollViewer != null)
            {
                ScrollViewer.ViewChanged += (s, args) =>
                {
                    var viewer = s as ScrollViewer;
                    ScrollListener?.Scrolled(ScrollPosition, viewer.VerticalOffset);
                    ScrollPosition = viewer.VerticalOffset;
                };
                ViewChangedUnadded = false;
            }
        }

        public static void AddMusicRequestListener(IMusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
        }

        private void AlternateRowBackgroud(int start = 0)
        {
            AlternateRowBackgroud(start, CurrentPlaylist.Count);
        }

        private void AlternateRowBackgroud(int start, int end)
        {
            if (!AlternatingRowColor) return;
            for (int i = start; i < end; i++)
                if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }

        public static Brush GetRowBackground(int index)
        {
            return index % 2 == 0 ? ColorHelper.WhiteSmokeBrush : ColorHelper.WhiteBrush;
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (AlternatingRowColor) args.ItemContainer.Background = GetRowBackground(args.ItemIndex);
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectionMode != ListViewSelectionMode.None) return;
            MusicView music = (MusicView)e.ClickedItem;
            if (IsNowPlaying)
            {
                MusicPlayer.MoveToMusic(music.Index);
                MusicPlayer.Play();
            }
            else
            {
                MusicPlayer.SetMusicAndPlay(CurrentPlaylist, music);
            }
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            MusicView music = args.Items[0] as MusicView;
            dropIndex = CurrentPlaylist.FindIndex(m => m == music && m.Index == music.Index);
            if (dragIndex == dropIndex) return;
            if (IsNowPlaying)
            {
                doNotListenToMusicPlayer = true;
                MusicPlayer.MoveMusic(dragIndex, dropIndex);
                doNotListenToMusicPlayer = false;
                ResetIndex(Math.Min(dragIndex, dropIndex), Math.Max(dragIndex, dropIndex));
            }
            AlternateRowBackgroud(Math.Min(dragIndex, dropIndex), Math.Max(dragIndex, dropIndex) + 1);
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            MusicView music = flyout.Target.DataContext as MusicView;
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption
            {
                ShowSelect = Selectable,
                ShowRemove = Removable,
                MultiSelectOption = new MultiSelectCommandBarOption { ShowRemove = Removable },
                ShowMoveToTop = AllowReorder && music.Index > 0
            });
        }

        private void RemoveItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            RemoveMusicAndNotifyUser(args.SwipeControl.DataContext as MusicView, true);
        }

        public async void AskRemoveMusic(MusicView music)
        {
            if (dialog == null) dialog = new Dialogs.RemoveDialog();
            if (dialog.IsChecked)
            {
                RemoveMusicAndNotifyUser(music, true);
            }
            else
            {
                dialog.Confirm = () => RemoveMusicAndNotifyUser(music, true);
                dialog.Message = Helper.LocalizeMessage("RemoveMusic", music.Name);
                await dialog.ShowAsync();
            }
        }

        public void AddMusic(Music music, SortBy criterion)
        {
            MusicView musicView = music.ToVO();
            int index = CurrentPlaylist.FindSortedListInsertIndex(musicView, criterion);
            CurrentPlaylist.Insert(index, musicView);
            AlternateRowBackgroud(index);
        }

        public void RemoveMusic(Music music)
        {
            int index = CurrentPlaylist.FindIndex(i => i.Equals(music));
            CurrentPlaylist.RemoveAt(index);
            AlternateRowBackgroud(index);
        }

        private int RemoveMusicAndNotifyListeners(MusicView music, bool alternateRowBackgroud = true)
        {
            int index = RemoveMusic(music, alternateRowBackgroud);
            if (index == -1) return index;
            if (IsNowPlaying) MusicPlayer.RemoveMusic(music.Index);
            PlaylistEventListener?.Execute(null, new PlaylistEventArgs(PlaylistEventType.RemoveMusic) { Music = music.FromVO() });
            return index;
        }

        private int RemoveMusic(MusicView music, bool alternateRowBackgroud = true)
        {
            int index = IsNowPlaying ? music.Index : CurrentPlaylist.IndexOf(music);
            if (index == -1) return index;
            removedMusic = music;
            CurrentPlaylist.RemoveAt(index);
            if (alternateRowBackgroud) AlternateRowBackgroud(index);
            return index;
        }

        private int RemoveMusicAndNotifyUser(MusicView music, bool showNotification)
        {
            doNotListenToMusicPlayer = true;
            int index = RemoveMusicAndNotifyListeners(music, true);
            doNotListenToMusicPlayer = false;
            if (showNotification)
            {
                Helper.ShowUndoableNotification(Helper.LocalizeMessage("MusicRemoved", music.Name), () => CancelMusicRemoval(index, music));
            }
            return index;
        }

        private void CancelMusicRemoval(int index, MusicView music)
        {
            if (IsNowPlaying)
            {
                MusicPlayer.AddMusic(music, index);
            }
            else
            {
                CurrentPlaylist.Insert(index, music);
                PlaylistEventListener?.Execute(null, new PlaylistEventArgs(PlaylistEventType.AddMusic) { Music = music.FromVO() });
            }
            AlternateRowBackgroud(index);
        }

        private void RemoveMusic(IEnumerable<MusicView> playlist)
        {
            foreach (var music in playlist)
            {
                RemoveMusicAndNotifyListeners(music, false);
            }
            AlternateRowBackgroud();
            Helper.ShowNotification("SelectedItemsRemoved");
        }

        private void FavoriteItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var music = args.SwipeControl.DataContext as MusicView;
            if (music.Favorite) MusicService.DislikeMusic(music.FromVO());
            else MusicService.LikeMusic(music.FromVO());
        }

        public void ScrollToTop()
        {
            SongsListView.ScrollToTop();
        }

        private int ScrollToMusicRequestedWhenUnloaded = -1;

        public async void ScrollToCurrentMusic(bool showNotification = false)
        {
            if (!ScrollToMusic(MusicPlayer.CurrentIndex, false))
            {
                await SongsListView.LoadMoreItemsAsync();
                ScrollToMusic(MusicPlayer.CurrentIndex, showNotification);
            }
        }
        private bool ScrollToMusic(int index, bool showNotification = false)
        {
            if (index < 0) return false;
            if (SongsListView.IsLoaded)
            {
                if (!SongsListView.ScrollToIndex(index))
                {
                    if (showNotification) Helper.ShowNotification("UnableToLocateMusic");
                    return false;
                }
            }
            else
            {
                ScrollToMusicRequestedWhenUnloaded = index;
            }
            return true;
        }

        private void SwipeControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Removable) (sender as SwipeControl).RightItems = null;
            if (ScrollToMusicRequestedWhenUnloaded != -1 && SongsListView.ScrollToIndex(ScrollToMusicRequestedWhenUnloaded))
            {
                ScrollToMusicRequestedWhenUnloaded = -1;
            }
        }

        private void SongsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            dragIndex = (e.Items[0] as MusicView).Index;
        }

        public void SelectAll()
        {
            SongsListView.SelectAll();
        }

        public void ReverseSelections()
        {
            SongsListView.ReverseSelections();
        }

        public void ClearSelections()
        {
            SongsListView.ClearSelections();
        }

        private void MusicModified(MusicView before, MusicView after)
        {
            CurrentPlaylist.FirstOrDefault(m => m == before)?.CopyFrom(after);
        }

        private void SongsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase listViewBase = (sender as ListViewBase);
            if (listViewBase.SelectionMode == ListViewSelectionMode.Multiple)
            {
                Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(listViewBase.SelectedItems.Count);
            }
        }

        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.Delete:
                    RemoveMusicAndNotifyUser(args.MusicView, false);
                    break;
                case MenuFlyoutEvent.Remove:
                    AskRemoveMusic(args.MusicView);
                    break;
                case MenuFlyoutEvent.Select:
                    SelectionMode = ListViewSelectionMode.Multiple;
                    SongsListView.SelectedItems.Add(args.Data);
                    break;
            }
        }

        async void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    SelectionMode = ListViewSelectionMode.None;
                    break;
                case MultiSelectEvent.AddTo:
                    if (SelectedItemsCount == 0) return;
                    args.FlyoutHelper.Data = SelectedItems;
                    break;
                case MultiSelectEvent.Play:
                    if (SelectedItemsCount == 0) return;
                    MusicPlayer.SetMusicAndPlay(SelectedItems);
                    break;
                case MultiSelectEvent.Remove:
                    if (SelectedItemsCount == 0) return;
                    if (SelectedItemsCount == 1)
                    {
                        AskRemoveMusic(SongsListView.SelectedItems[0] as MusicView);
                    }
                    else
                    {
                        if (dialog == null) dialog = new Dialogs.RemoveDialog();
                        if (dialog.IsChecked)
                        {
                            RemoveMusic(SelectedItems);
                        }
                        else
                        {
                            dialog.Confirm = () => RemoveMusic(SelectedItems);
                            dialog.Message = Helper.LocalizeMessage("RemoveMusicList", SongsListView.SelectedItems.Count);
                            await dialog.ShowAsync();
                        }
                    }
                    break;
                case MultiSelectEvent.SelectAll:
                    SongsListView.SelectAll();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItemsCount);
                    break;
                case MultiSelectEvent.ReverseSelections:
                    SongsListView.ReverseSelections();
                    Helper.GetMainPageContainer()?.GetMultiSelectCommandBar().CountSelections(SelectedItemsCount);
                    break;
                case MultiSelectEvent.ClearSelections:
                    SongsListView.ClearSelections();
                    break;
            }
            MultiSelectListener?.Execute(commandBar, args);
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    if (removedMusic.Equals(music))
                    {
                        CurrentPlaylist.Insert(removedMusicIndex, music.ToVO());
                    }
                    break;
                case MusicEventType.Remove:
                    RemoveMusicAndNotifyListeners(music.ToVO());
                    break;
                case MusicEventType.Like:
                    MusicModified(music.ToVO(), new MusicView(music.ToVO()) { Favorite = args.IsFavorite });
                    break;
                case MusicEventType.Modify:
                    MusicModified(music.ToVO(), args.ModifiedMusic.ToVO());
                    break;
            }
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            if (!IsNowPlaying || doNotListenToMusicPlayer) return;
            switch (args.EventType)
            {
                case MusicPlayerEventType.Add:
                    CurrentPlaylist.Insert(args.Index, args.Music.ToVO());
                    ResetIndex(args.Index);
                    break;
                case MusicPlayerEventType.Remove:
                    if (args.Index == -1)
                    {
                        ResetIndex(RemoveMusic(args.Music.ToVO()));
                    }
                    else
                    {
                        CurrentPlaylist.RemoveAt(args.Index);
                        AlternateRowBackgroud(args.Index);
                        ResetIndex(args.Index);
                    }
                    break;
                case MusicPlayerEventType.Clear:
                    CurrentPlaylist.Clear();
                    break;
                case MusicPlayerEventType.Move:
                    MusicPlayerMoveEventArgs moveArgs = args as MusicPlayerMoveEventArgs;
                    CurrentPlaylist.Move(moveArgs.Index, moveArgs.ToIndex);
                    ResetIndex(Math.Min(moveArgs.Index, moveArgs.ToIndex), Math.Max(moveArgs.Index, moveArgs.ToIndex));
                    break;
                case MusicPlayerEventType.Switch:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MusicPlayer.SetMusicPlaying(CurrentPlaylist, args.Music));
                    break;
            }
        }
        private void ResetIndex(int from)
        {
            ResetIndex(from, CurrentPlaylist.Count - 1);
        }

        private void ResetIndex(int from, int to)
        {
            if (from < 0 || to < 0) return;
            for (int i = from; i <= to; i++)
                CurrentPlaylist[i].Index = i;
        }
    }

    public interface IPlaylistScrollListener
    {
        void Scrolled(double before, double after);
    }

    public enum ScrollDirection
    {
        None = 0, Up = 1, Down = 2
    }
}
