using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class NowPlayingPage : Page
    {
        public NowPlayingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            PlaylistControl.NowPlayingPlaylist.CollectionChanged += (sender, args) => SetEnabled();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetEnabled();
        }

        private void SetEnabled()
        {
            SaveToButton.IsEnabled = ClearButton.IsEnabled = FullScreenButton.IsEnabled = PlaylistControl.NowPlayingPlaylist.Count != 0;
        }
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainPage.Instance.Frame.Navigate(typeof(NowPlayingFullPage));
            }
            catch (NullReferenceException)
            {
                // Clicking twice quickly will cause this exception
            }
        }
        private void SaveToButton_Click(object sender, RoutedEventArgs e)
        {
            var name = "Now Playing - " + DateTime.Now.ToString("yy/MM/dd");
            int index = Settings.settings.FindNextPlaylistNameIndex(name);
            var defaultName = index == 0 ? name : $"{name} ({index})";
            var helper = new MenuFlyoutHelper() { Data = PlaylistControl.NowPlayingPlaylist };
            helper.GetAddToPlaylistsMenuFlyout(defaultName, MenuFlyoutHelper.NowPlaying).ShowAt(sender as FrameworkElement);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
