using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static void Init()
        {
            Timer.Tick += (sender, e) =>
            {
                UpdateToast();
            };
            Timer.Start();
        }

        private static ToastButton BuildToastButton(string text)
        {
            return new ToastButton(Helper.LocalizeMessage(text), text)
            {
                ActivationType = ToastActivationType.Background
            };
        }

        private static async void ShowToast(Music music, MediaPlaybackState state)
        {
            Helper.Print("show toast, music: {0}, state: {1}", music.Name, state);
            if (!App.Inited) return;
            if (music == null || Settings.settings.NotificationSend == NotificationSendMode.Never
                              || GetToast(music, state) != null) return;
            NotificationDisplayMode display = Settings.settings.NotificationDisplay;
            if (Window.Current.Visible && (display == NotificationDisplayMode.Normal || display == NotificationDisplayMode.Quick))
                return;
            ToastNotification toast = await BuildToast(music, state, display);
            if (toast == null) return;
            lock (CurrentToastMap)
            {
                if (!AddToast(music, state, toast))
                {
                    return;
                }
                try
                {
                    Notifier.Show(toast);
                }
                catch (Exception)
                {
                    // 通知已发布
                }
            }
        }

        private static ToastContent BuildToastContent(Music music, MediaPlaybackState state, NotificationDisplayMode display)
        {
            ToastButton controlButton = null;
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    controlButton = BuildToastButton("Play");
                    break;
                case MediaPlaybackState.Playing:
                    controlButton = BuildToastButton("Pause");
                    break;
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
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        controlButton, BuildToastButton("Next")
                    }
                },
                ActivationType = ToastActivationType.Background,
                Launch = "Launch",
                Audio = SlientToast,
                Scenario = display == NotificationDisplayMode.Reminder || state == MediaPlaybackState.Paused ? ToastScenario.Reminder : ToastScenario.Default
            };
            return toastContent;
        }

        private static async Task<ToastNotification> BuildToast(Music music, MediaPlaybackState state, NotificationDisplayMode display)
        {
            string toastTag;
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    toastTag = ToastTagPlaying;
                    break;
                case MediaPlaybackState.Playing:
                    toastTag = ToastTagPaused;
                    break;
                default:
                    return null;
            }

            var toastContent = BuildToastContent(music, state, display);
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
            toast.Dismissed += (sender, args) => HideToast();
            toast.Data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            toast.Data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            return toast;
        }

        public static void ShowToast(Music music)
        {
            switch (MediaHelper.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    ShowPauseToast(music);
                    break;
                case MediaPlaybackState.Paused:
                    ShowPlayToast(music);
                    break;
            }
        }

        public static void ShowToast()
        {
            ShowToast(MediaHelper.CurrentMusic);
        }

        public static void ShowPlayToast(Music music)
        {
            ShowStateToast(music, MediaPlaybackState.Paused);
        }

        public static void ShowPauseToast(Music music)
        {
            ShowStateToast(music, MediaPlaybackState.Playing);
        }

        public static void ShowStateToast(Music music, MediaPlaybackState state)
        {
            if (GetToast(music, state) != null) return;
            HideToast();
            ShowToast(music, state);
        }

        private static void UpdateToast()
        {
            if (!MediaHelper.IsPlaying || CurrentToastMap.Count == 0) return;
            ToastNotification toast = GetToast(MediaHelper.CurrentMusic, MediaPlaybackState.Playing);
            if (toast == null) return;
            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData { SequenceNumber = 0 };
            data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            data.Values["Lyrics"] = Settings.settings.ShowLyricsInNotification ? LyricsHelper.GetLyrics() : "";

            // Update the existing notification's data by using tag/group
            Notifier.Update(data, ToastTagPaused, ToastGroup);
        }

        private static bool AddToast(Music music, MediaPlaybackState state, ToastNotification toast)
        {
            return CurrentToastMap.TryAdd(new ToastKey { Music = music, State = state }, toast);
        }

        private static ToastNotification GetToast(Music music, MediaPlaybackState state)
        {
            return CurrentToastMap.GetValueOrDefault(new ToastKey { Music = music, State = state });
        }

        public static void HideToast()
        {
            lock (CurrentToastMap)
            {
                foreach (var e in CurrentToastMap)
                {
                    try
                    {
                        Notifier.Hide(e.Value);
                        Helper.Print("hide toast, music: {0}, state: {1}", e.Key.Music.Name, e.Key.State);
                    }
                    catch (Exception)
                    {
                        // 通知已经隐藏。
                    }
                }
                CurrentToastMap.Clear();
            }
        }
    }

    class ToastKey
    {
        public Music Music { get; set; }
        public MediaPlaybackState State { get; set; }

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
}
