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
    public sealed partial class PlaylistControl : UserControl, ISwitchMusicListener, IMenuFlyoutItemClickListener, IMultiSelectListener
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
        
        private ObservableCollection<Music> currentPlaylist = new ObservableCollection<Music>();
        public ObservableCollection<Music> CurrentPlaylist
        {
            get => IsNowPlaying ? MediaHelper.CurrentPlaylist :
                                  currentPlaylist.Count == 0 ? currentPlaylist = ItemsSource : currentPlaylist;
            set => currentPlaylist = value;
        }

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
            get => SongsListView.ItemsSource as ObservableCollection<Music>;
            set => SongsListView.ItemsSource = CurrentPlaylist = value;
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
        private int dragIndex, dropIndex, removedMusicIndex;

        public PlaylistControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
            MediaControl.AddMusicModifiedListener(CopyMusic);
            MusicInfoControl.MusicModifiedListeners.Add(CopyMusic);
        }

        private void CopyMusic(Music before, Music after)
        {
            CurrentPlaylist.FirstOrDefault(m => m == before)?.CopyFrom(after);
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null) ItemsSource = MediaHelper.CurrentPlaylist;
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

        private void AlternateRowBackgroud(int start)
        {
            if (!AlternatingRowColor) return;
            for (int i = start; i < CurrentPlaylist.Count; i++)
                if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
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
            MediaHelper.SetMusicAndPlay(CurrentPlaylist, music);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MediaHelper.FindMusicAndSetPlaying(CurrentPlaylist, current, next));
        }

        public void CancelMusicRemoval(int index, Music music)
        {
            if (IsNowPlaying)
            {
                MediaHelper.AddMusic(music, index);
            }
            else
            {
                currentPlaylist.Insert(index, music);
            }
            AlternateRowBackgroud(index);
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (!AllowReorder) return;
            Music music = args.Items[0] as Music;
            dropIndex = CurrentPlaylist.FindIndex(m => m == music && m.Index == music.Index);
            if (dragIndex == dropIndex) return;
            if (IsNowPlaying) MediaHelper.MoveMusic(dragIndex, dropIndex, false);
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
                RemoveMusic(music);
            }
            else
            {
                dialog.Confirm = () => RemoveMusic(music);
                dialog.Message = Helper.LocalizeMessage("RemoveMusic", music.Name);
                await dialog.ShowAsync();
            }
        }

        private void RemoveMusic(Music music, bool showNotification = true)
        {
            removedMusicIndex = IsNowPlaying ?  music.Index : currentPlaylist.IndexOf(music);
            if (IsNowPlaying ? MediaHelper.RemoveMusic(music) : currentPlaylist.Remove(music))
            {
                AlternateRowBackgroud(removedMusicIndex);
                foreach (var listener in RemoveListeners) listener.MusicRemoved(removedMusicIndex, music, CurrentPlaylist);
                if (showNotification)
                    Helper.ShowCancelableNotification(Helper.LocalizeMessage("MusicRemoved", music.Name), () => CancelMusicRemoval(removedMusicIndex, music));
            }
        }

        private void RemoveMusic(IEnumerable<Music> playlist, bool showNotification = true)
        {
            if (IsNowPlaying)
            {
                foreach (var music in playlist)
                {
                    removedMusicIndex = MediaHelper.CurrentPlaylist.IndexOf(music);
                    if (MediaHelper.RemoveMusic(music))
                    {
                        foreach (var listener in RemoveListeners)
                            listener.MusicRemoved(removedMusicIndex, music, MediaHelper.CurrentPlaylist);
                    }
                }
            }
            else
            {
                foreach (var music in playlist)
                {
                    removedMusicIndex = currentPlaylist.IndexOf(music);
                    if (removedMusicIndex == -1) continue;
                    currentPlaylist.RemoveAt(removedMusicIndex);
                    foreach (var listener in RemoveListeners) listener.MusicRemoved(removedMusicIndex, music, currentPlaylist);
                }
            }
            AlternateRowBackgroud(0);
            if (showNotification)
                Helper.ShowNotification("MusicListRemoved");
        }

        private void FavoriteItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var music = args.SwipeControl.DataContext as Music;
            if (music.Favorite) Settings.settings.DislikeMusic(music);
            else Settings.settings.LikeMusic(music);
        }

        private int ScrollToMusicRequestedWhenUnloaded = -1;

        public async void ScrollToCurrentMusic(bool showNotification = false)
        {
            if (!ScrollToMusic(MediaHelper.CurrentMusic, false))
            {
                Windows.UI.Xaml.Data.LoadMoreItemsResult result = await SongsListView.LoadMoreItemsAsync();
                ScrollToMusic(MediaHelper.CurrentMusic, showNotification);
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
                ScrollToMusicRequestedWhenUnloaded = index;
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

        void IMenuFlyoutItemClickListener.AddTo(object data, object collection, int index, AddToCollectionType type)
        {
            if (type == AddToCollectionType.NowPlaying && IsNowPlaying)
            {
                AlternateRowBackgroud(index);
            }
        }

        void IMenuFlyoutItemClickListener.Delete(Music music)
        {
            RemoveMusic(music, false);
        }

        void IMenuFlyoutItemClickListener.Remove(Music music)
        {
            AskRemoveMusic(music);
        }

        void IMenuFlyoutItemClickListener.Favorite(object data) { }

        void IMenuFlyoutItemClickListener.Select(object data)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            SongsListView.SelectedItems.Add(data as Music);
        }

        void IMenuFlyoutItemClickListener.UndoDelete(Music music)
        {
            if (IsNowPlaying) return;
            currentPlaylist.Insert(removedMusicIndex, music);
            AlternateRowBackgroud(removedMusicIndex);
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

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            SelectionMode = ListViewSelectionMode.None;
            MultiSelectListener?.Cancel(commandBar);
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            helper.Data = SelectedItems;
            MultiSelectListener?.AddTo(commandBar, helper);
        }

        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar)
        {
            if (SelectedItemsCount == 0) return;
            MediaHelper.SetPlaylistAndPlay(SelectedItems);
            MultiSelectListener?.Play(commandBar);
        }

        async void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar)
        {
            if (SongsListView.SelectedItems.Count == 0)
            {
                return;
            }
            if (SongsListView.SelectedItems.Count == 1)
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
            MultiSelectListener?.Remove(commandBar);
        }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            SongsListView.SelectAll();
            MultiSelectListener?.SelectAll(commandBar);
        }

        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar)
        {
            SongsListView.ClearSelections();
            MultiSelectListener?.ClearSelections(commandBar);
        }

        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar)
        {
            SongsListView.ReverseSelections();
            MultiSelectListener?.ReverseSelections(commandBar);
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
