using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        public Func<string, string> Validation { get; set; }
        public Action Cancel { get; set; }
        public bool SelectAllText { get; set; } = false;

        public InputDialog()
        {
            this.InitializeComponent();
            if (SelectAllText) InputTextBox.SelectAll();
        }

        public InputDialog(FolderTree tree)
        {
            this.InitializeComponent();
            Title = Helper.LocalizeMessage("Search");
            PlaceholderText = Helper.LocalizeMessage("SearchDirectoryHint", tree.Name);
            Confirm = (inputText) =>
            {
                MainPage.Instance.Search(new SearchKeyword()
                {
                    Text = inputText,
                    Songs = tree.Flatten(),
                    Folder = tree
                });
            };
            if (SelectAllText) InputTextBox.SelectAll();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string result = Validation?.Invoke(InputText);
            if (string.IsNullOrEmpty(result))
            {
                ErrorTextBox.Visibility = Visibility.Collapsed;
                Confirm?.Invoke(InputText);
                this.Hide();
            }
            else
            {
                ErrorTextBox.Visibility = Visibility.Visible;
                ErrorTextBox.Text = Helper.LocalizeText(result);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke();
            this.Hide();
        }
    }
}
