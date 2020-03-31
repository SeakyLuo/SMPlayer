using SMPlayer.Models;
using System;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RecentPage : Page
    {
        private bool AddedModified = true, PlayedModifed = true;
        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.settings.RecentAdded.CollectionChanged += (sender, args) => AddedModified = true;
            Settings.settings.RecentPlayed.CollectionChanged += (sender, args) => PlayedModifed = true;
            if (Settings.settings.RecentAdded.Count == 0 && Settings.settings.RecentPlayed.Count > 0)
                RecentPivot.SelectedItem = RecentPlayedItem;
            else
                RecentPivot.SelectedItem = RecentAddedItem;
        }

        private void RecentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            if (RecentPivot.SelectedItem == RecentAddedItem)
                SetupAdded(Settings.settings.RecentAdded);
            else
                SetupPlayed(Settings.settings.RecentPlayed);
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        public void SetupAdded(ICollection<string> paths)
        {
            if (!AddedModified) return;
            try
            {
                AddedMusicView.Setup(Settings.PathToCollection(paths));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Recent Added");
            }
            AddedModified = false;
        }

        public void SetupPlayed(ICollection<string> paths)
        {
            if (!PlayedModifed) return;
            try
            {
                PlayedMusicView.Setup(Settings.PathToCollection(paths));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Recent Played");
            }
            PlayedModifed = false;
        }

        private async void ClearHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog(Helper.LocalizeMessage("ClearHistory"));
            messageDialog.Commands.Add(new UICommand(Helper.LocalizeMessage("Yes"), new UICommandInvokedHandler(command => Settings.settings.RecentPlayed.Clear())));
            messageDialog.Commands.Add(new UICommand(Helper.LocalizeMessage("No")));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }
    }
}
