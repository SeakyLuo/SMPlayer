using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                ScrollTextBlock.Text = value + new string(' ', 10) + value;
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
        // Using 16ms because 60Hz is already good for human eyes.
        private readonly DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(16) };
        public bool IsScrolling { get => timer.IsEnabled; }
        public ScrollingTextBlock()
        {
            this.InitializeComponent();
            timer.Tick += (sender, e) =>
            {
                // Calculate the total offset to scroll. It is fixed after your text is set.
                // Since we need to "scroll to the "start" of the text,
                // the offset is equal to the length of your text plus the length of the space,
                // which is the difference of the ActualWidth of the two TextBlocks.
                double offset = ScrollTextBlock.ActualWidth - NormalTextBlock.ActualWidth;
                // Scroll it horizontally.
                // Notice the Math.Min here. You cannot scroll more than the offset.
                // " + 2" is just the distance it advances,
                // meaning that it also controls the speed of the animation.
                //RealScrollViewer.ScrollToHorizontalOffset(Math.Min(RealScrollViewer.HorizontalOffset + 2, offset));
                RealScrollViewer.ChangeView(Math.Min(RealScrollViewer.HorizontalOffset + 2, offset), null, null);
                // If scroll to the offset
                if (RealScrollViewer.HorizontalOffset == offset)
                    StopScrolling();
            };
        }

        public void StartScrolling()
        {
            // Checking timer.IsEnabled is for avoidance of restarting the animation when the text is already scrolling.
            // IsEnabled is true if timer has started, false if timer is stopped.
            // Checking TextScrollViewer.ScrollableWidth is for making sure the text is scrollable.
            if (timer.IsEnabled || TextScrollViewer.ScrollableWidth == 0) return;
            // Display this first so that user won't feel NormalTextBlock will be hidden.
            ScrollTextBlock.Visibility = Visibility.Visible;
            // Hide the NormalTextBlock so that it won't overlap with ScrollTextBlock when scrolling.
            NormalTextBlock.Visibility = Visibility.Collapsed;
            // Start the animation/ticking.
            timer.Start();
        }

        public void StopScrolling()
        {
            // Re-display the NormalTextBlock first so that the text won't blink because they overlap.
            NormalTextBlock.Visibility = Visibility.Visible;
            // Hide the ScrollTextBlock.
            // Hiding it will also set the HorizontalOffset of RealScrollViewer to 0,
            // so that RealScrollViewer will be scrolling from the beginning of ScrollTextBlock next time.
            //RealScrollViewer.ScrollToHorizontalOffset(0);
            //RealScrollViewer.ChangeView(0, null, null);
            ScrollTextBlock.Visibility = Visibility.Collapsed;
            // Stop the animation/ticking.
            timer.Stop();
        }
    }
}
