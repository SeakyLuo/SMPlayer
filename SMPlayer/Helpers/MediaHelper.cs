using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    settings.LastMusic = MusicLibraryPage.AllSongs.FirstOrDefault((m) => m.Path == playlist[0]);
                CurrentMusic = settings.LastMusic;
                foreach (var path in playlist)
                {
                    var target = MusicLibraryPage.AllSongs.FirstOrDefault((m) => m.Path == path);
                    if (target == null) continue; // Reset Path Cause This
                    target.IsPlaying = target.Equals(CurrentMusic);
                    try
                    {
                        // 无效索引
                        await AddMusic(target);
                    }
                    catch (Exception)
                    {
                        break;
                    }
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
            _PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                if (PendingPlaybackList != null)
                {
                    _PlaybackList.Items.Clear();
                    foreach (var item in PendingPlaybackList.Items)
                        _PlaybackList.Items.Add(item);
                    PendingPlaybackList = null;
                    MoveToMusic(1);
                    Player.Play();
                    return;
                }
                if (sender.CurrentItemIndex >= CurrentPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.GetMusic();
                foreach (var listener in SwitchMusicListeners)
                    listener.MusicSwitching(current, next, args.Reason);
                settings.LastMusic = CurrentMusic = next;
            };
            Player.MediaEnded += (sender, args) =>
            {
                Timer.Stop();
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded();
            };
            MoveToMusic(CurrentMusic);
        }
        public static Music GetMusic(this MediaPlaybackItem item)
        {
            return item.Source.CustomProperties["Source"] as Music;
        }
        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, CurrentPlaylist.Select((m) => m.Path));
        }
        public static void SetMode(PlayMode mode)
        {
            switch (mode)
            {
                case PlayMode.Once:
                    Player.IsLoopingEnabled = false;
                    PlaybackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Repeat:
                    Player.IsLoopingEnabled = false;
                    PlaybackList.AutoRepeatEnabled = true;
                    break;
                case PlayMode.RepeatOne:
                    Player.IsLoopingEnabled = true;
                    PlaybackList.AutoRepeatEnabled = false;
                    break;
                case PlayMode.Shuffle:
                    Player.IsLoopingEnabled = false;
                    PlaybackList.AutoRepeatEnabled = true;
                    break;
            }
            Settings.settings.Mode = mode;
            ShuffleEnabled = mode == PlayMode.Shuffle;
        }
        public static async Task<bool> InsertMusic(Music music, int index = -1)
        {
            try
            {
                var item = await music.GetMediaPlaybackItemAsync();
                music.IsPlaying = false;
                if (index < 0)
                {
                    PlaybackList.Items.Add(item);
                    CurrentPlaylist.Add(music);
                }
                else
                {
                    PlaybackList.Items.Insert(index, item);
                    CurrentPlaylist.Insert(index, music);
                }
                return true;
            }
            catch (System.IO.FileNotFoundException)
            {
                return false;
            }
        }

        public static async Task<bool> AddMusic(Music music)
        {
            return await InsertMusic(music);
        }
        private static ExecutionStatus status = ExecutionStatus.Ready;
        private static int SetPlaylistCounter = 0;
        public static async Task<AddMusicResult> SetPlaylist(ICollection<Music> playlist)
        {
            SetPlaylistCounter++;
            if (status == ExecutionStatus.Ready) status = ExecutionStatus.Running;
            else if (status == ExecutionStatus.Running) while (SetPlaylistCounter > 1) { }
            if (CurrentPlaylist.Count > 0) Clear();
            var result = new AddMusicResult();
            foreach (var item in playlist)
            {
                if (SetPlaylistCounter > 1)
                {
                    SetPlaylistCounter--;
                    return result.Break();
                }
                else
                {
                    result.Check(await AddMusic(item), item);
                }
            }
            if (!CurrentPlaylist.Contains(CurrentMusic))
                CurrentMusic = CurrentPlaylist[0];
            SetPlaylistCounter--;
            status = ExecutionStatus.Ready;
            return result;
        }
        public static async Task<AddMusicResult> AddMusic(ICollection<Music> playlist)
        {
            SetPlaylistCounter++;
            if (status == ExecutionStatus.Ready) status = ExecutionStatus.Running;
            else if (status == ExecutionStatus.Running) while (SetPlaylistCounter > 1) { }
            var result = new AddMusicResult();
            foreach (var item in playlist)
            {
                if (SetPlaylistCounter > 1)
                {
                    if (CurrentPlaylist.Count > 0) Clear();
                    SetPlaylistCounter--;
                    return result.Break();
                }
                else
                {
                    result.Check(await AddMusic(item), item);
                }
            }
            SetPlaylistCounter--;
            status = ExecutionStatus.Ready;
            return result;
        }

        public static async Task<bool> SetMusicAndPlay(Music music)
        {
            if (CurrentPlaylist.Count > 0) Clear();
            bool successful = await AddMusic(music);
            if (successful) Play();
            return successful;
        }

        public static async Task<AddMusicResult> SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            var result = new AddMusicResult();
            if (Helper.SamePlaylist(CurrentPlaylist, playlist))
            {
                MoveToMusic(music);
                if (!IsPlaying) Play();
                return result;
            }
            else
            {
                Clear();
                if (result.Check(await AddMusic(music), music))
                {
                    if (CurrentPlaylist[0] != music) MoveToMusic(music);
                    if (!IsPlaying) Play();
                    var target = playlist.ToList();
                    if (ShuffleEnabled)
                    {
                        target.Remove(music);
                        return await AddMusic(ShufflePlaylist(target));
                    }
                    else
                    {
                        int index = target.IndexOf(music);
                        foreach (var m in target.GetRange(0, index))
                            result.Check(await InsertMusic(m, CurrentPlaylist.Count - 1), m);
                        foreach (var m in target.GetRange(index + 1, target.Count - index - 1))
                            result.Check(await AddMusic(m), m);
                    }
                }
                return result;
            }
        }

        public static async Task<AddMusicResult> ShuffleAndPlay(ICollection<Music> playlist)
        {
            SetMode(PlayMode.Shuffle);
            Play();
            var result = await SetPlaylist(ShufflePlaylist(playlist));
            return result;
        }

        public static async Task<AddMusicResult> ShuffleOthers()
        {
            PendingPlaybackList = new MediaPlaybackList();
            var playlist = ShufflePlaylist(CurrentPlaylist, CurrentMusic);
            CurrentPlaylist.Clear();
            var result = new AddMusicResult();
            foreach (var item in playlist)
                result.Check(await AddMusic(item), item);
            CurrentPlaylist[0].IsPlaying = true;
            return result;
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

        public static void MoveMusic(Music music, int to)
        {
            int index = CurrentPlaylist.IndexOf(music);
            if (index == -1) return;
            CurrentPlaylist.Move(index, to);
            MoveMusic(index, to);
        }

        public static void MoveMusic(int from, int to)
        {
            var item = PlaybackList.Items[from];
            PlaybackList.Items.RemoveAt(from);
            PlaybackList.Items.Insert(to, item);
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

        public static void PrevMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlaybackList.MovePrevious();
        }

        public static void NextMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlaybackList.MoveNext();
        }

        public static bool MoveToMusic(int index)
        {
            try
            {
                PlaybackList.MoveTo(Convert.ToUInt32(index));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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

        public static bool RemoveMusic(Music music)
        {
            return RemoveMusic(CurrentPlaylist.IndexOf(music));
        }

        public static bool RemoveMusic(int index)
        {
            if (index == -1) return false;
            Music music = CurrentPlaylist[index];
            if (music == CurrentMusic) CurrentMusic = null;
            CurrentPlaylist.RemoveAt(index);
            PlaybackList.Items.RemoveAt(index);
            foreach (var listener in RemoveMusicListeners) listener.MusicRemoved(index, music, CurrentPlaylist);
            return true;
        }

        public static void Clear()
        {
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
        public bool IsSuccessful { get => Status == AddMusicStatus.Successful; }
        public bool IsFailed { get => Status == AddMusicStatus.Failed; }
        public int FailCount { get => Failed.Count; }
        public bool Check(bool successful, Music music)
        {
            if (!successful)
            {
                Failed.Add(music);
                Status = AddMusicStatus.Failed;
            }
            return successful;
        }
        public AddMusicResult Break()
        {
            Status = AddMusicStatus.Break;
            return this;
        }
    }
}
