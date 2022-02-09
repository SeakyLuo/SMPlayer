using SMPlayer.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using SMPlayer.Interfaces;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class PlaylistControlItem : UserControl, IMusicPlayerEventListener
    {
        public bool ShowAlbumText
        {
            get => (bool)GetValue(ShowAlbumTextProperty);
            set
            {
                SetValue(ShowAlbumTextProperty, value);
                TitleColumnDefinition.Width = new GridLength(10, GridUnitType.Star);
                if (!value)
                {
                    TitleColumnDefinition.Width = new GridLength(10, GridUnitType.Star);
                }
                AlbumTextButton.Visibility = LongArtistAlbumPanelDot.Visibility = LongArtistTextButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty ShowAlbumTextProperty = DependencyProperty.Register("ShowAlbumText", typeof(bool), typeof(PlaylistControlItem), new PropertyMetadata(true));

        public MusicView Data { get; set; }
        public PlaylistControlItem()
        {
            this.InitializeComponent();
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        private void Album_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), Data.Album);
        }
        private void Artist_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlayingFullPage.Instance != null) NowPlayingFullPage.Instance.GoBack();
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage), Data.Artist);
        }

        private bool TextColorChanged = true;
        public void SetTextColor(Music music)
        {
            if (Data == null) return;
            if (Data.Index == -1 ? Data.Equals(music) : Data.Index == MusicPlayer.CurrentIndex)
            {
                PlayingIcon.Visibility = Visibility.Visible;
                TitleTextBlock.Foreground = ArtistTextButton.Foreground = AlbumTextButton.Foreground = DurationTextBlock.Foreground =
                LongArtistTextButton.Foreground = LongArtistAlbumPanelDot.Foreground = LongAlbumTextButton.Foreground = ColorHelper.HighlightBrush;
                TextColorChanged = true;
            }
            else if (TextColorChanged)
            {
                PlayingIcon.Visibility = Visibility.Collapsed;
                if (ActualTheme == ElementTheme.Dark)
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

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            SetTextColor(MusicPlayer.CurrentMusic);
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicPlayerEventType.Switch:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetTextColor(args.Music));
                    break;
            }
        }
    }
}
