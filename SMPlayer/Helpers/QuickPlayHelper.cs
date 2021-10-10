using SMPlayer.Models;
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
            HashSet<Music> songs = MusicLibraryPage.AllSongs.RandItems(randomLimit * 2).ToHashSet();
            PreferenceSetting preference = Settings.settings.Preference;
            HandlePreferredSongs(songs, preference);
            HandlePreferredArtists(songs, preference);
            HandlePreferredAlbums(songs, preference);
            HandlePreferredPlaylists(songs, preference);
            HandlePreferredFolders(songs, preference);
            HandledRecentAdded(songs, preference);
            HandledMyFavorites(songs, preference);
            HandledMostPlayed(songs, preference, randomLimit);
            HandledLeastPlayed(songs, preference, randomLimit);
            return songs.RandItems(randomLimit).ToList();
        }

        private static void HandlePreferredSongs(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Songs) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(preference.EnabledPreferredSongs);
            songs.AddRange(items.Select(i => new Music { Path = i.Id }));
        }

        private static void HandlePreferredArtists(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Artists) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(preference.EnabledPreferredArtists);
            songs.AddRange(items.SelectMany(i => MusicLibraryPage.AllSongs.Where(m => i.Id == m.Artist)
                                                                          .RandItems(GetRandomPreferredItems(i.Level))
                                .RandItems(randomItems)));
        }

        private static void HandlePreferredAlbums(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Albums) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(preference.EnabledPreferredArtists);
            songs.AddRange(items.SelectMany(i => MusicLibraryPage.AllSongs.Where(m => i.Id == m.Album)
                                                                .RandItems(GetRandomPreferredItems(i.Level))
                                .RandItems(randomItems)));
        }

        private static void HandlePreferredPlaylists(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Playlists) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(preference.EnabledPreferredPlaylists);
            songs.AddRange(items.SelectMany(i => Settings.settings.Playlists.Where(p => i.Name == p.Name)
                                                                  .SelectMany(p => p.Songs.RandItems(GetRandomPreferredItems(i.Level)))
                                .RandItems(randomItems)));
        }

        private static void HandlePreferredFolders(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Folders) return;
            IEnumerable<PreferenceItem> items = GetPreferenceItems(preference.EnabledPreferredFolders);
            songs.AddRange(items.Select(i => new { Folder = Settings.settings.Tree.FindTree(i.Id), i.Level })
                                .Where(i => i.Folder != null)
                                .SelectMany(i => i.Folder.Files.RandItems(GetRandomPreferredItems(i.Level))).RandItems(randomItems));
        }

        private static IEnumerable<PreferenceItem> GetPreferenceItems(List<PreferenceItem> items)
        {
            if (items.Count == 0) return new List<PreferenceItem>();
            if (items.Count == 1) return items;
            return items.SelectMany(item =>
            {
                int max = GetRandomPreferredItems(item.Level);
                List<PreferenceItem> ret = new List<PreferenceItem>();
                for (int i = 0; i < max; i++)
                {
                    if (random.Next(2) == 1)
                    {
                        ret.Add(item);
                    }
                }
                return ret.RandItems(randomItems).Distinct();
            });
        }

        private static void HandledRecentAdded(HashSet<Music> songs, PreferenceSetting preference)
        {
            int count = GetPreferenceItems(preference.RecentAdded);
            if (count == 0) return;
            songs.AddRange(RecentPage.RecentAdded.TimeLine.RandItems(count));
        }

        private static void HandledMyFavorites(HashSet<Music> songs, PreferenceSetting preference)
        {
            int count = GetPreferenceItems(preference.MyFavorites);
            if (count == 0) return;
            songs.AddRange(Settings.settings.MyFavorites.Songs.RandItems(count));
        }

        private static void HandledMostPlayed(HashSet<Music> songs, PreferenceSetting preference, int randomLimit)
        {
            int count = GetPreferenceItems(preference.MostPlayed);
            if (count == 0) return;
            songs.AddRange(MusicLibraryPage.GetMostPlayed(randomLimit));
        }

        private static void HandledLeastPlayed(HashSet<Music> songs, PreferenceSetting preference, int randomLimit)
        {
            int count = GetPreferenceItems(preference.LeastPlayed);
            if (count == 0) return;
            songs.AddRange(MusicLibraryPage.GetLeastPlayed(randomLimit));
        }

        private static int GetPreferenceItems(PreferenceItem item)
        {
            return item.IsEnabled ? GetRandomPreferredItems(item.Level) : 0;
        }

        private static int GetRandomPreferredItems(PreferLevel level)
        {
            return random.Next((int)level + 1);
        }
    }
}
