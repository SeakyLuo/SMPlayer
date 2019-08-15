using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public static class MediaControl
    {
        public static Music CurrentMusic;
        public static List<Music> CurrentPlayList = new List<Music>();

        public static MediaPlayer Player = new MediaPlayer();
        public static MediaPlaybackList PlayList = new MediaPlaybackList();
        private static int CurrentMusicIndex;
        private static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public async static void Init()
        {
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            await SetPlayList(MusicLibraryPage.AllSongs);
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

        public static async Task SetPlayList(List<Music> playlist)
        {
            PlayList.Items.Clear();
            CurrentPlayList.Clear();
            foreach (var music in playlist)
            {
                var item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await Helper.CurrentFolder.GetFileAsync(music.GetShortPath())));
                PlayList.Items.Add(item);
                CurrentPlayList.Add(music);
            }
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
            if (Player.IsLoopingEnabled)
            {
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            }
            else
            {
                PlayList.MovePrevious();
                if (PlayList.ShuffleEnabled)
                {
                    CurrentMusicIndex = int.Parse(PlayList.CurrentItemIndex.ToString());
                }
                else
                {
                    CurrentMusicIndex -= 1;
                    if (CurrentMusicIndex < 0) CurrentMusicIndex += CurrentPlayList.Count;
                }
                CurrentMusic = CurrentPlayList[CurrentMusicIndex];
            }
            return CurrentMusic;
        }

        public static Music NextMusic()
        {
            if (Player.IsLoopingEnabled)
            {
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            }
            else
            {
                PlayList.MoveNext();
                if (PlayList.ShuffleEnabled)
                {
                    var item = PlayList.CurrentItem;
                    string value = PlayList.CurrentItemIndex.ToString();
                    CurrentMusicIndex = int.Parse(value);
                }
                else
                {
                    CurrentMusicIndex = (CurrentMusicIndex + 1) % CurrentPlayList.Count;
                }
                CurrentMusic = CurrentPlayList[CurrentMusicIndex];
            }
            return CurrentMusic;
        }

        public static void SetMusic(Music music)
        {
            int index = CurrentPlayList.IndexOf(music);
            if (index == -1) return;
            CurrentMusicIndex = index;
            CurrentMusic = music;
            PlayList.MoveTo(Convert.ToUInt32(index));
        }

        public static void SetPosition(double seconds)
        {
            Player.PlaybackSession.Position = TimeSpan.FromSeconds(seconds);
        }

        public static void SetShuffle(bool isShuffle)
        {
            PlayList.ShuffleEnabled = isShuffle;
            if (isShuffle) PlayList.SetShuffledItems(PlayList.ShuffledItems);
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
