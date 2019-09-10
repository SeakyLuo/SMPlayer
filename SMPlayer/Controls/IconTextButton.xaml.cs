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
    public sealed partial class IconTextButton : UserControl
    {
        public string Label { get; set; }
        public IconElement Icon { get; set; }
        public IconTextButtonLabelPosition LabelPosition
        {
            get => labelPosition;
            set
            {
                Grid.SetColumn(LabelTextBlock, value == IconTextButtonLabelPosition.Left ? 0 : 2);
                labelPosition = value;
            }
        }
        private IconTextButtonLabelPosition labelPosition;

        public FlyoutBase Flyout { get; set; }
        public IconTextButton()
        {
            this.InitializeComponent();
        }

        private void Root_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.Flyout != null) Flyout.ShowAt(sender as FrameworkElement);
        }

        private void Root_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }

        private void Root_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }

    public enum IconTextButtonLabelPosition
    {
        Left = 0, Right = 1
    }
}
