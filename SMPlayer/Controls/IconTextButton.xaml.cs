﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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
        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon",
                                                                                      typeof(IconElement),
                                                                                      typeof(IconTextButton),
                                                                                      new PropertyMetadata(null));
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

        public bool IsHighlightAll
        {
            get => (bool)GetValue(IsHighlightAllProperty);
            set => SetValue(IsHighlightAllProperty, value);
        }
        public static readonly DependencyProperty IsHighlightAllProperty = DependencyProperty.Register("IsHighlightAll",
                                                                                                             typeof(bool),
                                                                                                             typeof(IconTextButton),
                                                                                                             new PropertyMetadata(false));

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
            VisualStateManager.GoToState(this, IsHighlightAll ? "PointerOverHighlightAll" : "PointerOver", true);
        }

        private void Root_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, IsHighlightAll ? "PressedHighlightAll" : "Pressed", true);
        }
    }

    public enum IconTextButtonLabelPosition
    {
        Left = 0, Right = 1
    }
}
