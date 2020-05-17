using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class AlbumDialog : ContentDialog
    {
        public AlbumDialog(AlbumDialogOption option, AlbumView album)
        {
            this.InitializeComponent();
            switch (option)
            {
                case AlbumDialogOption.Properties:
                    break;
                case AlbumDialogOption.AlbumArt:
                    break;
            }
            AlbumArtController.SetAlbumArt(album);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //if (AlbumArtController.IsProcessing)
            //{
            //    Helper.ShowNotification("ProcessingRequest");
            //    return;
            //}
            this.Hide();
        }
    }

    public enum AlbumDialogOption
    {
        Properties = 0,
        AlbumArt = 1
    }
}
