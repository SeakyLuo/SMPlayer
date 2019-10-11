using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class RenameDialog : ContentDialog
    {
        public RenameOption Option { get; set; }
        public string DefaultName { get; set; }
        public Func<string, string, bool> Confirm { get; set; }

        public RenameDialog(Func<string, string, bool> confirm, RenameOption option, string defaultName)
        {
            Constructor(confirm, option, defaultName);
        }
        public RenameDialog(RenameActionListener listener, RenameOption option, string defaultName)
        {
            Constructor(listener.Confirm, option, defaultName);
        }
        private void Constructor(Func<string, string, bool> confirm, RenameOption option, string defaultName)
        {
            this.InitializeComponent();
            Confirm = confirm;
            Option = option;
            DefaultName = defaultName;
            var dialogTitle = "";
            switch (option)
            {
                case RenameOption.New:
                    dialogTitle = "Create New Playlist";
                    break;
                case RenameOption.Rename:
                    dialogTitle = "Rename";
                    break;
            }
            TitleTextBlock.Text = dialogTitle;
            ConfirmButton.Content = dialogTitle;
            NewPlaylistNameTextBox.Text = defaultName;
            NewPlaylistNameTextBox.SelectAll();
        }

        public void ShowError(NamingError error)
        {
            string text = "";
            switch (error)
            {
                case NamingError.EmptyOrWhiteSpace:
                    text = "Playlist name cannot be empty or whitespaces!";
                    break;
                case NamingError.Used:
                    text = "This name has been used!";
                    break;
                case NamingError.Special:
                    text = "Playlist name cannot have \"+++\"!";
                    break;
            }
            NamingErrorTextBox.Text = text;
            NamingErrorTextBox.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (Confirm(DefaultName, NewPlaylistNameTextBox.Text))
            {
                this.Hide();
                NewPlaylistNameTextBox.Text = "";
                NamingErrorTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class VirtualRenameActionListener : RenameActionListener
    {
        public RenameDialog Dialog;
        public object Data;
        public bool Confirm(string oldName, string newName)
        {
            NamingError error = Settings.settings.CheckPlaylistNamingError(newName);
            if (error != NamingError.Good)
            {
                Dialog.ShowError(error);
                return false;
            }
            Settings.settings.RenamePlaylist(oldName, newName, Dialog.Option, Data);
            return true;
        }
    }

    public interface RenameActionListener
    {
        bool Confirm(string oldName, string newName);
    }
}
