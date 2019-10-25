using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.TemplateSelector
{
    public class FolderTreeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var content = (item as TreeViewNode).Content;
            if (content is Models.FolderTree) return FolderTemplate;
            if (content is Models.Music) return FileTemplate;
            return null;
        }
    }
}
