using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class Artist : ISearchEvaluator
    {
        public string Name { get; set; }
        public List<Music> Songs { get; set; }

        public Artist() { }

        public Artist(string name, IEnumerable<Music> songs)
        {
            Name = name;
            Songs = songs.ToList();
        }

        public double Evaluate(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword, -2);
        }

        public double Match(string keyword)
        {
            return SearchHelper.EvaluateString(Name, keyword);
        }
    }
}
