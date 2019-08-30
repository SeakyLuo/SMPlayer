using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NowPlayingFullPage : Page, MediaControlContainer
    {
        public static NowPlayingFullPage Instance { get => (Window.Current.Content as Frame).Content as NowPlayingFullPage; }
        public NowPlayingFullPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FullMediaControl.Update();
            PlaylistControl.SetPlaylist(MediaHelper.CurrentPlayList);
        }

        public void SetMusicAndPlay(Music music)
        {
            FullMediaControl.SetMusicAndPlay(music);
        }

        public void PauseMusic()
        {
            FullMediaControl.PauseMusic();
        }

        public void SetShuffle(bool isShuffle)
        {
            FullMediaControl.SetShuffle(isShuffle);
        }
    }
}
