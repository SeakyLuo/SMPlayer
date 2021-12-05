using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class PlaylistDAO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<long> Songs { get; set; }
    }
}
