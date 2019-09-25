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
        public string Text
        {
            get => NormalTextBlock.Text;
            set
            {
                NormalTextBlock.Text = value;
                PointerOverTextBlock.Text = value + new string(' ', 10) + value;
            }
        }
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment",
                                                                                                      typeof(TextAlignment),
                                                                                                      typeof(ScrollingTextBlock),
                                                                                                      new PropertyMetadata(TextAlignment.Start));
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(16) };
        public bool IsScrolling { get => timer.IsEnabled; }
        public ScrollingTextBlock()
        {
            this.InitializeComponent();
            timer.Tick += (sender, e) =>
            {
                double distance = ScrollTextBlock.ActualWidth - NormalTextBlock.ActualWidth;
                RealScrollViewer.ChangeView(Math.Min(RealScrollViewer.HorizontalOffset + 2, distance), null, null);
                if (RealScrollViewer.HorizontalOffset == distance)
                {
                    NormalTextBlock.Visibility = Visibility.Visible;
                    ScrollTextBlock.Visibility = Visibility.Collapsed;
                    timer.Stop();
                }
            };
        }

        public void StartScrolling()
        {
            if (timer.IsEnabled || TextScrollViewer.ScrollableWidth == 0) return;
            ScrollTextBlock.Visibility = Visibility.Visible;
            NormalTextBlock.Visibility = Visibility.Collapsed;
            timer.Start();
        }
    }
}
