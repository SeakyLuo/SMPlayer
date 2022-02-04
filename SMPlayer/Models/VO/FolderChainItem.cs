using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.VO
{
    public class FolderChainItem : PropertyChangedNotifier
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool HasChildren
        {
            get => _hasChildren;
            set
            {
                _hasChildren = value;
                OnPropertyChanged();
            }
        }
        private bool _hasChildren;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                _isHighlighted = value;
                OnPropertyChanged();
            }
        }
        private bool _isHighlighted;
        public SortBy Criterion { get; set; } = SortBy.Title;
        public ObservableCollection<FolderChainItem> Children { get; set; } = new ObservableCollection<FolderChainItem>();

        public FolderChainItem(FolderTree folder)
        {
            Id = folder.Id;
            Name = folder.Name;
            Path = folder.Path;
            Criterion = folder.Criterion;
            Children.SetTo(Settings.FindSubFolders(new FolderTree() { Id = Id })
                                   .Select(i => new FolderChainItem(i)));
            HasChildren = Children.IsNotEmpty();
        }

        public FolderTree ToFolderTree()
        {
            return new FolderTree
            {
                Id = Id,
                Path = Path,
                Criterion = Criterion,
            };
        }
    }
}