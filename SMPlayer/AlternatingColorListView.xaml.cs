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
    public sealed partial class AlternatingColorListView : UserControl
    {
        public Windows.UI.Color FirstColor { get => FirstColor; set => FirstColor = value; }
        public Windows.UI.Color SecondColor { get => SecondColor; set => SecondColor = value; }
        public DataTemplate ItemTemplate { get; set; }
        public object ItemsSource { get; set; }

        public AlternatingColorListView()
        {
            this.InitializeComponent();
        }

        private void MyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = new SolidColorBrush(args.ItemIndex % 2 == 0 ? FirstColor : SecondColor);
        }
    }
}
