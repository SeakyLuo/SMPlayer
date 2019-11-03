using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class PlaylistControl : UserControl, SwitchMusicListener, RemoveMusicListener
    {
        public bool IsNowPlaying { get; set; }
        public ObservableCollection<Music> CurrentPlaylist
        {
            get => IsNowPlaying ? MediaHelper.CurrentPlaylist :
                                  currentPlaylist.Count == 0 ? currentPlaylist = ItemsSource as ObservableCollection<Music> : currentPlaylist;
            set => currentPlaylist = value;
        }
        private ObservableCollection<Music> currentPlaylist = new ObservableCollection<Music>();
        public ElementTheme Theme
        {
            get => SongsListView.RequestedTheme;
            set => SongsListView.RequestedTheme = value;
        }
        public static ElementTheme CurrentTheme;
        private static List<MusicRequestListener> MusicRequestListeners = new List<MusicRequestListener>();
        public bool AlternatingRowColor { get; set; }
        public PlaylistScrollListener ScrollListener;
        public bool AllowReorder
        {
            get => SongsListView.CanReorderItems;
            set => SongsListView.CanReorderItems = SongsListView.AllowDrop = SongsListView.CanDrag = value;
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

        public object ItemsSource
        {
            get => SongsListView.ItemsSource;
            set
            {
                CurrentPlaylist = new ObservableCollection<Music>(value as ICollection<Music>);
                SongsListView.ItemsSource = value;
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
        public bool ShowAlbumText
        {
            get => (bool)GetValue(ShowAlbumTextProperty);
            set => SetValue(ShowAlbumTextProperty, value);
        }
        public static readonly DependencyProperty ShowAlbumTextProperty = DependencyProperty.Register("ShowAlbumText", typeof(bool), typeof(PlaylistControl), new PropertyMetadata(true));

        public ScrollViewer ScrollViewer
        {
            get => SongsListView.GetFirstDescendantOfType<ScrollViewer>();
        }
        public List<RemoveMusicListener> RemoveListeners = new List<RemoveMusicListener>();
        private Dialogs.RemoveDialog dialog;

        public PlaylistControl()
        {
            this.InitializeComponent();
            MediaHelper.SwitchMusicListeners.Add(this);
            MediaHelper.RemoveMusicListeners.Add(this);
        }

        private void PlaylistController_Loading(FrameworkElement sender, object args)
        {
            CurrentTheme = Theme;
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null) ItemsSource = MediaHelper.CurrentPlaylist;
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
                    if (ScrollListener != null) ScrollListener.Scrolled(ScrollPosition, viewer.VerticalOffset);
                    ScrollPosition = viewer.VerticalOffset;
                };
                ViewChangedUnadded = false;
            }
        }

        public static void AddMusicRequestListener(MusicRequestListener listener)
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
            Music music = (Music)e.ClickedItem;
            Helper.GetNotificationContainer().ShowAddMusicResultNotification(CurrentPlaylist, music);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MediaHelper.FindMusicAndSetPlaying(CurrentPlaylist, current, next));
        }

        public async void MusicRemoved(int index, Music music, ICollection<Music> newCollection)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.FindMusicAndSetPlaying(CurrentPlaylist, null, music);
                if (AlternatingRowColor)
                {
                    for (int i = index; i < MediaHelper.CurrentPlaylist.Count; i++)
                        if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                            container.Background = GetRowBackground(i);
                }
            });
        }

        private List<Music> beforeDragging;

        private void SongsListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            beforeDragging = MediaHelper.CurrentPlaylist.ToList();
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            int from = -1, to = -1;
            for (int i = 0; i < sender.Items.Count; i++)
            {
                if (AlternatingRowColor && sender.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
                if (!MediaHelper.CurrentPlaylist[i].Equals(beforeDragging[i]))
                {
                    if (from < 0) from = i;
                    else to = i;
                }
            }
            if (from == -1 || to == -1) return;
            if (beforeDragging[to].Equals(MediaHelper.CurrentPlaylist[from]))
            {
                int temp = from;
                from = to;
                to = temp;
            }
            MediaHelper.MoveMusic(from, to);
        }
        private void OpenMusicMenuFlyout(object sender, object e)
        {
            MenuFlyoutHelper.SetRemovableMusicMenu(sender);
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
                    MediaHelper.MoveMusic(music, 0);
                };
                flyout.Items.Add(item);
            }
        }

        private async void RemoveItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var music = args.SwipeControl.DataContext as Music;
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

        private void RemoveMusic(Music music)
        {
            int index = CurrentPlaylist.IndexOf(music);
            if (IsNowPlaying ? MediaHelper.RemoveMusic(index) : CurrentPlaylist.Remove(music))
            {
                if (AlternatingRowColor)
                {
                    for (int i = index; i < CurrentPlaylist.Count; i++)
                        if (SongsListView.ContainerFromIndex(i) is ListViewItem container)
                            container.Background = GetRowBackground(i);
                }
                foreach (var listener in RemoveListeners) listener.MusicRemoved(index, music, CurrentPlaylist);
            }
        }

        private void FavoriteItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            var music = args.SwipeControl.DataContext as Music;
            if (music.Favorite) Settings.settings.DislikeMusic(music);
            else Settings.settings.LikeMusic(music);
        }

        private int ScrollToMusicRequestedWhenUnloaded = -1;
        public void ScrollToMusic(Music music)
        {
            if (SongsListView.IsLoaded)
                SongsListView.ScrollIntoView(music);
            else
                ScrollToMusicRequestedWhenUnloaded = CurrentPlaylist.IndexOf(music);
        }

        private void SwipeControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ScrollToMusicRequestedWhenUnloaded != -1 && SongsListView.ContainerFromIndex(ScrollToMusicRequestedWhenUnloaded) is ListViewItem container)
            {
                container.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true });
                ScrollToMusicRequestedWhenUnloaded = -1;
            }
        }
    }

    public interface PlaylistScrollListener
    {
        void Scrolled(double before, double after);
    }

    public enum ScrollDirection
    {
        None = 0, Up = 1, Down = 2
    }
}
