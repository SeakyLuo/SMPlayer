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
    public class Music : ISearchEvaluator, IMusicable
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public int PlayCount { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public ActiveState State { get; set; }

        public Music(Music src)
        {
            CopyFrom(src);
        }
        public Music Copy()
        {
            return new Music(this);
        }

        public Music CopyFrom(Music src)
        {
            Id = src.Id;
            Path = src.Path;
            Name = src.Name;
            Artist = src.Artist;
            Album = src.Album;
            Duration = src.Duration;
            PlayCount = src.PlayCount;
            DateAdded = src.DateAdded;
            State = src.State;
            return src;
        }

        public void Played()
        {
            PlayCount++;
        }

        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -5);
        }

        public double Match(string keyword)
        {
            int basePoints = new List<int> { SearchHelper.EvaluateString(Name, keyword), SearchHelper.EvaluateString(Artist, keyword) - 10,
                                             SearchHelper.EvaluateString(Album, keyword) - 20, 0}.Max();
            return basePoints == 0 ? 0 : basePoints + PlayCount / 10;
        }

        public Music ToMusic()
        {
            return this;
        }
    }
}
