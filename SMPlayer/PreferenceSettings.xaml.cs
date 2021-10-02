using SMPlayer.Models;
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
        private static bool ShowAddPreferredSongsToolTip = true,
                            ShowAddPreferredArtistsToolTip = true,
                            ShowAddPreferredAlbumsToolTip = true,
                            ShowAddPreferredPlaylistsToolTip = true,
                            ShowAddPreferredFoldersToolTip = true;


        public PreferenceSettings()
        {
            this.InitializeComponent();
            MainPage.Instance?.SetHeaderText("PreferenceSettings");
            PreferredSongsCheckBox.IsChecked = Settings.settings.Preference.Songs;
            PreferredArtistsCheckBox.IsChecked = Settings.settings.Preference.Artists;
            PreferredAlbumsCheckBox.IsChecked = Settings.settings.Preference.Albums;
            PreferredPlaylistsCheckBox.IsChecked = Settings.settings.Preference.Playlists;
            PreferredFoldersCheckBox.IsChecked = Settings.settings.Preference.Folders;
            RecentAddedPreferenceCheckBox.IsChecked = Settings.settings.Preference.RecentAdded;
            MyFavoritePreferenceCheckBox.IsChecked = Settings.settings.Preference.MyFavorites;
            MostPlayedPreferenceCheckBox.IsChecked = Settings.settings.Preference.MostPlayed;
            LeastPlayedPreferenceCheckBox.IsChecked = Settings.settings.Preference.LeastPlayed;

            GoToAddPreferredSongsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredSongsToolTip"), false);
            GoToAddPreferredArtistsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredArtistsToolTip"), false);
            GoToAddPreferredAlbumsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredAlbumsToolTip"), false);
            GoToAddPreferredPlaylistsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredPlaylistsToolTip"), false);
            GoToAddPreferredFoldersButton.SetToolTip(Helper.LocalizeMessage("AddPreferredFoldersToolTip"), false);
        }

        private void GoToAddPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(MusicLibraryPage));
            if (ShowAddPreferredSongsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredSongsToolTip", 5000);
                ShowAddPreferredSongsToolTip = false;
            }
        }

        private void GoToAddPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage));
            if (ShowAddPreferredArtistsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredArtistsToolTip", 5000);
                ShowAddPreferredArtistsToolTip = false;
            }
        }

        private void GoToAddPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(AlbumsPage));
            if (ShowAddPreferredAlbumsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredAlbumsToolTip", 5000);
                ShowAddPreferredAlbumsToolTip = false;
            }
        }

        private void GoToAddPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(PlaylistsPage));
            if (ShowAddPreferredPlaylistsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredPlaylistsToolTip", 5000);
                ShowAddPreferredPlaylistsToolTip = false;
            }
        }

        private void GoToAddPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(LocalPage));
            if (ShowAddPreferredFoldersToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredFoldersToolTip", 5000);
                ShowAddPreferredFoldersToolTip = true;
            }
        }

        private void MyFavoritePreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MyFavorites = true;
        }

        private void MyFavoritePreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MyFavorites = false;
        }

        private void MostPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MostPlayed = true;
        }

        private void MostPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MostPlayed = false;
        }

        private void LeastPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.LeastPlayed = true;
        }

        private void LeastPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.LeastPlayed = false;
        }

        private void RecentAddedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.RecentAdded = true;
        }

        private void RecentAddedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.RecentAdded = false;
        }

        private void PreferredSongsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Songs = true;
        }

        private void PreferredSongsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Songs = false;
        }

        private void PreferredArtistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Artists = true;
        }

        private void PreferredArtistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Artists = false;
        }

        private void PreferredAlbumsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Albums = true;
        }

        private void PreferredAlbumsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Albums = false;
        }

        private void ClearAllPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisableAllPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisableAllPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearAllPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisableAllPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearAllPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisableAllPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearAllPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisableAllPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearAllPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreferredPlaylistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Playlists = true;
        }

        private void PreferredPlaylistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Playlists = false;
        }

        private void PreferredFoldersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Folders = true;
        }

        private void PreferredFoldersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Folders = false;
        }
    }
}
