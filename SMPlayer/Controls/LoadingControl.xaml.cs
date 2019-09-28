using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
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
    public sealed partial class LoadingControl : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
                                                                                              typeof(string),
                                                                                              typeof(LoadingControl),
                                                                                              new PropertyMetadata("Loading..."));
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
                                                                                                 typeof(double),
                                                                                                 typeof(LoadingControl),
                                                                                                 new PropertyMetadata(0d));

        public bool IsDeterminant { get; set; }
        public double Max
        {
            get => (double)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max",
                                                                                        typeof(double),
                                                                                        typeof(LoadingControl),
                                                                                        new PropertyMetadata(0d));
        public LoadingControl()
        {
            this.InitializeComponent();
        }

        public void StartLoading()
        {
            this.Visibility = Visibility.Visible;
        }

        public void FinishLoading()
        {
            this.Visibility = Visibility.Visible;
        }
    }
}
