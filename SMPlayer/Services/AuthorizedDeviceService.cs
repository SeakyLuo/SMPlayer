using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class AuthorizedDeviceService
    {
        private static readonly List<IDeviceAuthorizationListener> AuthorizationListeners = new List<IDeviceAuthorizationListener>();

        public static void AddAuthorizationListener(IDeviceAuthorizationListener listener)
        {
            AuthorizationListeners.Add(listener);
        }

        public static AuthorizationLevel IsAuthorized(string ip, string password)
        {
            if (Settings.settings.RemotePlayPassword != password) return AuthorizationLevel.WrongPassword;
            return GetIpAuth(ip);
        }

        private static AuthorizationLevel GetIpAuth(string ip)
        {
            AuthorizedDevice device = SelectByIp(ip);
            if (device == null || device.State == ActiveState.Inactive) return AuthorizationLevel.NotAuthorized;
            return device.Auth;
        }

        public static AuthorizationLevel AddAuthorization(AuthorizedDevice device)
        {
            string ip = device.Ip;
            if (!LockHelper.Lock(ip)) return AuthorizationLevel.Locked;
            try
            {
                AuthorizationLevel auth = GetIpAuth(device.Ip);
                if (auth == AuthorizationLevel.Blacklisted || auth == AuthorizationLevel.Allowed) return auth;
                SQLHelper.Run(c => c.InsertAuthorizedDevice(device));
                return AuthorizationLevel.Allowed;
            }
            finally
            {
                LockHelper.Unlock(ip);
            }
        }

        public static AuthorizedDevice SelectByIp(string ip)
        {
            return SQLHelper.Run(c => c.Query<AuthorizedDeviceDAO>("select * from AuthorizedDevice where Ip = ? and State", ip, ActiveState.Active))
                                       .Select(i => i.FromDao()).FirstOrDefault();
        }

        public static List<AuthorizedDevice> GetActiveAuthorizedDevice()
        {
            return new List<AuthorizedDevice>() {
                new AuthorizedDevice
            {
                Id = 3,
                Ip = "127.0.0.1",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Auth = AuthorizationLevel.Blacklisted,
                State = ActiveState.Active
            },
                new AuthorizedDevice
            {
                Id = 2,
                Ip = "123.123.123.123",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Auth = AuthorizationLevel.Allowed,
                State = ActiveState.Active
            },
            new AuthorizedDevice
            {
                Id = 1,
                Ip = "255.0.0.255",
                DeviceName = "luohaitian",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Auth = Models.Enums.AuthorizationLevel.Allowed,
                State = ActiveState.Active
            }
            };
            //return SQLHelper.Run(c => c.SelectAuthorizedDevices(ActiveState.Active));
        }

        public static void UpdateAuthorization(AuthorizedDevice device)
        {
            SQLHelper.Run(c => c.UpdateAuthorizedDevice(device));
        }

        public static void DeleteAuthorization(AuthorizedDevice device)
        {
            device.State = ActiveState.Inactive;
            UpdateAuthorization(device);
            foreach (var listener in AuthorizationListeners)
                listener.Execute(device, new DeviceAuthorizationEventArgs { EventType = DeviceAuthorizationEventType.Delete });
        }

    }
}
