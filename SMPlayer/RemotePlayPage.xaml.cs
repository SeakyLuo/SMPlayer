using SkiaSharp;
using SkiaSharp.QrCode;
using SkiaSharp.QrCode.Image;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RemotePlayPage : Page
    {
        private TcpListener tcpListener;

        public RemotePlayPage()
        {
            this.InitializeComponent();
            if (string.IsNullOrEmpty(Settings.settings.RemotePlayPassword))
            {
                UpdatePassword(GenRandomPassword());
            }
        }

        private static string GenRandomPassword()
        {
            char[] possibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            string password = "";
            for (int i = 0; i < 20; i++)
            {
                password += possibleChars[Helper.RandRange(possibleChars.Length)]; 
            }
            return password;
        }

        private async void LocalServiceToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (LocalServiceToggleSwitch.IsOn)
            {
                int port = 0823;
                string address = GetLocalServerAddress(port);
                UrlTextBox.Text = address;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    ShutdownLocalServer();
                    LaunchLocalServer(port);
                    QRCodeImage.Source = await ImageHelper.GenQRCode(address);
                });
            } 
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ShutdownLocalServer();
                });
            }
        }

        private static IPAddress GetIp()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            return ips.Length == 0 ? null : ips[0];
        }

        private static string GetLocalServerAddress(int port)
        {
            return $"http://{GetIp()}:{port}";
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            string deviceName = deviceInfo.FriendlyName;
            await new InputDialog()
            {
                Title = Helper.LocalizeText("ChangePassword"),
                PlaceholderText = Helper.LocalizeText("PasswordValidation"),
                InputText = Settings.settings.RemotePlayPassword,
                Validation = (password) =>
                {
                    Regex regex = new Regex(@"[a-zA-Z0-9]{4,30}");
                    if (regex.IsMatch(password)) return null;
                    return "PasswordValidation";
                },
                Confirm = UpdatePassword,
            }.ShowAsync();
        }

        private void UpdatePassword(string password)
        {
            Settings.settings.RemotePlayPassword = password;
            SettingsService.UpdateRemotePlayPassword(password);
        }

        private void AuthorizedDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.SetHeaderText("DeviceAuthorizations");
            MainPage.Instance.NavigateToPage(typeof(AuthorizedDevicePage));
        }

        private void CopyToClipBoardButton_Click(object sender, RoutedEventArgs e)
        {
            Helper.CopyStringToClipboard(UrlTextBox.Text);
            Helper.ShowNotification("CopySuccessful");
        }

        private async void OpenWebBroserButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(UrlTextBox.Text));
        }

        private void LaunchLocalServer(int port)
        {
            IPAddress ip = GetIp();
            tcpListener = new TcpListener(ip, port);
        }

        private void ShutdownLocalServer()
        {
            tcpListener?.Stop();
        }


    }
}
