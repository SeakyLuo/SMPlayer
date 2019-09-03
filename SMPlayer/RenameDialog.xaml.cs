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
        private string OldName;
        private RenameActionListener RenameListener;
        public RenameDialog(RenameActionListener listener, string title, string oldname)
        {
            this.InitializeComponent();
            RenameListener = listener;
            OldName = oldname;
            TitleTextBlock.Text = title;
            ConfirmButton.Content = title;
            NewPlaylistNameTextBox.Text = oldname;
            NewPlaylistNameTextBox.SelectAll();
        }

        public void ShowError(string error)
        {
            NamingErrorTextBox.Text = error;
            NamingErrorTextBox.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (RenameListener.Confirm(OldName, NewPlaylistNameTextBox.Text))
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

    public interface RenameActionListener
    {
        bool Confirm(string OldName, string NewName);
    }
}
