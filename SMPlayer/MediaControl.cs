using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace SMPlayer
{
    public class MediaControl
    {
        public Music CurrentMusic;
        public List<Music> CurrentPlayList = new List<Music>();

        public MediaPlayer Player = new MediaPlayer();
        public MediaPlaybackList PlayList = new MediaPlaybackList();
        private DispatcherTimer Timer = new DispatcherTimer {  Interval = TimeSpan.FromSeconds(1) };
        private List<MediaControlListener> MediaControlListeners = new List<MediaControlListener>();

        public void Init()
        {
            foreach (var music in MusicLibraryPage.AllSongs)
            {
                PlayList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(music.Path))));
                CurrentPlayList.Add(music);
            }
            Timer.Tick += (sender, e) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.Tick();
            };
            Player.Source = PlayList;
            Player.MediaOpened += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaOpened();
            };
            Player.MediaEnded += (sender, args) =>
            {
                Music before = new Music(CurrentMusic);
                CurrentMusic.PlayCount += 1;
                foreach (var listener in MediaControlListeners)
                    listener.MediaEnded(before, CurrentMusic);
                switch (Settings.settings.Mode)
                {
                    case PlayMode.Once:
                        Player.Pause();
                        Timer.Stop();
                        break;
                    case PlayMode.Repeat:
                        NextMusic();
                        break;
                    case PlayMode.RepeatOne:
                        break;
                    case PlayMode.Shuffle:
                        NextMusic();
                        break;
                    default:
                        break;
                }
            };
            Player.MediaFailed += (sender, args) =>
            {
                foreach (var listener in MediaControlListeners)
                    listener.MediaFailed(args);
            };
            Player.Volume = Settings.settings.Volume;
            CurrentMusic = Settings.settings.LastMusic;
        }

        public void AddMediaControlListener(MediaControlListener listener)
        {
            MediaControlListeners.Add(listener);
        }

        public void Play()
        {
            Player.Play();
            Timer.Start();
        }

        public void Pause()
        {
            Player.Pause();
            Timer.Stop();
        }

        public Music PrevMusic()
        {
            PlayList.MovePrevious();
            return CurrentPlayList[(int)PlayList.CurrentItemIndex];
        }

        public Music NextMusic()
        {
            PlayList.MoveNext();
            return CurrentPlayList[(int)PlayList.CurrentItemIndex];
        }

        public void SetMusic(Music music)
        {
            CurrentMusic = music;
            PlayList.MoveTo((uint)CurrentPlayList.IndexOf(music));
        }

        public void Shuffle()
        {
            PlayList.ShuffleEnabled = true;
        }

        public void UnShuffle()
        {
            PlayList.ShuffleEnabled = false;
        }
    }

    public interface MediaControlListener
    {
        void Tick();
        void MediaOpened();
        void MediaEnded(Music before, Music after);
        void MediaFailed(MediaPlayerFailedEventArgs args);
    }
}
