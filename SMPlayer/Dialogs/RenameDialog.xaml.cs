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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer
{
    public sealed partial class RenameDialog : ContentDialog
    {
        public TitleOption Option;
        private string DefaultName;
        private RenameActionListener RenameListener;
        public RenameDialog(RenameActionListener listener, TitleOption titleOption, string defaultName)
        {
            this.InitializeComponent();
            RenameListener = listener;
            Option = titleOption;
            DefaultName = defaultName;
            var dialogTitle = "";
            switch (titleOption)
            {
                case TitleOption.NewPlaylist:
                    dialogTitle = "Create New Playlist";
                    break;
                case TitleOption.Rename:
                    dialogTitle = "Rename";
                    break;
            }
            TitleTextBlock.Text = dialogTitle;
            ConfirmButton.Content = dialogTitle;
            NewPlaylistNameTextBox.Text = defaultName;
            NewPlaylistNameTextBox.SelectAll();
        }

        public void ShowError(ErrorOption error)
        {
            string text = "";
            switch (error)
            {
                case ErrorOption.EmptyOrWhiteSpace:
                    text = "Playlist name cannot be empty or whitespaces!";
                    break;
                case ErrorOption.Used:
                    text = "This name has been used!";
                    break;
            }
            NamingErrorTextBox.Text = text;
            NamingErrorTextBox.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (RenameListener.Confirm(DefaultName, NewPlaylistNameTextBox.Text))
            {
                this.Hide();
                NewPlaylistNameTextBox.Text = "";
                NamingErrorTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class VirtualRenameActionListener : RenameActionListener
    {
        public RenameDialog Dialog;
        public object Data;
        public bool Confirm(string OldName, string NewName)
        {
            if (string.IsNullOrEmpty(NewName) || string.IsNullOrWhiteSpace(NewName))
            {
                Dialog.ShowError(ErrorOption.EmptyOrWhiteSpace);
                return false;
            }
            if (NewName == MenuFlyoutHelper.NowPlaying || NewName == MenuFlyoutHelper.MyFavorites ||
                Settings.settings.Playlists.FindIndex((p) => p.Name == NewName) != -1)
            {
                Dialog.ShowError(ErrorOption.Used);
                return false;
            }
            switch (Dialog.Option)
            {
                case TitleOption.NewPlaylist:
                    Playlist playlist = new Playlist(NewName);
                    if (Data != null) playlist.Add(Data);
                    Settings.settings.Playlists.Add(playlist);
                    PlaylistsPage.Playlists.Add(playlist);
                    break;
                case TitleOption.Rename:
                    if (OldName == NewName) break;
                    int index = Settings.settings.Playlists.FindIndex((p) => p.Name == OldName);
                    Settings.settings.Playlists[index].Name = NewName;
                    PlaylistsPage.Playlists[index].Name = NewName;
                    break;
            }
            return true;
        }
    }

    public interface RenameActionListener
    {
        bool Confirm(string OldName, string NewName);
    }

    public enum TitleOption
    {
        NewPlaylist = 0,
        Rename = 1
    }

    public enum ErrorOption
    {
        EmptyOrWhiteSpace = 0,
        Used = 1
    }
}
