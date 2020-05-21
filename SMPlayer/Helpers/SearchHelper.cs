using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public static class SearchHelper
    {
        public static IEnumerable<Playlist> SearchArtists(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            var list = source.Where(m => IsTargetArtist(m, keyword))
                            .GroupBy(m => m.Artist)
                            .Select(group => new Playlist(group.Key, group) { Artist = group.Key });
            return SortArtists(list, keyword, criterion);
        }
        public static IEnumerable<AlbumView> SearchAlbums(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            var list = source.Where(m => IsTargetAlbum(m, keyword))
                            .GroupBy(m => m.Album)
                            .Select(group =>
                            {
                                Music music = group.ElementAt(0);
                                return new AlbumView(music.Album, music.Artist, group.OrderBy(m => m.Name).ThenBy(m => m.Artist), false);
                            });
            return SortAlbums(list, keyword, criterion);
        }
        public static IEnumerable<Music> SearchSongs(IEnumerable<Music> source, string keyword, SortBy criterion)
        {
            var list = source.Where(m => IsTargetMusic(m, keyword));
            return SortSongs(list, keyword, criterion);
        }
        public static IEnumerable<AlbumView> SearchPlaylists(IEnumerable<Playlist> source, string keyword, SortBy criterion)
        {
            List<AlbumView> list = source.Where(i => IsTargetPlaylist(i, keyword)).Select(i => i.ToSearchAlbumView()).ToList();
            return SortPlaylists(list, keyword, criterion);
        }
        public static IEnumerable<GridFolderView> SearchFolders(FolderTree source, string keyword, SortBy criterion)
        {
            List<GridFolderView> list = source.GetAllTrees().Where(i => IsTargetFolder(i, keyword)).Select(tree => new GridFolderView(tree)).ToList();
            return SortFolders(list, keyword, criterion);
        }
        public static bool IsTargetArtist(Music music, string keyword)
        {
            return IsTargetArtist(music.Artist, keyword);
        }
        public static bool IsTargetArtist(string artist, string keyword)
        {
            return artist.ToLowerInvariant().Contains(keyword);
        }

        public static IEnumerable<Playlist> SortArtists(IEnumerable<Playlist> list, string keyword, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return DefaultSort(list.OrderBy(i => i.Name), keyword, i => i.Name);
                case SortBy.Name:
                    return list.OrderBy(a => a.Name).ToList();
                case SortBy.Album:
                    return list.Select(i => new { data = i, count = i.Songs.Select(m => m.Album).Distinct().Count() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.PlayCount:
                    return list.Select(i => new { data = i, count = i.Songs.Select(m => m.PlayCount).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.Duration:
                    return list.Select(i => new { data = i, count = i.Songs.Select(m => m.Duration).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                default:
                    return null;
            }
        }
        public static bool IsTargetAlbum(Music music, string keyword)
        {
            return music.Album.ToLowerInvariant().Contains(keyword) || music.Artist.ToLower().Contains(keyword);
        }

        public static IEnumerable<AlbumView> SortAlbums(IEnumerable<AlbumView> list, string keyword, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return DefaultSort(list.OrderBy(i => i.Name), keyword, i => i.Name);
                case SortBy.Name:
                    return list.OrderBy(i => i.Name);
                case SortBy.PlayCount:
                    return list.Select(i => new { data = i, count = i.Songs.Select(m => m.PlayCount).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.Duration:
                    return list.Select(i => new { data = i, count = i.Songs.Select(m => m.Duration).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                default:
                    return null;
            }
        }
        public static bool IsTargetMusic(Music music, string keyword)
        {
            return music.Name.ToLowerInvariant().Contains(keyword) ||
                   music.Album.ToLowerInvariant().Contains(keyword) ||
                   music.Artist.ToLowerInvariant().Contains(keyword);
        }

        public static IEnumerable<Music> SortSongs(IEnumerable<Music> list, string keyword, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return DefaultSort(list.OrderBy(i => i.Name).ThenBy(i => i.Artist).ThenBy(i => i.Album), keyword, m => m.Name);
                case SortBy.Title:
                    return list.OrderBy(i => i.Name);
                case SortBy.Artist:
                    return list.OrderBy(i => i.Artist);
                case SortBy.Album:
                    return list.OrderBy(i => i.Album);
                case SortBy.PlayCount:
                    return list.OrderBy(i => i.PlayCount);
                case SortBy.Duration:
                    return list.OrderBy(i => i.Duration);
                default:
                    return null;
            }
        }
        public static bool IsTargetPlaylist(Playlist playlist, string keyword)
        {
            return playlist.Name.ToLowerInvariant().Contains(keyword) || playlist.Songs.Any(m => IsTargetMusic(m, keyword));
        }

        public static IEnumerable<AlbumView> SortPlaylists(IEnumerable<AlbumView> src, string keyword, SortBy criterion)
        {
            var nowPlaying = MediaHelper.NowPlaying;
            bool isNowPlayingTarget = IsTargetPlaylist(nowPlaying, keyword),
                 isFavoriteTarget = IsTargetPlaylist(Settings.settings.MyFavorites, keyword);
            List<AlbumView> list;
            if (criterion == SortBy.Default)
            {
                Func<AlbumView, string> defaultSelector = i => i.Name;
                list = DefaultSort(src, keyword, defaultSelector).ToList();
                int insert = list.Count > 0 && IsExact(list[0], keyword, defaultSelector) ? 1 : 0;
                if (isNowPlayingTarget) list.Insert(insert, nowPlaying.ToSearchAlbumView());
                if (isFavoriteTarget) list.Insert(isNowPlayingTarget ? ++insert : insert, Settings.settings.MyFavorites.ToSearchAlbumView());
            }
            else
            {
                list = src.ToList();
                if (isNowPlayingTarget) list.Add(nowPlaying.ToSearchAlbumView());
                if (isFavoriteTarget) list.Add(Settings.settings.MyFavorites.ToSearchAlbumView());
                switch (criterion)
                {
                    case SortBy.Name:
                        return list.OrderBy(a => a.Name);
                    case SortBy.PlayCount:
                        return list.Select(i => new { data = i, count = i.Songs.Select(m => m.PlayCount).Sum() })
                                   .OrderByDescending(p => p.count).Select(i => i.data);
                    case SortBy.Duration:
                        return list.Select(i => new { data = i, count = i.Songs.Select(m => m.Duration).Sum() })
                                   .OrderByDescending(p => p.count).Select(i => i.data);
                }
            }
            return list;
        }
        public static bool IsTargetFolder(FolderTree tree, string keyword)
        {
            return tree.Directory.Contains(keyword) || tree.Files.Any(music => IsTargetMusic(music, keyword));
        }
        public static IEnumerable<GridFolderView> SortFolders(IEnumerable<GridFolderView> src, string keyword, SortBy criterion)
        {
            List<GridFolderView> list;
            if (criterion == SortBy.Default)
            {
                list = DefaultSort(src, keyword, i => i.Name).ToList();
            }
            else
            {
                list = src.ToList();
                switch (criterion)
                {
                    case SortBy.Name:
                        return src.OrderBy(a => a.Name);
                }
            }
            return list;
        }
        public static ObservableCollection<T> DefaultSort<T>(IEnumerable<T> collection, string keyword, Func<T, string> selector)
        {
            ObservableCollection<T> list = new ObservableCollection<T>(collection);
            List<int> startswith = new List<int>();
            for (int i = 0; i < list.Count; i++)
                if (selector(list.ElementAt(i)).StartsWith(keyword))
                    startswith.Add(i);
            for (int i = 0; i < startswith.Count; i++)
                if (startswith[i] != i)
                    list.Move(startswith[i], i);
            int exact = list.FindIndex(m => IsExact(m, keyword, selector));
            if (exact != -1 && exact != 0) list.Move(exact, 0);
            return list;
        }

        public static bool IsExact<T>(T item, string keyword, Func<T, string> selector)
        {
            return selector(item) == keyword;
        }
    }
}
