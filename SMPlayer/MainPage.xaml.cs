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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatOneButton.IsChecked = false;
        }

        private void RepeatOneButton_Click(object sender, RoutedEventArgs e)
        {
            ShuffleButton.IsChecked = false;
            RepeatButton.IsChecked = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.Content = (string)PlayButton.Content == "&#xE768;" ? "&#xE769;" : "&#xE768;";
        }


        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeButton.Content = (string)VolumeButton.Content == "&#xE767;" ? "&#xE74F;" : "&#xE767;";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(typeof(MusicLibraryPage));
            MusicLibraryItem.IsSelected = true;
        }

        private void MainNavigationView_PaneClosing(NavigationView sender, object args)
        {
            NaviSearchBarItem.Visibility = Visibility.Collapsed;
            NaviSearchItem.Visibility = Visibility.Visible;
        }

        private void Open_Navigation()
        {
            NaviSearchBarItem.Visibility = Visibility.Visible;
            NaviSearchItem.Visibility = Visibility.Collapsed;
        }

        private void MainNavigationView_PaneOpening(NavigationView sender, object args)
        {
            Open_Navigation();
        }

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                MainFrame.Navigate(typeof(SettingsPage));
                return;
            }
            var item = (NavigationViewItem)MainNavigationView.SelectedItem;
            switch (item.Name)
            {
                case "NaviSearchItem":
                    Open_Navigation();
                    NaviSearchBar.Focus(FocusState.Programmatic);
                    break;
                case "MusicLibraryItem":
                    MainFrame.Navigate(typeof(MusicLibraryPage));
                    break;
                case "NowPlayingItem":
                    break;
                case "ToPlayItem":
                    MainFrame.Navigate(typeof(ToPlayPage));
                    break;
                case "HistoryItem":
                    MainFrame.Navigate(typeof(HistoryPage));
                    break;
                case "PlaylistsItem":
                    MainFrame.Navigate(typeof(PlaylistsPage));
                    break;
                default:
                    return;
            }

        }

        private void NaviSearchBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void NaviSearchBar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }
    }
}
