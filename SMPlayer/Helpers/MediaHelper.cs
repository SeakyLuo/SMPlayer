using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static ObservableCollection<Music> CurrentPlaylist = new ObservableCollection<Music>();

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
        private const string FILENAME = "NowPlayingPlaylist.json";

        public static async void Init()
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

            var playlist = JsonFileHelper.Convert<List<string>>(await JsonFileHelper.ReadAsync(FILENAME));
            if (playlist == null) return;
            while (Settings.settings == null) { System.Threading.Thread.Sleep(233); }
            var settings = Settings.settings;
            var hashset = MusicLibraryPage.AllSongs.ToHashSet();
            foreach (var music in playlist)
            {
                var target = hashset.FirstOrDefault((m) => m.Name == music);
                if (target == null) continue; // Reset Path Cause This
                target.IsPlaying = target.Equals(settings.LastMusic);
                await AddMusic(target);
            }
            Player.Volume = settings.Volume;
            MoveToMusic(settings.LastMusic);
            SetMode(settings.Mode);
        }
        public static void Save()
        {
            JsonFileHelper.SaveAsync(FILENAME, CurrentPlaylist.Select((m) => m.Name));
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
            }
            Settings.settings.Mode = mode;
            ShuffleEnabled = mode == PlayMode.Shuffle;
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
        public static async void AddMusic(ICollection<Music> playlist)
        {
            foreach (var music in playlist)
                await AddMusic(music);
        }

        public static async Task SetPlaylist(ICollection<Music> playlist, Music music = null)
        {
            int index = 0;
            if (CurrentPlaylist.Count > 0)
            {
                if (music == null) Clear();
                else
                {
                    Debug.WriteLine(music.Name);
                    foreach (var item in CurrentPlaylist.ToArray())
                    {
                        if (item.Equals(music)) index = 1;
                        else RemoveMusic(item);
                    }
                    Debug.WriteLine(new string('=', 20));
                }
            }
            foreach (var item in playlist.Skip(index))
                await AddMusic(item);
            if (!CurrentPlaylist.Contains(CurrentMusic))
                CurrentMusic = null;
        }

        public static async void SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            if (!Helper.SamePlaylist(CurrentPlaylist, playlist))
            {
                if (!music.Equals(CurrentMusic)) Pause();
                if (ShuffleEnabled) playlist = ShufflePlaylist(playlist, music);
                await SetPlaylist(playlist, music);
            }
            MoveToMusic(music);
            Play();
        }

        public static async void ShuffleAndPlay(ICollection<Music> playlist)
        {
            Pause();
            SetMode(PlayMode.Shuffle);
            await SetPlaylist(ShufflePlaylist(playlist));
            Play();
        }

        public static async void ShuffleOthers()
        {
            await SetPlaylist(ShufflePlaylist(CurrentPlaylist, CurrentMusic), CurrentMusic);
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

        public static void MoveMusic(Music music, int to)
        {
            int index = CurrentPlaylist.IndexOf(music);
            if (index == -1) return;
            MoveMusic(index, to);
        }

        public static void MoveMusic(int from, int to)
        {
            CurrentPlaylist.Move(from, to);
            var item = PlayBackList.Items[from];
            PlayBackList.Items.RemoveAt(from);
            PlayBackList.Items.Insert(to, item);
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

        public static void MoveToMusic(int index)
        {
            PlayBackList.MoveTo(Convert.ToUInt32(index));
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
        }

        public static void Clear()
        {
            CurrentPlaylist.Clear();
            PlayBackList.Items.Clear();
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

    public interface MediaControlListener
    {
        void Play();
        void Pause();
        void Tick();
        void MediaEnded();
    }
}
