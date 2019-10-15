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
    public sealed partial class MiniModePage : Page, MediaControlContainer
    {
        public MiniModePage()
        {
            this.InitializeComponent();
        }

        public void PauseMusic()
        {
            MiniMediaControl.PauseMusic();
        }

        public void ShowNotification(string message, int duration = 1500) { }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MiniMediaControl.Update();
        }
    }
}
