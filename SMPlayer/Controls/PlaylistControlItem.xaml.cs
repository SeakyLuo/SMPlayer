using SMPlayer.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class PlaylistControlItem : UserControl, SwitchMusicListener
    {
        public bool ShowAlbumText
        {
            get => (bool)GetValue(ShowAlbumTextProperty);
            set
            {
                SetValue(ShowAlbumTextProperty, value);
                AlbumTextButton.Visibility = LongArtistAlbumPanelDot.Visibility = LongArtistTextButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty ShowAlbumTextProperty = DependencyProperty.Register("ShowAlbumText", typeof(bool), typeof(PlaylistControlItem), new PropertyMetadata(true));

        public Music Data { get; set; }
        public PlaylistControlItem()
        {
            this.InitializeComponent();
        }

        private void Album_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            var playlist = new System.Collections.ObjectModel.ObservableCollection<Music>();
            foreach (var music in MusicLibraryPage.AllSongs)
                if (music.Album == Data.Album)
                    playlist.Add(music);
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), new AlbumView(Data.Album, Data.Artist)
            {
                Songs = playlist,
            });
        }
        private void Artist_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage), Data.Artist);
        }

        private bool TextColorChanged = true;
        public void SetTextColor(Music music)
        {
            if (Data == music)
            {
                TitleTextBlock.Foreground = ArtistTextButton.Foreground = AlbumTextButton.Foreground = DurationTextBlock.Foreground =
                LongArtistTextButton.Foreground = LongArtistAlbumPanelDot.Foreground = LongAlbumTextButton.Foreground = ColorHelper.HighlightBrush;
                TextColorChanged = true;
            }
            else if (TextColorChanged)
            {
                if (RequestedTheme == ElementTheme.Dark)
                {
                    TitleTextBlock.Foreground = ColorHelper.WhiteBrush;
                    ArtistTextButton.Foreground = AlbumTextButton.Foreground = DurationTextBlock.Foreground =
                    LongArtistTextButton.Foreground = LongArtistAlbumPanelDot.Foreground = LongAlbumTextButton.Foreground = ColorHelper.GrayBrush;
                }
                else
                {
                    TitleTextBlock.Foreground = ArtistTextButton.Foreground = AlbumTextButton.Foreground = DurationTextBlock.Foreground =
                    LongArtistTextButton.Foreground = LongArtistAlbumPanelDot.Foreground = LongAlbumTextButton.Foreground = ColorHelper.BlackBrush;
                }
                TextColorChanged = false;
            }
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetTextColor(next));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MediaHelper.SwitchMusicListeners.Add(this);
            SetTextColor(MediaHelper.CurrentMusic);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MediaHelper.SwitchMusicListeners.Remove(this);
        }
    }
}
