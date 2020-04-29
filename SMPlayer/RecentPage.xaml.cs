using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool AddedModified = true, PlayedModifed = true, SearchedModified = true;
        private Dialogs.RemoveDialog dialog;
        private ObservableCollection<string> recentSearches { get => Settings.settings.RecentSearches; }
        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.settings.RecentAdded.CollectionChanged += (sender, args) => AddedModified = true;
            Settings.settings.RecentPlayed.CollectionChanged += (sender, args) => PlayedModifed = true;
            Settings.settings.RecentSearches.CollectionChanged += (sender, args) => SearchedModified = true;
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
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
                SetupPlayed(Settings.settings.RecentPlayed);
            else if (RecentPivot.SelectedItem == RecentSearchesItem)
                SetupSearched(Settings.settings.RecentSearches);
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        public void SetupAdded(ICollection<string> list)
        {
            if (!AddedModified) return;
            try
            {
                AddedMusicView.Setup(Settings.PathToCollection(list));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Recent Added");
            }
            AddedModified = false;
        }

        public void SetupPlayed(ICollection<string> list)
        {
            if (!PlayedModifed) return;
            try
            {
                PlayedMusicView.Setup(Settings.PathToCollection(list));
                ClearPlayHistoryAppButton.IsEnabled = list.Count != 0;
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Recent Played");
            }
            PlayedModifed = false;
        }
        private void ResetColor(int start)
        {
            for (int i = start; i < recentSearches.Count; i++)
                if (SearchHistoryListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = PlaylistControl.GetRowBackground(i);
        }
        public void SetupSearched(ICollection<string> list)
        {
            if (!SearchedModified) return;
            try
            {
                ResetColor(0);
                ClearSearchHistoryAppButton.IsEnabled = list.Count != 0;
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Recent Searches");
            }
            SearchedModified = false;
        }

        private void ClearPlayHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearPlayHistory", () =>
            {
                Settings.settings.RecentPlayed.Clear();
                SetupPlayed(Settings.settings.RecentPlayed);
            });
        }

        private void SearchHistoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            string keyword = e.ClickedItem.ToString();
            MainPage.Instance.SetSearchBarText(keyword);
            MainPage.Instance.Search(keyword);
        }

        private void SearchHistoryListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = PlaylistControl.GetRowBackground(args.ItemIndex);
        }

        private async void ItemRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            string item = (sender as Button).DataContext.ToString();
            if (dialog == null) dialog = new Dialogs.RemoveDialog();
            if (dialog.IsChecked)
            {
                RemoveSearchHistory(item);
            }
            else
            {
                dialog.Confirm = () => RemoveSearchHistory(item);
                dialog.Message = Helper.LocalizeMessage("RemoveItem", item);
                await dialog.ShowAsync();
            }
        }

        private void RemoveSearchHistory(string item)
        {
            int index = recentSearches.IndexOf(item);
            recentSearches.RemoveAt(index);
            ResetColor(index);
        }

        private void SearchHistoryListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetupSearched(Settings.settings.RecentSearches);
        }

        private void ClearSearchHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearSearchHistory", () =>
            {
                Settings.settings.RecentSearches.Clear();
                SetupSearched(Settings.settings.RecentSearches);
            });
        }

        private async void ShowYesNoDialog(string message, Action onYes)
        {
            var messageDialog = new MessageDialog(Helper.LocalizeMessage(message));
            messageDialog.Commands.Add(new UICommand(Helper.LocalizeMessage("Yes"), new UICommandInvokedHandler(command => onYes.Invoke())));
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
