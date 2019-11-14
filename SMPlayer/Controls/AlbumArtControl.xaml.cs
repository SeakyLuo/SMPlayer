using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class AlbumArtControl : UserControl
    {
        public bool AllowMusicSwitching { get; set; } = false;
        public bool ShowHeader { get; set; } = false;
        private Music CurrentMusic;
        public AlbumArtControl()
        {
            this.InitializeComponent();
        }

        public async void SetAlbumArt(Music music)
        {
            CurrentMusic = music;
            var thumbnail = await Helper.GetStorageItemThumbnailAsync(music, 1024);
            if (thumbnail.IsThumbnail())
            {
                AlbumArt.Source = thumbnail.GetBitmapImage();
                AlbumArt.Visibility = Visibility.Visible;
            }
            else
            {
                AlbumArt.Visibility = Visibility.Collapsed;
            }
        }

        private async void ChangeAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            var music_file = await CurrentMusic.GetStorageFileAsync();
            using (var dst = await music_file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.MusicView, 1024))
            {
                if (!dst.CanWrite)
                {
                    MainPage.Instance.ShowNotification(Helper.LocalizeMessage("UnableToChangeAlbumArt"));
                    return;
                }
                FileOpenPicker picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                foreach (var item in new string[] { ".jpg", ".png", ".jpeg" })
                    picker.FileTypeFilter.Add(item);
                var file = await picker.PickSingleFileAsync();
                if (file == null) return;
                using (var src_stream = await file.OpenReadAsync())
                {
                    await dst.WriteAsync(await FileIO.ReadBufferAsync(file));
                    var src = new BitmapImage();
                    src.SetSource(src_stream);
                    AlbumArt.Source = src;
                }
            }
            //using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(await CurrentMusic.GetStorageFileAsync()), TagLib.ReadStyle.Average))
            //{
            //    //tagFile.Tag.Pictures[0] = new ;
            //    tagFile.Save();
            //}
        }
    }
}
