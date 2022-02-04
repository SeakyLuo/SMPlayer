using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System;
using SMPlayer.Helpers;
using SMPlayer.Models.VO;
using SMPlayer.Services;

namespace SMPlayer.Models
{
    public class AlbumView : INotifyPropertyChanged, IPreferable
    {
        public string Name { get; set; }
        public string Artist { get; set; }
        public ObservableCollection<Music> Songs { get; set; }
        public BitmapImage Thumbnail
        {
            get => thumbnail;
            set
            {
                thumbnail = value;
                ThumbnailLoaded = value != null;
                OnPropertyChanged();
            }
        }
        private BitmapImage thumbnail = MusicImage.DefaultImage;
        public string ThumbnailSource { get; set; }
        public bool ThumbnailLoaded { get; private set; } = false;
        public bool IsThumbnailLoading { get; private set; } = false;
        public bool DontLoad { get => ThumbnailLoaded || IsThumbnailLoading; }
        public EntityType EntityType { get; set; } = EntityType.Album;
        public long OriginalItemId;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public AlbumView() { }
        public AlbumView(Music music, bool setThumbnail = true)
        {
            Name = music.Album;
            Artist = music.Artist;
            Songs = new ObservableCollection<Music>() { music };
            if (setThumbnail) SetThumbnail();
        }
        public AlbumView(string name, string artist)
        {
            Name = name;
            Artist = artist;
            Songs = new ObservableCollection<Music>();
        }
        public AlbumView(string name, string artist, string thumbnail)
        {
            Name = name;
            Artist = artist;
            ThumbnailSource = thumbnail;
        }
        public AlbumView(string name, string artist, IEnumerable<Music> songs, bool setThumbnail = true)
        {
            Name = name;
            Artist = artist;
            Songs = new ObservableCollection<Music>(songs);
            if (setThumbnail) SetThumbnail();
        }
        public AlbumView(string name, IEnumerable<Music> songs, bool setThumbnail = true)
        {
            Name = name;
            Songs = new ObservableCollection<Music>(songs);
            List<string> artists = songs.GroupBy(m => m.Artist).OrderByDescending(i => i.Count()).Select(i => i.Key).ToList();
            if (artists.Count() >= 3)
            {
                Artist = Helper.LocalizeText("ArtistsAndSoOn", artists[0], artists[1], artists.Count());
            }
            else
            {
                Artist = string.Join(Helper.LocalizeText("ArtistSeperator"), artists);
            }
            if (setThumbnail) SetThumbnail();
        }
        private async void SetThumbnail()
        {
            if (ThumbnailSource == null)
                await SetThumbnailAsync();
            else if (ThumbnailSource == "")
                Thumbnail = MusicImage.DefaultImage;
            else
                Thumbnail = await ImageHelper.LoadImage(ThumbnailSource);
        }
        public async Task SetThumbnailAsync()
        {
            if (ThumbnailLoaded || IsThumbnailLoading) return;
            IsThumbnailLoading = true;
            if (string.IsNullOrEmpty(ThumbnailSource) || !(await ImageHelper.LoadImage(ThumbnailSource) is BitmapImage thumbnail))
            {
                MusicImage image = await GetAlbumCoverAsync(Songs);
                Thumbnail = image.Image;
                ThumbnailSource = image.Path;
            }
            else
            {
                Thumbnail = thumbnail;
            }
            IsThumbnailLoading = false;
        }
        public static async Task<MusicImage> GetAlbumCoverAsync(IEnumerable<Music> songs)
        {
            foreach (var music in songs)
                if (await ImageHelper.LoadImage(music) is BitmapImage image)
                    return new MusicImage(music.Path, image);
            return MusicImage.Default;
        }
        public void AddMusic(Music music)
        {
            Songs.InsertWithOrder(music);
        }
        public void RemoveMusic(Music music)
        {
            Songs.Remove(music);
        }
        public void SetSongs(IEnumerable<Music> music)
        {
            Songs = new ObservableCollection<Music>(music);
        }

        public Playlist ToPlaylist()
        {
            if (EntityType == EntityType.Album)
            {
                return new Playlist(Name, Songs.OrderBy(i => i.Artist).ThenBy(i => i.Name))
                {
                    Artist = Artist,
                    EntityType = EntityType.Album,
                };
            }
            else
            {
                return new Playlist(Name, Songs)
                {
                    Id = PlaylistService.FindPlaylist(Name)?.Id ?? 0,
                    Artist = Artist,
                };
            }
        }

        public bool Contains(Music music)
        {
            return Songs.Contains(music);
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is AlbumView album && Name == album.Name;
        }

        public override int GetHashCode()
        {
            return GetAlbumKey().GetHashCode();
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return new PreferenceItem(GetAlbumKey(), ConcatNameAndArtist(), EntityType.Album);
        }

        private string GetAlbumKey()
        {
            return Name;
        }

        private string ConcatNameAndArtist()
        {
            return (string.IsNullOrEmpty(Name) ? Helper.LocalizeMessage("UnknownAlbum") : Name) + " - " + Artist;
        }
    }
}