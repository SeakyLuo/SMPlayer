using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public enum ActiveState
    {
        Inactive = 0, Active = 1,
    }

    public static class ActiveStateExtensions
    {
        public static bool IsActive(this ActiveState state) { return state == ActiveState.Active; }
        public static bool IsInactive(this ActiveState state) { return state == ActiveState.Inactive; }

    }
}
