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
    public sealed partial class NowPlayingFullPage : Page, MediaControlContainer, MusicSwitchingListener, MusicRequestListener
    {
        public static NowPlayingFullPage Instance { get => (Window.Current.Content as Frame).Content as NowPlayingFullPage; }
        private MusicProperties musicProperties;
        private string Lyrics = "";
        private Music CurrentMusic;
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
            MediaControl.AddMusicRequestListener(this as MusicRequestListener);
            MediaHelper.MusicSwitchingListeners.Add(this as MusicSwitchingListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FullMediaControl.Update();
            SetMusic(MediaHelper.CurrentMusic);
        }

        public void SetMusic(Music music)
        {
            SetMusicInfo(MediaHelper.CurrentMusic);
            SetLyrics(MediaHelper.CurrentMusic);
            CurrentMusic = music;
        }

        public void SetMusicProperties(MusicProperties properties)
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
            //var mf = TagLib.File.Create(new TagLib.File.LocalFileAbstraction(CurrentMusic.Path), TagLib.ReadStyle.Average);
            //mf.Tag.Lyrics = Lyrics;
            //mf.Save();
            var file = await StorageFile.GetFileFromPathAsync(CurrentMusic.Path);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var mp3 = new Mp3(stream.AsStream(), Mp3Permissions.ReadWrite))
                {
                    var tag = mp3.GetTag(Id3TagFamily.Version2X);
                    tag.Lyrics[0] = Lyrics;
                    mp3.WriteTag(tag, tag.Version, WriteConflictAction.Replace);
                }
            }
            ShowNotification("Lyrics Updated!");
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

        public void SetShuffle(bool isShuffle)
        {
            FullMediaControl.SetShuffle(isShuffle);
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                // if being modified and not saved
                if (musicProperties.Title != TitleTextBox.Text) return;
                if (musicProperties.Subtitle != SubtitleTextBox.Text) return;
                if (musicProperties.Artist != ArtistTextBox.Text) return;
                if (musicProperties.AlbumArtist != AlbumArtistTextBox.Text) return;
                if (musicProperties.Publisher != PublisherTextBox.Text) return;
                if (Lyrics != LyricsTextBox.Text) return;
                if (PlayCountTextBox.Text == "" && CurrentMusic.PlayCount != 0) return;
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
            SetMusicProperties(musicProperties = await music.GetMusicProperties());
        }
        public void MusicInfoRequested(Music music)
        {
            SetMusicInfo(music);
            MusicPropertyBladeItem.StartBringIntoView();
        }

        public async void SetLyrics(Music music)
        {
            if (music.Equals(CurrentMusic)) return;
            var lyrics = await music.GetLyrics();
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
    }
}
