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

        public RemoveDialog()
        {
            this.InitializeComponent();
        }

        public Action Confirm { get; set; }
        public Action Cancel { get; set; }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Confirm?.Invoke();
            this.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke();
            this.Hide();
        }
    }
}
