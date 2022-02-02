using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers.Enums
{
    public class EnumDescription : Attribute
    {
        public EnumDescription(string description = null, string tooltip = null)
        {
            Description = description;
            ToolTip = tooltip;
        }

        public string Description { get; set; }
        public string ToolTip { get; set; }
        
    }

    // 排序顺序
    public class EnumOrder : Attribute
    {
        public EnumOrder(int order)
        {
            Order = order;
        }
        public int? Order { get; set; }
    }
}
