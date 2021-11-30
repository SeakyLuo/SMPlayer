using SMPlayer.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class DropShadowControl : UserControl
    {
        public event EventHandler<object> MenuFlyoutOpeningAction;
        public DropShadowControl()
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
            MenuFlyoutOpeningAction?.Invoke(sender, e);
        }
        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            MusicPlayer.ShuffleAndPlay(data.Songs);
        }
        private void AddToButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as Button).DataContext as AlbumView;
            var helper = new MenuFlyoutHelper() { Data = data.Songs, DefaultPlaylistName = data.Name };
            helper.GetAddToPlaylistsMenuFlyout().ShowAt(sender as FrameworkElement);
        }
    }
}
