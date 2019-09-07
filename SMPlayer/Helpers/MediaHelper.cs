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
        public static bool ShuffleEnabled;
        public static Music CurrentMusic;
        public static List<Music> CurrentPlaylist = new List<Music>();

        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }
        public static bool IsPlaying
        {
            get => Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
        }

        public static MediaPlaybackList PlayBackList = new MediaPlaybackList();
        public static MediaPlayer Player = new MediaPlayer()
        {
            Source = PlayBackList
        };
        public static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        public static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();
        public static List<MusicSwitchingListener> MusicSwitchingListeners = new List<MusicSwitchingListener>();
        public static List<ShuffleChangedListener> ShuffleChangedListeners = new List<ShuffleChangedListener>();

        public static void Init()
        {
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlayBackList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= CurrentPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.Source.CustomProperties["Source"] as Music;
                // Switching Folder Cause this
                if (!next.Equals(current))
                {
                    foreach (var listener in MusicSwitchingListeners)
                        listener.MusicSwitching(current, next, args.Reason);
                    CurrentMusic = next;
                }
            };
            Player.MediaEnded += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };

            SetPlaylist(PlaylistControl.NowPlayingPlaylist);
            while (Settings.settings == null) { System.Threading.Thread.Sleep(233); }
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
                    PlayBackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Repeat:
                    Player.IsLoopingEnabled = false;
                    PlayBackList.AutoRepeatEnabled = true;
                    break;
                case PlayMode.RepeatOne:
                    Player.IsLoopingEnabled = true;
                    PlayBackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Shuffle:
                    Player.IsLoopingEnabled = false;
                    PlayBackList.AutoRepeatEnabled = true;
                    break;
                default:
                    break;
            }
            bool isShuffle = mode == PlayMode.Shuffle;
            ShuffleEnabled = isShuffle;
            if (isShuffle) SetPlaylist(ShufflePlaylist(CurrentPlaylist, CurrentMusic));
            foreach (var listener in ShuffleChangedListeners)
                listener.ShuffleChanged(CurrentPlaylist, isShuffle);
            Settings.settings.Mode = mode;
        }

        public static async Task AddMusic(Music music)
        {
            try
            {
                var file = await Helper.CurrentFolder.GetFileAsync(music.GetShortPath());
                var source = MediaSource.CreateFromStorageFile(file);
                music.IsPlaying = false;
                source.CustomProperties.Add("Source", music);
                var item = new MediaPlaybackItem(source);
                PlayBackList.Items.Add(item);
                CurrentPlaylist.Add(music);
            }
            catch (System.IO.FileNotFoundException)
            {
                return;
            }
        }

        public static async void SetPlaylist(ICollection<Music> playlist)
        {
            if (Helper.SamePlayList(playlist, CurrentPlaylist)) return;
            int skip = 0;
            if (CurrentPlaylist.Count > 0)
            {
                if (playlist.ElementAt(0).Equals(CurrentPlaylist[0]))
                {
                    skip = 1;
                    CurrentPlaylist.RemoveRange(1, CurrentPlaylist.Count - 1);
                    for (int i = CurrentPlaylist.Count - 1; i > 0; i--)
                        PlayBackList.Items.RemoveAt(i);
                }
                else
                {
                    CurrentPlaylist.Clear();
                    PlayBackList.Items.Clear();
                }
            }
            foreach (var music in playlist.Skip(skip))
                await AddMusic(music);
            if (!CurrentPlaylist.Contains(CurrentMusic))
                CurrentMusic = null;
            PlaylistControl.SetPlaylist(CurrentPlaylist);
        }

        public static void SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            if (ShuffleEnabled) playlist = ShufflePlaylist(playlist, music);
            SetPlaylist(playlist);
            MoveToMusic(music);
            Play();
        }

        public static void ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetMode(PlayMode.Shuffle);
            SetPlaylist(playlist);
            Play();
        }

        public static List<Music> ShufflePlaylist(ICollection<Music> playlist, Music music = null)
        {
            Random random = new Random();
            var list = playlist.ToList();
            int n = playlist.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Music value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            if (music != null)
            {
                list.Remove(music);
                list.Insert(0, music);
            }
            return list;
        }

        public static void MoveMusic(Music music, int toIndex)
        {
            int index = CurrentPlaylist.IndexOf(music);
            if (index == -1) return;
            var item = PlayBackList.Items[index];
            PlayBackList.Items.RemoveAt(index);
            PlayBackList.Items.Insert(toIndex, item);
        }

        public static void SwitchMusic(Music music1, Music music2)
        {
            int index1 = CurrentPlaylist.IndexOf(music1);
            if (index1 == -1) return;
            int index2 = CurrentPlaylist.IndexOf(music2);
            if (index2 == -1) return;
            var temp = PlayBackList.Items[index1];
            PlayBackList.Items[index1] = PlayBackList.Items[index2];
            PlayBackList.Items[index2] = temp;
        }

        public static void Play()
        {
            Player.Play();
            Timer.Start();
            foreach (var listener in MediaControlListeners) listener.Play();
        }

        public static void Pause()
        {
            Player.Pause();
            Timer.Stop();
            foreach (var listener in MediaControlListeners) listener.Pause();
        }

        public static void PrevMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlayBackList.MovePrevious();
        }

        public static void NextMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlayBackList.MoveNext();
        }

        public static void MoveToMusic(Music music)
        {
            if (music == null) return;
            for (int i = 0; i < CurrentPlaylist.Count; i++)
            {
                if (CurrentPlaylist[i].Equals(music))
                {
                    PlayBackList.MoveTo(Convert.ToUInt32(i));
                    Debug.WriteLine("MediaControl: " + music.Name);
                    break;
                }
            }
        }

        public static void RemoveMusic(Music music)
        {
            int index = CurrentPlaylist.IndexOf(music);
            CurrentPlaylist.RemoveAt(index);
            PlayBackList.Items.RemoveAt(index);
            PlaylistControl.NowPlayingPlaylist.RemoveAt(index);
        }

        public static void FindMusicAndSetPlaying(ICollection<Music> playlist, Music current, Music next)
        {
            bool findCurrent = current == null, findNext = next == null;
            foreach (var music in playlist)
            {
                if (!findCurrent && (findCurrent = music.Equals(current)))
                    music.IsPlaying = false;
                if (!findNext && (findNext = music.Equals(next)))
                    music.IsPlaying = true;
                if (findCurrent && findNext) return;
            }
        }
    }

    public interface MusicSwitchingListener
    {
        void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason);
    }
    public interface ShuffleChangedListener
    {
        void ShuffleChanged(ICollection<Music> newPlayList, bool isShuffle);
    }

    public interface MediaControlListener
    {
        void Play();
        void Pause();
        void Tick();
        void MediaEnded();
    }
}
