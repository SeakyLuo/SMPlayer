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
        private static int CurrentMusicIndex = -1;
        private static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();
        private static Random random = new Random();
        private static List<int> RandomIndices = new List<int>();
        private static int RandomIndex = 0;
        private static bool ShuffleEnabled = false;

        public async static void Init()
        {
            await SetPlayList(MusicLibraryPage.AllSongs);
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
            for (int i = 0; i < CurrentPlayList.Count; i++)
                RandomIndices.Add(i);

            Player.Volume = Settings.settings.Volume;
            SetMusic(Settings.settings.LastMusic);
            SetMode(Settings.settings.Mode);
        }

        public static void SetMode(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Once:
                    Player.IsLoopingEnabled = false;
                    PlayList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Repeat:
                    Player.IsLoopingEnabled = false;
                    PlayList.AutoRepeatEnabled = true;
                    break;
                case PlayMode.RepeatOne:
                    Player.IsLoopingEnabled = true;
                    PlayList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Shuffle:
                    PlayList.AutoRepeatEnabled = true;
                    Player.IsLoopingEnabled = false;
                    break;
                default:
                    break;
            }
            SetShuffle(mode == PlayMode.Shuffle);
            Settings.settings.Mode = mode;
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
                if (ShuffleEnabled)
                {
                    RandomIndex = RandomIndex == 0 ? CurrentPlayList.Count - 1 : RandomIndex - 1;
                    CurrentMusic = CurrentPlayList[RandomIndices[RandomIndex]];
                    PlayList.MoveTo(Convert.ToUInt32(RandomIndices[RandomIndex]));
                }
                else
                {
                    CurrentMusicIndex = CurrentMusicIndex == 0 ? CurrentPlayList.Count - 1 : CurrentMusicIndex - 1;
                    CurrentMusic = CurrentPlayList[CurrentMusicIndex];
                    PlayList.MovePrevious();
                }
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
                if (ShuffleEnabled)
                {
                    RandomIndex = (RandomIndex + 1) % CurrentPlayList.Count;
                    CurrentMusic = CurrentPlayList[RandomIndices[RandomIndex]];
                    PlayList.MoveTo(Convert.ToUInt32(RandomIndices[RandomIndex]));
                }
                else
                {
                    CurrentMusicIndex = (CurrentMusicIndex + 1) % CurrentPlayList.Count;
                    CurrentMusic = CurrentPlayList[CurrentMusicIndex];
                    PlayList.MoveNext();
                }
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
            SetShuffle(ShuffleEnabled);
        }

        public static void SetPosition(double seconds)
        {
            Player.PlaybackSession.Position = TimeSpan.FromSeconds(seconds);
        }

        public static void SetShuffle(bool isShuffle)
        {
            ShuffleEnabled = isShuffle;
            if (!isShuffle) return;
            RandomIndex = 0;
            Shuffle(RandomIndices);
            RandomIndices.Remove(CurrentMusicIndex);
            RandomIndices.Insert(0, CurrentMusicIndex);
        }

        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
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
