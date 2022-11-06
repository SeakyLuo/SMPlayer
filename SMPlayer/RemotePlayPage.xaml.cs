using Newtonsoft.Json;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Networking.Sockets;
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
        private HttpListener httpListener;

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
                int port = 8023;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        string address = GetLocalServerAddress(port);
                        UrlTextBox.Text = address;
                        ShutdownLocalServer();
                        LaunchLocalServer(port);
                        QRCodeImage.Source = await ImageHelper.GenQRCode(address);

                        LocalServiceToggleSwitch.OnContent = Helper.LocalizeMessage("LocalServiceToggleSwitchLaunchSuccessful");
                        SetUrlPanelVisibility(Visibility.Visible);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"turn on local server failed {ex}");
                        LocalServiceToggleSwitch.OnContent = Helper.LocalizeMessage("LocalServiceToggleSwitchLaunchExceptional", ex);
                        SetUrlPanelVisibility(Visibility.Collapsed);
                        Helper.ShowNotification("LocalServiceToggleSwitchLaunchExceptionalNotification");
                    }
                });
            } 
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ShutdownLocalServer();
                    SetUrlPanelVisibility(Visibility.Collapsed);
                });
            }
        }

        private void SetUrlPanelVisibility(Visibility visibility)
        {
            RemotePlayConnectTextBlock.Visibility = QRCodeImage.Visibility
                                                  = UrlStackPanel.Visibility
                                                  = visibility;
        } 

        private static IPAddress GetIp()
        {
            try
            {
                string responseText = "";
                string url = "http://checkip.dyndns.org/";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PROPFIND";
                request.ContentType = "application/x-www-form-urlencoded;charset:utf-8";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        responseText = streamReader.ReadToEnd().ToString();
                    }
                }
                //Search for the ip in the html
                int first = responseText.IndexOf("Address: ") + 9;
                int last = responseText.LastIndexOf("</body>");
                return IPAddress.Parse(responseText.Substring(first, last - first));
            }
            catch (Exception e)
            {
                Log.Warn($"get ip failed {e}");
                throw e;
            }
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

        //private void LaunchLocalServer(int port)
        //{
        //    IPAddress ip = GetIp();
        //    tcpListener = new TcpListener(ip, port);
        //}

        private void LaunchLocalServer(int port)
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), httpListener);
        }

        private void GetContextCallBack(IAsyncResult ar)
        {
            try
            {
                HttpListener _listener = ar.AsyncState as HttpListener;
                if (_listener.IsListening)
                {
                    return;
                }

                HttpListenerContext context = _listener.EndGetContext(ar);
                _listener.BeginGetContext(new AsyncCallback(GetContextCallBack), _listener);

                #region 解析Request请求

                HttpListenerRequest request = context.Request;
                Uri url = request.Url;
                string content = "";
                switch (request.HttpMethod)
                {
                    case "POST":
                        {
                            Stream stream = context.Request.InputStream;
                            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                            content = reader.ReadToEnd();
                        }
                        break;
                    case "GET":
                        {
                            var data = request.QueryString;
                        }
                        break;
                }

                #endregion

                #region 构造Response响应

                HttpListenerResponse response = context.Response;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json;charset=UTF-8";
                //response.ContentType = "text/html; Charset=UTF-8";
                response.ContentEncoding = Encoding.UTF8;
                response.AppendHeader("Content-Type", "application/json;charset=UTF-8");

                //模拟返回的数据：Json格式
                //string responseBody = "<HTML><BODY> Hello world!</BODY></HTML>";
                var abcOject = new
                {
                    code = "200",
                    description = "success",
                    data = "time=" + DateTime.Now
                };
                string responseString = JsonConvert.SerializeObject(abcOject,
                    new JsonSerializerSettings()
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                    });

                using (StreamWriter writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
                {
                    writer.Write(responseString);
                    writer.Close();
                    response.Close();
                }

                #endregion
            }
            catch (Exception ex)
            {
                Log.Warn($"handle response failed {ex}");
            }
        }

        private void ShutdownLocalServer()
        {
            try
            {
                tcpListener?.Stop();
                httpListener?.Stop();
            }
            catch (Exception e)
            {
                Log.Warn($"ShutdownLocalServer failed {e}");
            }
        }

    }
}
