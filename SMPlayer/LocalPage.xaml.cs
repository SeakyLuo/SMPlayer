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
        private FolderTree Tree;
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetPage(Settings.settings.Tree.Trees.Count, Settings.settings.Tree.Files.Count);
            LocalNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = (NavigationViewItem)LocalNavigationView.SelectedItem;
            LocalNavigationView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            Type page = null;
            bool needsSetting = true;
            switch (item.Name)
            {
                case "LocalFoldersItem":
                    page = typeof(LocalFoldersPage);
                    needsSetting = LocalFoldersPage.infoSetter == null;
                    break;
                case "LocalSongsItem":
                    page = typeof(LocalMusicPage);
                    needsSetting = LocalMusicPage.infoSetter == null;
                    break;
                default:
                    return;
            }
            if (LocalFrame.CurrentSourcePageType != page)
            {
                if (needsSetting) LocalFrame.Navigate(page, this as InfoSetter);
                else LocalFrame.Navigate(page, Tree);
            }
        }

        private void LocalNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (!LocalFrame.CanGoBack) return;
            LocalFrame.GoBack();
            System.Diagnostics.Debug.WriteLine(LocalFrame.CurrentSourcePageType.Name);
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
            System.Diagnostics.Debug.WriteLine(LocalFoldersItem.IsSelected);
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

        public void SetInfo(FolderTree tree, bool redirect)
        {
            if (Tree != null && Tree.Path == tree.Path) return;
            Tree = tree;
            TitleTextBlock.Text = tree.GetFolderName();
            int folders = tree.Trees.Count, songs = tree.Files.Count;
            LocalFoldersItem.Content = string.Format("Folders ({0})", folders);
            LocalFoldersItem.IsEnabled = folders != 0;
            LocalSongsItem.Content = string.Format("Songs ({0})", songs);
            LocalSongsItem.IsEnabled = songs != 0;
            if (redirect) SetPage(folders, songs);
        }

        public void SetPage(int folders, int songs)
        {
            if (songs > folders)
                LocalSongsItem.IsSelected = true;
            else
                LocalFoldersItem.IsSelected = true;
        }
    }

    public interface InfoSetter
    {
        void SetInfo(FolderTree tree, bool click);
    }
}