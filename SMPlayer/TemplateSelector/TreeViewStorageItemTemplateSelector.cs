using SMPlayer.Models;
using SMPlayer.Models.VO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.TemplateSelector
{
    public class TreeViewStorageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is TreeViewFolder) return FolderTemplate;
            if (item is TreeViewFile) return FileTemplate;
            return null;
        }
    }
}
