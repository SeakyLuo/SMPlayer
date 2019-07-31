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
            SearchPanel.HorizontalAlignment = isOpen ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            SearchBar.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
            SearchButton.Visibility = isOpen ? Visibility.Collapsed : Visibility.Visible;
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

        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            RepeatButton.Content = (string)RepeatButton.Content == "&#xE8EE;" ? "&#xE8ED;" : "&#xE8EE;";
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.Content = (string)PlayButton.Content == "&#xE768;" ? "&#xE769;" : " &#xE768;";
        }


        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeButton.Content = (string)VolumeButton.Content == "&#xE767;" ? "&#xE74F;" : "&#xE767;";
        }
    }
}
