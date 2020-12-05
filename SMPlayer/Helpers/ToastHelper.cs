﻿using Microsoft.Toolkit.Uwp.Notifications;
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

        public static ToastNotification Toast;
        public static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        public static ToastAudio SlientToast = new ToastAudio() { Silent = true };
        private static Music ToastMusic;
        private static MediaPlaybackState ToastState;

        private static ToastButton BuildToastButton(string text)
        {
            return new ToastButton(Helper.LocalizeMessage(text), text)
            {
                ActivationType = ToastActivationType.Background
            };
        }

        private static void ShowToast(Music music, MediaPlaybackState state)
        {
            if (!App.Inited) return;
            if (music == null || Settings.settings.NotificationSend == NotificationSendMode.Never) return;
            NotificationDisplayMode display = Settings.settings.NotificationDisplay;
            if (Window.Current.Visible && (display == NotificationDisplayMode.Normal || display == NotificationDisplayMode.Quick))
                return;

            ToastMusic = music;
            ToastState = state;

            ToastButton controlButton = null;
            string toastTag = "";
            switch (state)
            {
                case MediaPlaybackState.Paused:
                    controlButton = BuildToastButton("Play");
                    toastTag = ToastTagPlaying;
                    break;
                case MediaPlaybackState.Playing:
                    controlButton = BuildToastButton("Pause");
                    toastTag = ToastTagPaused;
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

            // Create the toast notification
            Toast = new ToastNotification(toastContent.GetXml())
            {
                Tag = toastTag,
                Group = ToastGroup,
                Data = new NotificationData() { SequenceNumber = 0 },
                ExpiresOnReboot = true
            };
            if (Settings.settings.ShowLyricsInNotification)
            {
                LyricsHelper.SetLyrics();
            }
            if (display == NotificationDisplayMode.Quick)
            {
                Toast.ExpirationTime = DateTime.Now.AddSeconds(Math.Min(10, music.Duration));
            }
            Toast.Data.Values["Lyrics"] = "";
            Toast.Data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            Toast.Data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            try
            {
                toastNotifier.Show(Toast);
            }
            catch (Exception)
            {
                // 通知已发布
            }
        }

        public static void ShowToast()
        {
            MediaPlaybackState playbackState = MediaHelper.Player.PlaybackSession.PlaybackState;
            if (playbackState == MediaPlaybackState.Playing)
            {
                ShowPauseToast(MediaHelper.CurrentMusic);
            }
            else if (playbackState == MediaPlaybackState.Paused)
            {
                ShowPlayToast(MediaHelper.CurrentMusic);
            }
        }

        public static void ShowPlayToast(Music music)
        {
            ShowStateToast(music, MediaPlaybackState.Paused);
        }

        public static void ShowPauseToast(Music music)
        {
            ShowStateToast(music, MediaPlaybackState.Playing);
        }

        private static void ShowStateToast(Music music, MediaPlaybackState state)
        {
            if (ToastMusic == music && ToastState == state) return;
            HideToast();
            ShowToast(music, state);
        }

        public static void UpdateToast()
        {
            if (!MediaHelper.IsPlaying) return;
            // Create NotificationData and make sure the sequence number is incremented
            // since last update, or assign 0 for updating regardless of order
            var data = new NotificationData { SequenceNumber = 0 };
            data.Values["MediaControlPosition"] = MediaHelper.Progress.ToString();
            data.Values["MediaControlPositionTime"] = MusicDurationConverter.ToTime(MediaHelper.Position);
            data.Values["Lyrics"] = Settings.settings.ShowLyricsInNotification ? LyricsHelper.GetLyrics() : "";

            // Update the existing notification's data by using tag/group
            toastNotifier.Update(data, ToastTagPaused, ToastGroup);
        }

        public static void HideToast()
        {
            if (Toast != null)
            {
                try
                {
                    toastNotifier.Hide(Toast);
                }
                catch (Exception)
                {
                    // 通知已经隐藏。
                }
            }
        }
    }
}
