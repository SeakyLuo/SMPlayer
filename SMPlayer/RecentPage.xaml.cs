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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RecentPage : Page
    {
        private bool Modified = true;
        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Models.Settings.settings.Recent.CollectionChanged += (sender, args) =>
            {
                Modified = true;
            };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Setup(Models.Settings.settings.Recent);
        }

        private async void Setup(ICollection<string> paths)
        {
            if (!Modified) return;
            LoadingProgressBar.Visibility = Visibility.Visible;
            try
            {
                await GridMusicView.Setup(MusicLibraryPage.ConvertMusicPathToCollection(paths));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Local Music Page");
            }
            Modified = false;
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Models.Settings.settings.Recent.Clear();
        }
    }
}
