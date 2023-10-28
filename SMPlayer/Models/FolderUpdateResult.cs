using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public class FolderUpdateResult
    {
        public string Path { get; set; }

        public List<string> FilesAdded { get; set; } = new List<string>();
        public List<string> FilesRemoved { get; set; } = new List<string>();
        public List<string> FilesMoved { get; set; } = new List<string>();
        public List<string> FoldersAdded { get; set; } = new List<string>();
        public List<string> FoldersRemoved { get; set; } = new List<string>();
        public bool HasChange => FilesAdded.IsNotEmpty() || FilesRemoved.IsNotEmpty() || FilesMoved.IsNotEmpty();

        public FolderUpdateResult() { }

        public FolderUpdateResult(string folderPath)
        {
            Path = folderPath;
        }

        public void AddFile(string path)
        {
            FilesAdded.Add(path);
        }
        public void RemoveFile(string path)
        {
            FilesRemoved.Add(path);
        }
        public void AddFolder(string path)
        {
            FoldersAdded.Add(path);
        }
        public void RemoveFolder(string path)
        {
            FoldersRemoved.Add(path);
        }

        public List<FolderUpdateResultGroup> ToGroups()
        {
            List<FolderUpdateResultGroup> groups = new List<FolderUpdateResultGroup>();
            if (FilesAdded.IsNotEmpty())
                groups.Add(new FolderUpdateResultGroup(Path, "FilesAddedWithCount", FilesAdded, true));
            if (FilesRemoved.IsNotEmpty())
                groups.Add(new FolderUpdateResultGroup(Path, "FilesRemovedWithCount", FilesRemoved, false));
            if (FilesMoved.IsNotEmpty())
                groups.Add(new FolderUpdateResultGroup(Path, "FilesMovedWithCount", FilesMoved, true));
            return groups;
        }

        public void Finish()
        {
            foreach (var item in FilesAdded)
            {
                string filename = System.IO.Path.GetFileName(item);
                if (FilesRemoved.Count(i => i.EndsWith(filename)) == 1 &&
                    FilesAdded.Count(i => i.EndsWith(filename)) == 1)
                {
                    FilesRemoved.RemoveAll(i => i.EndsWith(filename));
                    FilesMoved.Add(item);
                }
            }
            FilesAdded.RemoveAll(i => FilesMoved.Contains(i));
        }

        public string ToDisplayMessage()
        {
            List<string> messages = new List<string>();
            if (FilesAdded.IsNotEmpty())
            {
                string filesAddedMsg;
                if (FilesAdded.Count == 1)
                {
                    string firstFileAdded = System.IO.Path.GetFileNameWithoutExtension(FilesAdded[0]);
                    filesAddedMsg = Helper.LocalizeMessage("CheckNewMusicResult1Added", firstFileAdded);
                }
                else
                {
                    filesAddedMsg = Helper.LocalizeMessage("CheckNewMusicResultMultipleAdded", FilesAdded.Count);
                }
                messages.Add(filesAddedMsg);
            }
            if (FilesRemoved.IsNotEmpty())
            {
                string filesRemovedMsg;
                if (FilesRemoved.Count == 1)
                {
                    string firstFileRemoved = System.IO.Path.GetFileNameWithoutExtension(FilesRemoved[0]);
                    filesRemovedMsg = Helper.LocalizeMessage("CheckNewMusicResult1Removed", firstFileRemoved);
                }
                else
                {
                    filesRemovedMsg = Helper.LocalizeMessage("CheckNewMusicResultMultipleRemoved", FilesRemoved.Count);
                }
                messages.Add(filesRemovedMsg);
            }
            if (FilesMoved.IsNotEmpty())
            {
                string filesMovedMsg;
                if (FilesRemoved.Count == 1)
                {
                    string firstFileMoved = System.IO.Path.GetFileNameWithoutExtension(FilesMoved[0]);
                    filesMovedMsg = Helper.LocalizeMessage("CheckNewMusicResult1Moved", firstFileMoved);
                }
                else
                {
                    filesMovedMsg = Helper.LocalizeMessage("CheckNewMusicResultMultipleMoved", FilesMoved.Count);
                }
                messages.Add(filesMovedMsg);
            }
            return messages.Where(i => !string.IsNullOrEmpty(i)).Join(Helper.LocalizeMessage("Comma"));
        }
    }

    public class FolderUpdateResultGroup
    {
        public string Tag { get; set; }
        public List<FolderUpdateResultGroupItem> Items { get; set; }
        public bool IsClickable { get; set; }
        public FolderUpdateResultGroup(string parentPath, string tag, List<string> items, bool isClickable)
        {
            Tag = Helper.LocalizeText(tag, items.Count());
            Items = items.Select(i => new FolderUpdateResultGroupItem(parentPath, i, isClickable)).ToList();
            foreach (var group in Items.GroupBy(i => i.Name))
            {
                if (group.Count() <= 1) continue;
                foreach (var item in group)
                {
                    item.Name = item.Path;
                }
            }
            IsClickable = isClickable;
        }
    }

    public class FolderUpdateResultGroupItem : PropertyChangedNotifier
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool ShowFlyout { get; set; }
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                OnPropertyChanged("IsPlaying");
            }
        }
        private bool isPlaying;
        public FolderUpdateResultGroupItem(string parentPath, string path, bool showFlyout)
        {
            string simplePath = path.Substring(parentPath.Length + 1);  // RootPath不以/结尾
            Name = simplePath.Substring(0, simplePath.LastIndexOf("."));
            Path = path;
            ShowFlyout = showFlyout;
        }
    }
}
