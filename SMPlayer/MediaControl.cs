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
    public static class MediaControl
    {
        public static Music CurrentMusic;
        public static List<Music> CurrentPlayList = new List<Music>();
        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }

        public static MediaPlayer Player = new MediaPlayer();
        public static MediaPlaybackList PlayList = new MediaPlaybackList();
        public static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public async static Task Init()
        {
            while (MusicLibraryPage.AllSongs == null) { }
            await SetPlayList(MusicLibraryPage.AllSongs);
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            Player.Source = PlayList;
            PlayList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= CurrentPlayList.Count) return;
                Music current = new Music(CurrentMusic), next = CurrentPlayList[Convert.ToInt32(sender.CurrentItemIndex)];
                foreach (var listener in MediaControlListeners)
                    listener.MusicSwitching(current, next);
                CurrentMusic = next;
            };
            Player.MediaEnded += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };

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
                    Player.IsLoopingEnabled = false;
                    PlayList.AutoRepeatEnabled = true;
                    break;
                default:
                    break;
            }
            PlayList.ShuffleEnabled = mode == PlayMode.Shuffle;
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
                PlayList.MovePrevious();
                CurrentMusic = WaitForCurrentMusic();
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
                CurrentMusic = WaitForCurrentMusic();
            }
            return CurrentMusic;
        }

        private static Music WaitForCurrentMusic()
        {
            while (PlayList.CurrentItem == null) { }
            return CurrentPlayList[(int)PlayList.CurrentItemIndex];
        }

        public static void SetMusic(Music music)
        {
            if (music == null) return; 
            int index = CurrentPlayList.IndexOf(music);
            if (index == -1) return;
            Debug.WriteLine("MediaControl: " + music.Name);
            do
            {
                try
                {
                    PlayList.MoveTo(Convert.ToUInt32(index));
                    break;
                }catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    continue;
                }
            } while (true);
            //CurrentMusic = music;
        }
    }

    public interface MediaControlListener
    {
        void Tick();
        void MusicSwitching(Music current, Music next);
        void MediaEnded();
    }
}
