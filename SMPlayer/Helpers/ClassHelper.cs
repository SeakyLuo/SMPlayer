﻿using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class ClassHelper
    {
        public static T AddToFirst<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
            return item;
        }
        public static void PopToSize<T>(this IList<T> list, int size)
        {
            while (list.Count() > size)
            {
                list.RemoveAt(0);
            }
        }
        public static void Sort<T>(this List<T> list, SortBy criterion) where T : ISortable
        {
            list.Sort((m1, m2) => m1.GetComparable(criterion).CompareTo(m2.GetComparable(criterion)));
        }
        public static void InsertWithOrder<T>(this ObservableCollection<T> list, T sortable, SortBy criterion = SortBy.Default) where T : ISortable
        {
            list.Insert(FindSortedListInsertIndex(list, sortable, criterion), sortable);
        }
        public static void InsertWithOrder<T>(this ObservableCollection<T> list, T comparable) where T : IComparable
        {
            list.Insert(FindSortedListInsertIndex(list, comparable), comparable);
        }
        public static void InsertWithOrder<T>(this List<T> list, T comparable) where T : IComparable
        {
            list.Insert(FindSortedListInsertIndex(list, comparable), comparable);
        }
        public static int FindSortedListInsertIndex<T>(this IEnumerable<T> list, T sortable, SortBy criterion) where T : ISortable
        {
            IComparable comparable = sortable.GetComparable(criterion);
            int count = list.Count();
            for (int i = 0; i < count - 1; i++)
                if (0 < comparable.CompareTo(list.ElementAt(i).GetComparable(criterion)) &&
                    comparable.CompareTo(list.ElementAt(i + 1).GetComparable(criterion)) < 0)
                    return i;
            return count;
        }
        public static int FindSortedListInsertIndex<T>(this IEnumerable<T> list, T comparable) where T : IComparable
        {
            int count = list.Count();
            for (int i = 0; i < count - 1; i++)
                if (0 < comparable.CompareTo(list.ElementAt(i)) && comparable.CompareTo(list.ElementAt(i + 1)) < 0)
                    return i;
            return count;
        }
        public static int FindSortedListInsertIndex<T>(this IEnumerable<T> list, T target, Func<T, IComparable> selector)
        {
            IComparable comparable = selector(target);
            int count = list.Count();
            for (int i = 0; i < count - 1; i++)
                if (0 < comparable.CompareTo(selector(list.ElementAt(i))) && comparable.CompareTo(selector(list.ElementAt(i + 1))) < 0)
                    return i;
            return count;
        }
        public static MenuFlyoutSubItem ToSubItem(this MenuFlyout flyout, string text, IconElement icon = null, string tooltip = null)
        {
            MenuFlyoutSubItem subItem = new MenuFlyoutSubItem
            {
                Text = Helper.LocalizeText(text),
                Icon = icon,
            };
            if (!string.IsNullOrEmpty(tooltip)) subItem.SetToolTip(Helper.LocalizeText(tooltip));
            foreach (var item in flyout.Items)
                subItem.Items.Add(item);
            return subItem;
        }
        public static MenuFlyout ToMenuFlyout(this MenuFlyoutSubItem subItem)
        {
            MenuFlyout flyout = new MenuFlyout();
            foreach (var item in subItem.Items)
                flyout.Items.Add(item);
            return flyout;
        }
        public static List<T> Shuffle<T>(this IEnumerable<T> enumerable)
        {
            List<T> list = enumerable.ToList();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Helper.RandRange(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static T RandItem<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(Helper.RandRange(list.Count()));
        }

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
            Debug.WriteLine(JoinIntoString(list));
        }
        public static string JoinIntoString<T>(this IEnumerable<T> list)
        {
            return $"[{string.Join(", ", list.Select(i => i.ToString()))}]";
        }
        public static string Join<T>(this IEnumerable<T> list, string joiner)
        {
            return string.Join(joiner, list.Select(i => i.ToString()));
        }
        public static void AddOrMoveToTheFirst<T>(this Collection<T> list, T item)
        {
            if (item == null) return;
            if (list.IsEmpty())
            {
                list.Add(item);
                return;
            }
            if (item.Equals(list.First())) return;
            list.Remove(item);
            list.Insert(0, item);
        }
        public static void AddOrMoveToTheFirst<T>(this List<T> list, T item)
        {
            if (item.Equals(list.ElementAtOrDefault(0)))
                return;
            list.Remove(item);
            list.Insert(0, item);
        }
        public static void Move<T>(this List<T> list, int from, int to)
        {
            T ele = list[from];
            list.Remove(ele);
            list.Insert(to, ele);
        }
        public static int FindIndex<T>(this IEnumerable<T> list, Predicate<T> match)
        {
            if (match == null) return -1;
            for (int i = 0; i < list.Count(); i++)
                if (match(list.ElementAt(i)))
                    return i;
            return -1;
        }
        public static int FindIndex<T>(this IEnumerable<T> list, T target)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                T element = list.ElementAt(i);
                if (element == null)
                {
                    if (target == null)
                        return i;
                }
                else
                {
                    if (element.Equals(target))
                        return i;
                }
            }
            return -1;
        }
        public static void SetTo(this MenuFlyout dst, MenuFlyout src)
        {
            dst.Items.Clear();
            foreach (var item in src.Items)
                dst.Items.Add(item);
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
        public static ObservableCollection<T> SetToCopy<T>(this ObservableCollection<T> dst, IEnumerable<T> src)
        {
            return dst.SetTo(src.ToList());
        }
        public static bool IsMusicFile(this StorageFile file)
        {
            return MusicHelper.SupportedFileTypes.Any(i => file.FileType.EndsWith(i));
        }
        public static async Task<int> CountFoldersAsync(this StorageFolder folder)
        {
            int count = 0;
            foreach (var sub in await folder.GetFoldersAsync())
                count += 1 + await CountFoldersAsync(sub);
            return count;
        }
        public static async Task<int> CountItemsAsync(this StorageFolder folder)
        {
            int count = 1 + (await folder.GetFilesAsync()).Count(f => f.IsMusicFile());
            foreach (var sub in await folder.GetFoldersAsync())
                count += 1 + await CountItemsAsync(sub);
            return count;
        }
        public static string GetLyrics(this StorageFile file)
        {
            using (var tagFile = TagLib.File.Create(new MusicFileAbstraction(file), TagLib.ReadStyle.Average))
            {
                return tagFile.Tag.Lyrics;
            }
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
            ToolTipService.SetToolTip(obj, localize ? Helper.LocalizeText(tooltip) : tooltip);
        }
        public static bool SameAs(this IEnumerable<IMusicable> list1, IEnumerable<IMusicable> list2)
        {
            return list1?.Count() == list2?.Count() && list1.Zip(list2, (m1, m2) => m1.ToMusic().Equals(m2.ToMusic())).All(res => res);
        }

        public static bool IsThumbnail(this StorageItemThumbnail thumbnail)
        {
            return thumbnail != null && thumbnail.Type == ThumbnailType.Image;
        }

        public static BitmapImage ToBitmapImage(this StorageItemThumbnail thumbnail)
        {
            if (thumbnail == null)
            {
                return null;
            }
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(thumbnail);
            return bitmapImage;
        }

        public static async Task<Brush> GetDisplayColor(this StorageItemThumbnail thumbnail)
        {
            return thumbnail == null ? null : await ColorHelper.GetThumbnailMainColorAsync(thumbnail.CloneStream());
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

        public static void ClearSelections(this ListViewBase view)
        {
            try
            {
                view.SelectedItems.Clear();
            }
            catch (Exception e)
            {
                // ???
                Log.Debug("灾难性故障, {0}", e);
            }
        }

        public static void ReverseSelections(this ListViewBase view)
        {
            if (view.SelectedItems.IsEmpty())
            {
                view.SelectAll();
                return;
            }
            if (view.SelectedItems.Count == view.Items.Count)
            {
                view.ClearSelections();
                return;
            }
            var selected = view.SelectedItems.ToHashSet();
            view.ClearSelections();
            foreach (var item in view.Items)
            {
                if (!selected.Contains(item))
                {
                    view.SelectedItems.Add(item);
                }
            }
        }

        public static void ClearSelections(this TreeView view)
        {
            try
            {
                view.SelectedNodes.Clear();
            } 
            catch (Exception e)
            {
                // ???
                Log.Debug("灾难性故障, {0}", e);
            }
        }

        public static void ReverseSelections(this TreeView view)
        {
            if (view.SelectedNodes.IsEmpty())
            {
                view.SelectAll();
                return;
            }
            if (view.SelectedNodes.Count == view.RootNodes.Count)
            {
                view.ClearSelections();
                return;
            }
            var selected = view.SelectedNodes.ToHashSet();
            view.ClearSelections();
            foreach (var item in view.RootNodes)
            {
                if (!selected.Contains(item))
                {
                    view.SelectedNodes.Add(item);
                }
            }
        }

        public static ICollection<T> RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            collection.Where(predicate).ToList().ForEach(e => collection.Remove(e));
            return collection;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                collection.Add(item);
            }
        }

        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> ienumerable, int start, int end)
        {
            return ienumerable.Skip(start).Take(end - start);
        }

        public static string GetValue(this MatchCollection matchCollection)
        {
            return matchCollection.ElementAt(0).Value.Trim();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> src)
        {
            return src == null || src.Count() == 0;
        }

        public static bool IsNotEmpty<T>(this IEnumerable<T> src)
        {
            return !IsEmpty(src);
        }

        public static Music GetMusic(this MediaPlaybackItem item)
        {
            object music = null;
            item?.Source.CustomProperties.TryGetValue("Source", out music);
            return (Music)music;
        }

        public static void SetMusic(this MediaPlaybackItem item, Music music)
        {
            item.Source.CustomProperties.Clear();
            item.Source.CustomProperties.AddRange(music.GetMediaPlaybackItem().Source.CustomProperties);
        }

        public static IEnumerable<T> RandItems<T>(this IEnumerable<T> enumerable, int count) where T : class
        {
            List<T> list = enumerable is List<T> l ? l : enumerable.ToList();
            if (list.Count <= count) return enumerable.Shuffle();
            HashSet<int> indices = new HashSet<int>();
            while (indices.Count < count)
            {
                indices.Add(Helper.RandRange(list.Count));
            }
            return indices.Select(index => list[index]);
        }

        public static bool IsMusic(this FileType fileType)
        {
            return fileType == FileType.Music;
        }

        public static void ScrollToIndex(this ListViewBase listViewBase, int index)
        {
            if (listViewBase?.ContainerFromIndex(index) is ListViewItem item)
            {
                item.Locate();
            }
        }

        public static async Task ScrollToIndexAsync(this ListViewBase listViewBase, int index)
        {
            if (listViewBase?.ContainerFromIndex(index) is ListViewItem item)
            {
                // 动画更好，优先这个
                item.Locate();
                return;
            }
            // 确保 ListView 中的项已经加载并可用
            await listViewBase.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                listViewBase.ScrollIntoView(listViewBase.Items[index], ScrollIntoViewAlignment.Default);
            });
            int tryLoadCount = 50;
            // 等待滚动完成，不用while，避免死循环
            for (int i = 0; i < tryLoadCount; i++)
            {
                if (listViewBase.ContainerFromIndex(index) == null)
                {
                    await Task.Delay(100); // 延迟一段时间等待项加载
                } 
                else
                {
                    return;
                }
            }
            Log.Info($"{tryLoadCount}次都没加载到");
        }

        public static void ScrollToTop(this ListViewBase listViewBase)
        {
            ScrollToIndex(listViewBase, 0);
        }

        public static void RemoveAfter<T>(this ObservableCollection<T> collection, int index)
        {
            while (collection.Count > index)
            {
                collection.RemoveAt(index);
            }
        }
    }
}