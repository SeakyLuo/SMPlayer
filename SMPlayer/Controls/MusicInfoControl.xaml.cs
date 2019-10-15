using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicInfoControl : UserControl, SwitchMusicListener, MusicControlListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        private Music CurrentMusic;
        private MusicProperties Properties;
        public MusicInfoControl()
        {
            this.InitializeComponent();
            MediaControl.AddMusicControlListener(this);
            MediaHelper.SwitchMusicListeners.Add(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void SetMusicProperties(MusicProperties properties)
        {
            TitleTextBox.Text = Properties.Title;
            SubtitleTextBox.Text = Properties.Subtitle;
            ArtistTextBox.Text = Properties.Artist;
            AlbumTextBox.Text = Properties.Album;
            AlbumArtistTextBox.Text = Properties.AlbumArtist;
            SetPlayCount(CurrentMusic);
            PublisherTextBox.Text = Properties.Publisher;
            TrackNumberTextBox.Text = IntConverter.ToStr((int)Properties.TrackNumber);
            YearTextBox.Text = IntConverter.ToStr((int)Properties.Year);
            BitRateTextBox.Text = Properties.Bitrate.ToString();
            ComposersTextBox.Text = string.Join(", ", Properties.Composers);
            DurationTextBox.Text = MusicDurationConverter.ToTime(Properties.Duration.TotalSeconds);
            GenreTextBox.Text = string.Join(", ", Properties.Genre);
            ProducersTextBox.Text = string.Join(", ", Properties.Producers);
        }
        public async void SetMusicInfo(Music music)
        {
            if (music == null) return;
            CurrentMusic = music;
            var file = await music.GetStorageFileAsync();
            SetBasicProperties(file);
            SetMusicProperties(Properties = await music.GetMusicPropertiesAsync());
        }
        private void ClearPlayCountButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMusic.PlayCount = 0;
            SetPlayCount(CurrentMusic);
            MusicLibraryPage.AllSongs.First((m) => m == CurrentMusic).PlayCount = 0;
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

        private void ResetMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            SetMusicProperties(Properties);
            ((Window.Current.Content as Frame).Content as MediaControlContainer).ShowNotification("Properties Reset!");
        }

        private async void SaveMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Title = TitleTextBox.Text;
            Properties.Subtitle = SubtitleTextBox.Text;
            Properties.Artist = ArtistTextBox.Text;
            Properties.Album = ArtistTextBox.Text;
            Properties.AlbumArtist = AlbumArtistTextBox.Text;
            AlbumArtistTextBox.Text = Properties.AlbumArtist;
            if (int.TryParse(PlayCountTextBox.Text, out int PlayCount))
            {
                MusicLibraryPage.AllSongs[MusicLibraryPage.AllSongs.IndexOf(CurrentMusic)].PlayCount = PlayCount;
                CurrentMusic.PlayCount = PlayCount;
            }
            Properties.Publisher = PublisherTextBox.Text;
            if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber))
                Properties.TrackNumber = TrackNumber;
            if (uint.TryParse(YearTextBox.Text, out uint Year))
                Properties.Year = Year;
            await Properties.SavePropertiesAsync();
            Helper.GetMediaControlContainer().ShowNotification("Properties Updated!");
        }
        public async void SetBasicProperties(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            FileSizeTextBox.Text = basicProperties.Size.ToString() + " Bytes";
            DateCreatedTextBox.Text = file.DateCreated.ToLocalTime().ToString();
            DateModifiedTextBox.Text = basicProperties.DateModified.ToLocalTime().ToString();
            PathTextBox.Text = file.Path;
        }
        private void CheckIfDigit(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
        public void MusicModified(Music before, Music after)
        {
            if (before != CurrentMusic) return;
            SetPlayCount(after);
            CurrentMusic.PlayCount = after.PlayCount;
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (!AllowMusicSwitching) return;
                // if being modified and not saved
                if (Properties.Title != TitleTextBox.Text) return;
                if (Properties.Subtitle != SubtitleTextBox.Text) return;
                if (Properties.Artist != ArtistTextBox.Text) return;
                if (Properties.AlbumArtist != AlbumArtistTextBox.Text) return;
                if (Properties.Publisher != PublisherTextBox.Text) return;
                if (int.TryParse(PlayCountTextBox.Text, out int PlayCount) && CurrentMusic.PlayCount != PlayCount) return;
                if (TrackNumberTextBox.Text == "" && Properties.TrackNumber != 0) return;
                if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber) && Properties.TrackNumber != TrackNumber) return;
                if (YearTextBox.Text == "" && Properties.Year != 0) return;
                if (uint.TryParse(YearTextBox.Text, out uint Year) && Properties.Year != Year) return;
                SetMusicInfo(next);
            });
        }
    }
}
