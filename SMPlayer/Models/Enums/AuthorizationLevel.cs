using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.Enums
{
    public enum AuthorizationLevel
    {
        Locked = -2,
        WrongPassword = -1,
        Blacklisted = 0,
        Allowed = 1,
        NotAuthorized = 2,
    }
}
