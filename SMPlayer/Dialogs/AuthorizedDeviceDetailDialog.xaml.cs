using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class AuthorizedDeviceDetailDialog : ContentDialog
    {
        private AuthorizedDeviceView Device;
        public AuthorizedDeviceDetailDialog(AuthorizedDeviceView Device)
        {
            this.InitializeComponent();
            this.Device = Device;
            IpTextBox.Text = Device.Ip;
            DeviceNameTextBox.Text = Device.DeviceName ?? "";
            DateCreatedTextBox.Text = Device.CreateTime.ToString();
            DateModifiedTextBox.Text = Device.UpdateTime.ToString();
            RemotePlayBlackListToggleSwitch.IsOn = !Device.IsAuthorized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newDeviceName = DeviceNameTextBox.Text;
            if (2 <= newDeviceName.Length && newDeviceName.Length <= 50)
            {
                Device.DeviceName = newDeviceName;
                AuthorizedDeviceService.UpdateAuthorization(Device.FromVO());
                DeviceNameErrorLabel.Visibility = DeviceNameErrorTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                DeviceNameErrorLabel.Visibility = DeviceNameErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveDialog dialog = new RemoveDialog(Helper.LocalizeMessage("DeleteAuthorizedDeviceHint"))
            {
                CheckBoxVisibility = Visibility.Collapsed,
                Confirm = () =>
                {
                    AuthorizedDeviceService.DeleteAuthorization(Device.FromVO());
                }
            };
        }

        private void RemotePlayBlackListToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Device.IsAuthorized = !RemotePlayBlackListToggleSwitch.IsOn;
            AuthorizedDeviceService.UpdateAuthorization(Device.FromVO());
        }
    }

}
