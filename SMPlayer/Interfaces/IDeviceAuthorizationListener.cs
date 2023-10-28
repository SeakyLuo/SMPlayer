using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Interfaces
{
    public interface IDeviceAuthorizationListener
    {
        void Execute(AuthorizedDevice device, DeviceAuthorizationEventArgs args);
    }

    public class DeviceAuthorizationEventArgs
    {
        public DeviceAuthorizationEventType EventType { get; set; }
    }

    public enum DeviceAuthorizationEventType
    {
        Delete
    }
}
