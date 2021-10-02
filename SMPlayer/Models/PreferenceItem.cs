using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;

        public PreferenceItem() { }

        public PreferenceItem(string Id)
        {
            this.Id = Id;
        }

        public PreferenceItem(string Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }
    }

    public enum PreferType
    {
        Song, Artist, Album, Playlist, Folder
    }

    public interface IPreferable
    {
        PreferenceItem AsPreferenceItem();
        PreferType GetPreferType();
    }
}
