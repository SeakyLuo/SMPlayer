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
            RemoveAlbumArtWarningTextBlock.Text = Helper.LocalizeMessage("RemoveAlbumArt", CurrentMusic.Name);
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
            try
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
                    if (FolderTree.IsMusicFile(file))
                    {
                        using (var source = TagLib.File.Create(new MusicFileAbstraction(file), ReadStyle.Average))
                        {
                            if (source.Tag.Pictures.Length == 0)
                            {
                                MainPage.Instance.ShowNotification(Helper.LocalizeMessage("MusicNoAlbumArt", file.DisplayName));
                                return;
                            }
                            tagFile.Tag.Pictures = source.Tag.Pictures;
                        }
                    }
                    else
                    {
                        tagFile.Tag.Pictures = new Picture[] { new Picture(new MusicFileAbstraction(file)) { Type = PictureType.BackCover } };
                    }
                    AlbumArt.Source = await BytesToBitmapImage(tagFile.Tag.Pictures[0].Data.ToArray());
                    AlbumArt.Visibility = Visibility.Visible;
                    tagFile.Save();
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("Error in AlbumArtControl: " + exception);
                Helper.ShowNotification(Helper.LocalizeMessage("Error"));
            }
        }
        public static async System.Threading.Tasks.Task<BitmapImage> BytesToBitmapImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                await image.SetSourceAsync(mem.AsRandomAccessStream());
            }
            return image;
        }

        private void DeleteAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlbumArtWarningPanel.Visibility = Visibility.Visible;
            //await new Dialogs.RemoveDialog()
            //{
            //    Confirm = () => ConfirmButton_Click(null, null),
            //    CheckBoxVisibility = Visibility.Collapsed
            //}.ShowAsync();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(await CurrentMusic.GetStorageFileAsync()), ReadStyle.Average))
            {
                tagFile.Tag.Pictures = null;
                tagFile.Save();
            }
            AlbumArt.Visibility = Visibility.Collapsed;
            RemoveAlbumArtWarningPanel.Visibility = Visibility.Collapsed;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlbumArtWarningPanel.Visibility = Visibility.Collapsed;
        }
    }
}
