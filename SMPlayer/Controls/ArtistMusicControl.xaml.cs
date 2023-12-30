using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class ArtistMusicControl : UserControl
    {
        public ArtistMusicControl()
        {
            this.InitializeComponent();
        }

        private void ArtistMusicControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void ArtistMusicControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayButtonIcon.Symbol == Symbol.Pause)
            {
                MusicPlayer.Pause();
                PlayButtonIcon.Symbol = Symbol.Play;
            }
            else
            {
                MusicView music = DataContext as MusicView;
                MusicPlayer.AddNextAndPlay(music);
                PlayButtonIcon.Symbol = Symbol.Pause;
            }
        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            MusicView music = DataContext as MusicView;
            MenuFlyoutHelper helper = new MenuFlyoutHelper() { Data = music };
            helper.GetAddToMenuFlyout().ShowAt(this);
        }
    }
}
