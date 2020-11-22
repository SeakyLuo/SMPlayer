using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class MultiSelectCommandBarOption
    {
        public bool ShowPlay { get; set; } = true;
        public bool ShowAdd { get; set; } = true;
        public bool ShowRemove { get; set; } = true;
        public bool ShowDelete { get; set; } = false;
        public bool ShowReverseSelection { get; set; } = true;
    }
}
