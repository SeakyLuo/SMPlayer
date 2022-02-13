using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class PlaylistService
    {
        public static void AddPlaylistEventListener(IPlaylistEventListener listener) { PlaylistEventListeners.Add(listener); }
        private static readonly List<IPlaylistEventListener> PlaylistEventListeners = new List<IPlaylistEventListener>();
        public static Playlist MyFavorites => FindPlaylist(Settings.settings.MyFavoritesId);
        public static List<Music> MyFavoriteSongs => FindPlaylistItems(Settings.settings.MyFavoritesId);
        public static List<Playlist> AllPlaylists => SQLHelper.Run(c => c.SelectAllPlaylists());
        public static Playlist FindPlaylist(string name)
        {
            return SQLHelper.Run(c => c.SelectPlaylistByName(name));
        }
        public static Playlist FindPlaylist(long id)
        {
            return SQLHelper.Run(c => c.SelectPlaylistById(id));
        }
        public static List<Music> FindPlaylistItems(long id)
        {
            return SQLHelper.Run(c => c.SelectPlaylistItems(id));
        }
        public static List<long> FindPlaylistIdsByItems(IEnumerable<long> itemIds)
        {
            return SQLHelper.Run(c => c.Query<PlaylistItemDAO>("select * from PlaylistItem where ItemId in (?) and State = ?", string.Join(",", itemIds), ActiveState.Active))
                            .Select(i => i.PlaylistId).Distinct().ToList();
        }
        public static bool IsFavorite(IMusicable music)
        {
            return MyFavoriteSongs.Contains(music.ToMusic());
        }

        public static void AddPlaylist(Playlist playlist)
        {
            SQLHelper.Run(c =>
            {
                if (playlist.Id == 0)
                {
                    c.InsertPlaylist(playlist);
                }
                else
                {
                    UpdatePlaylistState(c, playlist, ActiveState.Active);
                }
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, new PlaylistEventArgs(PlaylistEventType.Add));
        }

        public static Playlist AddPlaylist(string name, object data = null)
        {
            Playlist playlist = new Playlist(name);
            if (data != null) playlist.Add(data);
            SQLHelper.Run(c => c.InsertPlaylist(playlist));
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, new PlaylistEventArgs(PlaylistEventType.Add));
            return playlist;
        }

        public static void RenamePlaylist(PlaylistView playlist, string newName)
        {
            SQLHelper.Run(c =>
            {
                Playlist target = c.SelectPlaylistById(playlist.Id);
                if (target.Name == newName)
                {
                    return;
                }
                target.Name = newName;
                c.UpdatePlaylist(target);
                PreferenceItemDAO item = c.SelectPreferenceItem(EntityType.Playlist, target.Id.ToString());
                if (item != null)
                {
                    item.ItemName = newName;
                    c.Update(item);
                }
                foreach (var listener in PlaylistEventListeners)
                    listener.Execute(target, new PlaylistEventArgs(PlaylistEventType.Rename));
            });
        }

        public static void SortPlaylist(PlaylistView original, SortBy criterion)
        {
            original.SetCriterionAndSort(criterion);
            Playlist playlist = original.FromVO();
            SQLHelper.Run(c =>
            {
                c.UpdatePlaylist(playlist);
            });
            PlaylistEventArgs args = new PlaylistEventArgs(PlaylistEventType.Sort) { Criterion = criterion };
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, args);
        }

        public static void UpdatePlaylists(IEnumerable<PlaylistView> playlists)
        {
            SQLHelper.Run(c =>
            {
                foreach (var playlist in playlists)
                    c.UpdatePlaylist(playlist.FromVO());
            });
        }

        public static void RemovePlaylist(Playlist playlist)
        {
            SQLHelper.Run(c =>
            {
                UpdatePlaylistState(c, playlist, ActiveState.Inactive);
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, new PlaylistEventArgs(PlaylistEventType.Remove));
        }

        private static void UpdatePlaylistState(SQLiteConnection c, Playlist playlist, ActiveState state)
        {
            c.Execute("update Playlist set State = ? where Id = ?", state, playlist.Id);
        }

        public static void DislikeMusic(IMusicable music)
        {
            RemoveMusic(MyFavorites, music);
        }

        public static void RemoveMusic(Playlist playlist, IEnumerable<IMusicable> songs)
        {
            foreach (var item in songs)
            {
                RemoveMusic(playlist, item);
            }
        }

        public static void RemoveMusic(Playlist playlist, IMusicable music)
        {
            SQLHelper.Run(c =>
            {
                c.Execute("update PlaylistItem set State = ? where PlaylistId = ? and ItemId = ?", ActiveState.Inactive, playlist.Id, music.ToMusic().Id);
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, new PlaylistEventArgs(PlaylistEventType.RemoveMusic) { Music = music.ToMusic() });
        }

        public static void LikeMusic(Music music)
        {
            AddMusic(MyFavorites, music);
        }

        public static void AddMusic(Playlist playlist, Music music)
        {
            SQLHelper.Run(c =>
            {
                AddMusic(c, playlist.Id, music);
            });
            foreach (var listener in PlaylistEventListeners)
                listener.Execute(playlist, new PlaylistEventArgs(PlaylistEventType.AddMusic) { Music = music });
        }

        public static void AddMusic(Playlist playlist, IEnumerable<IMusicable> musicables)
        {
            foreach (var musicable in musicables)
            {
                AddMusic(playlist, musicable.ToMusic());
            }
        }

        private static void AddMusic(SQLiteConnection c, long playlist, Music music)
        {
            c.Insert(music.ToPlaylistItemDAO(playlist));
        }

        public static void ClearPlaylist(PlaylistView playlist)
        {
            playlist.Clear();
            SQLHelper.Run(c => c.Execute("update PlaylistItem set State = ? where PlaylistId = ?", ActiveState.Inactive, playlist.Id));
        }

        public static int FindNextPlaylistNameIndex(string Name)
        {
            if (!string.IsNullOrEmpty(Name))
            {
                var siblings = AllPlaylists.FindAll(p => p.Name.StartsWith(Name)).Select(p => p.Name).ToHashSet();
                for (int i = 1; i <= siblings.Count; i++)
                    if (!siblings.Contains(Helper.GetNextName(Name, i)))
                        return i;
            }
            return 0;
        }

        public static string FindNextPlaylistName(string Name)
        {
            int index = FindNextPlaylistNameIndex(Name);
            return index == 0 ? Name : Helper.GetNextName(Name, index);
        }

        public static NamingError ValidatePlaylistName(string newName)
        {
            if (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName))
                return NamingError.EmptyOrWhiteSpace;
            if (newName.Length > 50)
                return NamingError.TooLong;
            if (newName == Constants.NowPlaying || newName == Constants.MyFavorites || AllPlaylists.Any(p => p.Name == newName))
                return NamingError.Used;
            if (newName.Contains(TileHelper.StringConcatenationFlag) || newName.Contains("{0}") || newName.Contains("{1}"))
                return NamingError.Special;
            return NamingError.Good;
        }
    }
}
