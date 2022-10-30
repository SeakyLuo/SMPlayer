using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AuthorizedDevicePage : Page, IDeviceAuthorizationListener
    {
        private readonly ObservableCollection<AuthorizedDeviceView> ActiveAuthorizedDevices = new ObservableCollection<AuthorizedDeviceView>();

        public AuthorizedDevicePage()
        {
            this.InitializeComponent();
            AuthorizedDeviceService.AddAuthorizationListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ActiveAuthorizedDevices.Clear();
            ActiveAuthorizedDevices.AddRange(AuthorizedDeviceService.GetActiveAuthorizedDevice().Select(i => i.ToVO()));
        }

        private async void ActiveAuthoziedDeviceListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            AuthorizedDeviceView view = e.ClickedItem as AuthorizedDeviceView;
            Dialogs.AuthorizedDeviceDetailDialog dialog = new Dialogs.AuthorizedDeviceDetailDialog(view);
            await dialog.ShowAsync();
        }

        private void ActiveAuthoziedDeviceListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = PlaylistControl.GetRowBackground(args.ItemIndex);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            AuthorizedDeviceView view = (sender as FrameworkElement).DataContext as AuthorizedDeviceView;
            AuthorizedDeviceService.DeleteAuthorization(view.FromVO());
            ActiveAuthorizedDevices.Remove(view);
        }

        void IDeviceAuthorizationListener.Execute(AuthorizedDevice device, DeviceAuthorizationEventArgs args)
        {
            switch (args.EventType)
            {
                case DeviceAuthorizationEventType.Delete:
                    int index = ActiveAuthorizedDevices.FindIndex(i => i.Id == device.Id);
                    if (index == -1) return;
                    ActiveAuthorizedDevices.RemoveAt(index);
                    AlternateRowBackgroud(index, ActiveAuthorizedDevices.Count);
                    break;
            }
        }
        private void AlternateRowBackgroud(int start, int end)
        {
            for (int i = start; i < end; i++)
                if (ActiveAuthorziedDeviceListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = PlaylistControl.GetRowBackground(i);
        }
    }
}
