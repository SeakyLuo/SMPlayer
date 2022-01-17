using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public enum ActiveState
    {
        Inactive = 0, Active = 1,
    }

    public static class ActiveStateExtensions
    {
        public static bool isActive(this ActiveState state) { return state == ActiveState.Active; }
        public static bool isInactive(this ActiveState state) { return state == ActiveState.Inactive; }

    }
}
