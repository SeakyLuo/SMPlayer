using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer
{
    public interface IRecentEventListener
    {
        void Search(string keyword);
        void Played(Music music);
    }
}
