using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using SMPlayer.Models.Enums;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace SMPlayer.Helpers
{
    public static class ToastHelper
    {
        public const string ToastTaskName = "ToastBackgroundTask", ToastTagPaused = "SMPlayerMediaToastTagPaused", ToastTagPlaying = "SMPlayerMediaToastTagPlaying", ToastGroup = "SMPlayerMediaToastGroup";

        private static ToastAudio SlientToast = new ToastAudio() { Silent = true };
        private static DispatcherTimer Timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        private static Dictionary<ToastKey, ToastNotification> CurrentToastMap = new Dictionary<ToastKey, ToastNotification>();
        private static ToastNotifier Notifier = ToastNotificationManager.CreateToastNotifier();
        private static LyricsSource CurrentLyricsSource = LyricsSource.Internet;

        public static void Init()
        {
            CurrentLyricsSource = Settings.settings.NotificationLyricsSource;
            Timer.Tick += (sender, e) =>
            {
                UpdateToast();
            };
            Timer.Start();
        }

        public static async Task SwitchLyricsSource(LyricsSource source)
        {
            Music currentMusic = MusicPlayer.CurrentMusic;
            if (currentMusic != null)
            {
                string lyrics;
                switch (source)
                {
                    case LyricsSource.Music:
                        lyrics = await currentMusic.GetLyricsAsync();
                        break;
                    case LyricsSource.LrcFile:
                        lyrics = await currentMusic.GetLrcLyricsAsync();
                        break;
                    default:
                        lyrics = await LyricsHelper.SearchLyrics(currentMusic);
                        break;
                }
                LyricsHelper.SetLyrics(currentMusic, lyrics);
            }
            await ShowToast(currentMusic, MusicPlayer.PlaybackState, source);
        }

        public static async Task ShowToast(Music music, MediaPlaybackState state)
        {
            await ShowToast(music, state, Settings.settings.NotificationLyricsSource);
        }

        public static async Task ShowToast(Music music, MediaPlaybackState state, LyricsSource lyricsSource)
        {
            if (!App.Inited) return;
            if (music == null ||
                Settings.settings.NotificationSend == NotificationSendMode.Never ||
                (state != MediaPlaybackState.Playing && state != MediaPlaybackState.Paused)) return;
            NotificationDisplayMode display = Settings.settings.NotificationDisplay;
            if (Window.Current.Visible && (display == NotificationDisplayMode.Normal || display == NotificationDisplayMode.Quick))
                return;
            if (IsToastActive(music, state) && CurrentLyricsSource == lyricsSource) return;
            CurrentLyricsSource = lyricsSource;
            ToastNotification toast;
            try
            {
                toast = await BuildToast(music, state, display, lyricsSource);
            }
            catch (Exception e)
            {
                Log.Warn($"ShowToast.BuildToast Failed {e}");
                return;
            }
            lock (CurrentToastMap)
            {
                HideToast();
                CurrentToastMap.TryAdd(new ToastKey { Music = music, State = state }, toast);
                try
                {
                    Notifier.Show(toast);
                    Log.Debug($"show toast, music: {music.Name}, state: {state}");
                }
                catch (Exception)
                {
                    // 通知已发布
                }
            }
        }

        private static void UpdateToast()
        {
            if (!MusicPlayer.IsPlaying || CurrentToastMap.Count == 0) return;
            if (!IsToastActive(MusicPlayer.CurrentMusic, MediaPlaybackState.Playing)) return;
            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData { SequenceNumber = 0 };
            data.Values["MediaControlPosition"] = MusicPlayer.Progress.ToString();
            data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MusicPlayer.Position);
            data.Values["Lyrics"] = Settings.settings.ShowLyricsInNotification ? LyricsHelper.GetLyrics() : "";

            // Update the existing notification's data by using tag/group
            Notifier.Update(data, ToastTagPaused, ToastGroup);
        }

        private static bool IsToastActive(Music music, MediaPlaybackState state)
        {
            lock (CurrentToastMap)
            {
                return CurrentToastMap.ContainsKey(new ToastKey { Music = music, State = state });
            }
        }

        public static void HideToast()
        {
            foreach (var e in CurrentToastMap)
            {
                try
                {
                    Notifier.Hide(e.Value);
                    Log.Info($"hide toast, music: {e.Key.Music.Name}, state: {e.Key.State}");
                }
                catch (Exception)
                {
                    // 通知已经隐藏。
                }
            }
            CurrentToastMap.Clear();
        }

        private static ToastButton BuildToastButton(ToastButtonEnum button)
        {
            return new ToastButton(Helper.LocalizeText("ToastHelper" + button), button.ToString())
            {
                ActivationType = ToastActivationType.Background
            };
        }

        private static async Task<ToastContent> BuildToastContent(Music music, MediaPlaybackState state, NotificationDisplayMode display, LyricsSource lyricsSource)
        {
            ToastActionsCustom custom = new ToastActionsCustom() { Buttons = {} };
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    custom.Buttons.Add(BuildToastButton(ToastButtonEnum.Play));
                    break;
                case MediaPlaybackState.Playing:
                    custom.Buttons.Add(BuildToastButton(ToastButtonEnum.Pause));
                    break;
            }
            custom.Buttons.Add(BuildToastButton(ToastButtonEnum.Next));
            if (Settings.settings.ShowLyricsInNotification)
            {
                switch (lyricsSource)
                {
                    case LyricsSource.Internet:
                        if (await MusicService.HasLrcLyrics(music))
                        {
                            custom.Buttons.Add(BuildToastButton(ToastButtonEnum.SwitchLyricsSourceToLrcFile));
                        }
                        else if (await MusicService.HasLyrics(music))
                        {
                            custom.Buttons.Add(BuildToastButton(ToastButtonEnum.SwitchLyricsSourceToMusic));
                        }
                        break;
                    case LyricsSource.LrcFile:
                        if (await MusicService.HasLyrics(music))
                        {
                            custom.Buttons.Add(BuildToastButton(ToastButtonEnum.SwitchLyricsSourceToMusic));
                        }
                        else
                        {
                            custom.Buttons.Add(BuildToastButton(ToastButtonEnum.SwitchLyricsSourceToInternet));
                        }
                        break;
                    case LyricsSource.Music:
                        custom.Buttons.Add(BuildToastButton(ToastButtonEnum.SwitchLyricsSourceToInternet));
                        break;
                }
            }
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText() { Text = music.GetToastText() },
                            new AdaptiveProgressBar()
                            {
                                Value = new BindableProgressBarValue("MediaControlPosition"),
                                ValueStringOverride = MusicDurationConverter.ToTime(music.Duration),
                                Title = new BindableString("Lyrics"),
                                Status = new BindableString("MediaControlPositionTime")
                            }
                        }
                    }
                },
                Actions = custom,
                ActivationType = ToastActivationType.Background,
                Launch = "Launch",
                Audio = SlientToast,
                Scenario = display == NotificationDisplayMode.Reminder || state == MediaPlaybackState.Paused ? ToastScenario.Reminder : ToastScenario.Default
            };
            return toastContent;
        }

        private static async Task<ToastNotification> BuildToast(Music music, MediaPlaybackState state, NotificationDisplayMode display, LyricsSource lyricsSource)
        {
            string toastTag = null;
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    toastTag = ToastTagPlaying;
                    break;
                case MediaPlaybackState.Playing:
                    toastTag = ToastTagPaused;
                    break;
            }

            var toastContent = await BuildToastContent(music, state, display, lyricsSource);
            // Create the toast notification
            ToastNotification toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = toastTag,
                Group = ToastGroup,
                Data = new NotificationData() { SequenceNumber = 0 },
                ExpiresOnReboot = true
            };
            if (Settings.settings.ShowLyricsInNotification)
            {
                await LyricsHelper.SetLyrics();
                toast.Data.Values["Lyrics"] = LyricsHelper.GetLyrics();
            }
            if (display == NotificationDisplayMode.Quick)
            {
                toast.ExpirationTime = DateTime.Now.AddSeconds(Math.Min(10, music.Duration));
            }
            //toast.Dismissed += (sender, args) => HideToast("Dismissed");
            toast.Data.Values["MediaControlPosition"] = MusicPlayer.Progress.ToString();
            toast.Data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MusicPlayer.Position);
            return toast;
        }
    }

    class ToastKey
    {
        public Music Music { get; set; }
        public MediaPlaybackState State { get; set; }
        public LyricsSource LyricsSource { get; set; } = LyricsSource.Internet;
        

        public override bool Equals(object obj)
        {
            ToastKey key = (ToastKey)obj;
            return key != null && Music.Equals(key.Music) && State == key.State;
        }

        public override int GetHashCode()
        {
            int hashCode = 1763106858;
            hashCode = hashCode * -1521134295 + EqualityComparer<Music>.Default.GetHashCode(Music);
            hashCode = hashCode * -1521134295 + State.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"ToastKey[Music={Music?.Name},State={State}]";
        }
    }

    public enum ToastButtonEnum
    {
        Play, Pause, Next, SwitchLyricsSourceToMusic, SwitchLyricsSourceToLrcFile, SwitchLyricsSourceToInternet
    }
}
