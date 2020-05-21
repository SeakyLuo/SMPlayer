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

namespace SMPlayer.Dialogs
{
    public sealed partial class InputDialog : ContentDialog
    {
        public new string Title
        {
            get => TitleTextBlock.Text;
            set => TitleTextBlock.Text = value;
        }
        public string PlaceholderText
        {
            get => InputTextBox.PlaceholderText;
            set => InputTextBox.PlaceholderText = value;
        }
        public string InputText
        {
            get => InputTextBox.Text;
            set => InputTextBox.Text = value;
        }
        public Action<string> Confirm { get; set; }
        public Action Cancel { get; set; }

        public InputDialog()
        {
            this.InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Confirm?.Invoke(InputText);
            this.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke();
            this.Hide();
        }
    }
}
