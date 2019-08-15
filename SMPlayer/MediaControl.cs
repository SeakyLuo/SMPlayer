using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public static class MediaControl
    {
        public static Music CurrentMusic;
        public static List<Music> CurrentPlayList = new List<Music>();

        public static MediaPlayer Player = new MediaPlayer();
        public static MediaPlaybackList PlayList = new MediaPlaybackList();
        private static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public static void Init()
        {
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                PlayList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(music.Path))));
                CurrentPlayList.Add(music);
            }
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            Player.Source = PlayList;
            Player.MediaOpened += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaOpened();
            };
            Player.MediaEnded += (sender, args) =>
            {
                Music before = new Music(CurrentMusic);
                CurrentMusic.PlayCount += 1;
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded(before, CurrentMusic);
                switch (Settings.settings.Mode)
                {
                    case PlayMode.Once:
                        Player.Pause();
                        Timer.Stop();
                        break;
                    case PlayMode.Repeat:
                        NextMusic();
                        break;
                    case PlayMode.RepeatOne:
                        break;
                    case PlayMode.Shuffle:
                        NextMusic();
                        break;
                    default:
                        break;
                }
            };
            Player.MediaFailed += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaFailed(args);
            };
            Player.Volume = Settings.settings.Volume;
            SetMusic(Settings.settings.LastMusic);
        }

        public static void AddMediaControlListener(MediaControlListener listener)
        {
            MediaControlListeners.Add(listener);
        }

        public static void Play()
        {
            Player.Play();
            Timer.Start();
        }

        public static void Pause()
        {
            Player.Pause();
            Timer.Stop();
        }

        public static Music PrevMusic()
        {
            PlayList.MovePrevious();
            CurrentMusic = CurrentPlayList[(int)PlayList.CurrentItemIndex];
            return CurrentMusic;
        }

        public static Music NextMusic()
        {
            PlayList.MoveNext();
            CurrentMusic = CurrentPlayList[(int)PlayList.CurrentItemIndex];
            return CurrentMusic;
        }

        public static void SetMusic(Music music)
        {
            CurrentMusic = music;
            int index = CurrentPlayList.IndexOf(music);
            PlayList.MoveTo((uint)index);
        }

        public static void SetShuffle(bool isShuffle)
        {
            PlayList.ShuffleEnabled = isShuffle;
        }

        public static void UnShuffle()
        {
            PlayList.ShuffleEnabled = false;
        }
    }

    public interface MediaControlListener
    {
        void Tick();
        void MediaOpened();
        void MediaEnded(Music before, Music after);
        void MediaFailed(MediaPlayerFailedEventArgs args);
    }
}
