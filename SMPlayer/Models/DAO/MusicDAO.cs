using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table("Music")]
    public class MusicDAO
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        [Indexed, MaxLength(300)]
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public int PlayCount { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

    }
}
