using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class Playlist : ISearchEvaluator
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public SortBy Criterion { get; set; }
        public List<Music> Songs { get; set; }
        public int Priority { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;
        public int Count { get => Songs.Count; }
        public bool IsEmpty { get => Songs.IsEmpty(); }

        public Playlist() { Songs = new List<Music>(); }

        public Playlist(string Name)
        {
            this.Name = Name;
            this.Songs = new List<Music>();
        }

        public Playlist(string name, IEnumerable<Music> songs) 
        {
            Name = name;
            Songs = new List<Music>(songs);
        }

        public bool Contains(IMusicable music)
        {
            return Songs.Contains(music.ToMusic());
        }
        public void Add(object item)
        {
            if (item is IMusicable musicable)
            {
                if (Contains(musicable))
                {
                    return;
                }
                Songs.Add(musicable.ToMusic());
            }
            else if (item is IEnumerable<IMusicable> songs)
            {
                var set = Songs.Select(i => i.Id).ToHashSet();
                bool neverAdded = true;
                foreach (var song in songs)
                {
                    Music music = song.ToMusic();
                    if (!set.Contains(music.Id))
                    {
                        Songs.Add(music);
                        neverAdded = false;
                    }
                }
                if (neverAdded) return;
            }
            else
            {
                return;
            }
            Sort();
        }

        public void Sort()
        {
            Songs.Sort(Criterion);
        }

        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -4);
        }

        public double Match(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword);
        }
    }
}
