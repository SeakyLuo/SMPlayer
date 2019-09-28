using SMPlayer.Models;
using System;
using System.Collections.Generic;
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

namespace SMPlayer
{
    public sealed partial class GridAlbumControl : UserControl
    {
        public GridAlbumControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "PointerOver", true);
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "Normal", true);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            MenuFlyoutHelper.SetPlaylistMenu(sender);
        }
        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            MediaHelper.ShuffleAndPlay(data.Songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            var helper = new MenuFlyoutHelper() { Data = data.Songs, DefaultPlaylistName = data.Name };
            helper.GetAddToPlaylistsMenuFlyout().ShowAt(sender as FrameworkElement);
        }
    }
}
