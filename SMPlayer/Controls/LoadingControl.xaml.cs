using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
            get => (double)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }
        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max",
                                                                                        typeof(double),
                                                                                        typeof(LoadingControl),
                                                                                        new PropertyMetadata(0d));

        public bool AllowBreak
        {
            get => (bool)GetValue(AllowBreakProperty);
            set => SetValue(AllowBreakProperty, value);
        }
        public static readonly DependencyProperty AllowBreakProperty = DependencyProperty.Register("AllowBreak",
                                                                                        typeof(bool),
                                                                                        typeof(LoadingControl),
                                                                                        new PropertyMetadata(false));
        public List<Action> BreakLoadingListeners = new List<Action>();
        public LoadingControl()
        {
            this.InitializeComponent();
        }

        public void SetLocalizedText(string text, params object[] args)
        {
            Text = Helper.LocalizeMessage(text, args);
        }

        public void ShowDeterminant(string text, bool allowBreak = false, int progress = 0, Action action = null)
        { 
            SetLocalizedText(text);
            IsDeterminant = true;
            Progress = progress;
            AllowBreak = allowBreak;
            this.Visibility = Visibility.Visible;
            if (action != null)
            {
                action.Invoke();
                Hide();
            }
        }

        public void ShowIndeterminant(string text, bool allowBreak = false, Action action = null)
        {
            SetLocalizedText(text);
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
            foreach (var listener in BreakLoadingListeners) listener.Invoke();
            Hide();
        }
    }
}
