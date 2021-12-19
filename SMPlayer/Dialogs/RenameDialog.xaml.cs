using SMPlayer.Models;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class RenameDialog : ContentDialog
    {
        public RenameOption Option { get; set; }
        public RenameTarget Target { get; set; }
        public string DefaultName { get; set; }
        public Func<string, NamingError> Validate { get; set; }
        public Func<string, Task<NamingError>> ValidateAsync { get; set; }
        public Action<string> Confirmed { get; set; }

        public RenameDialog(RenameOption option, RenameTarget target, string defaultName)
        {
            this.InitializeComponent();
            Option = option;
            DefaultName = defaultName;
            string dialogTitle, confirmContent;
            switch (option)
            {
                case RenameOption.Create:
                    switch (target)
                    {
                        case RenameTarget.Folder:
                            dialogTitle = "CreateNewPlaylist";  
                            break;
                        case RenameTarget.Playlist:
                            dialogTitle = "CreateNewFolder";
                            break;
                        default:
                            dialogTitle = "";
                            break;
                    }
                    confirmContent = "Confirm";
                    break;
                case RenameOption.Rename:
                    switch (target)
                    {
                        case RenameTarget.Folder:
                            dialogTitle = "RenamePlaylist";
                            break;
                        case RenameTarget.Playlist:
                            dialogTitle = "RenameFolder";
                            break;
                        default:
                            dialogTitle = "";
                            break;
                    }
                    confirmContent = "Rename";
                    break;
                default:
                    dialogTitle = "";
                    confirmContent = "";
                    break;
            }
            TitleTextBlock.Text = Helper.LocalizeText(dialogTitle);
            ConfirmButton.Content = Helper.Localize(confirmContent);
            NewPlaylistNameTextBox.Text = defaultName;
            NewPlaylistNameTextBox.SelectAll();
        }

        public void ShowError(NamingError error)
        {
            NamingErrorTextBox.Text = Helper.LocalizeMessage(error.ToStr());
            NamingErrorTextBox.Visibility = Visibility.Visible;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string newName = NewPlaylistNameTextBox.Text;
            if (Option == RenameOption.Rename && DefaultName == newName)
            {
                Cancel();
                return;
            }
            NamingError error = NamingError.Good;
            if (ValidateAsync != null)
            {
                error = await ValidateAsync(newName);
            }
            else if (Validate != null)
            {
                error = Validate(newName);
            }
            if (error != NamingError.Good)
            {
                ShowError(error);
                return;
            }
            Cancel();
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            Confirmed?.Invoke(newName);
            MainPage.Instance?.Loader.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Cancel()
        {
            this.Hide();
            NewPlaylistNameTextBox.Text = "";
            NamingErrorTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
