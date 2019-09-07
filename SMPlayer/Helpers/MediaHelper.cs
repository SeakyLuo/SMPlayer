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
                if (PlayBackList.ShuffleEnabled)
                {
                    if (ShuffledPlaylist.Count == 0 || ShuffledPlaylist.Count != OrderedPlaylist.Count)
                    {
                        ShuffledPlaylist.Clear();
                        foreach (var item in PlayBackList.ShuffledItems)
                            ShuffledPlaylist.Add(item.Source.CustomProperties["Source"] as Music);
                    }
                    return ShuffledPlaylist;
                }
                else
                {
                    return OrderedPlaylist;
                }
            }
            private set => OrderedPlaylist = value;
        }
        private static List<Music> OrderedPlaylist = new List<Music>();
        private static List<Music> ShuffledPlaylist = new List<Music>();
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

        public async static void Init()
        {
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlayBackList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= OrderedPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.Source.CustomProperties["Source"] as Music;
                var si = sender.StartingItem.Source.CustomProperties["Source"] as Music;
                Debug.WriteLine("Next: " + next.Name);
                Debug.WriteLine("StartingItem: " + si.Name);
                Debug.WriteLine("ShuffledItems:");
                foreach(var item in sender.ShuffledItems)
                {
                    var m = item.Source.CustomProperties["Source"] as Music;
                    Debug.WriteLine(m.Name);
                }
                Debug.WriteLine("Items:");
                while (true)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var item in sender.Items)
                        {
                            var m = item.Source.CustomProperties["Source"] as Music;
                            sb.AppendLine(m.Name);
                        }
                        Debug.WriteLine(sb.ToString());
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
                Debug.WriteLine(new string('=', 30));

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

            while (Settings.settings == null) { System.Threading.Thread.Sleep(233); }
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
            PlayBackList.ShuffleEnabled = isShuffle;
            if (!isShuffle) ShuffledPlaylist.Clear();
            foreach (var listener in ShuffleChangedListeners)
                listener.ShuffleChanged(CurrentPlaylist, isShuffle);
            Settings.settings.Mode = mode;
        }

        public static async Task SetPlaylist(ICollection<Music> playlist)
        {
            if (!PlayBackList.ShuffleEnabled && Helper.SamePlayList(playlist, CurrentPlaylist)) return;
            Pause();
            PlayBackList.Items.Clear();
            foreach (var music in playlist)
            {
                try
                {
                    var file = await Helper.CurrentFolder.GetFileAsync(music.GetShortPath());
                    var source = MediaSource.CreateFromStorageFile(file);
                    music.IsPlaying = false;
                    source.CustomProperties.Add("Source", music);
                    var item = new MediaPlaybackItem(source);
                    PlayBackList.Items.Add(item);
                }
                catch (System.IO.FileNotFoundException)
                {
                    continue;
                }
            }
            OrderedPlaylist = playlist.ToList();
            ShuffledPlaylist.Clear();
            if (!OrderedPlaylist.Contains(CurrentMusic))
                CurrentMusic = null;
            PlaylistControl.SetPlaylist(CurrentPlaylist);
        }

        public static async void ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetMode(PlayMode.Shuffle);
            await SetPlaylist(playlist);
            Play();
        }

        public static void MoveMusic(Music music, int toIndex)
        {
            var item = PlayBackList.Items.FirstOrDefault((i) => i.Source.CustomProperties["Source"].Equals(music));
            if (item == null) return;
            PlayBackList.Items.Remove(item);
            PlayBackList.Items.Insert(toIndex, item);
        }

        public static int IndexOf(Music music)
        {
            for (int i = 0; i < OrderedPlaylist.Count; i++)
                if (PlayBackList.Items[i].Source.CustomProperties["Source"].Equals(music))
                    return i;
            return -1;
        }

        public static void SwitchMusic(Music music1, Music music2)
        {
            int index1 = IndexOf(music1);
            if (index1 == -1) return;
            int index2 = IndexOf(music1);
            if (index2 == -1) return;
            var temp = PlayBackList.Items[index1];
            PlayBackList.Items[index1] = PlayBackList.Items[index2];
            PlayBackList.Items[index2] = temp;
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
                if (PlayBackList.Items[i].Source.CustomProperties["Source"].Equals(music))
                {
                    PlayBackList.MoveTo(Convert.ToUInt32(i));
                    Debug.WriteLine("MediaControl: " + music.Name);
                    break;
                }
            }
        }

        public static void RemoveMusic(Music music)
        {
            OrderedPlaylist.Remove(music);
            ShuffledPlaylist.Remove(music);
            var target = PlayBackList.Items.FirstOrDefault((item) => music.Equals(item.Source.CustomProperties["Source"]));
            PlayBackList.Items.Remove(target);
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
        void Tick();
        void MediaEnded();
    }
}
