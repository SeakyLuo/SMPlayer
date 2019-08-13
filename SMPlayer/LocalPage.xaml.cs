using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page
    {
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LocalFoldersItem.IsSelected = true;
        }

        private void LocalFrame_Navigated(object sender, NavigationEventArgs e)
        {
            LocalNavigationView.IsBackButtonVisible = LocalFrame.CanGoBack ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = (NavigationViewItem)LocalNavigationView.SelectedItem;
            switch (item.Name)
            {
                case "LocalFoldersItem":
                    LocalFrame.Navigate(typeof(LocalFoldersPage));
                    break;
                case "LocalMusicItem":
                    LocalFrame.Navigate(typeof(LocalMusicPage));
                    break;
                default:
                    return;
            }
        }

        private void LocalNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            LocalFrame.GoBack();
            switch (LocalFrame.CurrentSourcePageType.Name)
            {
                case "LocalFoldersPage":
                    LocalFoldersItem.IsSelected = true;
                    break;
                case "LocalMusicPage":
                    LocalMusicItem.IsSelected = true;
                    break;
                default:
                    break;
            }
        }

        public void SetTitle(string title)
        {
            TitleTextBlock.Text = title;
        }

        private void LocalListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalListViewItem.Visibility = Visibility.Collapsed;
            LocalGridViewItem.Visibility = Visibility.Visible;
        }

        private void LocalGridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalListViewItem.Visibility = Visibility.Visible;
            LocalGridViewItem.Visibility = Visibility.Collapsed;
        }
    }
}
