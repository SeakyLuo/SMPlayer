using SMPlayer.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            string dialogTitle = "", confirmContent = "";
            switch (option)
            {
                case RenameOption.New:
                    dialogTitle = "Create New Playlist";
                    confirmContent = "Confirm";
                    break;
                case RenameOption.Rename:
                    dialogTitle = "Rename Playlist";
                    confirmContent = "Rename";
                    break;
            }
            TitleTextBlock.Text = Helper.Localize(dialogTitle);
            ConfirmButton.Content = Helper.Localize(confirmContent);
            NewPlaylistNameTextBox.Text = defaultName;
            NewPlaylistNameTextBox.SelectAll();
        }

        public void ShowError(NamingError error)
        {
            NamingErrorTextBox.Text = Helper.LocalizeMessage(error.ToStr());
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
        public Action ConfirmAction;
        public bool Confirm(string oldName, string newName)
        {
            NamingError error = Settings.settings.CheckPlaylistNamingError(newName);
            if (error != NamingError.Good)
            {
                Dialog.ShowError(error);
                return false;
            }
            Settings.settings.RenamePlaylist(oldName, newName, Dialog.Option, Data);
            ConfirmAction?.Invoke();
            return true;
        }
    }

    public interface RenameActionListener
    {
        bool Confirm(string oldName, string newName);
    }
}
