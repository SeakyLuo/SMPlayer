using SMPlayer.Controls;
using SMPlayer.Models;
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
    public sealed partial class PlaylistControl : UserControl, IMusicEventListener, ISwitchMusicListener, IMenuFlyoutItemClickListener, IMultiSelectListener, ICurrentPlaylistChangedListener
    {
        public bool IsNowPlaying
        {
            get => (bool)GetValue(IsNowPlayingProperty);
            set
            {
                SetValue(IsNowPlayingProperty, value);
                if (value)
                {
                    MenuFlyoutHelper.ClickListeners.Add(this);
                }
                else
                {
                    MenuFlyoutHelper.ClickListeners.Remove(this);
                }
            }
        }
        public static readonly DependencyProperty IsNowPlayingProperty = DependencyProperty.Register("SelectableProperty", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(false));

        public ObservableCollection<Music> CurrentPlaylist
        {
            get => currentPlaylist;
            set => currentPlaylist.SetTo(value);
        }
        private ObservableCollection<Music> currentPlaylist = new ObservableCollection<Music>();
        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        public bool AlternatingRowColor { get; set; }
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

        public ObservableCollection<Music> ItemsSource
        {
            get => currentPlaylist;
            set => currentPlaylist = value;
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public bool ShowAlbumText
        {
            get => (bool)GetValue(ShowAlbumTextProperty);
            set => SetValue(ShowAlbumTextProperty, value);
        }
        public static readonly DependencyProperty ShowAlbumTextProperty = DependencyProperty.Register("ShowAlbumText", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(true));
        public bool Removable
        {
            get => (bool)GetValue(RemovableProperty);
            set => SetValue(RemovableProperty, value);
        }
        public static readonly DependencyProperty RemovableProperty = DependencyProperty.Register("Removable", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(true));
        public bool AllowMultipleSelection
        {
            get => (bool)GetValue(AllowMultipleSelectionProperty);
            set => SetValue(AllowMultipleSelectionProperty, value);
        }
        public static readonly DependencyProperty AllowMultipleSelectionProperty = DependencyProperty.Register("AllowMultipleSelection", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(false));
        public ScrollViewer ScrollViewer
        {
            get => SongsListView.GetFirstDescendantOfType<ScrollViewer>();
        }
        public bool Selectable
        {
            get => (bool)GetValue(SelectableProperty);
            set => SetValue(SelectableProperty, value);
        }
        public static readonly DependencyProperty SelectableProperty = DependencyProperty.Register("SelectableProperty", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(true));

        public ListViewSelectionMode SelectionMode
        {
            get => SongsListView.SelectionMode;
            set => SongsListView.SelectionMode = value;
        }
        public List<Music> SelectedItems
        {
            get => SongsListView.SelectedItems.Select(m => (Music)m).ToList();
        }
        public int SelectedItemsCount
        {
            get => SongsListView.SelectedItems.Count;
        }
        public MultiSelectCommandBarOption MultiSelectOption { get; set; }
        public List<IRemoveMusicListener> RemoveListeners = new List<IRemoveMusicListener>();
        private Dialogs.RemoveDialog dialog;
        private int dragIndex, dropIndex, removedMusicIndex = -1;
        private Music removedMusic = null;

        public PlaylistControl()
        {
            this.InitializeComponent();
            MusicPlayer.CurrentPlaylistChangedListeners.Add(this);
            MusicPlayer.AddSwitchMusicListener(this);
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsNowPlaying) CurrentPlaylist.SetTo(MusicPlayer.CurrentPlaylist.Select(i => i.Copy()));
            Helper.GetMainPageContainer().SetMultiSelectListener(this);
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
            if (AlternatingRowColor)
                args.ItemContainer.Background = GetRowBackground(args.ItemIndex);
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectionMode != ListViewSelectionMode.None) return;
            Music music = (Music)e.ClickedItem;
            if (MusicPlayer.CurrentMusic == music && MusicPlayer.CurrentPlaylist.SameAs(CurrentPlaylist))
            {
                if (!MusicPlayer.IsPlaying)
                {
                    MusicPlayer.Play();
                }
            }
            else
            {
                MusicPlayer.SetMusicAndPlay(CurrentPlaylist, music);
            }
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MusicPlayer.SetMusicPlaying(CurrentPlaylist, next));
        }

        public void CancelMusicRemoval(int index, Music music)
        {
            if (IsNowPlaying)
            {
                MusicPlayer.AddMusic(music, index);
            }
            else
            {
                CurrentPlaylist.Insert(index, music);
            }
            AlternateRowBackgroud(index);
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (!AllowReorder) return;
            Music music = args.Items[0] as Music;
            dropIndex = CurrentPlaylist.FindIndex(m => m == music && m.Index == music.Index);
            if (dragIndex == dropIndex) return;
            if (IsNowPlaying) MusicPlayer.MoveMusic(dragIndex, dropIndex, false);
            AlternateRowBackgroud(Math.Min(dragIndex, dropIndex), Math.Max(dragIndex, dropIndex) + 1);
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            Music music = flyout.Target.DataContext as Music;
            MenuFlyoutHelper.SetMusicMenu(sender, this, null, new MenuFlyoutOption
            {
                ShowSelect = Selectable,
                ShowRemove = Removable,
                MultiSelectOption = MultiSelectOption,
                ShowMoveToTop = AllowReorder && music.Index > 0
            });
        }

        private void RemoveItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            AskRemoveMusic(args.SwipeControl.DataContext as Music);
        }

        public async void AskRemoveMusic(Music music)
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

        private void RemoveMusicAndNotifyListeners(Music music, bool alternateRowBackgroud = true)
        {
            if (!RemoveMusic(music)) return;
            if (IsNowPlaying) MusicPlayer.RemoveMusic(music);
            foreach (var listener in RemoveListeners) listener.MusicRemoved(removedMusicIndex, music, CurrentPlaylist);
        }

        private bool RemoveMusic(Music music, bool alternateRowBackgroud = true)
        {
            removedMusicIndex = IsNowPlaying ? music.Index : CurrentPlaylist.IndexOf(music);
            if (removedMusicIndex == -1) return false;
            removedMusic = music;
            CurrentPlaylist.RemoveAt(removedMusicIndex);
            if (alternateRowBackgroud) AlternateRowBackgroud(removedMusicIndex);
            return true;
        }

        private void RemoveMusicAndNotifyUser(Music music, bool showNotification)
        {
            RemoveMusicAndNotifyListeners(music, true);
            if (showNotification)
                Helper.ShowCancelableNotification(Helper.LocalizeMessage("MusicRemoved", music.Name), () => CancelMusicRemoval(removedMusicIndex, music));
        }

        private void RemoveMusic(IEnumerable<Music> playlist)
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
            var music = args.SwipeControl.DataContext as Music;
            if (music.Favorite) Settings.settings.DislikeMusic(music);
            else Settings.settings.LikeMusic(music);
        }

        public void ScrollToTop()
        {
            ScrollToIndex(0);
        }

        private int ScrollToMusicRequestedWhenUnloaded = -1;

        public async void ScrollToCurrentMusic(bool showNotification = false)
        {
            if (!ScrollToMusic(MusicPlayer.CurrentMusic, false))
            {
                Windows.UI.Xaml.Data.LoadMoreItemsResult result = await SongsListView.LoadMoreItemsAsync();
                ScrollToMusic(MusicPlayer.CurrentMusic, showNotification);
            }
        }
        public bool ScrollToMusic(Music music, bool showNotification = false)
        {
            if (music == null) return false;
            int index = IsNowPlaying ? music.Index : CurrentPlaylist.IndexOf(music);
            if (SongsListView.IsLoaded)
            {
                if (!ScrollToIndex(index))
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

        private bool ScrollToIndex(int index)
        {
            if (SongsListView.ContainerFromIndex(index) is ListViewItem item)
            {
                //SongsListView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                item.Locate();
                return true;
            }
            return false;
        }

        private void SwipeControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Removable) (sender as SwipeControl).RightItems = null;
            if (ScrollToMusicRequestedWhenUnloaded != -1 && ScrollToIndex(ScrollToMusicRequestedWhenUnloaded))
            {
                ScrollToMusicRequestedWhenUnloaded = -1;
            }
        }
        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.AddTo:
                    MenuFlyoutAddToEventArgs addToArgs = (MenuFlyoutAddToEventArgs)args;
                    if (addToArgs.CollectionType == AddToCollectionType.NowPlaying && IsNowPlaying)
                    {
                        AlternateRowBackgroud(addToArgs.Index);
                    }
                    break;
                case MenuFlyoutEvent.Delete:
                    RemoveMusicAndNotifyUser(args.Music, false);
                    break;
                case MenuFlyoutEvent.Remove:
                    AskRemoveMusic(args.Music);
                    break;
                case MenuFlyoutEvent.UndoDelete:
                    CurrentPlaylist.Insert(removedMusicIndex, args.Music);
                    AlternateRowBackgroud(removedMusicIndex);
                    break;
                case MenuFlyoutEvent.Select:
                    SelectionMode = ListViewSelectionMode.Multiple;
                    SongsListView.SelectedItems.Add(args.Data);
                    break;
            }
        }

        private void SongsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            dragIndex = (e.Items[0] as Music).Index;
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
                    MusicPlayer.SetPlaylistAndPlay(SelectedItems);
                    break;
                case MultiSelectEvent.Remove:
                    if (SelectedItemsCount == 0)
                    {
                        return;
                    }
                    if (SelectedItemsCount == 1)
                    {
                        AskRemoveMusic(SongsListView.SelectedItems[0] as Music);
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
                    break;
                case MultiSelectEvent.ReverseSelections:
                    SongsListView.ReverseSelections();
                    break;
                case MultiSelectEvent.ClearSelections:
                    SongsListView.ClearSelections();
                    break;
                case MultiSelectEvent.MoveToFolder:
                    break;
            }
            MultiSelectListener?.Execute(commandBar, args);
        }

        void IMusicEventListener.Liked(Music music, bool isFavorite)
        {
            MusicModified(music, music);
        }

        void IMusicEventListener.Added(Music music)
        {
            if (removedMusic == music)
            {
                CurrentPlaylist.Insert(removedMusicIndex, music);
            }
        }

        void IMusicEventListener.Removed(Music music)
        {
            RemoveMusicAndNotifyListeners(music);
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            MusicModified(before, after);
        }

        private void MusicModified(Music before, Music after)
        {
            CurrentPlaylist.FirstOrDefault(m => m == before)?.CopyFrom(after);
        }

        void ICurrentPlaylistChangedListener.AddMusic(Music music, int index)
        {
            if (IsNowPlaying)
                CurrentPlaylist.Insert(index, music);
        }

        void ICurrentPlaylistChangedListener.RemoveMusic(Music music)
        {
            if (IsNowPlaying)
                RemoveMusic(music);
        }

        void ICurrentPlaylistChangedListener.Cleared()
        {
            if (IsNowPlaying)
                CurrentPlaylist.Clear();
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
