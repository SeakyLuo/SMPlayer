using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SMPlayer.Helpers
{
    public static class FileHelper
    {
        private const string PathJoiner = "\\";

        public static string GetParentPath(string path)
        {
            int index = path.LastIndexOf(PathJoiner);
            return index == -1 ? "" : path.Substring(0, index);
        }

        public static string GetFilename(string path)
        {
            int startIndex = path.LastIndexOf(PathJoiner) + 1;
            return path.Substring(startIndex);
        }

        public static string GetDisplayName(string path)
        {
            string filename = GetFilename(path);
            int dot = path.LastIndexOf('.');
            return dot == -1 ? filename : filename.Substring(0, dot);
        }

        public static string MoveToPath(string path, string newPath)
        {
            return path.Replace(GetParentPath(path), newPath);
        }

        public static string JoinPaths(params string[] values)
        {
            return string.Join(PathJoiner, values);
        }
    }
}
