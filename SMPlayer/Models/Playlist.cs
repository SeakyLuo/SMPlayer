﻿using SMPlayer.Helpers;
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

        public Playlist() { Songs = new List<Music>(); }
        public Playlist(string name, IEnumerable<Music> songs) 
        {
            Name = name;
            Songs = new List<Music>(songs);
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
