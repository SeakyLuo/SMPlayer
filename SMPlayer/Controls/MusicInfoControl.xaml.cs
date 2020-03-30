using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicInfoControl : UserControl, SwitchMusicListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        private Music CurrentMusic;
        private MusicProperties Properties;
        public static List<Action<Music, Music>> MusicModifiedListeners = new List<Action<Music, Music>>();
        public MusicInfoControl()
        {
            this.InitializeComponent();
            MediaControl.AddMusicModifiedListener(MusicModified);
            MediaHelper.SwitchMusicListeners.Add(this);
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
            ComposersTextBox.Text = string.Join(Helper.LocalizeMessage("Comma"), Properties.Composers);
            DurationTextBox.Text = MusicDurationConverter.ToTime(Properties.Duration.TotalSeconds);
            GenreTextBox.Text = string.Join(Helper.LocalizeMessage("Comma"), Properties.Genre);
        }
        public async void SetMusicInfo(Music music)
        {
            if (music == null) return;
            CurrentMusic = music;
            SetBasicProperties(await music.GetStorageFileAsync());
            SetMusicProperties(Properties = await music.GetMusicPropertiesAsync());
        }
        private void ClearPlayCountButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMusic.PlayCount = 0;
            SetPlayCount(CurrentMusic);
            NotifyListeners(CurrentMusic, CurrentMusic);
        }
        private static void NotifyListeners(Music before, Music after)
        {
            Settings.settings.Tree.FindMusic(before).CopyFrom(after);
            MediaControl.NotifyMusicModifiedListeners(before, after);
            foreach (var listener in MusicModifiedListeners) listener.Invoke(before, after);
        }

        public void SetPlayCount(Music music)
        {
            if (music.PlayCount == 0)
            {
                PlayCountTextBox.Text = "";
                ClearPlayCountButton.Visibility = Visibility.Collapsed;
                PlayCountTextBox.SetToolTip(Helper.LocalizeMessage("NotPlayedYet", music.Name));
            }
            else
            {
                PlayCountTextBox.Text = music.PlayCount.ToString();
                ClearPlayCountButton.Visibility = Visibility.Visible;
                string times = Helper.CurrentLanguage.Contains("en") ? MusicDurationConverter.TryPlural("time", music.PlayCount) : "";
                PlayCountTextBox.SetToolTip(Helper.LocalizeMessage("HasBeenPlayed", music.Name, music.PlayCount, times));
            }
        }

        private void ResetMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            SetMusicProperties(Properties);
            Helper.ShowNotification("PropertiesReset");
        }

        private async void SaveMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            var newMusic = CurrentMusic.Copy();
            Properties.Title = newMusic.Name = TitleTextBox.Text;
            Properties.Subtitle = SubtitleTextBox.Text;
            Properties.Artist = newMusic.Artist = ArtistTextBox.Text;
            Properties.Album = newMusic.Album = AlbumTextBox.Text;
            Properties.AlbumArtist = AlbumArtistTextBox.Text;
            if (int.TryParse(PlayCountTextBox.Text, out int PlayCount))
                newMusic.PlayCount = PlayCount;
            Properties.Publisher = PublisherTextBox.Text;
            if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber))
                Properties.TrackNumber = TrackNumber;
            if (uint.TryParse(YearTextBox.Text, out uint Year))
                Properties.Year = Year;
            await Properties.SavePropertiesAsync();
            NotifyListeners(CurrentMusic, newMusic);
            CurrentMusic.CopyFrom(newMusic);
            Helper.ShowNotification("PropertiesUpdated");
        }
        public async void SetBasicProperties(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            // TODO: better size presentation?
            FileSizeTextBox.Text = Helper.ConvertBytes(basicProperties.Size); 
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
