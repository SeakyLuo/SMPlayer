using SMPlayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class AuthorizedDeviceView : PropertyChangedNotifier
    {
        public long Id { get; set; }
        public string Ip { get; set; }
        public string DeviceName 
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                OnPropertyChanged("NameInList");
            }
        }
        private string _deviceName;
        public ActiveState State { get; set; }
        public AuthorizationLevel Auth
        {
            get => _auth;
            set
            {
                _auth = value;
                OnPropertyChanged("IsAuthorized");
            }
        }
        private AuthorizationLevel _auth;
        public DateTimeOffset CreateTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }

        public string NameInList { get => string.IsNullOrEmpty(DeviceName) ? Ip : $"{DeviceName} ({Ip})"; }

        public bool IsAuthorized
        {
            get => Auth == AuthorizationLevel.Allowed;
            set => Auth = value ? AuthorizationLevel.Allowed : AuthorizationLevel.Blacklisted;  
        }

    }
}
