using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    [Serializable]
    public class Playlist : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public ObservableCollection<Music> Songs { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Playlist(string Name, IEnumerable<Music> Songs)
        {
            this.Name = Name;
            this.Songs = new ObservableCollection<Music>(Songs);
        }


        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

