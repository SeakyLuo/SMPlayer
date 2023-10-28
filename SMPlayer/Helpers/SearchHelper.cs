using SMPlayer.Interfaces;
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
        public const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;

        public static int EvaluateString(string str, string keyword, int offset = 0)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            if (str == keyword) return 100 + offset;
            if (str.Equals(keyword, Comparison)) return 95 + offset;
            if (str.StartsWith(keyword)) return 90 + offset;
            if (str.StartsWith(keyword, Comparison)) return 85 + offset;
            if (str.Contains(keyword)) return 80 + offset;
            if (str.Contains(keyword, Comparison)) return 75 + offset;
            if (keyword.Contains(str, Comparison)) return 70 + offset;
            int editDistance = GetEditDistance(str, keyword);
            int ratio = editDistance * 100 / Math.Max(str.Length, keyword.Length);
            if (ratio <= 60) return 70 - ratio + offset;
            return 0;
        }

        private static int GetEditDistance(string target, string given)
        {
            int n = target.Length;
            int m = given.Length;

            // 有一个字符串为空串
            if (n * m == 0)
            {
                return n + m;
            }
            // init
            List<List<int>> dp = new List<List<int>>();
            for (int i = 0; i < n + 1; i++) 
            {
                List<int> list = new List<int>(m) { i };
                if (i == 0)
                {
                    for (int j = 1; j < m + 1; j++) list.Add(j);
                }
                else
                {
                    for (int j = 1; j < m + 1; j++) list.Add(0);
                }
                dp.Add(list);
            }
            
            for (int i = 1; i < n + 1; i++)
            {
                for (int j = 1; j < m + 1; j++)
                {
                    int left = dp[i - 1][j] + 1;
                    int down = dp[i][j - 1] + 1;
                    int left_down = dp[i - 1][j - 1];
                    if (target[i - 1] != given[j - 1])
                    {
                        left_down += 1;
                    }
                    dp[i][j] = Math.Min(Math.Min(left, down), left_down);
                }
            }
            return dp[n][m];
        }

        public static bool Matches(string str, string keyword)
        {
            return str.Contains(keyword, Comparison);
        }

        public static IEnumerable<MatchResult<Artist>> SearchArtists(IEnumerable<Music> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Where(m => HasTargetArtist(m, keyword))
                             .GroupBy(m => m.Artist)
                             .Select(group => new Artist(group.Key, group))
                             .Select(i => new MatchResult<Artist>(i, keyword));
            return SortArtists(list, criterion);
        }

        public static bool HasTargetArtist(Music music, string keyword)
        {
            return Matches(music.Artist, keyword);
        }

        public static IEnumerable<MatchResult<Album>> SearchAlbums(IEnumerable<Music> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Where(m => HasTargetAlbum(m, keyword))
                             .GroupBy(m => m.Album)
                             .Select(group => new Album(group.Key, group))
                             .Select(i => new MatchResult<Album>(i, keyword));
            return SortAlbums(list, criterion);
        }
        public static bool HasTargetAlbum(Music music, string keyword)
        {
            return Matches(music.Album, keyword) || Matches(music.Artist, keyword);
        }

        public static IEnumerable<MatchResult<Music>> SearchSongs(IEnumerable<Music> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Select(i => new MatchResult<Music>(i, keyword)).Where(i => i.Matches);
            return SortSongs(list, criterion);
        }
        public static IEnumerable<MatchResult<Playlist>> SearchPlaylists(IEnumerable<Playlist> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Select(i => new MatchResult<Playlist>(i, keyword)).Where(i => i.Matches);
            return SortPlaylists(list, criterion);
        }

        public static IEnumerable<MatchResult<Playlist>> SearchPlaylists(IEnumerable<Music> songs, IEnumerable<Playlist> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Select(i => new MatchResult<Playlist>(i, keyword)).Where(i => i.Matches)
                             .Select(i => { i.Entity.Songs = PlaylistService.FindPlaylistItems(i.Entity.Id); return i; })
                             .ToList();
            var playlistIds = list.Select(i => i.Entity.Id).ToHashSet();
            list.AddRange(PlaylistService.FindPlaylistIdsByItems(songs.Select(i => i.Id))
                                         .Where(i => !playlistIds.Contains(i)).AsParallel()
                                         .Select(i => PlaylistService.FindPlaylist(i)).Where(i => i != null)
                                         .Select(i => new MatchResult<Playlist>(i, keyword)));
            var nowPlaying = new MatchResult<Playlist>(MusicPlayer.NowPlaying, keyword);
            if (nowPlaying.Matches) list.Add(nowPlaying);
            var myFavorites = new MatchResult<Playlist>(PlaylistService.MyFavorites, keyword);
            if (myFavorites.Matches) list.Add(myFavorites);
            return SortPlaylists(list, criterion);
        }
        public static IEnumerable<MatchResult<FolderTree>> SearchFolders(IEnumerable<FolderTree> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Select(i => new MatchResult<FolderTree>(i, keyword)).Where(i => i.Matches);
            return SortFolders(list, criterion);
        }
        public static IEnumerable<MatchResult<FolderTree>> SearchFolders(IEnumerable<Music> songs, IEnumerable<FolderTree> source, string keyword, SortBy criterion = SortBy.Default)
        {
            var list = source.Select(i => new MatchResult<FolderTree>(i, keyword))
                             .Where(i => i.Matches)
                             .AsParallel().Select(i => { i.Entity = StorageService.FindFolder(i.Entity.Id); return i; })
                             .ToList();
            var pathSet = list.Select(i => i.Entity.Path).ToHashSet();
            list.AddRange(songs.Select(i => StorageHelper.GetParentPath(i.Path))
                               .Distinct().Where(i => !pathSet.Contains(i))
                               .AsParallel().Select(i => StorageService.FindFolder(i))
                               .Where(i => i != null)
                               .Select(i => new MatchResult<FolderTree>(i, keyword)));
            return SortFolders(list, criterion);
        }

        private static IEnumerable<MatchResult<T>> SortByDefault<T>(IEnumerable<MatchResult<T>> list) where T : ISearchEvaluator
        {
            return list.OrderByDescending(i => i.Score);
        }

        public static IEnumerable<T> SearchAndSortByDefault<T>(IEnumerable<T> list, string keyword) where T : ISearchEvaluator
        {
            return SortByDefault(list.Select(i => new MatchResult<T>(i, keyword)).Where(i => i.Matches)).Select(i => i.Entity);
        }

        public static IEnumerable<MatchResult<Artist>> SortArtists(IEnumerable<MatchResult<Artist>> list, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return SortByDefault(list);
                case SortBy.Name:
                    return list.OrderBy(i => i.Entity.Name);
                case SortBy.Album:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.Album).Distinct().Count() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.PlayCount:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.PlayCount).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.Duration:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.Duration).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                default:
                    return list;
            }
        }

        public static IEnumerable<MatchResult<Album>> SortAlbums(IEnumerable<MatchResult<Album>> list, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return SortByDefault(list);
                case SortBy.Name:
                    return list.OrderBy(i => i.Entity.Name);
                case SortBy.PlayCount:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.PlayCount).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.Duration:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.Duration).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                default:
                    return list;
            }
        }

        public static IEnumerable<MatchResult<Music>> SortSongs(IEnumerable<MatchResult<Music>> list, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return SortByDefault(list);
                case SortBy.Title:
                    return list.OrderBy(i => i.Entity.Name).ThenByDescending(i => i.Entity.PlayCount);
                case SortBy.Artist:
                    return list.OrderBy(i => i.Entity.Artist).ThenByDescending(i => i.Entity.PlayCount);
                case SortBy.Album:
                    return list.OrderBy(i => i.Entity.Album).ThenByDescending(i => i.Entity.PlayCount);
                case SortBy.PlayCount:
                    return list.OrderByDescending(i => i.Entity.PlayCount);
                case SortBy.Duration:
                    return list.OrderBy(i => i.Entity.Duration).ThenByDescending(i => i.Entity.PlayCount);
                case SortBy.DateAdded:
                    return list.OrderBy(i => i.Entity.DateAdded).ThenByDescending(i => i.Entity.PlayCount);
                default:
                    return list;
            }
        }

        public static IEnumerable<MatchResult<Playlist>> SortPlaylists(IEnumerable<MatchResult<Playlist>> list, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return SortByDefault(list);
                case SortBy.PlayCount:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.PlayCount).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                case SortBy.Duration:
                    return list.Select(i => new { data = i, count = i.Entity.Songs.Select(m => m.Duration).Sum() })
                               .OrderByDescending(p => p.count).Select(i => i.data);
                default:
                    return list;
            }
        }

        public static IEnumerable<MatchResult<FolderTree>> SortFolders(IEnumerable<MatchResult<FolderTree>> list, SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return SortByDefault(list);
                case SortBy.Name:
                    return list.OrderBy(a => a.Entity.Name);
                default:
                    return list;
            }
        }

        public static async Task<EvaluateResult> Search(string keyword)
        {
            IEnumerable<Music> allSongs = MusicService.AllSongs;
            Music music = await Task.Run(() => SearchSongs(allSongs, keyword)?.FirstOrDefault()?.Entity);
            Artist artist = await Task.Run(() => SearchArtists(allSongs, keyword)?.FirstOrDefault()?.Entity);
            Album album = await Task.Run(() => SearchAlbums(allSongs, keyword)?.FirstOrDefault()?.Entity);
            Playlist playlist = await Task.Run(() => SearchPlaylists(PlaylistService.AllPlaylists, keyword)?.FirstOrDefault()?.Entity);
            FolderTree folder = await Task.Run(() => SearchFolders(StorageService.AllFolders, keyword)?.FirstOrDefault()?.Entity);
            return MergeSearchResult(keyword, music, artist, album, playlist, folder);
        }

        public static async Task<EvaluateResult> SearchByArtist(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, null);
        }

        public static async Task<EvaluateResult> SearchByArtistMusic(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, EntityType.Song);
        }

        public static async Task<EvaluateResult> SearchByArtistAlbum(string artist, string keyword)
        {
            return await SearchByArtist(artist, keyword, EntityType.Album);
        }

        public static async Task<EvaluateResult> SearchAlbumMusic(string albumName, string keyword)
        {
            Album album = await Task.Run(() => SearchAlbums(MusicService.AllSongs, albumName)?.FirstOrDefault()?.Entity);
            return SearchMusicInCollection(album?.Songs, keyword);
        }

        public static async Task<EvaluateResult> SearchPlaylistMusic(string playlistName, string keyword)
        {
            Playlist playlist = await Task.Run(() => SearchPlaylists(PlaylistService.AllPlaylists, playlistName)?.FirstOrDefault()?.Entity);
            return SearchMusicInCollection(playlist?.Songs, keyword);
        }

        public static async Task<EvaluateResult> SearchFolderMusic(string folderName, string keyword)
        {
            FolderTree folder = await Task.Run(() => SearchFolders(StorageService.AllFolders, folderName)?.FirstOrDefault()?.Entity);
            if (folder == null) return null;
            return SearchMusicInCollection(StorageService.FindSubSongs(folder), keyword);
        }

        private static EvaluateResult SearchMusicInCollection(IEnumerable<Music> list, string keyword)
        {
            if (list == null) return null;
            var music = SearchSongs(list, keyword)?.FirstOrDefault()?.Entity;
            return new EvaluateResult(EntityType.Song, music, keyword);
        }

        private static async Task<EvaluateResult> SearchByArtist(string artist, string keyword, EntityType? searchType)
        {
            IEnumerable<Music> artistMusic = MusicService.AllSongs.Where(i => HasTargetArtist(i, artist));
            Music music = null;
            Album album = null;
            if (!string.IsNullOrEmpty(keyword))
            {
                if (searchType == null || searchType == EntityType.Song)
                {
                    music = await Task.Run(() => SearchSongs(artistMusic, keyword)?.FirstOrDefault()?.Entity);
                }
                if (searchType == null || searchType == EntityType.Album)
                {
                    album = await Task.Run(() => SearchAlbums(artistMusic, keyword)?.FirstOrDefault()?.Entity);
                }
            }
            return MergeSearchResult(keyword, music, null, album, null, null);
        }

        private static EvaluateResult MergeSearchResult(string keyword, Music music, Artist artist, Album album, Playlist playlist, FolderTree folder)
        {
            List<EvaluateResult> results = new List<EvaluateResult>();
            if (music != null)
            {
                results.Add(new EvaluateResult(EntityType.Song, music, keyword));
            }
            if (artist != null)
            {
                results.Add(new EvaluateResult(EntityType.Artist, artist, keyword));
            }
            if (album != null)
            {
                results.Add(new EvaluateResult(EntityType.Album, album, keyword));
            }
            if (folder != null)
            {
                results.Add(new EvaluateResult(EntityType.Folder, folder, keyword));
            }
            if (playlist != null)
            {
                results.Add(new EvaluateResult(EntityType.Playlist, playlist, keyword));
            }
            return results.OrderByDescending(i => i.Score).FirstOrDefault();
        }
    }

    public class EvaluateResult
    {
        public EntityType SearchType { get; set; }
        public object Entity { get; set; }
        public double Score { get; set; }

        public EvaluateResult(EntityType type, ISearchEvaluator entity, string keyword)
        {
            SearchType = type;
            Entity = entity;
            Score = entity.Evaluate(keyword);
        }
    }

    public class MatchResult<T> where T : ISearchEvaluator
    {
        public T Entity { get; set; }
        public double Score { get; set; }
        public bool Matches => Score > 0;

        public MatchResult() { }

        public MatchResult(T entity, string keyword)
        {
            Entity = entity;
            Score = entity.Match(keyword);
        }
    }
}
