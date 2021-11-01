using SMPlayer.Models;
using System;
using System.Threading.Tasks;
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
        public Func<string, string, NamingError> Confirm { get; set; }
        public Func<string, string, Task<NamingError>> ConfirmAsync { get; set; }
        public Action<string, string> AfterConfirmation { get; set; }

        public RenameDialog(Func<string, string, NamingError> confirm, RenameOption option, RenameTarget target, string defaultName)
        {
            Constructor(confirm, null, option, target, defaultName);
        }
        public RenameDialog(Func<string, string, Task<NamingError>> confirmAsync, RenameOption option, RenameTarget target, string defaultName)
        {
            Constructor(null, confirmAsync, option, target, defaultName);
        }
        public RenameDialog(IRenameActionListener listener, RenameOption option, RenameTarget target, string defaultName)
        {
            AfterConfirmation = listener.AfterConfirmation;
            Constructor(listener.Confirm, null, option, target, defaultName);
        }
        private void Constructor(Func<string, string, NamingError> confirm, Func<string, string, Task<NamingError>> confirmAsync,
                                 RenameOption option, RenameTarget target, string defaultName)
        {
            this.InitializeComponent();
            Confirm = confirm;
            ConfirmAsync = confirmAsync;
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
            if (ConfirmAsync != null)
            {
                error = await ConfirmAsync(DefaultName, newName);
            }
            else if (Confirm != null)
            {
                error = Confirm(DefaultName, newName);
            }
            if (error != NamingError.Good)
            {
                ShowError(error);
                return;
            }
            Cancel();
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            AfterConfirmation?.Invoke(DefaultName, newName);
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

    public class VirtualRenameActionListener : IRenameActionListener
    {
        public RenameDialog Dialog { get; set; }
        public object Data { get; set; }
        public Action ConfirmAction { get; set; }
        public NamingError Confirm(string oldName, string newName)
        {
            return Settings.settings.CheckPlaylistNamingError(newName);
        }

        public void AfterConfirmation(string oldName, string newName)
        {
            Settings.settings.RenamePlaylist(oldName, newName, Dialog.Option, Data);
            ConfirmAction?.Invoke();
        }
    }

    public interface IRenameActionListener
    {
        NamingError Confirm(string oldName, string newName);
        void AfterConfirmation(string oldName, string newName);
    }
}
