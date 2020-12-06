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
        private bool ThumbanilNeedsLoading = false;

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
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void GridViewItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private async void UserControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (ThumbanilNeedsLoading = ImageHelper.NeedsLoading(sender, args))
            {
                await (sender.DataContext as GridFolderView)?.SetThumbnailAsync();
            }
        }

        private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            FrameworkElement parent = sender.Parent as FrameworkElement;
            Rect elementBounds = parent.TransformToVisual(sender).TransformBounds(new Rect(0.0, 0.0, parent.ActualWidth, parent.ActualHeight));
            Rect senderRect = new Rect(0.0, 0.0, sender.ActualWidth, sender.ActualHeight);
            bool isPartiallyVisible = senderRect.Contains(new Point(elementBounds.Left, elementBounds.Top)) || senderRect.Contains(new Point(elementBounds.Right, elementBounds.Bottom));
            bool isFullyVisible = senderRect.Contains(new Point(elementBounds.Left, elementBounds.Top)) && senderRect.Contains(new Point(elementBounds.Right, elementBounds.Bottom));
            if (ThumbanilNeedsLoading)
            {
                await (sender.DataContext as GridFolderView)?.SetThumbnailAsync();
            }
        }
    }
}
