using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class MultiSelectCommandBar : UserControl
    {
        public IMultiSelectListener MultiSelectListener { get; set; }
        public bool IsVisible { get => CommandBarContainer.IsOpen; }
        private bool shouldOpen = true;

        public MultiSelectCommandBar()
        {
            this.InitializeComponent();
            CommandBarContainer.Closing += (sender, e) =>
            {
                CommandBarContainer.IsOpen = shouldOpen;
            };
        }

        public void Show(MultiSelectCommandBarOption option = null)
        {
            if (option == null) option = new MultiSelectCommandBarOption();
            PlayAppButton.Visibility = option.ShowPlay ? Visibility.Visible : Visibility.Collapsed;
            AddToAppButton.Visibility = option.ShowAdd ? Visibility.Visible : Visibility.Collapsed;
            RemoveAppButton.Visibility = option.ShowRemove ? Visibility.Visible : Visibility.Collapsed;
            DeleteAppButton.Visibility = option.ShowDelete ? Visibility.Visible : Visibility.Collapsed;
            ReverseSelectionAppButton.Visibility = option.ShowReverseSelection ? Visibility.Visible : Visibility.Collapsed;
            CommandBarContainer.IsOpen = true;
        }

        public void Hide()
        {
            shouldOpen = false;
            CommandBarContainer.IsOpen = false;
            shouldOpen = true;
        }

        private void CancelAppButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            MultiSelectListener?.Cancel(this);
        }

        private void AddToAppButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper helper = new MenuFlyoutHelper();
            MultiSelectListener?.AddTo(this, helper);
            helper.GetAddToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        private void PlayAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Play(this);
        }

        private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Remove(this);
        }

        private void SelectAllAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.SelectAll(this);
        }

        private void ReverseSelectionAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.ReverseSelections(this);
        }

        private void ClearSelectionAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.ClearSelections(this);
        }

        private void DeleteAppButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public interface IMultiSelectListener
    {
        void Cancel(MultiSelectCommandBar commandBar);
        void AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper);
        void Play(MultiSelectCommandBar commandBar);
        void Remove(MultiSelectCommandBar commandBar);
        void SelectAll(MultiSelectCommandBar commandBar);
        void ReverseSelections(MultiSelectCommandBar commandBar);
        void ClearSelections(MultiSelectCommandBar commandBar);
    }
}
