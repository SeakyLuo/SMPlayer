using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer
{
    public interface IMusicEventListener
    {
        void Execute(Music music, MusicEventArgs args);
    }

    public enum MusicEventType
    {
        Like, Add, Remove, Modify
    }

    public class MusicEventArgs
    {
        public MusicEventType EventType { get; set; }
        public bool IsFavorite { get; set; }
        public Music ModifiedMusic { get; set; }

        public MusicEventArgs(MusicEventType eventType)
        {
            EventType = eventType;
        }
    }
}
