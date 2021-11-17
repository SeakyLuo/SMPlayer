using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class IdGenerator
    {
        public Dictionary<IdType, int> IdMap { get; set; } = new Dictionary<IdType, int>();
        private readonly Dictionary<IdType, bool> LockMap = new Dictionary<IdType, bool>();

        public IdGenerator()
        {
            foreach (var idType in Enum.GetValues(typeof(IdType)))
            {
                IdMap.Add((IdType)idType, 1);
                LockMap.Add((IdType)idType, false);
            }
        }

        public IdGenerator(Dictionary<IdType, int> src)
        {
            foreach (var p in src)
            {
                IdMap[p.Key] = p.Value;
                LockMap[p.Key] = false;
            }
        }

        private int LockAndGenerateId(IdType type)
        {
            if (LockMap[type]) return 0;
            LockMap[type] = true;
            int newValue = IdMap[type] + 1;
            IdMap[type] = newValue;
            LockMap[type] = false;
            return newValue;
        }

        private List<int> LockAndGenerateIds(IdType type, int itemCount)
        {
            if (LockMap[type]) return new List<int>();
            LockMap[type] = true;
            int currentId = IdMap[type];
            List<int> ids = new List<int>();
            for (int i = 1; i <= itemCount; i++)
            {
                ids.Add(currentId + i);
            }
            IdMap[type] = currentId + itemCount;
            LockMap[type] = false;
            return ids;
        }

        public int GenerateMusicId()
        {
            return GenerateId(IdType.Music);
        }

        public int GeneratePlaylistId()
        {
            return GenerateId(IdType.Playlist);
        }

        public int GenerateId(IdType type)
        {
            return Retry(() => LockAndGenerateId(type), id => id > 0, $"生成{type}类型ID失败");
        }

        public List<int> GenerateIds(IdType type, int itemCount)
        {
            return Retry(() => LockAndGenerateIds(type, itemCount), ids => ids.IsNotEmpty(), $"生成{type}类型ID失败");
        }

        private T Retry<T>(Func<T> operation, Func<T, bool> checker, string failMessage, int times = 5)
        {
            for (int i = 0; i < times; i++)
            {
                try
                {
                    T result = operation.Invoke();
                    if (checker.Invoke(result))
                    {
                        return result;
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Helper.Print(e.ToString());
                }
            }
            throw new Exception(failMessage);
            //return default;
        }

    }

    public enum IdType
    {
        Music, Playlist
    }
}
