using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class MenuFlyoutOption
    {
        public bool ShowRemove { get; set; } = false;
        public bool ShowSeeArtistsAndSeeAlbum { get; set; } = true;
        public bool ShowMusicProperties { get; set; } = true;
        public bool ShowSelect { get; set; } = true;
        public bool ShowMultiSelect { get; set; } = false;
        public bool ShowMoveToTop { get; set; } = false;
        public bool ShowMoveToFolder { get; set; } = false;
        public bool ShowDelete { get; set; } = true;
        public MultiSelectCommandBarOption MultiSelectOption { get; set; }
    }
}
