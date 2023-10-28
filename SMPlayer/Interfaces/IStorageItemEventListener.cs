using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Interfaces
{
    public interface IStorageItemEventListener
    {
        void ExecuteFileEvent(FolderFile file, StorageItemEventArgs args);
        void ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args);
    }

    public class StorageItemEventArgs
    {
        public StorageItemEventType EventType { get; set; }
        public string Path { get; set; }
        public FolderTree Folder { get; set; }
        public StorageItemEventArgs(StorageItemEventType eventType)
        {
            EventType = eventType;
        }
    }

    public enum StorageItemEventType
    {
        Add, Rename, Remove, Move, Update, BeforeReset, Reset, 
        HideFolder, ResumeFolder, HideFile, ResumeFile
    }
}
