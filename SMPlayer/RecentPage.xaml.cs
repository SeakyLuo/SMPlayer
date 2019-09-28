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
        public RecentPage()
        {
            this.InitializeComponent();
            Setup(Models.Settings.settings.Recent);
            Models.Settings.settings.Recent.CollectionChanged += (sender, args) =>
            {
                GridMusicView.Setup(MusicLibraryPage.ConvertMusicPathToCollection((ICollection<string>)args.NewItems));
            };
        }

        private void Setup(ICollection<string> paths)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            try
            {
                GridMusicView.Setup(MusicLibraryPage.ConvertMusicPathToCollection(paths));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Local Music Page");
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Models.Settings.settings.Recent.Clear();
            Setup(Models.Settings.settings.Recent);
        }
    }
}
