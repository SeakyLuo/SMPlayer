using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public class MusicPlayer : IMusicEventListener
    {
        public static bool ShuffleEnabled { get; private set; }
        public static Music CurrentMusic => PlaybackList.CurrentItem.GetMusic();
        public static IEnumerable<Music> CurrentPlaylist => PlaybackList.Items.Select(i => i.GetMusic()).ToList();
        public static int CurrentPlaylistCount => PlaybackList.Items.Count;
        public static int CurrentIndex { get; private set; } = -1;

        public static double Position
        {
            get => Player.PlaybackSession.Position.TotalSeconds;
            set => Player.PlaybackSession.Position = TimeSpan.FromSeconds(value);
        }
        public static double Progress
        {
            get
            {
                Music music = CurrentMusic;
                return music == null ? 0d : Position / music.Duration;
            }
        }
        public static MediaPlaybackState PlaybackState => Player.PlaybackSession.PlaybackState;
        public static bool IsPlaying => PlaybackState == MediaPlaybackState.Playing;
        public static Playlist NowPlaying => new Playlist(MenuFlyoutHelper.NowPlaying, CurrentPlaylist);
        private static MediaPlaybackList PlaybackList = new MediaPlaybackList() { MaxPlayedItemsToKeepOpen = 1 };
        public static MediaPlayer Player = new MediaPlayer() { Source = PlaybackList };
        private static List<IMusicPlayerEventListener> MusicPlayerEventListeners = new List<IMusicPlayerEventListener>();
        public static void AddMusicPlayerEventListener(IMusicPlayerEventListener listener) { MusicPlayerEventListeners.Add(listener); }
        public static List<Action> InitFinishedListeners = new List<Action>();
        public const string JsonFilename = "NowPlaying";

        public static void Init(Music music = null)
        {
            MusicService.AddMusicEventListener(new MusicPlayer());
            MainPage.AddMainPageLoadedListener(async () =>
            {
                await InitWithMusic(music);
            });
        }

        private static async Task InitWithMusic(Music music = null)
        {
            var settings = Settings.settings;
            if (music == null)
            {
                var playlist = await JsonFileHelper.ReadObjectAsync<List<long>>(JsonFilename);
                if (playlist.IsNotEmpty())
                {
                    if (settings.LastMusicIndex == -1)
                        settings.LastMusicIndex = 0;
                    SetPlaylist(MusicService.FindMusicList(playlist));
                    if (settings.LastMusicIndex < CurrentPlaylistCount)
                        CurrentIndex = settings.LastMusicIndex;
                }
            }
            if (music != null)
            {
                AddMusic(music);
            }
            else if (CurrentIndex != -1)
            {
                try
                {
                    PlaybackList.MoveTo(Convert.ToUInt32(CurrentIndex));
                }
                catch (Exception)
                {
                    // 无效索引
                }
            }
            Player.Volume = settings.Volume;
            // 如果非文件启动，并保存播放进度且有音乐
            if (music == null && settings.SaveMusicProgress && CurrentPlaylist.IsNotEmpty()) Position = settings.MusicProgress;
            SetMode(settings.Mode);

            PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                if (args.Reason == MediaPlaybackItemChangedReason.EndOfStream)
                {
                    SettingsService.Played(CurrentMusic);
                }
                Music next = args.NewItem.GetMusic();
                try
                {
                    Log.Info("Next Music {0}", next?.Path);
                    UpdateUniversalVolumeControl(next);
                    var switchArgs = new MusicPlayerMusicSwitchEventArgs(args.Reason) { Music = next  };
                    foreach (var listener in MusicPlayerEventListeners)
                    {
                        try
                        {
                            listener?.Execute(switchArgs);
                        }
                        catch (Exception e)
                        {
                            Log.Warn("MusicPlayerEventListeners Exception {0}", e);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Collection was modified; enumeration operation may not execute.
                }
                CurrentIndex = Settings.settings.LastMusicIndex = (int)PlaybackList.CurrentItemIndex;
                App.Save();
            };
            Player.PlaybackSession.PlaybackStateChanged += (sender, args) =>
            {
                try
                {
                    lock (MusicPlayerEventListeners)
                    {
                        foreach (var listener in MusicPlayerEventListeners)
                            listener.Execute(new MusicPlayerStateChangedEventArgs(sender.PlaybackState));
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"Player.PlaybackSession.PlaybackStateChanged failed {e}");
                }
            };
            Player.MediaEnded += (sender, args) =>
            {
                try
                {
                    SettingsService.Played(CurrentMusic);
                    lock (MusicPlayerEventListeners)
                    {
                        foreach (var listener in MusicPlayerEventListeners)
                            listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.MediaEnded));
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"Player.MediaEnded failed {e}");
                }
            };
            foreach (var listener in InitFinishedListeners)
                listener.Invoke();
            if (settings.AutoPlay || music != null) Play();
        }

        public static void Save()
        {
            var ids = CurrentPlaylist.Select(m => m.Id);
            JsonFileHelper.Save(JsonFilename, ids);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, ids);
        }

        private static async void UpdateUniversalVolumeControl(Music music)
        {
            SystemMediaTransportControlsDisplayUpdater updater = Player.SystemMediaTransportControls.DisplayUpdater;
            if (music == null)
            {
                updater.ClearAll();
            }
            else
            {
                await updater.CopyFromFileAsync(MediaPlaybackType.Music, await music.GetStorageFileAsync());
            }
            try
            {
                updater.Update();
            }
            catch (Exception)
            {
                // 参数错误。未提供用于复制媒体元数据的 StorageFile。
            }
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

        public static int AddMusic(IMusicable source, int index)
        {
            Music music = source.ToMusic();
            PlaybackList.Items.Insert(index, music.GetMediaPlaybackItem());
            foreach (var listener in MusicPlayerEventListeners)
                listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.Add){ Index = index, Music = music});
            return index;
        }

        public static void AddMusicAndPlay(IMusicable source, int index)
        {
            MoveToMusic(AddMusic(source, index));
            Play();
        }

        public static void AddMusicAndPlay(IMusicable source)
        {
            MoveToMusic(AddMusic(source));
            Play();
        }

        public static int AddMusic(IMusicable source)
        {
            return AddMusic(source, CurrentPlaylistCount);
        }

        private static void SetPlaylist(IEnumerable<IMusicable> playlist, IMusicable target = null)
        {
            Clear();
            foreach (var music in playlist) AddMusic(music);
            MoveToMusic(target);
        }

        // 跟SetMusicAndPlay的区别是不需要Shuffle
        public static void SetPlaylistAndPlay(IEnumerable<IMusicable> playlist, IMusicable target = null)
        {
            SetPlaylist(playlist, target);
            Play();
        }

        public static void SetMusicAndPlay(IMusicable music)
        {
            Clear();
            AddMusic(music);
            Play();
        }

        public static void SetMusicAndPlay(IEnumerable<IMusicable> playlist, IMusicable music = null)
        {
            if (!playlist.SameAs(CurrentPlaylist))
                SetPlaylist(ShuffleEnabled ? ShufflePlaylist(playlist, music) : playlist);
            MoveToMusic(music);
            Play();
        }

        // 跟SetMusicAndPlay的区别是强制Shuffle
        public static void ShuffleAndPlay(IEnumerable<IMusicable> playlist)
        {
            if (playlist.IsEmpty()) return;
            SetPlaylist(ShufflePlaylist(playlist));
            Play();
        }

        public static void ShuffleAndPlay()
        {
            ShuffleAndPlay(CurrentPlaylist);
        }

        public static void QuickPlay(int randomLimit = 100)
        {
            SetPlaylistAndPlay(QuickPlayHelper.GetPlaylist(randomLimit));
        }

        public static void ShuffleOthers()
        {
            int currentPlaylistCount = CurrentPlaylistCount;
            if (currentPlaylistCount == 0) return;
            // Creating a new MediaPlaybackList here is because removing old music
            var playlist = ShufflePlaylist(CurrentPlaylist, CurrentMusic);
            int currentIndex = CurrentIndex;
            // 删掉前面的
            for (int i = 0; i < currentIndex; i++)
            {
                RemoveMusic(0);
            }
            // 加到后面
            foreach (var music in playlist.Skip(1))
            {
                AddMusic(music);
            }
            // 删掉后面的
            for (int i = 1; i < currentPlaylistCount - currentIndex; i++)
            {
                RemoveMusic(1);
            }
        }

        public static IEnumerable<IMusicable> ShufflePlaylist(IEnumerable<IMusicable> playlist, IMusicable start = null)
        {
            var list = playlist.Shuffle();
            if (start != null)
            {
                list.Remove(start);
                list.Insert(0, start);
            }
            return list;
        }

        public static bool MoveToMusic(IMusicable musicable)
        {
            try
            {
                if (musicable != null)
                {
                    Music music = musicable.ToMusic();
                    for (int i = 0; i < CurrentPlaylistCount; i++)
                    {
                        if (music == GetMusicAt(i))
                        {
                            PlaybackList.MoveTo(Convert.ToUInt32(i));
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // 无效索引
            }
            return false;
        }

        public static bool MoveToMusic(int index)
        {
            try
            {
                if (-1 < index && index < CurrentPlaylistCount)
                {
                    PlaybackList.MoveTo(Convert.ToUInt32(index));
                    return true;
                }
            }
            catch (Exception e)
            {
                // 无效索引
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
                catch (Exception e)
                {
                    // 无效索引
                    Log.Warn("MovePrevious Exception {0}", e);
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
                catch (Exception e)
                {
                    // 无效索引
                    Log.Warn("MovePrevious Exception {0}", e);
                }
            }
        }
        private static void PrintPlaybackList(int from, int to)
        {
            Log.Debug("PrintPlaybackList:");
            for (int k = from; k <= to; k++)
                Debug.Write(GetMusicAt(k).Name + " ");
            Debug.Write("\n");
        }

        public static void MoveMusic(int from, int to)
        {
            if (from == to) return;
            MovePlaybackItem(from, to);
            foreach (var listener in MusicPlayerEventListeners)
                listener.Execute(new MusicPlayerMoveEventArgs(from, to));
        }

        private static void MovePlaybackItem(int from, int to)
        {
            var item = PlaybackList.Items[from];
            PlaybackList.Items.RemoveAt(from);
            PlaybackList.Items.Insert(to, item);
        }

        public static bool RemoveMusic(Music music)
        {
            if (music == null) return false;
            try
            {
                PlaybackList.Items.RemoveAll(i => i.GetMusic() == music);
                foreach (var listener in MusicPlayerEventListeners)
                    listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.Remove) { Music = music });
                return true;
            }
            catch (Exception e)
            {
                Log.Warn("RemoveMusic Exception {0}", e);
                return false;
            }
        }

        public static bool RemoveMusic(int index)
        {
            if (index < 0) return false;
            try
            {
                Music removed = GetMusicAt(index);
                PlaybackList.Items.RemoveAt(index);
                foreach (var listener in MusicPlayerEventListeners)
                    listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.Remove) { Index = index, Music = removed});
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
            if (CurrentPlaylistCount == 0) return;
            Position = 0;
            CurrentIndex = -1;
            PlaybackList.Items.Clear();
            foreach (var listener in MusicPlayerEventListeners)
                listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.Clear));
        }

        public static Music GetMusicAt(int index) => PlaybackList.Items[index].GetMusic();

        public static void SetMusicPlaying(IEnumerable<MusicView> playlist, IMusicable playing)
        {
            if (playlist.IsEmpty()) return;
            foreach (var music in playlist)
                music.IsPlaying = music.Index >= 0 ? music.Index == CurrentIndex : music.Equals(playing);
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Remove:
                    PlaybackList.Items.RemoveAll(i => i.GetMusic() == music);
                    break;
                case MusicEventType.Modify:
                    foreach (var item in PlaybackList.Items)
                    {
                        if (item.GetMusic() == music)
                        {
                            item.SetMusic(music);
                        }
                    }
                    break;
            }
        }
    }
}