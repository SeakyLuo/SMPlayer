using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class StorageService
    {
        public static FolderTree Root { get => FindFolder(Settings.settings.Tree.Id) ?? new FolderTree(); }
        public static List<FolderTree> AllFolders
        {
            get => SQLHelper.Run(c => c.Query<FolderDAO>("select * from Folder where State = ?", ActiveState.Active))
                                       .Select(i => i.FromDAO()).ToList();
        }
        public static FolderTree FindFolderInfo(long id)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(id));
        }
        public static FolderTree FindFolderInfo(string path)
        {
            return SQLHelper.Run(c => c.SelectFolderInfo(path));
        }
        public static FolderTree FindFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFolder(id));
        }
        public static FolderTree FindFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFolder(path));
        }
        public static FolderTree FindFullFolder(long id)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(id));
        }
        public static FolderTree FindFullFolder(string path)
        {
            return SQLHelper.Run(c => c.SelectFullFolder(path));
        }
        public static List<FolderTree> FindSubFolders(FolderTree folder)
        {
            return SQLHelper.Run(c => c.SelectSubFolders(folder)).Select(i => i).OrderBy(i => i.Name).ToList();
        }
        public static List<FolderFile> FindSubFiles(FolderTree folder)
        {
            return SQLHelper.Run(c => c.SelectSubFiles(folder));
        }
        public static FolderFile FindFile(long id)
        {
            return SQLHelper.Run(c => c.SelectFile(id));
        }
        public static FolderFile FindFile(string path)
        {
            return SQLHelper.Run(c => c.SelectFileByPath(path));
        }
        public static List<Music> FindSubSongs(FolderTree folder)
        {
            return SQLHelper.Run(c =>
            {
                return c.SelectMusicByIds(c.SelectSubFiles(folder).Select(i => i.FileId)).ToList();
            });
        }
    }
}
