using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLib;
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
            using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(await CurrentMusic.GetStorageFileAsync()), ReadStyle.Average))
            {
                FileOpenPicker picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                foreach (var item in new string[] { ".jpg", ".png", ".jpeg", ".mp3" })
                    picker.FileTypeFilter.Add(item);
                var file = await picker.PickSingleFileAsync();
                if (file == null) return;
                var pics = tagFile.Tag.Pictures;
                if (pics.Length == 0) Array.Resize(ref pics, 1);
                if (FolderTree.IsMusicFile(file))
                {
                    using (var source = TagLib.File.Create(new MusicFileAbstraction(file), ReadStyle.Average))
                    {
                        if (source.Tag.Pictures.Length == 0)
                        {
                            MainPage.Instance.ShowNotification(Helper.LocalizeMessage("MusicNoAlbumArt", file.DisplayName));
                            return;
                        }
                        pics[0] = source.Tag.Pictures[0];
                    }
                }
                else
                {
                    pics[0] = new Picture(new MusicFileAbstraction(file))
                    {
                        Type = PictureType.BackCover,
                    };
                }
                // AlbumArt.Source = pics[0];
                tagFile.Tag.Pictures = pics;
                tagFile.Save();
            }
        }
    }
}
