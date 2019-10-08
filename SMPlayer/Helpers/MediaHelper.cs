﻿using SMPlayer.Models;
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
        public static DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        public static List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();
        public static List<SwitchMusicListener> SwitchMusicListeners = new List<SwitchMusicListener>();
        private const string FILENAME = "NowPlayingPlaylist.json";

        public static async void Init()
        {
            var playlist = JsonFileHelper.Convert<List<string>>(await JsonFileHelper.ReadAsync(FILENAME));
            if (playlist == null || playlist.Count == 0) return;
            while (Settings.settings == null) { System.Threading.Thread.Sleep(233); }
            var settings = Settings.settings;
            var hashset = MusicLibraryPage.AllSongs.ToHashSet();
            if (settings.LastMusic == null)
                settings.LastMusic = hashset.FirstOrDefault((m) => m.Name == playlist[0]);
            foreach (var music in playlist)
            {
                var target = hashset.FirstOrDefault((m) => m.Name == music);
                if (target == null) continue; // Reset Path Cause This
                target.IsPlaying = target.Equals(settings.LastMusic);
                await AddMusic(target);
            }
            Player.Volume = settings.Volume;
            MoveToMusic(settings.LastMusic);
            SetMode(Settings.settings.Mode);

            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            _PlaybackList.CurrentItemChanged += (sender, args) =>
            {
                if (sender.CurrentItemIndex >= CurrentPlaylist.Count) return;
                Music current = CurrentMusic?.Copy(), next = args.NewItem.GetMusic();
                foreach (var listener in SwitchMusicListeners)
                    listener.MusicSwitching(current, next, args.Reason);
                CurrentMusic = next;
                settings.LastMusic = next;
            };
            Player.MediaEnded += (sender, args) =>
            {
                Timer.Stop();
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
            JsonFileHelper.SaveAsync(FILENAME, CurrentPlaylist.Select((m) => m.Name));
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

        public static async Task AddMusic(Music music)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(music.Path);
                var source = MediaSource.CreateFromStorageFile(file);
                music.IsPlaying = false;
                source.CustomProperties.Add("Source", music);
                var item = new MediaPlaybackItem(source);
                PlaybackList.Items.Add(item);
                CurrentPlaylist.Add(music);
            }
            catch (System.IO.FileNotFoundException)
            {
                
            }
        }
        public static async void AddMusic(ICollection<Music> playlist)
        {
            foreach (var music in playlist)
                await AddMusic(music);
        }

        public static async Task SetPlaylist(ICollection<Music> playlist)
        {
            if (CurrentPlaylist.Count > 0) Clear();
            foreach (var item in playlist)
                await AddMusic(item);
            if (!CurrentPlaylist.Contains(CurrentMusic))
                CurrentMusic = null;
        }

        public static void SetMusicAndPlay(Music music)
        {
            CurrentPlaylist.Clear();
            CurrentPlaylist.Add(music);
            MoveToMusic(music);
            Play();
        }

        public static async void SetMusicAndPlay(ICollection<Music> playlist, Music music)
        {
            if (!Helper.SamePlaylist(CurrentPlaylist, playlist))
            {
                if (!music.Equals(CurrentMusic)) Pause();
                if (ShuffleEnabled) await SetPlaylist(ShufflePlaylist(playlist, music));
                else await SetPlaylist(playlist);
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
            PendingPlaybackList = new MediaPlaybackList();
            var playlist = ShufflePlaylist(CurrentPlaylist, CurrentMusic);
            CurrentPlaylist.Clear();
            foreach (var item in playlist)
                await AddMusic(item);
            CurrentPlaylist[0].IsPlaying = true;
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
                PlaybackList.MovePrevious();
        }

        public static void NextMusic()
        {
            if (Player.IsLoopingEnabled)
                Player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            else
                PlaybackList.MoveNext();
        }

        public static void MoveToMusic(int index)
        {
            PlaybackList.MoveTo(Convert.ToUInt32(index));
        }

        public static void MoveToMusic(Music music)
        {
            if (music == null) return;
            for (int i = 0; i < CurrentPlaylist.Count; i++)
            {
                if (CurrentPlaylist[i].Equals(music))
                {
                    PlaybackList.MoveTo(Convert.ToUInt32(i));
                    Debug.WriteLine("MediaControl: " + music.Name);
                    break;
                }
            }
        }

        public static void RemoveMusic(Music music)
        {
            RemoveMusic(CurrentPlaylist.IndexOf(music));
        }

        public static void RemoveMusic(int index)
        {
            if (index == -1) return;
            Music music = CurrentPlaylist[index];
            if (music.Equals(CurrentMusic)) CurrentMusic = null;
            CurrentPlaylist.RemoveAt(index);
            PlaybackList.Items.RemoveAt(index);
            foreach (var listener in RemoveMusicListeners) listener.MusicRemoved(index, music);
        }

        public static void Clear()
        {
            CurrentMusic = null;
            CurrentPlaylist.Clear();
            PlaybackList.Items.Clear();
            foreach (var listener in RemoveMusicListeners) listener.MusicRemoved(-1, CurrentMusic);
        }

        public static void FindMusicAndSetPlaying(ICollection<Music> playlist, Music current, Music next)
        {
            foreach (var music in playlist)
                music.IsPlaying = music.Equals(next);
            //bool findCurrent = current == null, findNext = next == null;
            //foreach (var music in playlist)
            //{
            //    if (!findCurrent && (findCurrent = music.Equals(current)))
            //        music.IsPlaying = false;
            //    if (!findNext && (findNext = music.Equals(next)))
            //        music.IsPlaying = true;
            //    if (findCurrent && findNext) return;
            //}
        }
    }

    public interface RemoveMusicListener
    {
        void MusicRemoved(int index, Music music);
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
}
