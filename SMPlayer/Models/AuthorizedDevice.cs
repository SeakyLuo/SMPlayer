using SMPlayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class AuthorizedDevice
    {
        public long Id { get; set; }
        public string Ip { get; set; }
        public string DeviceName { get; set; }
        public ActiveState State { get; set; }
        public AuthorizationLevel Auth { get; set; } 
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }

    }
}
