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

        private void SearchBar_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void SearchBar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void OpenSplitView()
        {
            bool isOpen = !MySplitView.IsPaneOpen;
            MySplitView.IsPaneOpen = isOpen;
            SearchBar.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
            SearchButtonItem.Visibility = isOpen ? Visibility.Collapsed : Visibility.Visible;
        }

        private void HambergurButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSplitView();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            OpenSplitView();
            SearchBar.Focus(FocusState.Programmatic);
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
            MainFrame.Navigate(typeof(MusicLibraryPage));
        }

        private void SplitViewListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicLibraryItem.IsSelected) MainFrame.Navigate(typeof(MusicLibraryPage));
            else if (NowPlayingItem.IsSelected) return;
            else if (ToPlayItem.IsSelected) MainFrame.Navigate(typeof(ToPlayPage));
            else if (HistoryItem.IsSelected) MainFrame.Navigate(typeof(HistoryPage));
            else if (PlaylistsItem.IsSelected) MainFrame.Navigate(typeof(PlaylistsPage));
            else if (SettingsItem.IsSelected) MainFrame.Navigate(typeof(SettingsPage));
            else return;
        }

    }
}
