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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class MusicDialog : ContentDialog
    {
        public MusicDialog(MusicDialogOption option, Music music)
        {
            this.InitializeComponent();
            switch (option)
            {
                case MusicDialogOption.Properties:
                    MusicDialogPivot.SelectedItem = PropertiesItem;
                    break;
                case MusicDialogOption.Lyrics:
                    MusicDialogPivot.SelectedItem = LyricsItem;
                    break;
            }
            MusicInfoController.SetMusicInfo(music);
            MusicLyricsController.SetLyrics(music);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public enum MusicDialogOption
    {
        Properties = 0,
        Lyrics = 1
    }
}
