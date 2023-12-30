using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class AlbumArtControl : UserControl
    {
        public bool AllowMusicSwitching { get; set; } = false;
        public bool ShowHeader { get; set; } = false;
        public volatile bool IsProcessing = false;
        public static List<IImageSavedListener> ImageSavedListeners = new List<IImageSavedListener>();
        private MusicView CurrentMusic;
        private AlbumView CurrentAlbum;
        private IPicture[] sourcePics = null;

        public AlbumArtControl()
        {
            this.InitializeComponent();
        }

        public async void SetAlbumArt(AlbumView album)
        {
            CurrentAlbum = album;
            RemoveAlbumArtWarningTextBlock.Text = Helper.LocalizeMessage("RemoveAlbumArt", string.IsNullOrEmpty(CurrentAlbum.Name) ? Helper.LocalizeMessage("UnknownAlbum") : CurrentAlbum.Name);
            foreach (var music in CurrentAlbum.Songs)
            {
                var thumbnail = await Helper.GetStorageItemThumbnailAsync(music, 1024);
                if (thumbnail.IsThumbnail())
                {
                    AlbumArt.Source = thumbnail.ToBitmapImage();
                    AlbumArt.Visibility = Visibility.Visible;
                    return;
                }

            }
            AlbumArt.Visibility = Visibility.Collapsed;
        }

        public async void SetAlbumArt(Music music)
        {
            CurrentMusic = music.ToVO();
            RemoveAlbumArtWarningTextBlock.Text = Helper.LocalizeMessage("RemoveAlbumArt", CurrentMusic.Name);
            var thumbnail = await Helper.GetStorageItemThumbnailAsync(music.ToVO(), 1024);
            if (thumbnail.IsThumbnail())
            {
                AlbumArt.Source = thumbnail.ToBitmapImage();
                AlbumArt.Visibility = Visibility.Visible;
            }
            else
            {
                AlbumArt.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            if (sourcePics == null)
            {
                return;
            }
            IsProcessing = true;
            SaveProgress.Visibility = Visibility.Visible;
            SetButtonEnability(false);
            try
            {
                BitmapImage image = await BytesToBitmapImage(sourcePics[0].Data.ToArray());
                if (CurrentMusic == null)
                {
                    await Task.Run(async () =>
                    {
                        foreach (var music in CurrentAlbum.Songs)
                            await SaveAlbumArt(music, sourcePics);
                    });
                    foreach (var listener in ImageSavedListeners)
                        listener.SaveAlbum(CurrentAlbum, image);
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        await SaveAlbumArt(CurrentMusic, sourcePics);
                    });
                    foreach (var listener in ImageSavedListeners)
                        listener.SaveMusic(CurrentMusic, image);
                }
                Helper.ShowNotification("AlbumArtSaved");
            }
            catch (Exception ex)
            {
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("UpdateFailed", ex.Message), 5000);
            }
            finally
            {
                SetButtonEnability(true);
                SaveProgress.Visibility = Visibility.Collapsed;
                IsProcessing = false;
            }
        }

        private void SetButtonEnability(bool isEnabled)
        {
            ChangeAlbumArtButton.IsEnabled = SaveAlbumArtButton.IsEnabled = DeleteAlbumArtButton.IsEnabled = isEnabled;
        }

        private async Task SaveAlbumArt(MusicView music, IPicture[] source)
        {
            using (var tagFile = await music.GetTagFileAsync())
            {
                tagFile.Tag.Pictures = source;
                tagFile.Save();
            }
        }

        private async void ChangeAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            SaveProgress.Visibility = Visibility.Visible;
            SetButtonEnability(false);
            try
            {
                FileOpenPicker picker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                List<string> fileTypes = new List<string>() { ".jpg", ".png", ".jpeg" };
                fileTypes.AddRange(MusicHelper.SupportedFileTypes);
                foreach (var item in fileTypes)
                    picker.FileTypeFilter.Add(item);
                var file = await picker.PickSingleFileAsync();
                if (file == null) goto Finally;
                await Task.Run(() =>
                {
                    if (file.IsMusicFile())
                    {
                        using (var source = file.CreateTagFile())
                        {
                            if (source == null)
                            {
                                return;
                            }
                            if (source.Tag.Pictures.Length == 0)
                            {
                                Helper.ShowNotificationRaw(Helper.LocalizeMessage("MusicNoAlbumArt", file.DisplayName));
                                return;
                            }
                            sourcePics = source.Tag.Pictures;
                        }
                    }
                    else
                    {
                        sourcePics = new Picture[] { new Picture(new MusicFileAbstraction(file)) { Type = PictureType.BackCover } };
                    }
                });
                AlbumArt.Source = await BytesToBitmapImage(sourcePics[0].Data.ToArray());
                AlbumArt.Visibility = Visibility.Visible;
            }
            catch (Exception exception)
            {
                Log.Warn($"ChangeAlbumArtButton_Click Failed {exception}");
                Helper.ShowNotification("Error");
            }
        Finally:
            SetButtonEnability(true);
            SaveProgress.Visibility = Visibility.Collapsed;
            IsProcessing = false;
        }

        public static async Task<BitmapImage> BytesToBitmapImage(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                await image.SetSourceAsync(stream.AsRandomAccessStream());
            }
            return image;
        }

        private void DeleteAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlbumArtWarningPanel.Visibility = Visibility.Visible;
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsProcessing)
            {
                Helper.ShowNotification("ProcessingRequest");
                return;
            }
            IsProcessing = true;
            SetButtonEnability(false);
            try
            {
                if (CurrentMusic == null)
                {
                    foreach (var music in CurrentAlbum.Songs)
                        await SaveAlbumArt(music, null);
                    foreach (var listener in ImageSavedListeners)
                        listener.SaveAlbum(CurrentAlbum, null);
                }
                else
                {
                    await SaveAlbumArt(CurrentMusic, null);
                    foreach (var listener in ImageSavedListeners)
                        listener.SaveMusic(CurrentMusic, null);
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"ConfirmButton_Click failed {ex}");
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("UpdateFailed", ex.Message), 5000);
            }
            finally
            {
                AlbumArt.Visibility = Visibility.Collapsed;
                RemoveAlbumArtWarningPanel.Visibility = Visibility.Collapsed;
                IsProcessing = false;
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveAlbumArtWarningPanel.Visibility = Visibility.Collapsed;
        }
    }

    public interface IImageSavedListener
    {
        void SaveAlbum(AlbumView album, BitmapImage image);
        void SaveMusic(MusicView music, BitmapImage image);
    }
}
