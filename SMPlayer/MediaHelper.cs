using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public static class MediaHelper
    {
        public static Music CurrentMusic;
        // This is an ordered one, not the shuffled one
        public static List<Music> CurrentPlayList = new List<Music>();
        private static List<Music> ShuffledPlayList = new List<Music>();
        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }

        public static MediaPlaybackList PlayList = new MediaPlaybackList();
        public static MediaPlayer Player = new MediaPlayer()
        {
            Source = PlayList
        };
        public static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public async static Task Init()
        {
            while (Settings.settings == null) { }
            if (Settings.settings.PlayList.Count > 0)
            {
                await SetPlayList(Settings.settings.PlayList);
            }
            else if (!string.IsNullOrEmpty(Settings.settings.RootPath))
            {
                while (MusicLibraryPage.AllSongs == null) { }
                await SetPlayList(MusicLibraryPage.AllSongs);
            }
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlayList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= CurrentPlayList.Count) return;
                Music current = CurrentMusic?.Copy(), next = CurrentPlayList[Convert.ToInt32(sender.CurrentItemIndex)];
                foreach (var listener in MediaControlListeners)
                    listener.MusicSwitchingAsync(current, next);
                CurrentMusic = next;
            };
            Player.MediaEnded += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };

            Player.Volume = Settings.settings.Volume;
            MoveToMusic(Settings.settings.LastMusic);
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
                    Player.IsLoopingEnabled = false;
                    PlayList.AutoRepeatEnabled = true;
                    break;
                default:
                    break;
            }
            PlayList.ShuffleEnabled = mode == PlayMode.Shuffle;
            Settings.settings.Mode = mode;
        }

        public static async Task SetPlayList(IEnumerable<Music> playlist)
        {
            if (Helper.SamePlayList(playlist, CurrentPlayList)) return;
            PlayList.Items.Clear();
            foreach (var music in playlist)
            {
                try
                {
                    var file = await Helper.CurrentFolder.GetFileAsync(music.GetShortPath());
                    var item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
                    PlayList.Items.Add(item);
                }
                catch (System.IO.FileNotFoundException)
                {
                    continue;
                }
            }
            CurrentPlayList = playlist.ToList();
        }

        public static async Task<List<Music>> GetRealPlayList()
        {
            if (PlayList.ShuffleEnabled)
            {
                if (ShuffledPlayList.Count == 0)
                {
                    foreach (var music in PlayList.ShuffledItems)
                    {
                        var p = music.GetDisplayProperties();
                        ShuffledPlayList.Add(await Music.GetMusic(music.Source.Uri.AbsolutePath));
                    }
                }
                return ShuffledPlayList;
            }
            else
                return CurrentPlayList;
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
            }
            return WaitForCurrentMusic();
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
            }
            return WaitForCurrentMusic();
        }

        private static Music WaitForCurrentMusic()
        {
            while (PlayList.CurrentItem == null) { }
            return CurrentPlayList[(int)PlayList.CurrentItemIndex];
        }

        public static void MoveToMusic(Music music)
        {
            if (music == null) return; 
            int index = CurrentPlayList.IndexOf(music);
            if (index == -1) return;
            Debug.WriteLine("MediaControl: " + music.Name);
            PlayList.MoveTo(Convert.ToUInt32(index));
        }
    }

    public interface MediaControlListener
    {
        void Tick();
        void MusicSwitchingAsync(Music current, Music next);
        void MediaEnded();
    }
}
