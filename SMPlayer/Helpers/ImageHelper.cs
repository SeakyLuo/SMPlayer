using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer
{
    public static class ImageHelper
    {
        private static readonly LoadingCache<string, BitmapImage> imageCache = new LoadingCache<string, BitmapImage>(600)
        {
            MaxSize = 100
        };
        private static readonly LoadingCache<string, StorageItemThumbnail> thumbnailCache = new LoadingCache<string, StorageItemThumbnail>(600)
        {
            MaxSize = 10
        };

        public static async Task<BitmapImage> LoadImage(string path)
        {
            BitmapImage image = imageCache.Get(path);
            if (image == null)
            {
                image = await Helper.GetThumbnailAsync(path, false);
                imageCache.PutIfPresent(path, image);
            }
            if (image == null)
            {
                image = thumbnailCache.Get(path)?.ToBitmapImage();
            }
            return image;
        }

        public static async Task<BitmapImage> LoadImage(Music music)
        {
            return await LoadImage(music.Path);
        }

        public static async Task<StorageItemThumbnail> LoadThumbnail(Music music)
        {
            return await LoadThumbnail(music.Path);
        }

        public static async Task<StorageItemThumbnail> LoadThumbnail(string path)
        {
            StorageItemThumbnail thumbnail = thumbnailCache.Get(path);
            if (thumbnail == null)
            {
                thumbnail = await Helper.GetStorageItemThumbnailAsync(path);
                thumbnailCache.PutIfPresent(path, thumbnail);
            }
            return thumbnail;
        }

        public static void CacheImage(string path, BitmapImage item)
        {
            imageCache.Put(path, item);
        }

        public static void CacheThumbnail(string path, StorageItemThumbnail item)
        {
            thumbnailCache.Put(path, item);
        }
    }

    public class LoadingCache<K, V> where V : class
    {
        public int MaxSize { get; set; } = int.MaxValue;
        public long TimeOfExpiration { get; set; }
        public Func<K, V> Load { private get ; set; }
        private readonly Dictionary<K, CacheValue<V>> dict = new Dictionary<K, CacheValue<V>>();
        private readonly Timer timer = new Timer() { Interval = 1000 };
        public LoadingCache(long timeOfExpirationInSeconds)
        {
            this.TimeOfExpiration = timeOfExpirationInSeconds;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            long time = DateTime.Now.Second;
            List<K> keys = new List<K>();
            foreach (KeyValuePair<K, CacheValue<V>> pair in dict)
            {
                if (time - pair.Value.TimeOfLastAccessed > TimeOfExpiration)
                {
                    keys.Add(pair.Key);
                }
            }
            foreach (K key in keys)
            {
                dict.Remove(key);
            }
        }

        public void PutIfPresent(K key, V value)
        {
            if (value != null)
            {
                Put(key, value);
            }
        }
        public void Put(K key, V value)
        {
            if (dict.Count > MaxSize)
            {
                RemoveLRU();
            }
            dict.TryAdd(key, new CacheValue<V>(value));
        }
        private void RemoveLRU()
        {
            List<K> keys = new List<K>();
            long time = long.MaxValue;
            foreach (KeyValuePair<K, CacheValue<V>> pair in dict)
            {
                if (pair.Value.TimeOfLastAccessed < time)
                {
                    keys.Clear();
                    keys.Add(pair.Key);
                }
                else if (pair.Value.TimeOfLastAccessed == time)
                {
                    keys.Add(pair.Key);
                }
            }
            foreach (K key in keys)
            {
                dict.Remove(key);
            }
        }

        public V Get(K key)
        {
            CacheValue<V> cachedValue = dict.GetValueOrDefault(key, null);
            if (cachedValue == null)
            {
                if (Load == null)
                {
                    return null;
                }
                cachedValue = new CacheValue<V>(Load.Invoke(key));
            }
            else
            {
                cachedValue.TimeOfLastAccessed = DateTime.Now.Second;
            }
            return cachedValue.Value;
        }

        class CacheValue<T> {
            public T Value { get; set; }
            public long TimeOfLastAccessed { get; set; } = 0;
            public CacheValue(T value)
            {
                this.Value = value;
                this.TimeOfLastAccessed = DateTime.Now.Second;
            }
        }
    }
}
