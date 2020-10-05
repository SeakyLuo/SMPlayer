using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class MenuFlyoutOption
    {
        public bool WithNavigation { get; set; } = true;
        public bool WithSelect { get; set; } = true;
        public MultiSelectCommandBarOption MultiSelectOption { get; set; }
    }
}
