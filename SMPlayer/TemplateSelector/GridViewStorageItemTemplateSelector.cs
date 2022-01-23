using SMPlayer.Models.VO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.TemplateSelector
{
    public class GridViewStorageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate FileTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            switch ((item as GridViewStorageItem).Type)
            {
                case StorageType.Folder:
                    return FolderTemplate;
                case StorageType.File:
                    return FileTemplate;
                default:
                    return null;
            }
        }
    }
}
