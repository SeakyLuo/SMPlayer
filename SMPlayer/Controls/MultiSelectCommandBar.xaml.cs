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
    public sealed partial class MultiSelectCommandBar : UserControl, IMenuFlyoutItemClickListener
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
            PlayAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowPlay);
            AddToAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowAdd);
            RemoveAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowRemove);
            MoveToFolderAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowMoveToFolder);
            DeleteAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowDelete);
            ReverseSelectionAppButton.Visibility = VisibilityConverter.BoolToVisibility(option.ShowReverseSelection);
            SetButtonEnablity(false);
            CommandBarContainer.IsOpen = true;
        }

        public void CountSelections(int selections)
        {
            if (selections == 0)
            {
                CountSelectionTextBlock.Text = "";
                SetButtonEnablity(false);
            }
            else
            {
                CountSelectionTextBlock.Text = Helper.LocalizeText("ItemsSelected", selections);
                SetButtonEnablity(true);
            }
        }

        private void SetButtonEnablity(bool isEnabled)
        {
            PlayAppButton.IsEnabled = AddToAppButton.IsEnabled = RemoveAppButton.IsEnabled
                = MoveToFolderAppButton.IsEnabled = DeleteAppButton.IsEnabled = isEnabled;
        }

        public void Hide()
        {
            Cancel();
        }

        private void Close()
        {
            shouldOpen = false;
            CommandBarContainer.IsOpen = false;
            shouldOpen = true;
        }

        public void HideAfterOperation()
        {
            if (Settings.settings.HideMultiSelectCommandBarAfterOperation) Cancel();
        }

        private void Cancel()
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.ClearSelections));
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.Cancel));
            Close();
        }

        private void CancelAppButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void AddToAppButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper helper = new MenuFlyoutHelper();
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.AddTo) { FlyoutHelper = helper });
            helper.GetAddToMenuFlyout(this).ShowAt(sender as FrameworkElement);
        }

        private void PlayAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.Play));
            HideAfterOperation();
        }

        private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.Remove));
            HideAfterOperation();
        }

        private void MoveToFolderAppButton_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutHelper helper = new MenuFlyoutHelper();
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.MoveToFolder) { FlyoutHelper = helper });
            helper.GetMoveToFolderFlyout(listener: this).ShowAt(sender as FrameworkElement);
        }

        private void SelectAllAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.SelectAll));
        }

        private void ReverseSelectionAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.ReverseSelections));
        }

        private void ClearSelectionAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.ClearSelections));
            CountSelections(0);
        }

        private void DeleteAppButton_Click(object sender, RoutedEventArgs e)
        {
            MultiSelectListener?.Execute(this, new MultiSelectEventArgs(MultiSelectEvent.Delete));
        }

        void IMenuFlyoutItemClickListener.Execute(MenuFlyoutEventArgs args)
        {
            switch (args.Event)
            {
                case MenuFlyoutEvent.AddTo:
                case MenuFlyoutEvent.Favorite:
                case MenuFlyoutEvent.MoveToFolder:
                    HideAfterOperation();
                    break;
            }
        }
    }

    public interface IMultiSelectListener
    {
        void Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args);
    }

    public enum MultiSelectEvent
    {
        Cancel, AddTo, Play, Remove, Delete, MoveToFolder, SelectAll, ReverseSelections, ClearSelections,
    }

    public class MultiSelectEventArgs
    {
        public MultiSelectEvent Event { get; set; }
        public MenuFlyoutHelper FlyoutHelper { get; set; }
        public string TargetPath { get; set; }

        public MultiSelectEventArgs() { }
        public MultiSelectEventArgs(MultiSelectEvent Event)
        {
            this.Event = Event;
        }
    }
}
