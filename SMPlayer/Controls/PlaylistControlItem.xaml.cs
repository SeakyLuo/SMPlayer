using SMPlayer.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class PlaylistControlItem : UserControl
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
        public PlaylistControlItem()
        {
            this.InitializeComponent();
        }

        private void Album_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            var data = DataContext as Music;
            var playlist = new System.Collections.ObjectModel.ObservableCollection<Music>();
            foreach (var music in MusicLibraryPage.AllSongs)
                if (music.Album == data.Album)
                    playlist.Add(music);
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), new AlbumView(data.Album, data.Artist, false)
            {
                Songs = playlist,
            });
        }
        private void Artist_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage), (sender as HyperlinkButton).Content);
        }
    }
}
