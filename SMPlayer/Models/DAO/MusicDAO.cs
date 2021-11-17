using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class MusicDAO
    {
        public long Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Duration { get; set; }
        public bool Favorite { get; set; }
        public int PlayCount { get; set; }
        public DateTimeOffset DateAdded { get; set; }

    }
}
