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
    public sealed partial class LocalPage : Page, InfoSetter
    {
        private bool firstLoad = true;
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.settings.Tree.Trees.Count == 0 && Settings.settings.Tree.Files.Count > 0)
                LocalSongsItem.IsSelected = true;
            else
                LocalFoldersItem.IsSelected = true;
            LocalNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = (NavigationViewItem)LocalNavigationView.SelectedItem;
            LocalNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            switch (item.Name)
            {
                case "LocalFoldersItem":
                    if (LocalFrame.CurrentSourcePageType != typeof(LocalFoldersPage))
                        LocalFrame.Navigate(typeof(LocalFoldersPage), this as InfoSetter);
                    break;
                case "LocalSongsItem":
                    if (LocalFrame.CurrentSourcePageType != typeof(LocalMusicPage))
                        LocalFrame.Navigate(typeof(LocalMusicPage), this as InfoSetter);
                    break;
                default:
                    break;
            }
        }

        private void LocalNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (!LocalFrame.CanGoBack) return;
            LocalFrame.GoBack();
            switch (LocalFrame.CurrentSourcePageType.Name)
            {
                case "LocalFoldersPage":
                    LocalFoldersItem.IsSelected = true;
                    break;
                case "LocalMusicPage":
                    LocalSongsItem.IsSelected = true;
                    break;
                default:
                    break;
            }
            LocalNavigationView.IsBackButtonVisible = LocalFrame.CanGoBack ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;
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

        public void SetInfo(string folder, int folders, int songs)
        {
            TitleTextBlock.Text = folder;
            LocalFoldersItem.Content = string.Format("Folders ({0})", folders);
            LocalFoldersItem.IsEnabled = folders != 0;
            LocalSongsItem.Content = string.Format("Songs ({0})", songs);
            bool songsEnabled = songs != 0;
            LocalSongsItem.IsEnabled = songsEnabled;
            if (songsEnabled && !LocalFoldersItem.IsEnabled) LocalSongsItem.IsSelected = true;
            else LocalFoldersItem.IsSelected = true;
        }
    }

    public interface InfoSetter
    {
        void SetInfo(string folder, int folders, int songs);
    }
}