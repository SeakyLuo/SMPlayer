using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class PlaylistControl : UserControl, MusicSwitchingListener
    {
        private const string FILENAME = "NowPlayingPlaylist.json";
        public ObservableCollection<Music> CurrentPlaylist
        {
            get => currentPlaylist.Count == 0 ? MediaHelper.CurrentPlaylist : currentPlaylist;
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
        public bool AllowReorder
        {
            get => SongsListView.CanReorderItems;
            set
            {
                SongsListView.CanReorderItems = value;
                SongsListView.AllowDrop = value;
                SongsListView.CanDrag = value;
            }
        }
        public object Header
        {
            set => SongsListView.Header = value;
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(PlaylistControl), new PropertyMetadata(null));
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
        public PlaylistControl()
        {
            this.InitializeComponent();
            MediaHelper.MusicSwitchingListeners.Add(this as MusicSwitchingListener);
        }

        private void PlaylistController_Loading(FrameworkElement sender, object args)
        {
            CurrentTheme = Theme;
        }

        private void PlaylistController_Loaded(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null) ItemsSource = MediaHelper.CurrentPlaylist;
        }

        public static void AddMusicRequestListener(MusicRequestListener listener)
        {
            MusicRequestListeners.Add(listener);
        }

        private void SongsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (AlternatingRowColor)
                args.ItemContainer.Background = args.ItemIndex % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
        }

        private void SongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Music music = (Music)e.ClickedItem;
            MediaHelper.SetMusicAndPlay(CurrentPlaylist, music);
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MediaHelper.FindMusicAndSetPlaying(CurrentPlaylist, current, next));
        }

        private void SongsListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            int from = -1, to = 0;
            for (int i = 0; i < sender.Items.Count; i++)
            {
                var container = sender.ContainerFromIndex(i) as ListViewItem;
                container.Background = i % 2 == 0 ? Helper.WhiteSmokeBrush : Helper.WhiteBrush;
                if (!MediaHelper.CurrentPlaylist[i].Equals(args.Items[0]))
                {
                    if (from < 0) from = i;
                    else to = i;
                }
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
                    Text = "Move To Top",
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
    }
}
