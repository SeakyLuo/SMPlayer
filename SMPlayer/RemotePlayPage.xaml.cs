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
        private Socket socket;

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
                    //LaunchLocalServer(port);
                    QRCodeImage.Source = await ImageHelper.GenQRCode(address);
                });
            } 
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //ShutdownLocalServer();
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
            TcpListener listener = new TcpListener(ip, port);
            IPEndPoint endpoint = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[0], port);
            //创建服务端负责监听的套接字，参数（使用IPV4协议，使用流式连接，使用TCO协议传输数据）
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            if (socket.Connected)
            {
                Log.Info(socket.RemoteEndPoint + "连接成功");
            }
        }

        private void ShutdownLocalServer()
        {
            if (socket == null) return;
            socket.Close();
        }

        /// <summary>
        /// 向远程主机发送数据
        /// </summary>
        /// <param name="socket">要发送数据且已经连接到远程主机的 Socket</param>
        /// <param name="buffer">待发送的数据</param>
        /// <param name="outTime">发送数据的超时时间，以秒为单位，可以精确到微秒</param>
        /// <returns>0:发送数据成功；-1:超时；-2:发送数据出现错误；-3:发送数据时出现异常</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要发送
        /// </remarks>
        public static int SendData(Socket socket, byte[] buffer, int outTime)
        {
            if (socket == null || socket.Connected == false)
            {
                throw new ArgumentException("参数socket 为null，或者未连接到远程计算机");
            }
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("参数buffer 为null ,或者长度为 0");
            }

            int flag = 0;
            try
            {
                int left = buffer.Length;
                int sndLen = 0;

                while (true)
                {
                    if ((socket.Poll(outTime * 100, SelectMode.SelectWrite) == true))
                    {        // 收集了足够多的传出数据后开始发送
                        sndLen = socket.Send(buffer, sndLen, left, SocketFlags.None);
                        left -= sndLen;
                        if (left == 0)
                        {                                        // 数据已经全部发送
                            flag = 0;
                            break;
                        }
                        else
                        {
                            if (sndLen > 0)
                            {                                    // 数据部分已经被发送
                                continue;
                            }
                            else
                            {                                                // 发送数据发生错误
                                flag = -2;
                                break;
                            }
                        }
                    }
                    else
                    {                                                        // 超时退出
                        flag = -1;
                        break;
                    }
                }
            }
            catch (SocketException e)
            {

                flag = -3;
            }
            return flag;
        }


        /// <summary>
        /// 向远程主机发送文件
        /// </summary>
        /// <param name="socket" >要发送数据且已经连接到远程主机的 socket</param>
        /// <param name="fileName">待发送的文件名称</param>
        /// <param name="maxBufferLength">文件发送时的缓冲区大小</param>
        /// <param name="outTime">发送缓冲区中的数据的超时时间</param>
        /// <returns>0:发送文件成功；-1:超时；-2:发送文件出现错误；-3:发送文件出现异常；-4:读取待发送文件发生错误</returns>
        /// <remarks >
        /// 当 outTime 指定为-1时，将一直等待直到有数据需要发送
        /// </remarks>
        public static int SendFile(Socket socket, string fileName, int maxBufferLength, int outTime)
        {
            if (fileName == null || maxBufferLength <= 0)
            {
                throw new ArgumentException("待发送的文件名称为空或发送缓冲区的大小设置不正确.");
            }
            int flag = 0;
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                long fileLen = fs.Length;                        // 文件长度
                long leftLen = fileLen;                            // 未读取部分
                int readLen = 0;                                // 已读取部分
                byte[] buffer = null;

                if (fileLen <= maxBufferLength)
                {            /* 文件可以一次读取*/
                    buffer = new byte[fileLen];
                    readLen = fs.Read(buffer, 0, (int)fileLen);
                    flag = SendData(socket, buffer, outTime);
                }
                else
                {
                    /* 循环读取文件,并发送 */

                    while (leftLen != 0)
                    {
                        if (leftLen < maxBufferLength)
                        {
                            buffer = new byte[leftLen];
                            readLen = fs.Read(buffer, 0, Convert.ToInt32(leftLen));
                        }
                        else
                        {
                            buffer = new byte[maxBufferLength];
                            readLen = fs.Read(buffer, 0, maxBufferLength);
                        }
                        if ((flag = SendData(socket, buffer, outTime)) < 0)
                        {
                            break;
                        }
                        leftLen -= readLen;
                    }
                }
                fs.Flush();
                fs.Close();
            }
            catch (IOException e)
            {

                flag = -4;
            }
            return flag;
        }

    }
}
