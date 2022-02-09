using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class Album : ISearchEvaluator
    {
        public string Name { get; set; }
        public List<Music> Songs { get; set; }
        public string Artist { get; set; }
        public List<string> Artists { get; set; }

        public Album(string name, IEnumerable<Music> songs)
        {
            Name = name;
            Songs = songs.ToList();
            Artists = songs.GroupBy(m => m.Artist).OrderByDescending(i => i.Count()).Select(i => i.Key).ToList();
            Artist = Artists.FirstOrDefault();
        }

        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -1);
        }

        public double Match(string keyword)
        {
            return Math.Max(SearchHelper.EvaluateString(Name, keyword),
                            Artists.Max(i => Math.Max(SearchHelper.EvaluateString(i, keyword) - 10, 0)));
        }
    }
}
