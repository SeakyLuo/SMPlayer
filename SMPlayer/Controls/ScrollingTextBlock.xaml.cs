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
    public sealed partial class ScrollingTextBlock : UserControl
    {
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(16) };
        public ScrollingTextBlock()
        {
            this.InitializeComponent();
            timer.Tick += (ss, ee) =>
            {
                TextScrollViewer.ChangeView(TextScrollViewer.HorizontalOffset + 2, null, null);
                if (TextScrollViewer.HorizontalOffset == TextScrollViewer.ScrollableWidth)
                {
                    TextScrollViewer.ChangeView(0, null, null);
                    PointerOverTextBlock.Visibility = Visibility.Collapsed;
                    timer.Stop();
                }
            };
        }

        private void TextScrollViewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (timer.IsEnabled) return;
            PointerOverTextBlock.Visibility = Visibility.Visible;
            timer.Start();
        }
    }
}
