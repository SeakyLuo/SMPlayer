using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
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
                                 return new AlbumView(music.Album, group, false);
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
        public static IEnumerable<AlbumView> SearchPlaylists(IEnumerable<Music> songs, IEnumerable<Playlist> source, string keyword, SortBy criterion)
        {
            var list = source.Where(i => IsTargetPlaylist(i, keyword))
                             .Select(i => { i.Songs.SetTo(PlaylistService.FindPlaylistItems(i.Id)); return i; })
                             .ToList();
            var playlistIds = list.Select(i => i.Id).ToHashSet();
            list.AddRange(PlaylistService.FindPlaylistIdsByItems(songs.Select(i => i.Id))
                                         .Where(i => playlistIds.Contains(i)).AsParallel()
                                         .Select(i => PlaylistService.FindPlaylist(i)));
            return SortPlaylists(list.Select(i => i.ToSearchAlbumView()), keyword, criterion);
        }
        public static IEnumerable<GridViewFolder> SearchFolders(IEnumerable<FolderTree> source, string keyword, SortBy criterion)
        {
            var list = source.Where(i => IsTargetFolder(i, keyword)).Select(tree => new GridViewFolder(tree));
            return SortFolders(list, keyword, criterion);
        }
        public static IEnumerable<GridViewFolder> SearchFolders(IEnumerable<Music> songs, IEnumerable<FolderTree> source, string keyword, SortBy criterion)
        {
            var list = source.Where(i => IsTargetFolder(i, keyword)).Select(tree => new GridViewFolder(tree)).ToList();
            var pathSet = list.Select(i => i.Path).ToHashSet();
            list.AddRange(songs.Select(i => StorageHelper.GetParentPath(i.Path))
                               .Distinct().Where(i => !pathSet.Contains(i))
                               .AsParallel().Select(i => StorageService.FindFolderInfo(i))
                               .Where(i => i != null).Select(tree => new GridViewFolder(tree)).ToList());
            return SortFolders(list.AsParallel().Select(i =>
            {
                i.Source = StorageService.FindFullFolder(i.Source.Id);
                return i;
            }), keyword, criterion);
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

        public static bool IsTargetAlbum(AlbumView album, string keyword)
        {
            return album.Name.Contains(keyword, Comparison) || album.Artist.Contains(keyword, Comparison);
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
            return playlist.Name.Contains(keyword, Comparison) || 
                   (!playlist.IsEmpty && playlist.Songs.Any(m => IsTargetMusic(m, keyword)));
        }

        public static IEnumerable<AlbumView> SortPlaylists(IEnumerable<AlbumView> src, string keyword, SortBy criterion)
        {
            var nowPlaying = MusicPlayer.NowPlaying;
            Playlist myFavorites = PlaylistService.MyFavorites;
            bool isNowPlayingTarget = IsTargetPlaylist(nowPlaying, keyword),
                 isFavoriteTarget = IsTargetPlaylist(myFavorites, keyword);
            List<AlbumView> list;
            if (criterion == SortBy.Default)
            {
                string defaultSelector(AlbumView i) => i.Name;
                list = DefaultSort(src, keyword, defaultSelector).ToList();
                int insert = list.Count > 0 && IsExact(list[0], keyword, defaultSelector) ? 1 : 0;
                if (isNowPlayingTarget) list.Insert(insert, nowPlaying.ToSearchAlbumView(EntityType.NowPlaying));
                if (isFavoriteTarget) list.Insert(isNowPlayingTarget ? ++insert : insert, myFavorites.ToSearchAlbumView(EntityType.MyFavorites));
            }
            else
            {
                list = src.ToList();
                if (isNowPlayingTarget) list.Add(nowPlaying.ToSearchAlbumView(EntityType.NowPlaying));
                if (isFavoriteTarget) list.Add(myFavorites.ToSearchAlbumView(EntityType.MyFavorites));
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
            return tree.Name.Contains(keyword, Comparison);
        }
        public static IEnumerable<GridViewFolder> SortFolders(IEnumerable<GridViewFolder> src, string keyword, SortBy criterion)
        {
            List<GridViewFolder> list;
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
            IEnumerable<Music> allSongs = Settings.AllSongs;
            Music music = (await Task.Run(() => SearchSongs(allSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            Playlist artist = (await Task.Run(() => SearchArtists(allSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            AlbumView album = (await Task.Run(() => SearchAlbums(allSongs, keyword, SortBy.Default)))?.FirstOrDefault();
            AlbumView playlist = (await Task.Run(() => SearchPlaylists(PlaylistService.AllPlaylists, keyword, SortBy.Default)))?.FirstOrDefault();
            GridViewFolder folder = (await Task.Run(() => SearchFolders(StorageService.AllFolders, keyword, SortBy.Default)))?.FirstOrDefault();
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

        public static async Task<SearchResult> SearchAlbumMusic(string albumName, string keyword)
        {
            AlbumView album = (await Task.Run(() => SearchAlbums(Settings.AllSongs, albumName, SortBy.Default)))?.FirstOrDefault();
            return SearchMusicInCollection(album?.Songs, keyword);
        }

        public static async Task<SearchResult> SearchPlaylistMusic(string playlistName, string keyword)
        {
            AlbumView playlist = (await Task.Run(() => SearchPlaylists(PlaylistService.AllPlaylists, playlistName, SortBy.Default)))?.FirstOrDefault();
            return SearchMusicInCollection(playlist?.Songs, keyword);
        }

        public static async Task<SearchResult> SearchFolderMusic(string folderName, string keyword)
        {
            GridViewFolder folder = (await Task.Run(() => SearchFolders(StorageService.AllFolders, folderName, SortBy.Default)))?.FirstOrDefault();
            if (folder == null) return null;
            folder.Source = StorageService.FindFolder(folder.Id);
            return SearchMusicInCollection(folder.Songs, keyword);
        }

        private static SearchResult SearchMusicInCollection(IEnumerable<Music> list, string keyword)
        {
            if (list == null) return null;
            var music = SearchSongs(list, keyword, SortBy.Default).FirstOrDefault();
            return new SearchResult(SearchType.Songs, music, EvaluateMusic(music, keyword));
        }

        private static async Task<SearchResult> SearchByArtist(string artist, string keyword, SearchType? searchType)
        {
            IEnumerable<Music> artistMusic = Settings.AllSongs.Where(i => IsTargetArtist(i, artist));
            Music music = null;
            AlbumView album = null;
            if (!string.IsNullOrEmpty(keyword))
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

        private static SearchResult MergeSearchResult(string keyword, Music music, Playlist artist, AlbumView album, AlbumView playlist, GridViewFolder folder)
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
        public static int EvaluateFolder(GridViewFolder item, string keyword)
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
