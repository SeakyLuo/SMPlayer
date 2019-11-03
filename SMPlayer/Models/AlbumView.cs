﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace SMPlayer.Models
{
    public class AlbumView : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; }
        public BitmapImage Cover
        {
            get => thumbnail;
            set
            {
                if (value == null) return;
                thumbnail = value;
                coverLoaded = true;
                OnPropertyChanged();
            }
        }
        private BitmapImage thumbnail = Helper.DefaultAlbumCover;
        private bool coverLoaded = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public AlbumView() { }
        public AlbumView(string name, string artist)
        {
            Name = name;
            Artist = artist;
        }
        public AlbumView(string name, string artist, IEnumerable<Music> songs, bool setCover = true)
        {
            Name = name;
            Artist = artist;
            Songs = new ObservableCollection<Music>(songs);
            if (setCover) SetCover();
        }
        public async void SetCover()
        {
            if (!coverLoaded) Cover = await GetAlbumCoverAsync(Songs);
        }
        public static async System.Threading.Tasks.Task<BitmapImage> GetAlbumCoverAsync(ICollection<Music> songs)
        {
            BitmapImage cover;
            foreach (var music in songs)
                if ((cover = await Helper.GetThumbnailAsync(music, false)) != null)
                    return cover;
            return Helper.DefaultAlbumCover;
        }
        public Playlist ToPlaylist()
        {
            return new Playlist(Name, Songs)
            {
                Artist = Artist
            };
        }
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return Name == (obj as AlbumView).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}