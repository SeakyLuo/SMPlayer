using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table(TableName)]
    public class RecentRecordDAO
    {
        public const string TableName = "RecentRecord";

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }
        public RecentType Type { get; set; }
        public string Item { get; set; }
        public DateTimeOffset Time { get; set; }
        public ActiveState State { get; set; } = ActiveState.Active;

    }

    public enum RecentType
    {
        Play, Add, Search
    }
}
