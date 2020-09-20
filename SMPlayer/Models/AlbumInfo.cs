using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class AlbumInfo
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Thumbnail { get; set; }

        public AlbumInfo(string name, string artist, string thumbnail)
        {
            Name = name;
            Artist = artist;
            Thumbnail = thumbnail;
        }

        public AlbumView ToAlbumView()
        {
            return new AlbumView(Name, Artist, Thumbnail);
        }

        public override bool Equals(object obj)
        {
            return (obj is AlbumView album && Name == album.Name && Artist == album.Artist) ||
                   (obj is AlbumInfo info && Name == info.Name && Artist == info.Artist);
        }

        public override int GetHashCode()
        {
            return (Name + "%" + Artist).GetHashCode();
        }
    }
}
