using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer.Controls
{
    public sealed partial class TreeViewFolderControl : UserControl
    {
        public TreeViewFolderControl()
        {
            this.InitializeComponent();
        }
        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewFolder folder = (sender as FrameworkElement).DataContext as GridViewFolder;
            List<Music> songs = folder.Source.Flatten();
            if (songs.IsEmpty())
            {
                Helper.ShowNotification("NoMusicUnderCurrentFolder");
            }
            else
            {
                MusicPlayer.ShuffleAndPlay(songs);
            }
        }

        private void GridViewFolder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void GridViewFolder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private void RefreshFolderButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewFolder folder = (sender as FrameworkElement).DataContext as GridViewFolder;
            UpdateHelper.RefreshFolder(folder.Source);
        }

        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            GridViewFolder folder = frameworkElement.DataContext as GridViewFolder;
            MenuFlyoutHelper helper = new MenuFlyoutHelper() { Data = folder.Source.Flatten() };
            helper.GetAddToMenuFlyout().ShowAt(frameworkElement);
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewFolder folder = (sender as FrameworkElement).DataContext as GridViewFolder;
            FolderTree tree = folder.Source;
            await new InputDialog(tree).ShowAsync();
        }

        private void OpenLocalButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewFolder folder = (sender as FrameworkElement).DataContext as GridViewFolder;
            MenuFlyoutHelper.ShowInExplorerWithLoader(folder.Path, StorageItemTypes.Folder);
        }
    }
}
