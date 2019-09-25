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
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label",
                                                                                              typeof(string),
                                                                                              typeof(IconTextButton),
                                                                                              new PropertyMetadata(""));
        public IconElement Icon { get; set; }
        public IconTextButtonLabelPosition LabelPosition
        {
            get => (IconTextButtonLabelPosition)GetValue(LabelPositionProperty);
            set
            {
                SetValue(LabelPositionProperty, value);
                Grid.SetColumn(LabelTextBlock, value == IconTextButtonLabelPosition.Left ? 0 : 2);
            }
        }
        public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register("LabelPosition", 
                                                                                                      typeof(IconTextButtonLabelPosition), 
                                                                                                      typeof(IconTextButton), 
                                                                                                      new PropertyMetadata(IconTextButtonLabelPosition.Left));
        public Thickness IconTextMargin
        {
            get => (Thickness)GetValue(IconTextMarginProperty);
            set => SetValue(IconTextMarginProperty, value);
        }
        public static readonly DependencyProperty IconTextMarginProperty = DependencyProperty.Register("IconTextMargin",
                                                                                                       typeof(Thickness),
                                                                                                       typeof(IconTextButton),
                                                                                                       new PropertyMetadata(null));
        public Brush IconBackground
        {
            get { return (Brush)GetValue(IconBackgroundProperty); }
            set { SetValue(IconBackgroundProperty, value); }
        }
        public static readonly DependencyProperty IconBackgroundProperty = DependencyProperty.Register("IconBackground",
                                                                                              typeof(Brush),
                                                                                              typeof(IconTextButton),
                                                                                              new PropertyMetadata(null));

        public double IconRadius
        {
            get => (double)GetValue(IconRadiusProperty);
            set => SetValue(IconRadiusProperty, value);
        }
        public static readonly DependencyProperty IconRadiusProperty = DependencyProperty.Register("IconRadius",
                                                                                              typeof(double),
                                                                                              typeof(IconTextButton),
                                                                                              new PropertyMetadata(30d));

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
