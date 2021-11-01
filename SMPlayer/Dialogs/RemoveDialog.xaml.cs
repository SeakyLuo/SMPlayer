using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class RemoveDialog : ContentDialog
    {
        public bool IsChecked
        {
            get => (bool)DisplayCheckBox.IsChecked;
            set => DisplayCheckBox.IsChecked = value;
        }
        public string Message
        {
            get => MessageTextBlock.Text;
            set => MessageTextBlock.Text = value;
        }
        public Visibility CheckBoxVisibility
        {
            get => DisplayCheckBox.Visibility;
            set => DisplayCheckBox.Visibility = value;
        }

        public RemoveDialog(string message = "")
        {
            this.InitializeComponent();
            this.Message = message;
        }

        public Action Confirm { get; set; }
        public Action Cancel { get; set; }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            Confirm?.Invoke();
            MainPage.Instance?.Loader.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke();
            this.Hide();
        }
    }
}
