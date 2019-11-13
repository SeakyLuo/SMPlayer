using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public static class MediaHelper
    {
        public static bool ShuffleEnabled;
        public static Music CurrentMusic;
        public static ObservableCollection<Music> CurrentPlaylist = new ObservableCollection<Music>();

        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }
        public static double Progress
        {
            get => CurrentMusic == null ? 0d : Position / CurrentMusic.Duration;
        }
        public static bool IsPlaying
        {
            get => Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing;
        }
        public static MediaPlaybackList PlaybackList
        {
            get
            {
                if (PendingPlaybackList != null)
                {
                    if (PendingPlaybackList.Items.Count < _PlaybackList.Items.Count)
                        return PendingPlaybackList;
                    _PlaybackList.Items.Clear();
                    foreach (var item in PendingPlaybackList.Items)
                        _PlaybackList.Items.Add(item);
                    PendingPlaybackList = null;
                    _PlaybackList.MoveTo(0);
                }
                return _PlaybackList;
            }
        }
        private static MediaPlaybackList _PlaybackList = new MediaPlaybackList();
        private static MediaPlaybackList PendingPlaybackList = null;
        public static MediaPlayer Player = new MediaPlayer()
        {
            Source = PlaybackList
        };
        public static List<RemoveMusicListener> RemoveMusicListeners = new List<RemoveMusicListener>();
        public static DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        public static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();
        public static List<SwitchMusicListener> SwitchMusicListeners = new List<SwitchMusicListener>();
        private const string FILENAME = "NowPlayingPlaylist.json";

        public static async void Init()
        {
            var settings = Settings.settings;
            var playlist = JsonFileHelper.Convert<List<string>>(await JsonFileHelper.ReadAsync(FILENAME));
            if (playlist != null && playlist.Count != 0)
            {
                if (settings.LastMusic == null)
                    settings.LastMusic = MusicLibraryPage.MusicFromPath(playlist[0]);
                CurrentMusic = settings.LastMusic;
                foreach (var path in playlist)
                {
                    var target = MusicLibraryPage.MusicFromPath(path);
                    if (target != null) AddMusic(target);
                }
            }
            Player.Volume = settings.Volume;
            SetMode(Settings.settings.Mode);
            Timer.Start();

            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= CurrentPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.GetMusic();
                foreach (var listener in SwitchMusicListeners)
                    listener.MusicSwitching(current, next, args.Reason);
                Settings.settings.LastMusic = CurrentMusic = next;
            };
            Player.MediaEnded += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };
        }
        public static Music GetMusic(this MediaPlaybackItem item)
        {
            return item.Source.CustomProperties["Source"] as Music;
        }
        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, CurrentPlaylist.Select(m => m.Path));
        }
        public static void SetMode(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Once:
                    Player.IsLoopingEnabled = false;
                    _PlaybackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Repeat:
                    Player.IsLoopingEnabled = false;
                    _PlaybackList.AutoRepeatEnabled = true;
                    break;
                case PlayMode.RepeatOne:
                    Player.IsLoopingEnabled = true;
                    _PlaybackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Shuffle:
                    Player.IsLoopingEnabled = false;
                    _PlaybackList.AutoRepeatEnabled = true;
                    break;
            }
            Settings.settings.Mode = mode;
            ShuffleEnabled = mode == PlayMode.Shuffle;
        }

        public static void AddMusic(Music source)
        {
            Music music = source.Copy();
            music.IsPlaying = music.Equals(CurrentMusic);
            music.Index = CurrentPlaylist.Count;
            CurrentPlaylist.Add(music);
            PlaybackList.Items.Add(music.GetMediaPlaybackItem());
        }
        public static AddMusicResult SetPlaylist(ICollection<Music> playlist, Music target = null)
        {
            Clear();
            foreach (var music in playlist) AddMusic(music);
            return new AddMusicResult();
        }

        public static AddMusicResult SetMusicAndPlay(Music music)
        {
            Clear();
            AddMusic(music);
            return new AddMusicResult();
        }

        public static AddMusicResult SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            if (playlist.SameAs(CurrentPlaylist))
            {
                MoveToMusic(music);
            }
            else
            {
                SetPlaylist(ShuffleEnabled ? ShufflePlaylist(playlist, music) : playlist, music);
            }
            Play();
            return new AddMusicResult();
        }

        public static AddMusicResult ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetMode(PlayMode.Shuffle);
            SetPlaylist(ShufflePlaylist(playlist));
            Play();
            return new AddMusicResult();
        }

        public static void ShuffleOthers()
        {
            PendingPlaybackList = new MediaPlaybackList();
            var playlist = ShufflePlaylist(CurrentPlaylist, CurrentMusic);
            CurrentPlaylist.Clear();
            foreach (var music in playlist)
                AddMusic(music);
            CurrentPlaylist[0].IsPlaying = true;
        }

        public static List<Music> ShufflePlaylist(ICollection<Music> playlist, Music start = null)
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
            if (start != null)
            {
                list.Remove(start);
                list.Insert(0, start);
            }
            return list;
        }
        public static bool MoveToMusic(Music music)
        {
            if (music != null)
            {
                for (int i = 0; i < CurrentPlaylist.Count; i++)
                {
                    if (CurrentPlaylist[i] == music)
                    {
                        PlaybackList.MoveTo(Convert.ToUInt32(i));
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Play()
        {
            Player.Play();
            foreach (var listener in MediaControlListeners) listener.Play();
        }

        public static void Pause()
        {
            Player.Pause();
            foreach (var listener in MediaControlListeners) listener.Pause();
        }

        public static void MovePrev()
        {
            if (Player.IsLoopingEnabled)
                Position = 0;
            else
                PlaybackList.MovePrevious();
        }

        public static void MoveNext()
        {
            if (Player.IsLoopingEnabled)
                Position = 0;
            else
                PlaybackList.MoveNext();
        }

        public static bool RemoveMusic(Music music)
        {
            try
            {
                CurrentPlaylist.RemoveAt(music.Index);
                PlaybackList.Items.RemoveAt(music.Index);
                foreach (var listener in RemoveMusicListeners) listener.MusicRemoved(music.Index, music, CurrentPlaylist);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void Clear()
        {
            if (CurrentPlaylist.Count == 0) return;
            CurrentMusic = null;
            CurrentPlaylist.Clear();
            PlaybackList.Items.Clear();
            foreach (var listener in RemoveMusicListeners) listener.MusicRemoved(-1, CurrentMusic, CurrentPlaylist);
        }

        public static void FindMusicAndSetPlaying(ICollection<Music> playlist, Music current, Music next)
        {
            foreach (var music in playlist)
                music.IsPlaying = music.Equals(next);
        }
    }

    public interface RemoveMusicListener
    {
        void MusicRemoved(int index, Music music, ICollection<Music> newCollection);
    }

    public interface SwitchMusicListener
    {
        void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason);
    }

    public interface MediaControlListener
    {
        void Play();
        void Pause();
        void Tick();
        void MediaEnded();
    }

    public enum AddMusicStatus
    {
        Successful = 0, Failed = 1, Break = 2
    }

    public class AddMusicResult
    {
        public AddMusicStatus Status = AddMusicStatus.Successful;
        public List<Music> Failed = new List<Music>();
        public bool IsFailed { get => Status == AddMusicStatus.Failed; }
        public int FailCount { get => Failed.Count; }
        public AddMusicStatus TryInsert(Music music, int index = -1)
        {
            try
            {
                MediaHelper.PlaybackList.Items.Add(music.GetMediaPlaybackItem());
                return Status = AddMusicStatus.Successful;
            }
            catch (System.IO.FileNotFoundException)
            {
                Failed.Add(music);
                return Status = AddMusicStatus.Failed;
            }
        }
    }
}
