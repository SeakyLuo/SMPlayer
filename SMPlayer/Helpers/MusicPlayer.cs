using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public class MusicPlayer : IMusicEventListener
    {
        public const string JsonFilename = "NowPlaying";
        public static bool ShuffleEnabled { get; private set; }
        public static Music CurrentMusic => PlaybackList.CurrentItem.GetMusic();
        public static IEnumerable<Music> CurrentPlaylist => PlaybackList.Items.Select(i => i.GetMusic());
        public static int CurrentPlaylistCount => PlaybackList.Items.Count;
        public static int CurrentIndex => CurrentPlaylistCount > 0 ? (int) PlaybackList.CurrentItemIndex : -1;

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
        public static PlayMode PlayMode
        {
            get
            {
                if (Player.IsLoopingEnabled)
                {
                    return PlayMode.RepeatOne;
                }
                if (!PlaybackList.AutoRepeatEnabled)
                {
                    return PlayMode.Once;
                }
                return ShuffleEnabled ? PlayMode.Shuffle : PlayMode.Repeat;
            }
            set
            {
                switch (value)
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
                Settings.settings.Mode = value;
                ShuffleEnabled = value == PlayMode.Shuffle;
            }
        }
        public static MediaPlaybackState PlaybackState => Player.PlaybackSession.PlaybackState;
        public static bool IsPlaying => PlaybackState == MediaPlaybackState.Playing;
        public static Playlist NowPlaying => new Playlist(MenuFlyoutHelper.NowPlayingPlaylistName, CurrentPlaylist);
        private static MediaPlaybackList PlaybackList = new MediaPlaybackList() { MaxPlayedItemsToKeepOpen = 1 };
        public static MediaPlayer Player = new MediaPlayer() { Source = PlaybackList };
        private static List<IMusicPlayerEventListener> MusicPlayerEventListeners = new List<IMusicPlayerEventListener>();
        public static void AddMusicPlayerEventListener(IMusicPlayerEventListener listener) { MusicPlayerEventListeners.Add(listener); }
        public static void RemoveMusicPlayerEventListener(IMusicPlayerEventListener listener) { MusicPlayerEventListeners.Remove(listener); }
        public static List<Action> InitFinishedListeners = new List<Action>();
        public static Music NextMusic => PlayMode == PlayMode.RepeatOne ? CurrentMusic : PlaybackList.Items[(CurrentIndex + 1) % CurrentPlaylistCount].GetMusic();

        public static void Init(Music music = null)
        {
            MusicService.AddMusicEventListener(new MusicPlayer());
            MainPage.AddMainPageLoadedListener(async () =>
            {
                try
                {
                    await InitWithMusic(music);
                }
                catch (Exception e)
                {
                    Log.Warn($"InitWithMusic failed {e}");
                }
            });
        }

        private static async Task LoadPlaylist(Music music = null)
        {
            var settings = Settings.settings;
            var playlist = await JsonFileHelper.ReadObjectAsync<List<string>>(JsonFilename);
            if (playlist.IsNotEmpty())
            {
                SetPlaylist(await MusicService.FindMusicList(playlist));
            }
            if (music == null)
            {
                MoveToMusic(settings.LastMusicIndex);
            }
            else
            {
                Log.Info("InitWithMusic: " + music.Path);
                AddMusic(music, CurrentIndex + 1);
            }
        }

        private static async Task InitWithMusic(Music music = null)
        {
            var settings = Settings.settings;
            try
            {
                await LoadPlaylist(music);
            }
            catch (Exception e)
            {
                Log.Warn($"LoadPlaylist failed {e}");
            }
            Player.Volume = settings.Volume;
            // 如果非文件启动，并保存播放进度且有音乐
            if (music == null && settings.SaveMusicProgress && CurrentPlaylist.IsNotEmpty()) Position = settings.MusicProgress;
            PlayMode = settings.Mode;
            PlaybackList.CurrentItemChanged += OnMusicChanged;
            Player.PlaybackSession.PlaybackStateChanged += OnPlaybackStateChanged;
            Player.MediaEnded += OnMediaEnded;
            foreach (var listener in InitFinishedListeners)
                listener.Invoke();
            if (settings.AutoPlay || music != null) Play();
        }

        public static void Save()
        {
            var ids = CurrentPlaylist.Select(m => m.Path).ToList();
            JsonFileHelper.Save(JsonFilename, ids);
            JsonFileHelper.SaveAsync(Helper.TempFolder, JsonFilename + Helper.TimeStamp, ids);
        }

        private static void OnMusicChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (args.Reason == MediaPlaybackItemChangedReason.EndOfStream)
            {
                SettingsService.Played(args.OldItem.GetMusic());
            }
            Music next = args.NewItem.GetMusic();
            try
            {
                Log.Debug($"Next Music {next?.Path}");
                UpdateUniversalVolumeControl(next);
                NotifyListeners(new MusicPlayerMusicSwitchEventArgs(args.Reason) { Music = next });
            }
            catch (Exception e)
            {
                Log.Warn($"OnMusicChanged failed {e}");
            }
            Settings.settings.LastMusicIndex = (int)PlaybackList.CurrentItemIndex;
            App.Save();
        }

        private static void OnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            try
            {
                NotifyListeners(new MusicPlayerStateChangedEventArgs(sender.PlaybackState));
            }
            catch (Exception e)
            {
                Log.Warn($"OnPlaybackStateChanged failed {e}");
            }
        }

        private static void OnMediaEnded(MediaPlayer sender, object args)
        {
            try
            {
                SettingsService.Played(CurrentMusic);
                NotifyListeners(new MusicPlayerEventArgs(MusicPlayerEventType.MediaEnded));
            }
            catch (Exception e)
            {
                Log.Warn($"OnMediaEnded failed {e}");
            }
        }

        private static void NotifyListeners(MusicPlayerEventArgs args)
        {
            lock (MusicPlayerEventListeners)
            {
                foreach (var listener in MusicPlayerEventListeners)
                {
                    try
                    {
                        listener?.Execute(args);
                    }
                    catch (Exception e)
                    {
                        Log.Warn($"MusicPlayerEventListener.Execute Exception {e}");
                    }
                }
            }
        }

        private static async void UpdateUniversalVolumeControl(Music music)
        {
            try
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
                updater.Update();
            }
            catch (Exception)
            {
                // 参数错误。未提供用于复制媒体元数据的 StorageFile。
            }
        }

        public static void AddNextAndPlay(IMusicable source)
        {
            if (source == null) return;
            if (!MoveToMusic(source))
            {
                MoveToMusic(AddMusic(source, CurrentIndex + 1));
            }
            Play();
        }

        public static int AddMusic(IMusicable source, int index)
        {
            Music music = source.ToMusic();
            PlaybackList.Items.Insert(index, music.GetMediaPlaybackItem());
            foreach (var listener in MusicPlayerEventListeners)
                listener.Execute(new MusicPlayerEventArgs(MusicPlayerEventType.Add){ Index = index, Music = music});
            return index;
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
            if (musicable == null)
            {
                return false;
            }
            try
            {
                Music music = musicable.ToMusic();
                for (int i = 0; i < CurrentPlaylistCount; i++)
                {
                    Music m = GetMusicAt(i);
                    if (music == GetMusicAt(i))
                    {
                        PlaybackList.MoveTo(Convert.ToUInt32(i));
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                // 无效索引
                Log.Warn($"move to music exception {e}");
            }
            return false;
        }

        public static void MoveToMusicOrPlay(IMusicable musicable, int index)
        {
            Music music = musicable.ToMusic();
            if (0 <= index && index < CurrentPlaylistCount && PlaybackList.Items[index].GetMusic() == music)
            {
                MoveToMusic(index);
                Play();
                return;
            }
            if (CurrentMusic == music)
            {
                Play();
                return;
            }
            int playlistIndex = CurrentPlaylist.FindIndex(i => i == music);
            if (playlistIndex == -1)
            {
                AddNextAndPlay(musicable);
            }
            else
            {
                MoveToMusic(playlistIndex);
                Play();
                return;
            }
        }

        public static bool MoveToMusic(int index)
        {
            if (CurrentIndex == index)
            {
                return true;
            }
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
                Log.Warn($"move to music {index} exception {e}");
            }
            return false;
        }

        public static void Play()
        {
            if (CurrentPlaylistCount == 0) return;
            try
            {
                Player.Play();
            }
            catch(Exception e)
            {
                Log.Warn($"Play exception {e}");
            }
        }

        public static void Pause()
        {
            try
            {
                Player.Pause();
            }
            catch (Exception e)
            {
                Log.Warn($"Pause exception {e}");
            }
        }

        public static void MovePrev()
        {
            try
            {
                PlaybackList.MovePrevious();
            }
            catch (Exception e)
            {
                // 无效索引
                Log.Warn($"MovePrevious Exception {e}");
            }
        }

        public static void MoveNext()
        {
            try
            {
                PlaybackList.MoveNext();
            }
            catch (Exception e)
            {
                // 无效索引
                Log.Warn($"MoveNext Exception {e}");
            }
        }

        private static void PrintPlaybackList(int from, int to)
        {
            Log.Debug("PrintPlaybackList:");
            for (int k = from; k <= to; k++)
                Debug.Write(GetMusicAt(k).Name + " ");
            Debug.Write("\n");
        }

        public static void PlayNext(Music target, int index)
        {
            if (index >= 0 && PlaybackList.Items[index].GetMusic() == target)
            {
                int currentIndex = CurrentIndex;
                MoveMusic(index, currentIndex + (index < currentIndex ? 0 : 1));
            }
            else
            {
                AddMusic(target, CurrentIndex + 1);
            }
        }

        public static void MoveMusic(int from, int to)
        {
            if (from == to) return;
            int currentIndex = CurrentIndex;
            Log.Debug($"MoveMusic current {currentIndex} from {from} to {to}");
            if (currentIndex == from)
            {
                // 避免当前的歌被暂停播放，所以移动其他歌曲
                for (int i = 0; i < Math.Abs(from - to); i++)
                {
                    var item = PlaybackList.Items[to];
                    PlaybackList.Items.RemoveAt(to);
                    PlaybackList.Items.Insert(from, item);
                }
            }
            else
            {
                var item = PlaybackList.Items[from];
                PlaybackList.Items.RemoveAt(from);
                PlaybackList.Items.Insert(to, item);
            }
            foreach (var listener in MusicPlayerEventListeners)
                listener.Execute(new MusicPlayerMoveEventArgs(from, to));
        }

        public static bool RemoveMusic(int index)
        {
            if (index < 0) return false;
            try
            {
                Music removed = GetMusicAt(index);
                PlaybackList.Items.RemoveAt(index);
                Settings.settings.LastMusicIndex = CurrentIndex;
                MusicPlayerEventArgs args = new MusicPlayerEventArgs(MusicPlayerEventType.Remove) { Index = index, Music = removed };
                foreach (var listener in MusicPlayerEventListeners)
                    listener.Execute(args);
                return true;
            }
            catch (Exception e)
            {
                Log.Warn($"RemoveMusic Exception {e}");
                return false;
            }
        }

        public static void Clear()
        {
            if (CurrentPlaylistCount == 0) return;
            Position = 0;
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