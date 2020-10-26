using System;
using System.Collections.Generic;
using System.Timers;

namespace SMPlayer
{
    public class LoadingCache<K, V> where V : class
    {
        private static long CurrentTimeInSeconds { get => DateTime.Now.Second; }
        public int MaxSize { get; set; } = int.MaxValue;
        public long TimeOfExpiration { get; set; }
        public Func<K, V> Load { private get ; set; }
        private readonly Dictionary<K, CacheValue<V>> dict = new Dictionary<K, CacheValue<V>>();
        private readonly Timer timer = new Timer();
        public LoadingCache(long timeOfExpiration, TimeUnit unit)
        {
            this.TimeOfExpiration = timeOfExpiration;
            switch (unit)
            {
                case TimeUnit.Day:
                    timer.Interval = 86400000;
                    break;
                case TimeUnit.Hour:
                    timer.Interval = 3600000;
                    break;
                case TimeUnit.Minute:
                    timer.Interval = 60000;
                    break;
                case TimeUnit.Second:
                    timer.Interval = 1000;
                    break;
                case TimeUnit.Millisecond:
                    timer.Interval = 1;
                    break;
            }
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (dict.Count == 0) return;
            long time = CurrentTimeInSeconds;
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

        public void PutIfNonNull(K key, V value)
        {
            if (value != null)
            {
                Put(key, value);
            }
        }
        public void Put(K key, V value)
        {
            if (dict.Count >= MaxSize)
            {
                RemoveLRU();
            }
            dict[key] = new CacheValue<V>(value);
            //dict.TryAdd(key, new CacheValue<V>(value));
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
                cachedValue.TimeOfLastAccessed = CurrentTimeInSeconds;
            }
            return cachedValue.Value;
        }

        public bool Delete(K key)
        {
            return dict.Remove(key);
        }

        public bool ContainsKey(K key)
        {
            return dict.ContainsKey(key);
        }

        class CacheValue<T>
        {
            public T Value { get; set; }
            public long TimeOfLastAccessed { get; set; } = 0;
            public CacheValue(T value)
            {
                this.Value = value;
                this.TimeOfLastAccessed = CurrentTimeInSeconds;
            }
        }
    }

    public enum TimeUnit
    {
        Day, Hour, Minute, Second, Millisecond
    }
}
