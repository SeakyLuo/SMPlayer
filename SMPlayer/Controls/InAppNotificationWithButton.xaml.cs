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
        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }
        private string buttonText;

        public Action ButtonAction { get; set; }

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

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonAction.Invoke();
            ButtonNotification.Dismiss();
        }

        public void Show(string message, string buttonText, Action buttonAction, int duration = 2000)
        {
            Message = message;
            ButtonText = buttonText;
            ButtonAction = buttonAction;
            ButtonNotification.Show(duration);
        }
    }
}
