using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace SMPlayer.Interfaces
{
    public interface IMusicPlayerEventListener
    {
        void Execute(MusicPlayerEventArgs args);
    }

    public enum MusicPlayerEventType
    {
        Add, Remove, Switch, Clear, Move, Tick, MediaEnded, StateChanged
    }

    public class MusicPlayerEventArgs
    {
        public MusicPlayerEventType EventType { get; set; }
        public Music Music { get; set; }
        public int Index { get; set; } = -1;

        public MusicPlayerEventArgs() { }

        public MusicPlayerEventArgs(MusicPlayerEventType eventType)
        {
            EventType = eventType;
        }
    }

    public class MusicPlayerMoveEventArgs : MusicPlayerEventArgs
    {
        public int ToIndex { get; set; }

        public MusicPlayerMoveEventArgs(int fromIndex, int toIndex)
        {
            EventType = MusicPlayerEventType.Move;
            Index = fromIndex;
            ToIndex = toIndex;
        }
    }

    public class MusicPlayerMusicSwitchEventArgs : MusicPlayerEventArgs
    {
        public MediaPlaybackItemChangedReason Reason { get; set; }

        public MusicPlayerMusicSwitchEventArgs(MediaPlaybackItemChangedReason reason)
        {
            EventType = MusicPlayerEventType.Switch;
            Reason = reason;
        }
    }
    public class MusicPlayerStateChangedEventArgs : MusicPlayerEventArgs
    {
        public MediaPlaybackState State { get; set; }

        public MusicPlayerStateChangedEventArgs(MediaPlaybackState state)
        {
            EventType = MusicPlayerEventType.StateChanged;
            State = state;
        }
    }
}
