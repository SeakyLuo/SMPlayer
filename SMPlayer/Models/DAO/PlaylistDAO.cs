using SMPlayer.Models.DAO;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("Playlist")]
    public class PlaylistDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public string Name { get; set; }
        public SortBy Criterion { get; set; }
        [Ignore]
        public List<PlaylistItemDAO> Songs { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

    }
}
