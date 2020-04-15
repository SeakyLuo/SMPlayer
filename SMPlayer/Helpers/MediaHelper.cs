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
                    // Use this for AddMusic
                    if (PendingPlaybackList.Items.Count < _PlaybackList.Items.Count)
                        return PendingPlaybackList;
                    _PlaybackList.Items.Clear();
                    foreach (var item in PendingPlaybackList.Items)
                        _PlaybackList.Items.Add(item);
                    PendingPlaybackList = null;
                    _PlaybackList.MoveTo(1);
                }
                return _PlaybackList;
            }
        }
        private static MediaPlaybackList _PlaybackList = new MediaPlaybackList() { MaxPlayedItemsToKeepOpen = 1 };
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
                if (settings.LastMusicIndex == -1)
                    settings.LastMusicIndex = 0;
                foreach (var path in playlist)
                {
                    var target = Settings.FindMusic(path);
                    if (target != null) AddMusic(target);
                }
                CurrentMusic = CurrentPlaylist[settings.LastMusicIndex];
            }
            if (settings.LastMusicIndex != -1)
            {
                try
                {
                    PlaybackList.MoveTo(Convert.ToUInt32(settings.LastMusicIndex));
                }
                catch (Exception)
                {
                    // 无效索引
                }
            }
            Player.Volume = settings.Volume;
            if (settings.SaveMusicProgress) Position = settings.MusicProgress;
            SetMode(Settings.settings.Mode);
            Timer.Start();
            if (settings.AutoPlay) Play();

            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                if (PlaybackList.CurrentItemIndex >= CurrentPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.GetMusic();
                foreach (var listener in SwitchMusicListeners)
                    listener.MusicSwitching(current, next, args.Reason);
                CurrentMusic = next;
                Settings.settings.LastMusicIndex = (int)PlaybackList.CurrentItemIndex;
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
        public static void SetPlaylist(ICollection<Music> playlist, Music target = null)
        {
            Clear();
            foreach (var music in playlist) AddMusic(music);
            if (target != null) MoveToMusic(target);
        }

        public static void SetMusicAndPlay(Music music)
        {
            Clear();
            AddMusic(music);
            Play();
        }

        public static void SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            if (playlist.SameAs(CurrentPlaylist))
                MoveToMusic(music);
            else
                SetPlaylist(ShuffleEnabled ? ShufflePlaylist(playlist, music) : playlist, music);
            Play();
        }

        public static void ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetPlaylist(ShufflePlaylist(playlist));
            Play();
        }

        public static void ShuffleOthers()
        {
            // Creating a new MediaPlaybackList here is because removing old music
            // somehow restarts the current playing music.
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
        private static void PrintPlaylist(int from, int to)
        {
            for (int k = from; k <= to; k++)
                Debug.Write(CurrentPlaylist[k].Name + " ");
            Debug.Write("\n");
        }
        private static void PrintPlaybackList(int from, int to)
        {
            for (int k = from; k <= to; k++)
                Debug.Write(PlaybackList.Items[k].GetMusic().Name + " ");
            Debug.Write("\n");
        }

        public static void MoveMusic(int from, int to)
        {
            MediaPlaybackItem item;
            if (CurrentMusic.Index == from)
            {
                CurrentMusic.Index = to;
                if (from < to)
                {
                    for (int i = from + 1; i <= to; i++)
                    {
                        CurrentPlaylist[i].Index = i - 1;
                        PlaybackList.Items.RemoveAt(i);
                        PlaybackList.Items.Insert(i - 1, CurrentPlaylist[i].GetMediaPlaybackItem());
                    }
                }
                else
                {
                    for (int i = to + 1; i <= from; i++)
                    {
                        CurrentPlaylist[i].Index = i;
                        PlaybackList.Items.RemoveAt(to);
                        PlaybackList.Items.Insert(from, CurrentPlaylist[i].GetMediaPlaybackItem());
                    }
                }
            }
            else
            {
                if (from < CurrentMusic.Index && CurrentMusic.Index <= to) CurrentMusic.Index--;
                else if (to <= CurrentMusic.Index && CurrentMusic.Index < from) CurrentMusic.Index++;
                item = PlaybackList.Items[from];
                PlaybackList.Items.RemoveAt(from);
                PlaybackList.Items.Insert(to, item);
                for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                {
                    CurrentPlaylist[i].Index = i;
                    if (i != CurrentMusic.Index) PlaybackList.Items[i] = CurrentPlaylist[i].GetMediaPlaybackItem();
                }
            }
        }

        public static bool RemoveMusic(Music music)
        {
            try
            {
                if (music == null) return false;
                if (music.Index == CurrentMusic.Index) MoveNext();
                CurrentPlaylist.RemoveAt(music.Index);
                PlaybackList.Items.RemoveAt(music.Index);
                for (int i = music.Index; i < CurrentPlaylist.Count; i++) CurrentPlaylist[i].Index = i;
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
            if (playlist == null) return;
            foreach (var music in playlist)
                music.IsPlaying = music.Equals(next);
        }

        public static async void RemoveBadMusic()
        {
            if (CurrentMusic != null && await CurrentMusic.GetStorageFileAsync() == null)
                CurrentMusic = null;
            //foreach (var music in CurrentPlaylist)
            //    if (await music.GetStorageFileAsync() == null)
            //        RemoveMusic(music);
        }

        public static void LikeMusic(Music music)
        {
            if (CurrentPlaylist.FirstOrDefault(item => item == music) is Music m)
                m.Favorite = true;
        }
        public static void DislikeMusic(Music music)
        {
            if (CurrentPlaylist.FirstOrDefault(item => item == music) is Music m)
                m.Favorite = false;
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

    }
}
