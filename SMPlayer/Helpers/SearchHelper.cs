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
        private const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;

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
            var list = source.Where(i => IsTargetPlaylist(i, keyword)).Select(i => i.ToSearchAlbumView());
            return SortPlaylists(list, keyword, criterion);
        }
        public static IEnumerable<GridFolderView> SearchFolders(FolderTree source, string keyword, SortBy criterion)
        {
            var list = source.GetAllTrees().Where(i => IsTargetFolder(i, keyword)).Select(tree => new GridFolderView(tree));
            return SortFolders(list, keyword, criterion);
        }
        public static bool IsTargetArtist(Music music, string keyword)
        {
            return IsTargetArtist(music.Artist, keyword);
        }
        public static bool IsTargetArtist(string artist, string keyword)
        {
            return artist.Contains(keyword, Comparison);
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
            return music.Album.Contains(keyword, Comparison) || music.Artist.Contains(keyword, Comparison);
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
            return music.Name.Contains(keyword, Comparison) ||
                   music.Album.Contains(keyword, Comparison) ||
                   music.Artist.Contains(keyword, Comparison);
        }

        public static IEnumerable<Music> SortSongs(IEnumerable<Music> list, string keyword, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return DefaultSort(list.OrderBy(i => i.Name)
                                           .ThenByDescending(i => i.PlayCount)
                                           .ThenBy(i => i.Artist)
                                           .ThenBy(i => i.Album), keyword, m => m.Name);
                case SortBy.Title:
                    return list.OrderBy(i => i.Name).ThenByDescending(i => i.PlayCount);
                case SortBy.Artist:
                    return list.OrderBy(i => i.Artist).ThenByDescending(i => i.PlayCount);
                case SortBy.Album:
                    return list.OrderBy(i => i.Album).ThenByDescending(i => i.PlayCount);
                case SortBy.PlayCount:
                    return list.OrderByDescending(i => i.PlayCount);
                case SortBy.Duration:
                    return list.OrderBy(i => i.Duration).ThenByDescending(i => i.PlayCount);
                case SortBy.DateAdded:
                    return list.OrderBy(i => i.DateAdded).ThenByDescending(i => i.PlayCount);
                default:
                    return null;
            }
        }
        public static bool IsTargetPlaylist(Playlist playlist, string keyword)
        {
            return playlist.Name.Contains(keyword, Comparison) || playlist.Songs.Any(m => IsTargetMusic(m, keyword));
        }

        public static IEnumerable<AlbumView> SortPlaylists(IEnumerable<AlbumView> src, string keyword, SortBy criterion)
        {
            var nowPlaying = MediaHelper.NowPlaying;
            bool isNowPlayingTarget = IsTargetPlaylist(nowPlaying, keyword),
                 isFavoriteTarget = IsTargetPlaylist(Settings.settings.MyFavorites, keyword);
            List<AlbumView> list;
            if (criterion == SortBy.Default)
            {
                string defaultSelector(AlbumView i) => i.Name;
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
            return tree.Directory.Contains(keyword, Comparison) || tree.Files.Any(music => IsTargetMusic(music, keyword));
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
            if (exact > 0) list.Move(exact, 0);
            return list;
        }

        public static bool IsExact<T>(T item, string keyword, Func<T, string> selector)
        {
            return keyword.Equals(selector(item), Comparison);
        }

        public static async Task<SearchResult> Search(string keyword)
        {
            Music music = (await Task.Run(() => SearchSongs(MusicLibraryPage.AllSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            Playlist artist = (await Task.Run(() => SearchArtists(MusicLibraryPage.AllSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            AlbumView album = (await Task.Run(() => SearchAlbums(MusicLibraryPage.AllSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            AlbumView playlist = (await Task.Run(() => SearchPlaylists(Settings.settings.Playlists, keyword, SortBy.Default)))?.FirstOrDefault();
            GridFolderView folder = (await Task.Run(() => SearchFolders(Settings.settings.Tree, keyword, SortBy.Default)))?.FirstOrDefault();
            return MergeSearchResult(keyword, music, artist, album, playlist, folder);
        }

        public static async Task<SearchResult> SearchByArtist(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, null);
        }

        public static async Task<SearchResult> SearchByArtistMusic(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, SearchType.Songs);
        }

        public static async Task<SearchResult> SearchByArtistAlbum(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, SearchType.Albums);
        }

        private static async Task<SearchResult> SearchByArtist(string artist, string keyword, SearchType? searchType)
        {
            IEnumerable<Music> artistMusic = MusicLibraryPage.AllSongs.Where(i => IsTargetArtist(i, artist));
            Music music = null;
            AlbumView album = null;
            if (string.IsNullOrEmpty(keyword))
            {
                if (searchType == null || searchType == SearchType.Songs)
                {
                    music = (await Task.Run(() => SearchSongs(artistMusic, keyword, SortBy.Default)))?.FirstOrDefault();
                }
                if (searchType == null || searchType == SearchType.Albums)
                {
                    album = (await Task.Run(() => SearchAlbums(artistMusic, keyword, SortBy.Default)))?.FirstOrDefault();
                }
            }
            return MergeSearchResult(keyword, music, null, album, null, null);
        }

        private static SearchResult MergeSearchResult(string keyword, Music music, Playlist artist, AlbumView album, AlbumView playlist, GridFolderView folder)
        {
            List<SearchResult> results = new List<SearchResult>();
            if (music != null)
            {
                results.Add(new SearchResult(SearchType.Songs, music, EvaluateMusic(music, keyword)));
            }
            if (artist != null)
            {
                results.Add(new SearchResult(SearchType.Artists, artist, EvaluateArtist(artist, keyword)));
            }
            if (album != null)
            {
                results.Add(new SearchResult(SearchType.Albums, album, EvaluateAlbum(album, keyword)));
            }
            if (folder != null)
            {
                results.Add(new SearchResult(SearchType.Folders, folder, EvaluateFolder(folder, keyword)));
            }
            if (playlist != null)
            {
                results.Add(new SearchResult(SearchType.Playlists, playlist, EvaluatePlaylist(playlist, keyword)));
            }
            if (results.Count == 0)
            {
                return null;
            }
            results.Sort((r1, r2) => r2.Score - r1.Score);
            return results[0];
        }

        public static int EvaluateArtist(Playlist item, string keyword)
        {
            if (item == null) return 0;
            if (item.Name == keyword) return 98;
            if (item.Name.Equals(keyword, Comparison)) return 93;
            if (item.Name.Contains(keyword)) return 88;
            if (item.Name.Contains(keyword, Comparison)) return 83;
            return 0;
        }
        public static int EvaluateMusic(Music item, string keyword)
        {
            if (item == null) return 0;
            if (item.Name == keyword) return 95;
            if (item.Name.Equals(keyword, Comparison)) return 90;
            if (item.Name.Contains(keyword)) return 85;
            if (item.Name.Contains(keyword, Comparison)) return 80;
            return 0;
        }
        public static int EvaluateAlbum(AlbumView item, string keyword)
        {
            if (item == null) return 0;
            if (item.Name == keyword) return 99;
            if (item.Name.Equals(keyword, Comparison)) return 94;
            if (item.Name.Contains(keyword)) return 89;
            if (item.Name.Contains(keyword, Comparison)) return 84;
            return 0;
        }
        public static int EvaluateFolder(GridFolderView item, string keyword)
        {
            if (item == null) return 0;
            if (item.Name == keyword) return 97;
            if (item.Name.Equals(keyword, Comparison)) return 92;
            if (item.Name.Contains(keyword)) return 87;
            if (item.Name.Contains(keyword, Comparison)) return 82;
            return 0;
        }
        public static int EvaluatePlaylist(AlbumView item, string keyword)
        {
            if (item == null) return 0;
            if (item.Name == keyword) return 96;
            if (item.Name.Equals(keyword, Comparison)) return 91;
            if (item.Name.Contains(keyword)) return 86;
            if (item.Name.Contains(keyword, Comparison)) return 81;
            return 0;
        }
    }

    public class SearchResult
    {
        public SearchType SearchType { get; set; }
        public object Result { get; set; }
        public int Score { get; set; }

        public SearchResult(SearchType type, object result, int score)
        {
            SearchType = type;
            Result = result;
            Score = score;
        }
    }
}
