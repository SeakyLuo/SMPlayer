﻿using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
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
            Helper.SetBackButtonVisibility(AppViewBackButtonVisibility.Visible);
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack) Frame.GoBack();
            };
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LargeAlbumCover.Source = await Helper.GetThumbnail(MediaControl.CurrentMusic);
        }
    }
}
