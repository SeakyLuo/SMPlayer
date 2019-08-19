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
    public sealed partial class LoadingControl : UserControl
    {
        public bool IsLoading
        {
            get => MainControl.IsLoading;
            set => MainControl.IsLoading = value;
        }
        public string Text
        {
            get => LoadingTextBlock.Text;
            set => LoadingTextBlock.Text = value;
        }

        public Brush TextForeground
        {
            get => LoadingTextBlock.Foreground;
            set => LoadingTextBlock.Foreground = value;
        }

        public Brush ProgressForeground
        {
            get => LoadingProgressRing.Foreground;
            set => LoadingProgressRing.Foreground = value;
        }
        public LoadingControl()
        {
            this.InitializeComponent();
        }
    }
}
