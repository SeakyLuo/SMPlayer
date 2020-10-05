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
        public bool IsNowPlaying { get; set; }
        private ObservableCollection<Music> currentPlaylist = new ObservableCollection<Music>();
        public ObservableCollection<Music> CurrentPlaylist
        {
            get => IsNowPlaying ? MediaHelper.CurrentPlaylist :
                                  currentPlaylist.Count == 0 ? currentPlaylist = ItemsSource : currentPlaylist;
            set => currentPlaylist = value;
        }

        private static List<IMusicRequestListener> MusicRequestListeners = new List<IMusicRequestListener>();
        public bool AlternatingRowColor { get; set; }
        public IPlaylistScrollListener ScrollListener;
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
        public bool AllowMultipleSection
        {
            get => (bool)GetValue(AllowMultipleSectionProperty);
            set => SetValue(AllowMultipleSectionProperty, value);
        }
        public static readonly DependencyProperty AllowMultipleSectionProperty = DependencyProperty.Register("AllowMultipleSection", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(false));
        public ScrollViewer ScrollViewer
        {
            get => SongsListView.GetFirstDescendantOfType<ScrollViewer>();
        }
        public ListViewSelectionMode SelectionMode
        {
            get => SongsListView.SelectionMode;
            set => SongsListView.SelectionMode = value;
        }
        public IEnumerable<Music> SelectedItems
        {
            get
            {
                var list = new List<Music>();
                foreach (Music music in SongsListView.SelectedItems)
                {
                    list.Add(music);
                }
                return list;
            }
        }
        public MultiSelectCommandBarOption MultiSelectOption { get; set; }
        public List<RemoveMusicListener> RemoveListeners = new List<RemoveMusicListener>();
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
            Helper.GetMainPageContainer().GetMultiSelectCommandBar().MultiSelectListener = this;
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
            if (SelectionMode == ListViewSelectionMode.Multiple)
            {

            }
            else
            {
                Music music = (Music)e.ClickedItem;
                MediaHelper.SetMusicAndPlay(CurrentPlaylist, music);
            }
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
            for (int i = index; i < CurrentPlaylist.Count; i++)
                if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (!AllowReorder) return;
            Music music = args.Items[0] as Music;
            dropIndex = CurrentPlaylist.FindIndex(m => m == music && m.Index == music.Index);
            if (dragIndex == dropIndex) return;
            if (IsNowPlaying) MediaHelper.MoveMusic(dragIndex, dropIndex);
            for (int i = Math.Min(dragIndex, dropIndex); i <= Math.Max(dragIndex, dropIndex); i++)
                if (AlternatingRowColor && sender.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            MenuFlyoutOption option = new MenuFlyoutOption() { MultiSelectOption = MultiSelectOption };
            if (Removable) MenuFlyoutHelper.SetRemovableMusicMenu(sender, this, null, option);
            else MenuFlyoutHelper.SetMusicMenu(sender, this, null, option);
            if (AllowReorder)
            {
                var flyout = sender as MenuFlyout;
                var item = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Move To Top"),
                    Icon = new SymbolIcon(Symbol.Upload)
                };
                item.Click += (s, args) =>
                {
                    Music music = (s as MenuFlyoutItem).DataContext as Music;
                    MediaHelper.MoveMusic(music.Index, 0);
                };
                flyout.Items.Add(item);
            }
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
                if (AlternatingRowColor)
                {
                    for (int i = removedMusicIndex; i < CurrentPlaylist.Count; i++)
                        if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                            container.Background = GetRowBackground(i);
                }
                foreach (var listener in RemoveListeners) listener.MusicRemoved(removedMusicIndex, music, CurrentPlaylist);
                if (showNotification)
                    Helper.ShowCancelableNotification(Helper.LocalizeMessage("MusicRemoved", music.Name), () => CancelMusicRemoval(removedMusicIndex, music));
            }
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

        void IMenuFlyoutItemClickListener.Delete(Music music)
        {
            RemoveMusic(music, false);
        }

        void IMenuFlyoutItemClickListener.Remove(Music music)
        {
            AskRemoveMusic(music);
        }

        void IMenuFlyoutItemClickListener.Favorite(object data)
        {

        }

        void IMenuFlyoutItemClickListener.Select(Music music)
        {
            if (SelectionMode != ListViewSelectionMode.Multiple)
            {
                SelectionMode = ListViewSelectionMode.Multiple;
            }
            SongsListView.SelectedItems.Add(music);
        }

        void IMenuFlyoutItemClickListener.UndoDelete(Music music)
        {
            if (IsNowPlaying) return;
            currentPlaylist.Insert(removedMusicIndex, music);
            for (int i = removedMusicIndex; i < CurrentPlaylist.Count; i++)
                if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }

        private void SongsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            dragIndex = (e.Items[0] as Music).Index;
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            SelectionMode = ListViewSelectionMode.None;
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            helper.Data = SelectedItems;
        }

        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar)
        {
            MediaHelper.SetPlaylistAndPlay(SelectedItems);
            SelectionMode = ListViewSelectionMode.None;
        }

        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar)
        {
            //var playlist = CurrentPlaylist;
            //foreach (Music music in SongsListView.SelectedItems)
            //{
            //    playlist.Remove(music);
            //}
            //SelectionMode = ListViewSelectionMode.None;
        }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            SongsListView.SelectAll();
        }

        void IMultiSelectListener.ClearSelection(MultiSelectCommandBar commandBar)
        {
            SongsListView.SelectedItems.Clear();
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
