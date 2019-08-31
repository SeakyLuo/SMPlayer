using Id3;
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
    public sealed partial class NowPlayingFullPage : Page, MediaControlContainer, MediaControlListener, MusicRequestListener
    {
        public static NowPlayingFullPage Instance { get => (Window.Current.Content as Frame).Content as NowPlayingFullPage; }
        private MusicProperties musicProperties;
        private string Lyrics;
        private Id3Tag MusicTag;
        private Music CurrentMusic;
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
            MediaControl.AddMusicRequestListener(this as MusicRequestListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FullMediaControl.Update();
            PlaylistControl.SetPlaylist(MediaHelper.CurrentPlayList);
        }

        public void SetMusicProperty(MusicProperties properties)
        {
            TitleTextBox.Text = musicProperties.Title;
            SubtitleTextBox.Text = musicProperties.Subtitle;
            ArtistTextBox.Text = musicProperties.Artist;
            AlbumTextBox.Text = musicProperties.Album;
            AlbumArtistTextBox.Text = musicProperties.AlbumArtist;
            PlayCountTextBox.Text = IntConverter.ToStr(CurrentMusic.PlayCount);
            PublisherTextBox.Text = musicProperties.Publisher;
            TrackNumberTextBox.Text = IntConverter.ToStr((int)musicProperties.TrackNumber);
            YearTextBox.Text = IntConverter.ToStr((int)musicProperties.Year);
            BitRateTextBox.Text = musicProperties.Bitrate.ToString();
            ComposersTextBox.Text = string.Join(", ", musicProperties.Composers);
            DurationTextBox.Text = MusicDurationConverter.ToTime(musicProperties.Duration.TotalSeconds);
            GenreTextBox.Text = string.Join(", ", musicProperties.Genre);
            ProducersTextBox.Text = string.Join(", ", musicProperties.Producers);
        }

        private void ResetLyricsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            SetMusicProperty(musicProperties);
        }

        private async void SaveMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            await musicProperties.SavePropertiesAsync();
        }

        public async Task<Id3Tag> GetMusicTag(Music music)
        {
            var file = await StorageFile.GetFileFromPathAsync(music.Path);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var mp3 = new Mp3(stream.AsStream());
            return mp3.GetTag(Id3TagFamily.Version2X);
        }

        public void SetMusicAndPlay(Music music)
        {
            FullMediaControl.SetMusicAndPlay(music);
        }

        public void PauseMusic()
        {
            FullMediaControl.PauseMusic();
        }

        public void SetShuffle(bool isShuffle)
        {
            FullMediaControl.SetShuffle(isShuffle);
        }

        public void Tick()
        {
            return;
        }

        public void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            return;
        }

        public void MediaEnded()
        {
            return;
        }

        public void ShuffleChanged(IEnumerable<Music> newPlayList, bool isShuffle)
        {
            return;
        }

        public async void MusicInfoRequested(Music music)
        {
            var file = await StorageFile.GetFileFromPathAsync(music.Path);
            SetMusicProperty(musicProperties = await music.GetMusicProperties());
            MusicPropertyBladeItem.StartBringIntoView();
        }

        public async void LyricsRequested(Music music)
        {
            Lyrics = await music.GetLyrics();
            LyricsTextBox.Text = Lyrics;
            LyricsBladeItem.StartBringIntoView();
        }
    }
}
