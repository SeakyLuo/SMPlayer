using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    class MusicManager
    {
        public static List<Music> AllSongs = new List<Music>();
        public static List<Music> SortedSongs = new List<Music>();

        public static void Sort(Func<Music, string> lambda, ObservableCollection<Music> list)
        {
            SortedSongs = AllSongs.ToList();
            SortedSongs.OrderBy(lambda);
            CollectionConvert(SortedSongs, list);
        }

        private static void CollectionConvert(List<Music> fromCollection, ObservableCollection<Music> toCollection)
        {
            toCollection.Clear();
            foreach (var item in fromCollection)
                toCollection.Add(item);
        }

    }
}
