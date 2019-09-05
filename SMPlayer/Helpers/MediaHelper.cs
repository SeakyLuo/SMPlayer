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
        public static List<Music> CurrentPlaylist
        {
            get
            {
                if (PlayList.ShuffleEnabled)
                {
                    if (ShuffledPlayList.Count == 0 || ShuffledPlayList.Count != OrderedPlayList.Count)
                    {
                        ShuffledPlayList.Clear();
                        foreach (var item in PlayList.ShuffledItems)
                            ShuffledPlayList.Add(item.Source.CustomProperties["Source"] as Music);
                    }
                    return ShuffledPlayList;
                }
                else
                {
                    return OrderedPlayList;
                }
            }
            private set => OrderedPlayList = value;
        }
        private static List<Music> OrderedPlayList = new List<Music>();
        private static List<Music> ShuffledPlayList = new List<Music>();
        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }
        public static bool IsPlaying
        {
            get => Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
        }

        public static MediaPlaybackList PlayList = new MediaPlaybackList();
        public static MediaPlayer Player = new MediaPlayer()
        {
            Source = PlayList
        };
        public static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public async static void Init()
        {
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlayList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= OrderedPlayList.Count) return;
                Music current = CurrentMusic?.Copy(), next = PlayList.Items[Convert.ToInt32(sender.CurrentItemIndex)].Source.CustomProperties["Source"] as Music;
                // Switching Folder Cause this
                if (!next.Equals(current))
                {
                    foreach (var listener in MediaControlListeners)
                        listener.MusicSwitching(current, next, args.Reason);
                    CurrentMusic = next;
                }
            };
            Player.MediaEnded += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };

            while (Settings.settings == null) { System.Threading.Thread.Sleep(500); }
            await SetPlaylist(PlaylistControl.NowPlayingPlaylist);
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
            bool isShuffle = mode == PlayMode.Shuffle;
            PlayList.ShuffleEnabled = isShuffle;
            if (!isShuffle) ShuffledPlayList.Clear();
            foreach (var listener in MediaControlListeners)
                listener.ShuffleChanged(CurrentPlaylist, isShuffle);
            Settings.settings.Mode = mode;
        }

        public static async Task SetPlaylist(ICollection<Music> playlist)
        {
            if (Helper.SamePlayList(playlist, CurrentPlaylist)) return;
            Pause();
            PlayList.Items.Clear();
            foreach (var music in playlist)
            {
                try
                {
                    var file = await Helper.CurrentFolder.GetFileAsync(music.GetShortPath());
                    var source = MediaSource.CreateFromStorageFile(file);
                    music.IsPlaying = false;
                    source.CustomProperties.Add("Source", music);
                    var item = new MediaPlaybackItem(source);
                    PlayList.Items.Add(item);
                }
                catch (System.IO.FileNotFoundException)
                {
                    continue;
                }
            }
            OrderedPlayList = playlist.ToList();
            ShuffledPlayList.Clear();
            if (!OrderedPlayList.Contains(CurrentMusic))
                CurrentMusic = null;
            PlaylistControl.SetPlaylist(CurrentPlaylist);
        }

        public static async void ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetMode(PlayMode.Shuffle);
            await SetPlaylist(playlist);
            Play();
        }

        public static void AddMediaControlListener(MediaControlListener listener)
        {
            MediaControlListeners.Add(listener);
        }

        public static void MoveMusic(Music music, int toIndex)
        {
            var item = PlayList.Items.FirstOrDefault((i) => i.Source.CustomProperties["Source"].Equals(music));
            if (item == null) return;
            PlayList.Items.Remove(item);
            PlayList.Items.Insert(toIndex, item);
        }

        public static int IndexOf(Music music)
        {
            for (int i = 0; i < OrderedPlayList.Count; i++)
                if (PlayList.Items[i].Source.CustomProperties["Source"].Equals(music))
                    return i;
            return -1;
        }

        public static void SwitchMusic(Music music1, Music music2)
        {
            int index1 = IndexOf(music1);
            if (index1 == -1) return;
            int index2 = IndexOf(music1);
            if (index2 == -1) return;
            var temp = PlayList.Items[index1];
            PlayList.Items[index1] = PlayList.Items[index2];
            PlayList.Items[index2] = temp;
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

        public static void PrevMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlayList.MovePrevious();
        }

        public static void NextMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlayList.MoveNext();
        }

        public static void MoveToMusic(Music music)
        {
            if (music == null) return;
            for (int i = 0; i < CurrentPlaylist.Count; i++)
            {
                if (PlayList.Items[i].Source.CustomProperties["Source"].Equals(music))
                {
                    PlayList.MoveTo(Convert.ToUInt32(i));
                    Debug.WriteLine("MediaControl: " + music.Name);
                    break;
                }
            }
        }

        public static void RemoveMusic(Music music)
        {
            OrderedPlayList.Remove(music);
            ShuffledPlayList.Remove(music);
            var target = PlayList.Items.FirstOrDefault((item) => music.Equals(item.Source.CustomProperties["Source"]));
            PlayList.Items.Remove(target);
        }
    }

    public interface MediaControlListener
    {
        void Tick();
        void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason);
        void MediaEnded();
        void ShuffleChanged(ICollection<Music> newPlayList, bool isShuffle);
    }
}
