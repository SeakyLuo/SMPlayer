using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ObservableCollection<MusicTimeLine> RecentAddedTimeLineList = new ObservableCollection<MusicTimeLine>();
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

        public async Task SetupAdded(IEnumerable<string> list)
        {
            if (!AddedModified) return;
            RecentAddedProgressRing.IsActive = true;
            try
            {
                RecentAddedTimeLineList.SetTo(await Task.Run(() =>
                {
                    List<Music> songs = Settings.PathToCollection(list);
                    return GenerateTimeLine(songs);
                }));
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                Debug.WriteLine("InvalidOperationException On Recent Added");
            }
            RecentAddedProgressRing.IsActive = AddedModified = false;
        }

        private static List<MusicTimeLine> GenerateTimeLine(List<Music> songs)
        {
            List<MusicTimeLine> ret = new List<MusicTimeLine>();
            MusicTimeLine today = new MusicTimeLine("Today");
            MusicTimeLine thisWeek = new MusicTimeLine("ThisWeek");
            MusicTimeLine thisMonth = new MusicTimeLine("ThisMonth");
            MusicTimeLine thisYear = new MusicTimeLine("ThisYear");
            Dictionary<int, MusicTimeLine> yearDict = new Dictionary<int, MusicTimeLine>();
            
            foreach (var music in songs)
            {
                if (music.DateAdded.Year == DateTime.Now.Year)
                {
                    if (music.DateAdded.Month == DateTime.Now.Month)
                    {
                        if (music.DateAdded.Day == DateTime.Now.Day)
                        {
                            today.AddItem(music);
                        }
                        else if (music.DateAdded.DayOfWeek <= DateTime.Now.DayOfWeek)
                        {
                            thisWeek.AddItem(music);
                        }
                        else
                        {
                            thisMonth.AddItem(music);
                        }
                    }
                    else
                    {
                        thisYear.AddItem(music);
                    }
                }
                else
                {
                    MusicTimeLine yearTimeLine = yearDict.GetValueOrDefault(music.DateAdded.Year, new MusicTimeLine(music.DateAdded.Year));
                    yearTimeLine.AddItem(music);
                    yearDict[music.DateAdded.Year] = yearTimeLine;
                }
            }
            ret.JoinTimeLine(today, thisWeek, thisMonth, thisYear);
            if (yearDict.Count > 0)
            {
                List<int> years = new List<int>(yearDict.Keys);
                years.Sort((i1, i2) => i2 - i1);
                foreach (var year in years)
                {
                    ret.Add(yearDict[year]);
                }
            }
            return ret;
        }

        public async Task SetupPlayed(IEnumerable<string> list)
        {
            if (!PlayedModifed) return;
            RecentPlayedProgressRing.IsActive = true;
            try
            {
                PlayedMusicView.Setup(await Task.Run(() => Settings.PathToCollection(list)));
                ClearPlayHistoryAppButton.IsEnabled = list.Count() != 0;
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                Debug.WriteLine("InvalidOperationException On Recent Played");
            }
            RecentPlayedProgressRing.IsActive = PlayedModifed = false;
        }

        private void ResetColor(int start)
        {
            for (int i = start; i < recentSearches.Count; i++)
                if (SearchHistoryListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = PlaylistControl.GetRowBackground(i);
        }

        public void SetupSearched(IEnumerable<string> list)
        {
            if (!SearchedModified) return;
            try
            {
                ResetColor(0);
                ClearSearchHistoryAppButton.IsEnabled = list.Count() != 0;
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                Debug.WriteLine("InvalidOperationException On Recent Searches");
            }
            SearchedModified = false;
        }

        private void ClearPlayHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearPlayHistory", async () =>
            {
                Settings.settings.RecentPlayed.Clear();
                await SetupPlayed(Settings.settings.RecentPlayed);
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

        private void ItemRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            AskRemoveSearchHistory((sender as Button).DataContext.ToString());
        }

        private async void AskRemoveSearchHistory(string item)
        {
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
            MainPage.Instance.ShowUndoNotification(Helper.LocalizeMessage("ItemRemoved", item), () =>
            {
                recentSearches.Insert(index, item);
                ResetColor(index);
            });
        }

        private void SearchHistoryListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetupSearched(recentSearches);
        }

        private void RemoveItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            AskRemoveSearchHistory(args.SwipeControl.DataContext.ToString());
        }

        private void AddedMusicView_Loaded(object sender, RoutedEventArgs e)
        {
            GridMusicControl control = sender as GridMusicControl;
            control.Setup((control.DataContext as MusicTimeLine).Data);
        }

        private async void RecentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (RecentPivot.SelectedItem == RecentAddedItem)
                await SetupAdded(Settings.settings.RecentAdded);
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
                await SetupPlayed(Settings.settings.RecentPlayed);
            else if (RecentPivot.SelectedItem == RecentSearchesItem)
                SetupSearched(recentSearches);
        }

        private async void RecentAddedGrid_Loaded(object sender, RoutedEventArgs e)
        {
            await SetupAdded(Settings.settings.RecentAdded);
        }

        private async void RecentPlayed_Loaded(object sender, RoutedEventArgs e)
        {
            await SetupPlayed(Settings.settings.RecentPlayed);
        }

        private void ClearSearchHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearSearchHistory", () =>
            {
                recentSearches.Clear();
                SetupSearched(recentSearches);
            });
        }

        private async void ShowYesNoDialog(string message, Action onYes)
        {
            var messageDialog = new MessageDialog(Helper.LocalizeMessage(message));
            messageDialog.Commands.Add(new UICommand(Helper.LocalizeMessage("Yes"), new UICommandInvokedHandler(command => 
            {
                onYes.Invoke();
            })));
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
