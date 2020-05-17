using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class ClassHelper
    {
        public static string RemoveBraces(this string str, char left, char right)
        {
            int start = str.IndexOf(left), end = str.IndexOf(right);
            return start != -1 && end != -1 ? str.Substring(0, start) + str.Substring(end + 1) : str;
        }
        public static void Locate(this UIElement uIElement)
        {
            uIElement.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true, VerticalAlignmentRatio = 0 });
        }
        public static void Print<T>(this IEnumerable<T> list)
        {
            Debug.WriteLine($"[{string.Join(", ", list.Select(i => i.ToString()))}]");
        }
        public static void AddOrMoveToTheFirst<T>(this Collection<T> list, T item)
        {
            if (item.Equals(list.ElementAtOrDefault(0)))
                return;
            list.Remove(item);
            list.Insert(0, item);
        }
        public static int FindIndex<T>(this IEnumerable<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count(); i++)
                if (match(list.ElementAt(i)))
                    return i;
            return -1;
        }
        public static ObservableCollection<T> SetTo<T>(this ObservableCollection<T> dst, IEnumerable<T> src)
        {
            if (dst == null) dst = new ObservableCollection<T>(src);
            else
            {
                dst.Clear();
                foreach (var item in src) dst.Add(item);
            }
            return dst;
        }
        public static bool IsMusicFile(this StorageFile file)
        {
            return file.FileType.EndsWith("mp3");
        }
        public static async Task<bool> Contains(this StorageFolder folder, string name)
        {
            try
            {
                return await folder.TryGetItemAsync(name) != null;
            }
            catch (ArgumentException)
            {
                // Value does not fall within the expected range.
                // e.g. ?.png
                return false;
            }
        }
        public static void SetToolTip(this DependencyObject obj, string tooltip, bool localize = true)
        {
            if (obj == null) return;
            ToolTipService.SetToolTip(obj, localize ? Helper.Localize(tooltip) : tooltip);
        }
        public static bool SameAs(this IEnumerable<Music> list1, IEnumerable<Music> list2)
        {
            return list1.Count() == list2.Count() && list1.Zip(list2, (m1, m2) => m1.Equals(m2)).All(res => res);
        }

        public static bool IsThumbnail(this StorageItemThumbnail thumbnail)
        {
            return thumbnail != null && thumbnail.Type == ThumbnailType.Image;
        }

        public static BitmapImage GetBitmapImage(this StorageItemThumbnail thumbnail)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(thumbnail);
            return bitmapImage;
        }

        public static async Task<Brush> GetDisplayColor(this StorageItemThumbnail thumbnail)
        {
            return await ColorHelper.GetThumbnailMainColorAsync(thumbnail.CloneStream());
        }


        public static async Task<StorageFile> SaveAsync(this StorageItemThumbnail thumbnail, StorageFolder folder, string name, bool encode = false)
        {
            using (var stream = thumbnail.CloneStream())
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                var filename = name + ".png";
                if (encode) filename = WebUtility.UrlEncode(filename);
                bool notExists = !await folder.Contains(filename);
                var file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                if (notExists)
                {
                    using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, filestream);
                        encoder.SetSoftwareBitmap(softwareBitmap);
                        await encoder.FlushAsync();
                    }
                }
                return file;
            }
        }
    }
}
