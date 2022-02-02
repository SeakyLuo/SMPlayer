using SMPlayer.Helpers.Enums;
using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceItem : IPreferable
    {
        public List<PreferLevel> levels;
        public long ThisId { get; set; } // id of this record
        public string Id { get; set; } // Preferred Item's Id
        public long LongId { get => long.Parse(Id); }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public PreferLevel Level { get; set; }
        public EntityType Type { get; set; }

        public PreferenceItem()
        {
            IsEnabled = false;
            Level = PreferLevel.Normal;
        }

        public PreferenceItem(string Id, string Name, EntityType preferType)
        {
            this.Id = Id;
            this.Name = Name;
            this.IsEnabled = true;
            this.Type = preferType;
        }

        public PreferenceItemView AsView()
        {
            return new PreferenceItemView()
            {
                Id = ThisId,
                ItemId = Id,
                Name = Name,
                IsEnabled = IsEnabled,
                Level = Level,
                PreferType = Type,
            };
        }

        PreferenceItem IPreferable.AsPreferenceItem()
        {
            return this;
        }
    }

    public enum PreferLevel
    {
        [EnumDescription("PreferLevelDoNotAppear", "PreferLevelDoNotAppearToolTip"), EnumOrder(-1)]
        DoNotAppear = 0,
        [EnumDescription("PreferLevelDislike", "PreferLevelDislikeToolTip"), EnumOrder(0)]
        Dislike = -1,
        [EnumDescription("PreferLevelNormal", "PreferLevelNormalToolTip")]
        Normal = 1,
        [EnumDescription("PreferLevelHigh", "PreferLevelHighToolTip")]
        High = 2,
        [EnumDescription("PreferLevelHigher", "PreferLevelHigherToolTip")]
        Higher = 3,
        [EnumDescription("PreferLevelVeryHigh", "PreferLevelVeryHighToolTip")]
        VeryHigh = 4,

    }

    public interface IPreferable
    {
        PreferenceItem AsPreferenceItem();
    }
}
