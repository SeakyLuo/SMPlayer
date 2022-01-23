﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class PreferenceItem
    {
        public long ItemId { get; set; } // id of this record
        public string Id { get; set; } // Preferred Item's Id
        public long LongId { get => long.Parse(Id); }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public PreferLevel Level { get; set; } = PreferLevel.Normal;

        public PreferenceItem()
        {
            IsEnabled = false;
        }

        public PreferenceItem(string Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
            this.IsEnabled = true;
        }

        public PreferenceItemView AsView()
        {
            return new PreferenceItemView()
            {
                Id = Id,
                Name = Name,
                IsEnabled = IsEnabled,
                Level = Level
            };
        }
    }

    public class PreferenceItemView : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public long LongId { get => long.Parse(Id); }
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }
        private string name;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool isEnabled = true;
        public bool IsValid
        {
            get => isValid;
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowRemove { get; set; } = true;

        private bool isValid = true;
        public string ToolTip { get; set; }
        public PreferType PreferType { get; set; }
        public PreferLevel Level { get; set; }

        public PreferenceItem AsModel()
        {
            return new PreferenceItem()
            {
                Id = Id,
                Name = Name,
                IsEnabled = IsEnabled,
                Level = Level
            };
        }

        public PreferenceItemView() { }
        public PreferenceItemView(string Id, string Name, string ToolTip, PreferType PreferType)
        {
            this.Id = Id;
            this.Name = Name;
            this.ToolTip = ToolTip;
            this.PreferType = PreferType;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PreferType
    {
        Song, Artist, Album, Playlist, Folder, RecentAdded, MyFavorites, MostPlayed, LeastPlayed
    }

    public enum PreferLevel
    {
        Low = 1, Normal = 2, High = 3, Higher = 4
    }

    public interface IPreferable
    {
        PreferenceItem AsPreferenceItem();
        PreferenceItemView AsPreferenceItemView();
    }
}
