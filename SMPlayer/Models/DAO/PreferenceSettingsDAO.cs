using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class PreferenceSettingsDAO
    {
        public List<PreferenceItemDAO> PreferredSongs { get; set; } = new List<PreferenceItemDAO>();
        public List<PreferenceItemDAO> PreferredArtists { get; set; } = new List<PreferenceItemDAO>();
        public List<PreferenceItemDAO> PreferredAlbums { get; set; } = new List<PreferenceItemDAO>();
        public List<PreferenceItemDAO> PreferredPlaylists { get; set; } = new List<PreferenceItemDAO>();
        public List<PreferenceItemDAO> PreferredFolders { get; set; } = new List<PreferenceItemDAO>();
        public bool Songs { get; set; } = false;
        public bool Artists { get; set; } = false;
        public bool Albums { get; set; } = false;
        public bool Playlists { get; set; } = false;
        public bool Folders { get; set; } = false;
        public PreferenceItemDAO RecentAdded { get; set; } = new PreferenceItemDAO();
        public PreferenceItemDAO MyFavorites { get; set; } = new PreferenceItemDAO();
        public PreferenceItemDAO MostPlayed { get; set; } = new PreferenceItemDAO();
        public PreferenceItemDAO LeastPlayed { get; set; } = new PreferenceItemDAO();

    }
}
