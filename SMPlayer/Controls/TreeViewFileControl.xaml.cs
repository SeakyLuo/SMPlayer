using SMPlayer.Models.VO;
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
    public sealed partial class TreeViewFileControl : UserControl
    {
        public TreeViewFileControl()
        {
            this.InitializeComponent();
        }

        private void CreatorHyperLinkButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewMusic music = ((FrameworkElement)sender).DataContext as GridViewMusic;
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage), music.Artist);
        }

        private void CollectionHyperLinkButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewMusic music = ((FrameworkElement)sender).DataContext as GridViewMusic;
            MainPage.Instance.NavigateToPage(typeof(AlbumPage), music.Album);
        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            GridViewMusic music = frameworkElement.DataContext as GridViewMusic;
            MenuFlyoutHelper helper = new MenuFlyoutHelper() { Data = music };
            helper.GetAddToMenuFlyout().ShowAt(frameworkElement);
        }

        private void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewMusic music = (sender as FrameworkElement).DataContext as GridViewMusic;
            MusicPlayer.AddMusic(music, MusicPlayer.CurrentIndex + 1);
            Helper.ShowNotificationRaw(Helper.LocalizeMessage("SetPlayNext", music.Name));
        }

        private void TreeViewFileControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
            SetPlayButtonIcon(sender);
        }

        private void TreeViewFileControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
            SetPlayButtonIcon(sender);
        }

        private void SetPlayButtonIcon(object sender)
        {
            GridViewMusic music = (sender as FrameworkElement).DataContext as GridViewMusic;
            if (music == null)
            {
                return;
            }
            PlayButtonIcon.Symbol = music.IsPlaying && MusicPlayer.IsPlaying ? Symbol.Pause : Symbol.Play;
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
                FrameworkElement frameworkElement = sender as FrameworkElement;
                GridViewMusic music = frameworkElement.DataContext as GridViewMusic;
                MusicPlayer.AddNextAndPlay(music);
                PlayButtonIcon.Symbol = Symbol.Pause;
            }
        }
    }
}
