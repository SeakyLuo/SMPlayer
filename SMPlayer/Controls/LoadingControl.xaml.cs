using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class LoadingControl : UserControl, INotifyPropertyChanged
    {
        public string Message
        {
            get => Message_;
            set
            {
                Message_ = value;
                OnPropertyChanged("Message");
            }
        }
        private string Message_ = "";

        public double Progress
        {
            get => Progress_;
            set
            {
                Progress_ = value;
                OnPropertyChanged("Progress");
            }
        }
        private double Progress_ { get; set; } = 0;


        public bool IsDeterminant
        {
            get => (bool)GetValue(IsDeterminantProperty);
            set => SetValue(IsDeterminantProperty, value);
        }
        public static readonly DependencyProperty IsDeterminantProperty = DependencyProperty.Register("IsDeterminant",
                                                                                                    typeof(bool),
                                                                                                    typeof(LoadingControl),
                                                                                                    new PropertyMetadata(false));
        public double Max
        {
            get => Max_;
            set
            {
                Max_ = value;
                OnPropertyChanged("Max");
            }
        }
        private double Max_ { get; set; } = 0;

        public bool AllowBreak
        {
            get => AllowBreak_;
            set
            {
                AllowBreak_ = value;
                OnPropertyChanged("AllowBreak");
            }
        }
        private bool AllowBreak_ = false;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public Action BreakLoadingListener { get; set; }
        public LoadingControl()
        {
            this.InitializeComponent();
        }

        public void SetMessage(string text, params object[] args)
        {
            Message = Helper.LocalizeMessage(text, args);
        }

        public void SetRawMessage(string message)
        {
            Message = message;
        }

        public void ShowDeterminant(string text, bool allowBreak = false, int max = 0)
        { 
            SetMessage(text);
            IsDeterminant = true;
            Progress = 0;
            Max = max;
            AllowBreak = allowBreak;
            this.Visibility = Visibility.Visible;
        }

        public void ShowIndeterminant(string text, bool allowBreak = false, Action action = null)
        {
            SetMessage(text);
            IsDeterminant = false;
            AllowBreak = allowBreak;
            this.Visibility = Visibility.Visible;
            if (action != null)
            {
                action.Invoke();
                Hide();
            }
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            BreakLoadingListener?.Invoke();
            Hide();
        }

        public void Increment(string text = null)
        {
            Progress++;
            if (!string.IsNullOrWhiteSpace(text))
            {
                SetMessage(text);
            }
        }


        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
