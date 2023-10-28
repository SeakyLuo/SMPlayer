using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public class LockHelper
    {
        private static Dictionary<string, bool> LockMap = new Dictionary<string, bool>();

        public static bool Lock(string key)
        {
            lock (LockMap)
            {
                if (LockMap.ContainsKey(key)) return false;
                LockMap.Add(key, true);
                return true;
            }
        }

        public static void Unlock(string key)
        {
            lock (LockMap)
            {
                LockMap.Remove(key);
            }
        }
    }
}
