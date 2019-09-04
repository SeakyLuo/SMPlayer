﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    class ArtistView
    {
        public string Name { get; set; }
        public ObservableCollection<AlbumView> Albums { get; set; }

        public ArtistView(string Name, ObservableCollection<AlbumView> Albums)
        {
            this.Name = Name;
            this.Albums = Albums;
        }

        public List<Music> GetSongs()
        {
            List<Music> list = new List<Music>();
            foreach (var album in Albums)
                list.AddRange(album.Songs.ToList());
            return list;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is ArtistView && Name == (obj as ArtistView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}
