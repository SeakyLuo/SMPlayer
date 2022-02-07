using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class MusicInfoControl : UserControl, ISwitchMusicListener, IMediaPlayerStateChangedListener
    {
        public bool AllowMusicSwitching { get; set; }
        public bool ShowHeader { get; set; }
        public bool ShowPlayButton { get; set; } = false;
        public Windows.UI.Xaml.Media.Brush ProgressBarColor
        {
            get => SaveProgress.Foreground;
            set => SaveProgress.Foreground = value;
        }
        private MusicView CurrentMusic;
        private MusicProperties Properties;
        public static List<Action<MusicView, MusicView>> MusicModifiedListeners = new List<Action<MusicView, MusicView>>();
        public bool IsProcessing { get; private set; } = false;
        public bool IsCurrentMusic
        {
            get => CurrentMusic == MusicPlayer.CurrentMusic;
        }
        public MusicInfoControl()
        {
            this.InitializeComponent();
            MusicPlayer.AddSwitchMusicListener(this);
            MusicPlayer.MediaPlayerStateChangedListeners.Add(this);
        }

        public void SetMusicProperties(MusicProperties properties)
        {
            TitleTextBox.Text = properties.Title;
            SubtitleTextBox.Text = properties.Subtitle;
            ArtistTextBox.Text = properties.Artist;
            AlbumTextBox.Text = properties.Album;
            AlbumArtistTextBox.Text = properties.AlbumArtist;
            SetPlayCount(CurrentMusic);
            PublisherTextBox.Text = properties.Publisher;
            TrackNumberTextBox.Text = IntConverter.ToStr((int)properties.TrackNumber);
            YearTextBox.Text = IntConverter.ToStr((int)properties.Year);
            BitRateTextBox.Text = properties.Bitrate.ToString();
            ComposersTextBox.Text = string.Join(Helper.LocalizeMessage("Comma"), properties.Composers);
            DurationTextBox.Text = MusicDurationConverter.ToTime(properties.Duration.TotalSeconds);
            GenreTextBox.Text = string.Join(Helper.LocalizeMessage("Comma"), properties.Genre);
            if (ShowPlayButton)
            {
                SetPlayButtonVisibility(IsCurrentMusic && MusicPlayer.IsPlaying);
            }
        }
        public async void SetMusicInfo(MusicView music)
        {
            if (music == null) return;
            SaveProgress.Visibility = Visibility.Visible;
            IsProcessing = true;
            CurrentMusic = music;
            SetBasicProperties(await music.GetStorageFileAsync());
            SetMusicProperties(Properties = await music.GetMusicPropertiesAsync());
            IsProcessing = false;
            SaveProgress.Visibility = Visibility.Collapsed;
        }
        private void ClearPlayCountButton_Click(object sender, RoutedEventArgs e)
        {
            MusicView oldMusic = CurrentMusic.Copy();
            CurrentMusic.PlayCount = 0;
            SetPlayCount(CurrentMusic);
            Settings.settings.MusicModified(oldMusic, CurrentMusic);
        }

        public void SetPlayCount(MusicView music)
        {
            if (music.PlayCount == 0)
            {
                PlayCountTextBlock.Text = "";
                ClearPlayCountButton.Visibility = Visibility.Collapsed;
                PlayCountTextBlock.SetToolTip(Helper.LocalizeMessage("NotPlayedYet", music.Name), false);
            }
            else
            {
                PlayCountTextBlock.Text = music.PlayCount.ToString();
                ClearPlayCountButton.Visibility = Visibility.Visible;
                string times = Helper.CurrentLanguage.LanguageTag == Helper.Language_EN ? MusicDurationConverter.TryPlural("time", music.PlayCount) : "";
                PlayCountTextBlock.SetToolTip(Helper.LocalizeMessage("HasBeenPlayed", music.Name, music.PlayCount, times), false);
            }
        }

        private void ResetMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            SetMusicProperties(Properties);
            Helper.ShowNotification("PropertiesReset");
        }

        private async void SaveMusicPropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            if (IsPropertiesModified)
            {
                SaveProgress.Visibility = Visibility.Visible;
                var newMusic = CurrentMusic.Copy();
                Properties.Title = newMusic.Name = TitleTextBox.Text;
                Properties.Subtitle = SubtitleTextBox.Text;
                Properties.Artist = newMusic.Artist = ArtistTextBox.Text;
                Properties.Album = newMusic.Album = AlbumTextBox.Text;
                Properties.AlbumArtist = AlbumArtistTextBox.Text;
                if (int.TryParse(PlayCountTextBlock.Text, out int PlayCount))
                    newMusic.PlayCount = PlayCount;
                Properties.Publisher = PublisherTextBox.Text;
                if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber))
                    Properties.TrackNumber = TrackNumber;
                if (uint.TryParse(YearTextBox.Text, out uint Year))
                    Properties.Year = Year;
                await Task.Run(async () =>
                {
                    await Properties.SavePropertiesAsync();
                });
                Settings.settings.MusicModified(CurrentMusic, newMusic);
                CurrentMusic.CopyFrom(newMusic);
                SaveProgress.Visibility = Visibility.Collapsed;
            }
            IsProcessing = false;
            Helper.ShowNotification("PropertiesUpdated");
        }

        private void ShowInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper.ShowInExplorerWithLoader(CurrentMusic.Path, StorageItemTypes.File);
        }

        public async void SetBasicProperties(StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            FileSizeTextBox.Text = Helper.ConvertBytes(basicProperties.Size); 
            DateCreatedTextBox.Text = file.DateCreated.ToLocalTime().ToString();
            DateModifiedTextBox.Text = basicProperties.DateModified.ToLocalTime().ToString();
            PathTextBox.Text = file.Path;
        }
        private void CheckIfDigit(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        public async void MusicSwitching(MusicView current, MusicView next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (!AllowMusicSwitching) return;
                // if modified but not saved
                if (!IsPropertiesModified)
                    SetMusicInfo(next);
            });
        }

        private bool IsPropertiesModified
        {
            get
            {
                // Using multiple returns for easier debugging 
                if (Properties.Title != TitleTextBox.Text) return true;
                if (Properties.Subtitle != SubtitleTextBox.Text) return true;
                if (Properties.Artist != ArtistTextBox.Text) return true;
                if (Properties.Album != AlbumTextBox.Text) return true;
                if (Properties.AlbumArtist != AlbumArtistTextBox.Text) return true;
                if (Properties.Publisher != PublisherTextBox.Text) return true;
                if (int.TryParse(PlayCountTextBlock.Text, out int PlayCount) && CurrentMusic.PlayCount != PlayCount) return true;
                if (TrackNumberTextBox.Text == "" && Properties.TrackNumber != 0) return true;
                if (uint.TryParse(TrackNumberTextBox.Text, out uint TrackNumber) && Properties.TrackNumber != TrackNumber) return true;
                if (YearTextBox.Text == "" && Properties.Year != 0) return true;
                if (uint.TryParse(YearTextBox.Text, out uint Year) && Properties.Year != Year) return true;
                return false;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsCurrentMusic)
            {
                MusicPlayer.Play();
            }
            else
            {
                MusicPlayer.SetMusicAndPlay(CurrentMusic);
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.Pause();
        }

        private void SetPlayButtonVisibility(bool isPlaying)
        {
            if (isPlaying)
            {
                PlayButton.Visibility = Visibility.Collapsed;
                PauseButton.Visibility = Visibility.Visible;
            }
            else
            {
                PlayButton.Visibility = Visibility.Visible;
                PauseButton.Visibility = Visibility.Collapsed;
            }
        }

        async void IMediaPlayerStateChangedListener.StateChanged(MediaPlaybackState state)
        {
            // For F3 Support
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetPlayButtonVisibility(state == MediaPlaybackState.Playing);
            });
        }
    }
}
