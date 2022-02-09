using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class GridFolderControl : UserControl
    {
        public GridFolderControl()
        {
            this.InitializeComponent();
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            var data = fe.DataContext;
            if (data is GridViewFolder folder)
            {
                MusicPlayer.ShuffleAndPlay(folder.Songs);
            }
            else if (data is GridViewMusic music)
            {
                MusicPlayer.AddMusicAndPlay(music.Source);
            }
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            var data = fe.DataContext;
            var helper = new MenuFlyoutHelper();
            if (data is GridViewFolder folder)
            {
                helper.Data = folder.Songs;
                helper.DefaultPlaylistName = folder.Name;
            }
            else if (data is GridViewMusic music)
            {
                helper.Data = music.Source;
            }
            helper.GetAddToMenuFlyout().ShowAt(fe);
        }
        private void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Control control = sender as Control;
            if ((control.DataContext is GridViewFolder folder && folder.Source.IsNotEmpty) ||
                 control.DataContext is GridViewMusic)
            {
                VisualStateManager.GoToState(control, "PointerOver", true);
            }
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            GridViewStorageItem item = sender.DataContext as GridViewStorageItem;
            if (item == null) return;
            await item.LoadThumbnailAsync();
        }
    }
}
