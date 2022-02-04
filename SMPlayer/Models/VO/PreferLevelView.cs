using SMPlayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class PreferLevelView
    {
        public static List<PreferLevelView> Views = EnumHelper.GetOrderedValues(typeof(PreferLevel))
                                                              .OfType<PreferLevel>()
                                                              .Select(i => new PreferLevelView(i))
                                                              .ToList();
        public string LevelName { get; set; }
        public string ToolTip { get; set; }
        public PreferLevel Level { get; set; }

        public PreferLevelView(PreferLevel level)
        {
            Level = level;
            LevelName = Helper.LocalizeText(level.GetDescription());
            ToolTip = Helper.LocalizeText(level.GetToolTip());
        }

        public override bool Equals(object obj)
        {
            return obj is PreferLevelView view && Level == view.Level;
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode();
        }
    }
}
