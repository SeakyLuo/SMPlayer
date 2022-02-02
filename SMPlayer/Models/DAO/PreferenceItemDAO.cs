using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    [Table(TableName)]
    public class PreferenceItemDAO
    {
        public const string TableName = "PreferenceItem";

        [PrimaryKey, AutoIncrement]
        public long Id { get; set; } 
        public EntityType Type { get; set; }
        // 偏好项目ID
        public string ItemId { get; set; }
        // 为了防止数据失效后无法通过实时查询展示项目名称，故此记录
        public string ItemName { get; set; }
        public bool IsEnabled { get; set; }
        public PreferLevel Level { get; set; } = PreferLevel.High;
        public ActiveState State { get; set; } = ActiveState.Active;

    }
}
