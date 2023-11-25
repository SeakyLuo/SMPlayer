using SMPlayer.Controls;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        private int dropIndex;
        private MusicView removedMusic = null;
        private bool doNotListenToMusicPlayer; // 避免递归监听

        public PlaylistControl()
        {
            this.InitializeComponent();
            MusicService.AddMusicEventListener(this);
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsNowPlaying)
            {
                CurrentPlaylist.SetTo(MusicPlayer.CurrentPlaylist.Select(i => i.ToVO()));
            }
            for (int i = 0; i < CurrentPlaylist.Count; i++)
            {
                CurrentPlaylist[i].Index = i;
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

        private void AfterPlaylistModified(int from)
        {
            int to = CurrentPlaylist.Count - 1;
            if (from >= to) return;
            AfterPlaylistModified(from, to);
        }

        private void AfterPlaylistModified(int from, int to)
        {
            int start = Math.Max(0, Math.Min(from, to)),
            end = Math.Min(CurrentPlaylist.Count - 1, Math.Max(from, to));
            AlternateRowBackgroud(start, end);
            ResetIndex(start, end);
        }

        private void ResetIndex(int from, int to)
        {
            for (int i = from; i <= to; i++)
                CurrentPlaylist[i].Index = i;
        }

        private void AlternateRowBackgroud(int from, int to)
        {
            if (!AlternatingRowColor) return;
            for (int i = from; i <= to; i++)
                if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = ColorHelper.GetRowBackground(i);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (AlternatingRowColor) args.ItemContainer.Background = ColorHelper.GetRowBackground(args.ItemIndex);
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
            int dragIndex = music.Index;
            dropIndex = CurrentPlaylist.FindIndex(m => m == music && m.Index == music.Index);
            if (dragIndex == dropIndex) return;
            if (IsNowPlaying)
            {
                doNotListenToMusicPlayer = true;
                MusicPlayer.MoveMusic(dragIndex, dropIndex);
                doNotListenToMusicPlayer = false;
            }
            AfterPlaylistModified(dragIndex, dropIndex);
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
            RemoveMusic(args.SwipeControl.DataContext as MusicView, notifyUser: true);
        }

        public async void AskRemoveMusic(MusicView music)
        {
            if (dialog == null) dialog = new Dialogs.RemoveDialog();
            if (dialog.IsChecked)
            {
                RemoveMusic(music, notifyUser: true);
            }
            else
            {
                dialog.Confirm = () => RemoveMusic(music, notifyUser: true);
                dialog.Message = Helper.LocalizeMessage("RemoveMusic", music.Name);
                await dialog.ShowAsync();
            }
        }

        public void AddMusic(Music music, SortBy criterion)
        {
            MusicView musicView = music.ToVO();
            int index = CurrentPlaylist.FindSortedListInsertIndex(musicView, criterion);
            CurrentPlaylist.Insert(index, musicView);
            AfterPlaylistModified(index);
        }

        public void DeleteMusic(Music music)
        {
            CurrentPlaylist.RemoveAll(i => i.Path == music.Path);
            AfterPlaylistModified(0);
            PlaylistEventListener?.Execute(null, new PlaylistEventArgs(PlaylistEventType.RemoveMusic) { Music = music });
        }

        public int RemoveMusic(MusicView music, bool alterIndex = true, 
                               bool notifyListener = true, bool notifyUser = false)
        {
            int index = music.Index == -1 ? CurrentPlaylist.IndexOf(music) : music.Index;
            if (index == -1) return index;
            removedMusic = music;
            CurrentPlaylist.RemoveAt(index);
            if (IsNowPlaying)
            {
                doNotListenToMusicPlayer = true;
                MusicPlayer.RemoveMusic(index);
                doNotListenToMusicPlayer = false;
            }
            if (alterIndex)
            {
                AfterPlaylistModified(index);
            }
            if (notifyListener)
            {
                PlaylistEventListener?.Execute(null, new PlaylistEventArgs(PlaylistEventType.RemoveMusic) { Music = music.FromVO() });
            }
            if (notifyUser)
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
                AfterPlaylistModified(index);
                PlaylistEventListener?.Execute(null, new PlaylistEventArgs(PlaylistEventType.AddMusic) { Music = music.FromVO() });
            }
        }

        private void RemoveMusic(IEnumerable<MusicView> playlist)
        {
            int minIndex = CurrentPlaylist.Count, maxIndex = -1;
            foreach (var music in playlist)
            {
                int index = RemoveMusic(music, false);
                minIndex = Math.Min(minIndex, index);
                maxIndex = Math.Max(maxIndex, index);
            }
            AfterPlaylistModified(minIndex, maxIndex);
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
            int index = MusicPlayer.CurrentIndex;
            if (SongsListView.IsLoaded)
            {
                await SongsListView.ScrollToIndexAsync(index);
            }
            else
            {
                ScrollToMusicRequestedWhenUnloaded = index;
            }
        }

        private async void SwipeControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Removable) (sender as SwipeControl).RightItems = null;
            if (ScrollToMusicRequestedWhenUnloaded != -1)
            {
                await SongsListView.ScrollToIndexAsync(ScrollToMusicRequestedWhenUnloaded);
                ScrollToMusicRequestedWhenUnloaded = -1;
            }
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
            MusicView musicView = CurrentPlaylist.FirstOrDefault(m => m == before);
            if (musicView == null) return;
            if (after.Index == -1)
            {
                // 避免被覆盖
                after.Index = musicView.Index;
            }
            musicView.CopyFrom(after);
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
                    DeleteMusic(args.MusicView.ToMusic());
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
                    if (removedMusic != null && removedMusic.Equals(music))
                    {
                        CurrentPlaylist.Insert(removedMusic.Index, music.ToVO());
                    }
                    break;
                case MusicEventType.Remove:
                    DeleteMusic(music);
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
                    CurrentPlaylist.Insert(args.Index, args.Music.ToVO(args.Index));
                    AfterPlaylistModified(args.Index);
                    break;
                case MusicPlayerEventType.Remove:
                    CurrentPlaylist.RemoveAt(args.Index);
                    AfterPlaylistModified(args.Index);
                    break;
                case MusicPlayerEventType.Clear:
                    CurrentPlaylist.Clear();
                    break;
                case MusicPlayerEventType.Move:
                    MusicPlayerMoveEventArgs moveArgs = (MusicPlayerMoveEventArgs) args;
                    CurrentPlaylist.Move(moveArgs.Index, moveArgs.ToIndex);
                    AfterPlaylistModified(moveArgs.Index, moveArgs.ToIndex);
                    break;
                case MusicPlayerEventType.Switch:
                    await Helper.RunInMainUIThread(Dispatcher, () => MusicPlayer.SetMusicPlaying(CurrentPlaylist, args.Music));
                    break;
            }
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
