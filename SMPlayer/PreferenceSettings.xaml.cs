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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PreferenceSettings : Page
    {
        public PreferenceSettings()
        {
            this.InitializeComponent();
            MainPage.Instance?.SetHeaderText("PreferenceSettings");
        }

        private void PreferredSongsIsEnabledButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreferredArtistsIsEnabledButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GoToAddPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreferredAlbumsIsEnabledButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreferredPlaylistsIsEnabledButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GoToAddPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RecentPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RecentPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void MyFavoritePreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MostPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void MostPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void MyFavoritePreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void LeastPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void LeastPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void GoToAddPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GoToAddPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
