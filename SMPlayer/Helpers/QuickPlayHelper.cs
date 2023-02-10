using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    class QuickPlayHelper
    {
        private const int randomItems = 5;
        private static readonly Random random = new Random();

        public static List<Music> GetPlaylist(int randomLimit)
        {
            //HashSet<Music> songs = Helper.Timer(() => Settings.AllSongs.RandItems(randomLimit * 2).ToHashSet(), "AllSongs.RandItems");
            //PreferenceSettings preference = PreferenceSettings.settings;
            //Helper.Timer(() => HandlePreferredSongs(songs, preference), "HandlePreferredSongs");
            //Helper.Timer(() => HandlePreferredArtists(songs, preference), "HandlePreferredArtists");
            //Helper.Timer(() => HandlePreferredAlbums(songs, preference), "HandlePreferredAlbums");
            //Helper.Timer(() => HandlePreferredPlaylists(songs, preference), "HandlePreferredPlaylists");
            //Helper.Timer(() => HandlePreferredFolders(songs, preference), "HandlePreferredFolders");
            //Helper.Timer(() => HandleRecentAdded(songs), "HandledRecentAdded");
            //Helper.Timer(() => HandleMyFavorites(songs), "HandledMyFavorites");
            //Helper.Timer(() => HandleMostPlayed(songs, randomLimit), "HandledMostPlayed");
            //Helper.Timer(() => HandleLeastPlayed(songs, randomLimit), "HandledLeastPlayed");
            //Helper.Timer(() => HandleDislikedItems(songs), "HandleDislikedItems");
            //Helper.Timer(() => HandleDoNotAppearItems(songs), "HandleDoNotAppearItems");
            //return Helper.Timer(() => songs.RandItems(randomLimit).ToList(), "Final RandItems");
            HashSet<Music> songs = MusicService.AllSongs.RandItems(randomLimit * 2).ToHashSet();
            PreferenceSettings preference = PreferenceSettings.settings;
            HandlePreferredSongs(songs, preference);
            HandlePreferredArtists(songs, preference);
            HandlePreferredAlbums(songs, preference);
            HandlePreferredPlaylists(songs, preference);
            HandlePreferredFolders(songs, preference);
            HandleRecentAdded(songs);
            HandleMyFavorites(songs);
            HandleMostPlayed(songs, randomLimit);
            HandleLeastPlayed(songs, randomLimit);
            HandleDislikedItems(songs);
            HandleDoNotAppearItems(songs);
            return songs.RandItems(randomLimit).ToList();
        }

        private static void HandlePreferredSongs(HashSet<Music> songs, PreferenceSettings preference)
        {
            if (!preference.Songs) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(PreferenceSettings.EnabledPreferredSongs);
            songs.AddRange(items.Select(i => MusicService.FindMusic(i.LongId)).Where(i => i != null));
        }

        private static void HandlePreferredArtists(HashSet<Music> songs, PreferenceSettings preference)
        {
            if (!preference.Artists) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(PreferenceSettings.EnabledPreferredArtists);
            songs.AddRange(items.SelectMany(i => MusicService.AllSongs.Where(m => i.Id == m.Artist)
                                                             .RandItems(GetRandomPreferredItems(i.Level))
                                .RandItems(randomItems)));
        }

        private static void HandlePreferredAlbums(HashSet<Music> songs, PreferenceSettings preference)
        {
            if (!preference.Albums) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(PreferenceSettings.EnabledPreferredAlbums);
            songs.AddRange(items.SelectMany(i => MusicService.AllSongs.Where(m => i.Id == m.Album)
                                                             .RandItems(GetRandomPreferredItems(i.Level))
                                .RandItems(randomItems)));
        }

        private static void HandlePreferredPlaylists(HashSet<Music> songs, PreferenceSettings preference)
        {
            if (!preference.Playlists) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(PreferenceSettings.EnabledPreferredPlaylists);
            songs.AddRange(items.SelectMany(i => PlaylistService.FindPlaylistItems(i.LongId)
                                                                .RandItems(GetRandomPreferredItems(i.Level)).ToList())
                                .RandItems(randomItems).ToList());
        }

        private static void HandlePreferredFolders(HashSet<Music> songs, PreferenceSettings preference)
        {
            if (!preference.Folders) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(PreferenceSettings.EnabledPreferredFolders);
            songs.AddRange(items.Select(i => new { Folder = StorageService.FindFolder(i.LongId), i.Level })
                                .Where(i => i.Folder != null && i.Folder.IsNotEmpty)
                                .SelectMany(i => i.Folder.Songs.RandItems(GetRandomPreferredItems(i.Level)))
                                .RandItems(randomItems).ToList());
        }

        private static IEnumerable<PreferenceItem> GetPreferenceItems(List<PreferenceItem> items)
        {
            if (items.Count == 0) return new List<PreferenceItem>();
            if (items.Count == 1) return items;
            return items.SelectMany(item =>
            {
                if (item.Level <= 0) return new List<PreferenceItem>();
                int max = GetRandomPreferredItems(item.Level);
                List<PreferenceItem> ret = new List<PreferenceItem>();
                for (int i = 0; i < max; i++)
                {
                    if (Toss())
                    {
                        ret.Add(item);
                    }
                }
                return ret.RandItems(randomItems).Distinct();
            });
        }

        private static void HandleRecentAdded(HashSet<Music> songs)
        {
            int count = GetPreferenceItems(PreferenceSettings.FindRecentAdded);
            if (count == 0) return;
            songs.AddRange(RecentPage.RecentAdded.TimeLine.RandItems(count).Select(i => i.FromVO()));
        }

        private static void HandleMyFavorites(HashSet<Music> songs)
        {
            int count = GetPreferenceItems(PreferenceSettings.FindMyFavorites);
            if (count == 0) return;
            songs.AddRange(PlaylistService.MyFavoriteSongs.RandItems(count));
        }

        private static void HandleMostPlayed(HashSet<Music> songs, int randomLimit)
        {
            int count = GetPreferenceItems(PreferenceSettings.FindMostPlayed);
            if (count == 0) return;
            songs.AddRange(MusicService.GetMostPlayed(randomLimit));
        }

        private static void HandleLeastPlayed(HashSet<Music> songs, int randomLimit)
        {
            int count = GetPreferenceItems(PreferenceSettings.FindLeastPlayed);
            if (count == 0) return;
            songs.AddRange(MusicService.GetLeastPlayed(randomLimit));
        }

        private static int GetPreferenceItems(PreferenceItem item)
        {
            return item.IsEnabled ? GetRandomPreferredItems(item.Level) : 0;
        }

        private static int GetRandomPreferredItems(PreferLevel level)
        {
            int min = (int)level + 1;
            return random.Next(min, min * 3);
        }

        private static void HandleDislikedItems(HashSet<Music> songs)
        {
            HandleRemoveMusic(songs, PreferLevel.Dislike);
        }

        private static void HandleDoNotAppearItems(HashSet<Music> songs)
        {
            HandleRemoveMusic(songs, PreferLevel.DoNotAppear);
        }

        private static void HandleRemoveMusic(HashSet<Music> songs, PreferLevel level)
        {
            int probability = level == PreferLevel.DoNotAppear ? 1 : 2;
            foreach (var group in PreferenceSettings.FindEnabledByLevel(level).GroupBy(i => i.Type))
            {
                switch (group.Key)
                {
                    case EntityType.Song:
                        HashSet<long> ids = group.Select(i => long.Parse(i.Id)).ToHashSet();
                        songs.RemoveWhere(i => Toss(probability) && ids.Contains(i.Id));
                        break;
                    case EntityType.Artist:
                        HashSet<string> artists = group.Select(i => i.Id).ToHashSet();
                        songs.RemoveWhere(i => Toss(probability) && artists.Contains(i.Artist));
                        break;
                    case EntityType.Album:
                        HashSet<string> albums = group.Select(i => i.Id).ToHashSet();
                        songs.RemoveWhere(i => Toss(probability) && albums.Contains(i.Album));
                        break;
                    case EntityType.Playlist:
                        HashSet<long> playlistItems = group.Select(i => long.Parse(i.Id))
                                                           .SelectMany(id => PlaylistService.FindPlaylistItems(id))
                                                           .Select(i => i.Id).ToHashSet();
                        songs.RemoveWhere(i => Toss(probability) && playlistItems.Contains(i.Id));
                        break;
                    case EntityType.Folder:
                        HashSet<string> folders = group.AsParallel()
                                                       .Select(i => StorageService.FindFolderInfo(long.Parse(i.Id)))
                                                       .Where(i => i != null)
                                                       .Select(i => i.Path).ToHashSet();
                        songs.RemoveWhere(i => Toss(probability) && folders.Any(f => i.Path.StartsWith(f)));
                        break;
                }
            }
        }

        private static bool Toss(int probability = 2)
        {
            if (probability <= 1) return true;
            return random.Next(probability) == 0;
        }
    }
}
