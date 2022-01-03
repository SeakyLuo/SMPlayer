using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public class MusicPlayer : IMusicEventListener
    {
        public static bool ShuffleEnabled;
        public static Music CurrentMusic;
        public static List<Music> CurrentPlaylist = new List<Music>();

        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }
        public static double Progress
        {
            get => CurrentMusic == null ? 0d : Position / CurrentMusic.Duration;
        }
        public static MediaPlaybackState PlaybackState
        {
            get => Player.PlaybackSession.PlaybackState;
        }
        public static bool IsPlaying
        {
            get => PlaybackState == MediaPlaybackState.Playing;
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
        public static Playlist NowPlaying { get => new Playlist(MenuFlyoutHelper.NowPlaying, CurrentPlaylist); }
        private static MediaPlaybackList _PlaybackList = new MediaPlaybackList() { MaxPlayedItemsToKeepOpen = 1 };
        private static MediaPlaybackList PendingPlaybackList = null;
        public static MediaPlayer Player = new MediaPlayer() { Source = PlaybackList };
        public static DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        public static List<IMediaControlListener> MediaControlListeners = new List<IMediaControlListener>();
        public static List<ISwitchMusicListener> SwitchMusicListeners = new List<ISwitchMusicListener>();
        public static List<ICurrentPlaylistChangedListener> CurrentPlaylistChangedListeners = new List<ICurrentPlaylistChangedListener>();
        public static List<IMediaPlayerStateChangedListener> MediaPlayerStateChangedListeners = new List<IMediaPlayerStateChangedListener>();
        public static List<Action> InitFinishedListeners = new List<Action>();
        public const string JsonFilename = "NowPlaying";

        public MusicPlayer() { Settings.AddMusicEventListener(this); }

        public static void Init(Music music = null)
        {
            MainPage.AddMainPageLoadedListener(async () =>
            {
                await InitWithMusic(music);
            });
        }

        public static async Task InitWithMusic(Music music = null)
        {
            var settings = Settings.settings;
            if (music == null)
            {
                var playlist = await JsonFileHelper.ReadObjectAsync<List<long>>(JsonFilename);
                if (playlist.IsNotEmpty())
                {
                    if (settings.LastMusicIndex == -1)
                        settings.LastMusicIndex = 0;
                    SetPlaylist(Settings.FindMusicList(playlist));
                    if (settings.LastMusicIndex < CurrentPlaylist.Count)
                        CurrentMusic = CurrentPlaylist[settings.LastMusicIndex];
                }
            }
            if (music != null)
            {
                AddMusic(music);
            }
            else if (settings.LastMusicIndex != -1)
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
            // 如果非文件启动，并保存播放进度且有音乐
            if (music == null && settings.SaveMusicProgress && CurrentMusic != null) Position = settings.MusicProgress;
            SetMode(Settings.settings.Mode);

            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                lock (CurrentPlaylist)
                {
                    if (PlaybackList.CurrentItemIndex >= CurrentPlaylist.Count) return;
                    Music current = CurrentMusic?.Copy(), next = args.NewItem.GetMusic();
                    try
                    {
                        foreach (var listener in SwitchMusicListeners)
                            listener.MusicSwitching(current, next, args.Reason);
                    }
                    catch (InvalidOperationException)
                    {
                        // Collection was modified; enumeration operation may not execute.
                    }
                    CurrentMusic = next;
                    Settings.settings.LastMusicIndex = (int)PlaybackList.CurrentItemIndex;
                    App.Save();
                }
            };
            Player.PlaybackSession.PlaybackStateChanged += (sender, args) =>
            {
                foreach (var listener in MediaPlayerStateChangedListeners)
                    listener.StateChanged(sender.PlaybackState);
            };
            Player.MediaEnded += (sender, args) =>
            {
                Log.Info("MediaEnded");
                Settings.settings.Played(CurrentMusic);
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };

            foreach (var listener in InitFinishedListeners)
                listener.Invoke();
            Timer.Start();
            if (settings.AutoPlay || music != null) Play();
        }

        public static void Save()
        {
            var paths = CurrentPlaylist.Select(m => m.Id);
            JsonFileHelper.Save(JsonFilename, paths);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, paths);
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
            //PrintPlaybackList(0, CurrentPlaylist.Count - 1);
            Settings.settings.Mode = mode;
            ShuffleEnabled = mode == PlayMode.Shuffle;
        }

        public static void AddMusic(IMusicable source, int index)
        {
            Music music = source.ToMusic().Copy();
            music.IsPlaying = IsMusicPlaying(music);
            music.Index = index;
            CurrentPlaylist.Insert(index, music);
            PlaybackList.Items.Insert(index, music.GetMediaPlaybackItem());
            for (int i = index + 1; i < CurrentPlaylist.Count; i++)
            {
                CurrentPlaylist[i].Index = i;
                PlaybackList.Items[i].GetMusic().Index = i;
            }
            foreach (var listener in CurrentPlaylistChangedListeners)
                listener.AddMusic(music, index);
        }

        public static bool IsMusicPlaying(Music music)
        {
            return music.IndexedEquals(CurrentMusic);
        }

        public static void AddMusicAndPlay(IMusicable source)
        {
            AddMusic(source);
            MoveToMusic(source.ToMusic());
            Play();
        }

        public static void AddMusic(IMusicable source)
        {
            AddMusic(source, CurrentPlaylist.Count);
        }

        public static void SetPlaylist(IEnumerable<IMusicable> playlist, Music target = null)
        {
            Clear();
            foreach (var music in playlist) AddMusic(music);
            if (target != null) MoveToMusic(target);
        }

        public static void SetPlaylistAndPlay(IEnumerable<IMusicable> playlist, Music target = null)
        {
            SetPlaylist(playlist, target);
            Play();
        }

        public static void SetMusicAndPlay(Music music)
        {
            Clear();
            AddMusic(music);
            Play();
        }

        public static void SetMusicAndPlay(IEnumerable<IMusicable> playlist, Music music)
        {
            if (playlist.SameAs(CurrentPlaylist))
                MoveToMusic(music);
            else
                SetPlaylist(ShuffleEnabled ? ShufflePlaylist(playlist, music) : playlist, music);
            Play();
        }

        public static void SetMusicAndPlay(IEnumerable<IMusicable> playlist)
        {
            if (playlist.SameAs(CurrentPlaylist))
                PlaybackList.MoveTo(0);
            else
                SetPlaylist(ShuffleEnabled ? ShufflePlaylist(playlist) : playlist);
            Play();
        }

        public static void ShuffleAndPlay(IEnumerable<Music> playlist)
        {
            SetPlaylist(ShufflePlaylist(playlist));
            Play();
        }

        public static void ShuffleAndPlay()
        {
            ShuffleAndPlay(CurrentPlaylist);
        }

        public static void QuickPlay(int randomLimit = 100)
        {
            SetPlaylistAndPlay(Helpers.QuickPlayHelper.GetPlaylist(randomLimit));
        }

        public static void ShuffleOthers()
        {
            if (CurrentPlaylist.Count == 0) return;
            // Creating a new MediaPlaybackList here is because removing old music
            // somehow restarts the current playing music.
            PendingPlaybackList = new MediaPlaybackList();
            var playlist = ShufflePlaylist(CurrentPlaylist, CurrentMusic);
            CurrentPlaylist.Clear();
            foreach (var music in playlist)
                AddMusic(music);
            CurrentPlaylist[0].IsPlaying = true;
        }

        public static List<IMusicable> ShufflePlaylist(IEnumerable<IMusicable> playlist, IMusicable start = null)
        {
            var list = playlist.Shuffle();
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
                    if (music.Index > -1 ? music.IndexedEquals(CurrentPlaylist[i]) : music == CurrentPlaylist[i])
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
        }

        public static void Pause()
        {
            Player.Pause();
        }

        public static void MovePrev()
        {
            if (Player.IsLoopingEnabled)
            {
                Position = 0;
            }
            else
            {
                try
                {
                    PlaybackList.MovePrevious();
                }
                catch (Exception)
                {
                    // 无效索引
                }
            }
        }

        public static void MoveNext()
        {
            if (Player.IsLoopingEnabled)
            {
                Position = 0;
            }
            else
            {
                try
                {
                    PlaybackList.MoveNext();
                }
                catch (Exception)
                {
                    // 无效索引
                }

            }
        }
        private static void PrintPlaylist(int from, int to)
        {
            Log.Info("PrintPlaylist:");
            for (int k = from; k <= to; k++)
                Debug.Write(CurrentPlaylist[k].Name + " ");
            Debug.Write("\n");
        }
        private static void PrintPlaybackList(int from, int to)
        {
            Log.Info("PrintPlaybackList:");
            for (int k = from; k <= to; k++)
                Debug.Write(PlaybackList.Items[k].GetMusic().Name + " ");
            Debug.Write("\n");
        }

        public static void MoveMusic(int from, int to, bool moveCurrentPlaylist = true)
        {
            if (from == to) return;
            if (moveCurrentPlaylist) CurrentPlaylist.Move(from, to);
            if (CurrentMusic.Index == from)
            {
                CurrentMusic.Index = to;
                if (from < to)
                {
                    for (int i = from + 1; i <= to; i++)
                    {
                        CurrentPlaylist[i].Index = i - 1;
                        MovePlaybackItem(i, i - 1);
                    }
                }
                else
                {
                    for (int i = to; i < from; i++)
                    {
                        CurrentPlaylist[i].Index = i + 1;
                        MovePlaybackItem(to, from);
                    }
                }
                CurrentPlaylist[to].Index = to;
            }
            else
            {
                MovePlaybackItem(from, to);
                for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                {
                    CurrentPlaylist[i].Index = i;
                }
            }
        }

        private static void MovePlaybackItem(int from, int to)
        {
            var item = PlaybackList.Items[from];
            PlaybackList.Items.RemoveAt(from);
            PlaybackList.Items.Insert(to, item);
        }

        public static bool RemoveMusic(Music music)
        {
            if (music == null || music.Index < 0) return false;
            try
            {
                CurrentPlaylist.RemoveAt(music.Index);
                PlaybackList.Items.RemoveAt(music.Index);
                for (int i = music.Index; i < CurrentPlaylist.Count; i++) CurrentPlaylist[i].Index = i;
                if (music.Index == CurrentMusic.Index) MoveNext();
                foreach (var listener in CurrentPlaylistChangedListeners)
                    listener.RemoveMusic(music);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("RemoveMusic Exception {0}", e);
                return false;
            }
        }

        public static void Clear()
        {
            if (CurrentPlaylist.IsEmpty()) return;
            Position = 0;
            CurrentMusic = null;
            CurrentPlaylist.Clear();
            PlaybackList.Items.Clear();
            foreach (var listener in CurrentPlaylistChangedListeners)
                listener.Cleared();
        }

        public static void SetMusicPlaying(IEnumerable<Music> playlist, Music playing)
        {
            if (playlist.IsEmpty()) return;
            foreach (var music in playlist)
                music.IsPlaying = music.Index >= 0 ? IsMusicPlaying(music) : music.Equals(playing);
        }

        public static async void RemoveBadMusic()
        {
            if (CurrentMusic != null && await CurrentMusic.GetStorageFileAsync() == null)
                CurrentMusic = null;
        }

        private static void MusicModified(Music before, Music after)
        {
            if (CurrentMusic == before)
                CurrentMusic?.CopyFrom(after);
        }

        void IMusicEventListener.Liked(Music music, bool isFavorite)
        {
            MusicModified(music, music);
        }

        void IMusicEventListener.Added(Music music) { }

        void IMusicEventListener.Removed(Music music)
        {
            RemoveMusic(music);
        }

        void IMusicEventListener.Modified(Music before, Music after)
        {
            MusicModified(before, after);
        }
    }

    public interface ICurrentPlaylistChangedListener
    {
        void AddMusic(Music music, int index);
        void RemoveMusic(Music music);
        void Cleared();
    }

    public interface IRemoveMusicListener
    {
        void MusicRemoved(int index, Music music, IEnumerable<Music> newCollection);
    }

    public interface ISwitchMusicListener
    {
        void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason);
    }

    public interface IMediaControlListener
    {
        void Tick();
        void MediaEnded();
    }

    public interface IMediaPlayerStateChangedListener
    {
        void StateChanged(MediaPlaybackState state);
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
