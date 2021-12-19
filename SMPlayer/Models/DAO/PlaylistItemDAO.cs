using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("PlaylistItem")]
    public class PlaylistItemDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        [Indexed]
        public long PlaylistId { get; set; }
        public long ItemId { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

    }
}
