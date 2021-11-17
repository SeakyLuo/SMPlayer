using SMPlayer.Models;
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
            var data = (sender as Button).DataContext as GridFolderView;
            MediaHelper.ShuffleAndPlay(data.Songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as GridFolderView;
            var helper = new MenuFlyoutHelper() { Data = data.Songs, DefaultPlaylistName = data.Name };
            helper.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
        }
        private void GridViewItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Control control = sender as Control;
            GridFolderView folder = control.DataContext as GridFolderView;
            if (folder.Songs.IsNotEmpty())
            {
                VisualStateManager.GoToState(control, "PointerOver", true);
            }
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private async void UserControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (sender.DataContext is GridFolderView data)
            {
                Helper.Print($"EffectiveViewportChanged, name {data.Name}, isLoaded {data.ThumbnailLoaded}, isPartiallyVisible {ImageHelper.NeedsLoading(sender, args)}");
                if (!data.ThumbnailLoaded && ImageHelper.NeedsLoading(sender, args))
                {
                    await data.LoadThumbnailAsync();
                }
            }
        }
    }
}
