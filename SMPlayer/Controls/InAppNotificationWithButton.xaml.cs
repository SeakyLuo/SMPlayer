using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace SMPlayer.Controls
{
    public sealed partial class InAppNotificationWithButton : UserControl, INotifyPropertyChanged
    {
        public string Message
        {
            get => message;
            set 
            {
                message = value;
                OnPropertyChanged();
            }
        }
        private string message;
        public string Button1Text
        {
            get => button1Text;
            set
            {
                button1Text = value;
                OnPropertyChanged();
            }
        }
        private string button1Text;
        public string Button2Text
        {
            get => button2Text;
            set
            {
                button2Text = value;
                OnPropertyChanged();
            }
        }
        private string button2Text;

        public Action<InAppNotificationWithButton> Button1Action { get; set; }
        public Action<InAppNotificationWithButton> Button2Action { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public InAppNotificationWithButton()
        {
            this.InitializeComponent();
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Button1Action?.Invoke(this);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Button2Action?.Invoke(this);
        }

        public void Show(string message, string buttonText, Action<InAppNotificationWithButton> buttonAction, int duration = 2000)
        {
            Message = message;
            Button1Text = buttonText;
            Button1Action = buttonAction;
            Button2Text = null;
            Button2Action = null;
            ButtonNotification.Show(duration);
        }

        public void Show(string message, string button1Text, Action<InAppNotificationWithButton> button1Action, string button2Text, Action<InAppNotificationWithButton> button2Action, int duration = 2000)
        {
            Message = message;
            Button1Text = button1Text;
            Button1Action = button1Action;
            Button2Text = button2Text;
            Button2Action = button2Action;
            ButtonNotification.Show(duration);
        }

        public void Dismiss()
        {
            ButtonNotification.Dismiss();
        }
    }
}
