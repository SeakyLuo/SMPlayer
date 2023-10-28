using SMPlayer.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class MusicDialog : ContentDialog
    {
        private Music CurrentMusic;
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
                case MusicDialogOption.AlbumArt:
                    MusicDialogPivot.SelectedItem = AlbumArtItem;
                    break;
            }
            this.CurrentMusic = music;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //if (MusicInfoController.IsProcessing || MusicLyricsController.IsProcessing || AlbumArtController.IsProcessing)
            //{
            //    Helper.ShowNotification("ProcessingRequest");
            //    return;
            //}
            this.Hide();
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await MusicInfoController.SetMusicInfo(CurrentMusic);
            MusicLyricsController.SetLyrics(CurrentMusic);
            AlbumArtController.SetAlbumArt(CurrentMusic);
        }
    }

    public enum MusicDialogOption
    {
        Properties = 0,
        Lyrics = 1,
        AlbumArt = 2
    }
}
