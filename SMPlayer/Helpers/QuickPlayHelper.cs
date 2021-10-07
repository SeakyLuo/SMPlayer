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
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            List<PreferenceItem> preferred = preference.EnabledPreferredSongs;
            if (preferred.Count == 0) return;
            songs.AddRange(preferred.RandItems(randomCount).Select(i => new Music { Path = i.Id }));
        }

        private static void HandlePreferredArtists(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Artists) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            List<PreferenceItem> preferred = preference.EnabledPreferredArtists;
            if (preferred.Count == 0) return;
            HashSet<string> preferredArtists = preferred.RandItems(randomCount).Select(i => i.Id).ToHashSet();
            songs.AddRange(MusicLibraryPage.AllSongs.Where(m => preferredArtists.Contains(m.Artist)).RandItems(randomCount));
        }

        private static void HandlePreferredAlbums(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Albums) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            List<PreferenceItem> preferred = preference.EnabledPreferredAlbums;
            if (preferred.Count == 0) return;
            HashSet<string> preferredAlbums = preferred.RandItems(randomCount).Select(i => i.Id).ToHashSet();
            songs.AddRange(MusicLibraryPage.AllSongs.Where(m => preferredAlbums.Contains(TileHelper.BuildAlbumNavigationFlag(m.Album, m.Artist))).RandItems(randomCount));
        }

        private static void HandlePreferredPlaylists(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Playlists) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            List<PreferenceItem> preferred = preference.EnabledPreferredPlaylists;
            if (preferred.Count == 0) return;
            HashSet<string> preferredPlaylists = preferred.RandItems(randomCount).Select(i => i.Id).ToHashSet();
            songs.AddRange(Settings.settings.Playlists.Where(i => preferredPlaylists.Contains(i.Name)).SelectMany(i => i.Songs.RandItems(randomCount)).RandItems(randomCount));
        }

        private static void HandlePreferredFolders(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.Folders) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            List<PreferenceItem> preferred = preference.EnabledPreferredFolders;
            if (preferred.Count == 0) return;
            HashSet<string> preferredFolders = preferred.RandItems(randomCount).Select(i => i.Id).ToHashSet();
            songs.AddRange(preferredFolders.Select(i => Settings.settings.Tree.FindTree(i)).Where(i => i != null).SelectMany(i => i.Files.RandItems(randomCount)).RandItems(randomCount));
        }

        private static void HandledRecentAdded(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.RecentAdded.IsEnabled) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            songs.AddRange(RecentPage.RecentAdded.TimeLine.RandItems(randomCount));
        }

        private static void HandledMyFavorites(HashSet<Music> songs, PreferenceSetting preference)
        {
            if (!preference.MyFavorites.IsEnabled) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            songs.AddRange(Settings.settings.MyFavorites.Songs.RandItems(randomCount));
        }

        private static void HandledMostPlayed(HashSet<Music> songs, PreferenceSetting preference, int randomLimit)
        {
            if (!preference.MostPlayed.IsEnabled) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            songs.AddRange(MusicLibraryPage.GetMostPlayed(randomLimit));
        }

        private static void HandledLeastPlayed(HashSet<Music> songs, PreferenceSetting preference, int randomLimit)
        {
            if (!preference.LeastPlayed.IsEnabled) return;
            int randomCount = GetRandomPreferredItems();
            if (randomCount == 0) return;
            songs.AddRange(MusicLibraryPage.GetLeastPlayed(randomLimit));
        }

        private static int GetRandomPreferredItems()
        {
            return random.Next(0, 2);
        }
    }
}
