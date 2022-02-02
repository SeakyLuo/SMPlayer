using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class PreferenceItemView : IPreferable, INotifyPropertyChanged
    {
        public long Id { get; set; }
        public string ItemId { get; set; }
        public long LongId { get => long.Parse(ItemId); }
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
        public EntityType PreferType { get; set; }
        public PreferLevel Level { get; set; }
        public PreferLevelView LevelView
        {
            get => new PreferLevelView(Level);
            set
            {
                if (value != null) Level = value.Level;
            }
        }
        public List<PreferLevelView> Levels => PreferLevelView.Views;

        public int LevelWidth { get => Helper.CurrentLanguage.LanguageTag == Helper.Language_CN ? 90 : 105; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public PreferenceItem AsPreferenceItem()
        {
            return new PreferenceItem
            {
                ThisId = Id,
                Id = ItemId,
                Name = Name,
                IsEnabled = IsEnabled,
                Level = Level,
                Type = PreferType,
            };
        }
    }

}
