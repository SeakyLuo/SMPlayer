using System;
using System.Collections.Generic;
using System.Linq;
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
            MaxHeap<long> maxHeap = new MaxHeap<long>(dict.Values.Select(v => v.TimeOfLastAccessed))
            {
                MaxItems = 50
            };
            List<K> keys = new List<K>();
            foreach (KeyValuePair<K, CacheValue<V>> pair in dict)
            {
                if (maxHeap.Contains(pair.Value.TimeOfLastAccessed))
                {
                    keys.Add(pair.Key);
                }
            }
            foreach (K key in keys)
            {
                dict.Remove(key);
            }
        }

        public V Get(K key, V defaultValue = null)
        {
            CacheValue<V> cachedValue = dict.GetValueOrDefault(key, null);
            if (cachedValue == null)
            {
                if (Load == null)
                {
                    return defaultValue;
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

    public class MaxHeap<T> where T : IComparable
    {
        public int MaxItems { get; set; } = 1;
        private List<T> list = new List<T>();

        public MaxHeap(params T[] input)
        {
            foreach (T item in input)
            {
                Add(item);
            }
        }

        public MaxHeap(IEnumerable<T> input)
        {
            foreach (T item in input)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            if (list.Count > 0 && item.CompareTo(list[0]) <= 0)
            {
                return;
            }
            if (list.Count == MaxItems)
            {
                list.RemoveAt(0);
            }
            list.InsertWithOrder(item);
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }
    }
}
