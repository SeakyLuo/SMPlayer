using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class LoadingControl : UserControl, INotifyPropertyChanged
    {
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }
        private string _message = "";

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
        private double _progress { get; set; } = 0;


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
            get => _max;
            set
            {
                _max = value;
                OnPropertyChanged("Max");
            }
        }
        private double _max { get; set; } = 0;

        public bool AllowBreak
        {
            get => _allowBreak;
            set
            {
                _allowBreak = value;
                OnPropertyChanged("AllowBreak");
            }
        }
        private bool _allowBreak = false;

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

        public async Task SetMessageAsync(string text, params object[] args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SetMessage(text, args));
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

        public async Task IncrementAsync(string message = null)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => Increment(message));
        }

        public void Increment(string message = null)
        {
            if (!IsDeterminant) return;
            Progress++;
            if (!string.IsNullOrWhiteSpace(message))
            {
                SetRawMessage(message);
            }
        }

        public async Task ResetAsync(string message = null, int max = 0)
        {
            Log.Info($"ResetAsync {message}");
            await Helper.RunInMainUIThread(Dispatcher, () => Reset(message, max));
        }

        public void Reset(string message = null, int max = 0)
        {
            Progress = 0;
            Max = max;
            if (!string.IsNullOrWhiteSpace(message))
            {
                SetMessage(message);
            }
            if (max > 0)
            {
                IsDeterminant = true;
            }
        }


        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
