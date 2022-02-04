using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class FolderUpdateResultDialog : ContentDialog
    {
        private readonly ObservableCollection<FolderUpdateResultGroup> Groups = new ObservableCollection<FolderUpdateResultGroup>();
        public FolderUpdateResultDialog()
        {
            this.InitializeComponent();
        }

        public async Task ShowAsync(FolderUpdateResult result)
        {
            FolderUpdateResultTextBlock.Text = Helper.LocalizeText("UpdateResultOfFolder", Path.GetFileName(result.FolderPath));
            Groups.AddRange(result.ToGroups());
            await ShowAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = PlaylistControl.GetRowBackground(args.ItemIndex);
        }
    }
}
