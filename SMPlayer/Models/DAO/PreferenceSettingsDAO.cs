using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("PreferenceSetting")]
    public class PreferenceSettingsDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public bool Songs { get; set; } = false;
        public bool Artists { get; set; } = false;
        public bool Albums { get; set; } = false;
        public bool Playlists { get; set; } = false;
        public bool Folders { get; set; } = false;
        public long RecentAddedId { get; set; }
        public long MyFavoritesId { get; set; }
        public long MostPlayedId { get; set; }
        public long LeastPlayedId { get; set; }

    }
}
