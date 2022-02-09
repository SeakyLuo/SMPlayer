using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Interfaces
{
    public interface IPlaylistEventListener
    {
        void Execute(Playlist playlist, PlaylistEventArgs args);
    }

    public enum PlaylistEventType
    {
        Add, Rename, Remove, Sort, AddMusic, RemoveMusic,
    }

    public class PlaylistEventArgs
    {
        public PlaylistEventType EventType;
        public SortBy Criterion { get; set; }
        public Music Music { get; set; }

        public PlaylistEventArgs() { }

        public PlaylistEventArgs(PlaylistEventType eventType)
        {
            EventType = eventType;
        }
    }
}
