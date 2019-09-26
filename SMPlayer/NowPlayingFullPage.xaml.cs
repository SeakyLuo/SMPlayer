using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingFullPage : Page, MediaControlContainer, SwitchMusicListener, MusicRequestListener, MusicControlListener
    {
        public static NowPlayingFullPage Instance { get => (Window.Current.Content as Frame).Content as NowPlayingFullPage; }
        private MusicProperties musicProperties;
        private string Lyrics = "";
        private Music CurrentMusic;
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
            MediaControl.AddMusicRequestListener(this as MusicRequestListener);
            MediaControl.AddMusicControlListener(this as MusicControlListener);
            MediaHelper.SwitchMusicListeners.Add(this as SwitchMusicListener);

            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += (sender, args) => UpdateTitleBarLayout(sender);
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += (sender, args) => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarHelper.SetDarkTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            FullMediaControl.Update();
            SetMusic(MediaHelper.CurrentMusic);
            FullPlaylistControl.ScrollToMusic(MediaHelper.CurrentMusic);
        }
        private void UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        public void SetMusic(Music music)
        {
            SetMusicInfo(music);
            SetLyrics(music);
            CurrentMusic = music;
        }

        public void SetPlayCount(Music music)
        {
            if (music.PlayCount == 0)
            {
                PlayCountTextBox.Text = "";
                ClearPlayCountButton.Visibility = Visibility.Collapsed;
                ToolTipService.SetToolTip(PlayCountTextBox, new ToolTip() { Content = $"{music.Name} has not been played yet." });
            }
            else
            {
                PlayCountTextBox.Text = music.PlayCount.ToString();
                ClearPlayCountButton.Visibility = Visibility.Visible;
                string times = MusicDurationConverter.TryPlural("time", music.PlayCount);
                ToolTipService.SetToolTip(PlayCountTextBox, new ToolTip() { Content = $"{music.Name} has been played {music.PlayCount} {times}." });
            }
        }

        public void SetMusicProperties(MusicProperties properties)
        {
            TitleTextBox.Text = musicProperties.Title;
            SubtitleTextBox.Text = musicProperties.Subtitle;
            ArtistTextBox.Text = musicProperties.Artist;
            AlbumTextBox.Text = musicProperties.Album;
            AlbumArtistTextBox.Text = musicProperties.AlbumArtist;
            SetPlayCount(CurrentMusic);
            PublisherTextBox.Text = musicProperties.Publisher;
            TrackNumberTextBox.Text = IntConverter.ToStr((int)musicProperties.TrackNumber);
            YearTextBox.Text = IntConverter.ToStr((int)musicProperties.Year);
            BitRateTextBox.Text = musicProperties.Bitrate.ToString();
            ComposersTextBox.Text = string.Join(", ", musicProperties.Composers);
            DurationTextBox.Text = MusicDurationConverter.ToTime(musicProperties.Duration.TotalSeconds);
            GenreTextBox.Text = string.Join(", ", musicProperties.Genre);
            ProducersTextBox.Text = string.Join(", ", musicProperties.Producers);
        }

        private void ResetMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            SetMusicProperties(musicProperties);
            ShowNotification("Properties Reset!");
        }

        private async void SaveMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            musicProperties.Title = TitleTextBox.Text;
            musicProperties.Subtitle = SubtitleTextBox.Text;
            musicProperties.Artist = ArtistTextBox.Text;
            musicProperties.Album = ArtistTextBox.Text;
            musicProperties.AlbumArtist = AlbumArtistTextBox.Text;
            AlbumArtistTextBox.Text = musicProperties.AlbumArtist;
            if (int.TryParse(PlayCountTextBox.Text, out int PlayCount))
            {
                MusicLibraryPage.AllSongs[MusicLibraryPage.AllSongs.IndexOf(CurrentMusic)].PlayCount = PlayCount;
                CurrentMusic.PlayCount = PlayCount;
            }
            musicProperties.Publisher = PublisherTextBox.Text;
            if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber))
                musicProperties.TrackNumber = TrackNumber;
            if (uint.TryParse(YearTextBox.Text, out uint Year))
                musicProperties.Year = Year;
            await musicProperties.SavePropertiesAsync();
            ShowNotification("Lyrics Updated!");
        }

        public async void SetBasicProperties(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            FileSizeTextBox.Text = basicProperties.Size.ToString() + " Bytes";
            DateCreatedTextBox.Text = file.DateCreated.ToLocalTime().ToString();
            DateModifiedTextBox.Text = basicProperties.DateModified.ToLocalTime().ToString();
            PathTextBox.Text = file.Path;
        }

        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            LyricsTextBox.Text = Lyrics;
            ShowNotification("Lyrics Reset!");
        }

        private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                SavingLyricsProgressRing.Visibility = Visibility.Visible;
                LyricsTextBox.IsEnabled = false;
                var music = await CurrentMusic.GetStorageFileAsync();
                using (var file = TagLib.File.Create(new MusicFileAbstraction(music), TagLib.ReadStyle.Average))
                {
                    Lyrics = LyricsTextBox.Text;
                    file.Tag.Lyrics = Lyrics;
                    file.Save();
                }
                SavingLyricsProgressRing.Visibility = Visibility.Collapsed;
                LyricsTextBox.IsEnabled = true;
                ShowNotification("Lyrics Updated!");
            });
        }

        private void SearchLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotification("Searching Lyrics!");
        }

        public void ShowNotification(string text)
        {
            ShowResultInAppNotification.Content = text;
            ShowResultInAppNotification.Show(1500);
        }

        public void PauseMusic()
        {
            FullMediaControl.PauseMusic();
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                // if being modified and not saved
                if (musicProperties.Title != TitleTextBox.Text) return;
                if (musicProperties.Subtitle != SubtitleTextBox.Text) return;
                if (musicProperties.Artist != ArtistTextBox.Text) return;
                if (musicProperties.AlbumArtist != AlbumArtistTextBox.Text) return;
                if (musicProperties.Publisher != PublisherTextBox.Text) return;
                if (Lyrics != LyricsTextBox.Text) return;
                if (int.TryParse(PlayCountTextBox.Text, out int PlayCount) && CurrentMusic.PlayCount != PlayCount) return;
                if (TrackNumberTextBox.Text == "" && musicProperties.TrackNumber != 0) return;
                if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber) && musicProperties.TrackNumber != TrackNumber) return;
                if (YearTextBox.Text == "" && musicProperties.Year != 0) return;
                if (uint.TryParse(YearTextBox.Text, out uint Year) && musicProperties.Year != Year) return;
                SetMusic(next);
            });
        }

        public async void SetMusicInfo(Music music)
        {
            if (music == null || music.Equals(CurrentMusic)) return;
            var file = await StorageFile.GetFileFromPathAsync(music.Path);
            SetBasicProperties(file);
            SetMusicProperties(musicProperties = await music.GetMusicPropertiesAsync());
        }

        public void PlaylistRequested(ICollection<Music> playlist)
        {
            PlaylistBladeItem.StartBringIntoView();
        }
        public void MusicInfoRequested(Music music)
        {
            SetMusicInfo(music);
            MusicPropertyBladeItem.StartBringIntoView();
        }

        public async void SetLyrics(Music music)
        {
            if (music.Equals(CurrentMusic)) return;
            var lyrics = await music.GetLyricsAsync();
            LyricsTextBox.Text = string.IsNullOrEmpty(lyrics) ? "" : lyrics;
            Lyrics = LyricsTextBox.Text;
        }

        public void LyricsRequested(Music music)
        {
            SetLyrics(music);
            LyricsBladeItem.StartBringIntoView();
        }

        private void CheckIfDigit(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        public void MusicModified(Music before, Music after)
        {
            if (!before.Equals(CurrentMusic)) return;
            SetPlayCount(after);
            CurrentMusic.PlayCount = after.PlayCount;
        }

        private void ClearPlayCountButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMusic.PlayCount = 0;
            SetPlayCount(CurrentMusic);
            MusicLibraryPage.AllSongs.First((m) => m.Equals(CurrentMusic)).PlayCount = 0;
        }

        public void GoBack()
        {
            if (Frame.CanGoBack) Frame.GoBack();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
    }
}
