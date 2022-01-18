using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.VO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
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
    public sealed partial class RecentPage : Page, IMultiSelectListener, IMenuFlyoutHelperBuildListener, IMusicEventListener, IRecentEventListener
    {
        public static RecentTimeLine RecentAdded;
        private const string JsonFileName = "RecentAddedTimeLine";
        private static bool AddedModified = true, PlayedModifed = true, SearchModified = true;
        private readonly ObservableCollection<string> RecentSearches = new ObservableCollection<string>();
        private RemoveDialog recentPlayedRemoveDialog, recentSearchesRemoveDialog;
        private string CurrentCategory;
        private RecentType? CurrentMultiSelectItem;

        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.AddRecentEventListener(this);

            AddedMusicView.TopItemEffectiveViewportChanged += (sender, args) =>
            {
                if (sender.DataContext is GridViewMusic music)
                {
                    string category = RecentTimeLine.Categorize(music.Source.DateAdded);
                    if (category != CurrentCategory)
                    {
                        CurrentCategory = category;
                        RecentAddedHeader.Text = Helper.LocalizeText(category);
                    }
                }
            };
            PlayedMusicView.MultiSelectListener = this;
            PlayedMusicView.MenuFlyoutHelperBuildListener = this;
            PlayedMusicView.MenuFlyoutOpeningOption = new MenuFlyoutOption()
            {
                ShowRemove = true
            };
        }

        public static void Save()
        {
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (RecentAdded == null)
            {
                //List<Music> recentAdded = await JsonFileHelper.ReadObjectAsync<List<Music>>(JsonFileName);
                //if (recentAdded.IsEmpty()) recentAdded = Settings.AllSongs.OrderByDescending(m => m.DateAdded).ToList();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RecentAdded = RecentTimeLine.FromMusicList(Settings.AllSongs);
                    SetupAdded();
                });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string selectedItem && selectedItem == "RecentAdded")
            {
                RecentPivot.SelectedItem = RecentAddedItem;
            }
        }

        private void SetMultiSelectListener()
        {
            if (CurrentMultiSelectItem == null) return;
            switch (CurrentMultiSelectItem)
            {
                case RecentType.Add:
                    MainPage.Instance.SetMultiSelectListener(AddedMusicView);
                    break;
                case RecentType.Play:
                    MainPage.Instance.SetMultiSelectListener(PlayedMusicView);
                    break;
                case RecentType.Search:
                    MainPage.Instance.SetMultiSelectListener(this);
                    break;
            }
        }

        public void SetupAdded()
        {
            if (RecentAdded == null || !AddedModified) return;
            RecentAddedProgressRing.IsActive = true;
            ObservableCollection<Music> list = RecentAdded.TimeLine;
            SetupAddedButtonState(list);
            AddedMusicView.Setup(list);
            RecentAddedProgressRing.IsActive = AddedModified = false;
        }

        private void SetupAddedButtonState(ObservableCollection<Music> list)
        {
            RecentAddedMultiSelectAppButton.IsEnabled = list.IsNotEmpty();
        }

        public void SetupPlayed()
        {
            if (!PlayedModifed) return;
            RecentPlayedProgressRing.IsActive = true;
            IEnumerable<Music> list = Settings.RecentPlay;
            SetupPlayedButtonState(list);
            PlayedMusicView.Setup(list);
            RecentPlayedProgressRing.IsActive = PlayedModifed = false;
        }

        private void SetupPlayedButtonState(IEnumerable<Music> list)
        {
            RecentPlayedMultiSelectAppButton.IsEnabled = list.IsNotEmpty();
            ClearPlayHistoryAppButton.IsEnabled = list.IsNotEmpty();
        }

        private void ResetSearchHistoryRowColor(int start = 0)
        {
            for (int i = start; i < RecentSearches.Count; i++)
                if (SearchHistoryListView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = PlaylistControl.GetRowBackground(i);
        }

        public void SetupSearched()
        {
            if (!SearchModified) return;
            ResetSearchHistoryRowColor();
            IEnumerable<string> list = Settings.RecentSearch;
            RecentSearches.SetTo(list);
            SetupSearchedButtonState(list);
            SearchModified = false;
        }

        private void SetupSearchedButtonState(IEnumerable<string> list)
        {
            RecentPlayedMultiSelectAppButton.IsEnabled = list.IsNotEmpty();
            ClearPlayHistoryAppButton.IsEnabled = list.IsNotEmpty();
        }

        private void SearchHistoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SearchHistoryListView.SelectionMode != ListViewSelectionMode.None) return;
            string keyword = e.ClickedItem.ToString();
            MainPage.Instance.SetSearchBarText(keyword);
            MainPage.Instance.Search(keyword);
        }

        private void SearchHistoryListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = PlaylistControl.GetRowBackground(args.ItemIndex);
        }

        private async void AskRemoveRecentPlayed(object item)
        {
            if (recentPlayedRemoveDialog == null) recentPlayedRemoveDialog = new RemoveDialog();
            GridViewMusic music = null;
            if (item is GridViewMusic gridViewMusic)
            {
                music = gridViewMusic;
            }
            else if (item is List<object> list && list.Count() == 1)
            {
                music = list.ElementAt(0) as GridViewMusic;
            }
            if (recentPlayedRemoveDialog.IsChecked)
            {
                if (music == null)
                {
                    RemoveRecentPlayed(item);
                }
                else
                {
                    RemoveRecentPlayed(music);
                }
            }
            else
            {
                if (music == null)
                {
                    recentPlayedRemoveDialog.Confirm = () => RemoveRecentPlayed(item);
                    recentPlayedRemoveDialog.Message = Helper.LocalizeMessage("RemoveItems", PlayedMusicView.SelectedItems.Count);
                }
                else
                {
                    recentPlayedRemoveDialog.Confirm = () => RemoveRecentPlayed(music);
                    recentPlayedRemoveDialog.Message = Helper.LocalizeMessage("RemoveItem", music.Name);
                }
                await recentPlayedRemoveDialog.ShowAsync();
            }
        }

        private async void AskRemoveSearchHistory(object item)
        {
            if (recentSearchesRemoveDialog == null) recentSearchesRemoveDialog = new RemoveDialog();
            string keyword = null;
            if (item is string record)
            {
                keyword = record;
            }
            else if (item is List<object> list && list.Count() == 1)
            {
                keyword = list.ElementAt(0) as string;
            }
            if (recentSearchesRemoveDialog.IsChecked)
            {
                if (keyword == null)
                {
                    RemoveRecentSearches(item);
                }
                else
                {
                    RemoveSearchHistory(keyword);
                }
            }
            else
            {
                if (keyword == null)
                {
                    recentSearchesRemoveDialog.Confirm = () => RemoveRecentSearches(item);
                    recentSearchesRemoveDialog.Message = Helper.LocalizeMessage("RemoveItems", SearchHistoryListView.SelectedItems.Count);

                }
                else
                {
                    recentSearchesRemoveDialog.Confirm = () => RemoveSearchHistory(keyword);
                    recentSearchesRemoveDialog.Message = Helper.LocalizeMessage("RemoveItem", keyword);
                }
                await recentSearchesRemoveDialog.ShowAsync();
            }
        }

        private void RemoveRecentPlayed(GridViewMusic item)
        {
            PlayedMusicView.RemoveMusic(item.Source);
            Settings.settings.RemoveRecentPlayed(item.Source);
            SetupPlayedButtonState(PlayedMusicView.MusicCollection);
            MainPage.Instance.ShowUndoableNotification(Helper.LocalizeMessage("ItemRemoved", item.Name), () =>
            {
                PlayedMusicView.UndoDelete(item.Source);
                Settings.settings.UndoRemoveRecentPlayed(item.Source);
            });
        }

        private void RemoveSearchHistory(string item)
        {
            int index = RecentSearches.IndexOf(item);
            RecentSearches.RemoveAt(index);
            ResetSearchHistoryRowColor(index);
            Settings.settings.RemoveSearchHistory(item);
            SetupSearchedButtonState(RecentSearches);
            MainPage.Instance.ShowUndoableNotification(Helper.LocalizeMessage("ItemRemoved", item), () =>
            {
                RecentSearches.Insert(index, item);
                ResetSearchHistoryRowColor(index);
                Settings.settings.UndoRemoveSearchHistory(item);
                SetupSearchedButtonState(RecentSearches);
            });
        }

        private void SearchHistoryListView_Loaded(object sender, RoutedEventArgs e)
        {
            SetupSearched();
        }

        private void RemoveItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            AskRemoveSearchHistory(args.SwipeControl.DataContext.ToString());
        }

        private void RecentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (RecentPivot.SelectedItem == RecentAddedItem)
            {
                SetupAdded();
            }
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                SetupPlayed();
            }
            else if (RecentPivot.SelectedItem == RecentSearchesItem)
            {
                SetupSearched();
            }
        }

        private void RecentAddedMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMultiSelectItem = RecentType.Add;
            AddedMusicView.SelectionMode = ListViewSelectionMode.Multiple;
            SetMultiSelectListener();
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption()
            {
                ShowRemove = false
            });
        }

        private void RecentSearchesMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMultiSelectItem = RecentType.Search;
            SearchHistoryListView.SelectionMode = ListViewSelectionMode.Multiple;
            SetMultiSelectListener();
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption()
            {
                ShowPlay = false,
                ShowAdd = false,
                ShowRemove = true
            });
        }

        private void RecentPlayedMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMultiSelectItem = RecentType.Play;
            PlayedMusicView.SelectionMode = ListViewSelectionMode.Multiple;
            SetMultiSelectListener();
            MainPage.Instance.ShowMultiSelectCommandBar();
        }

        private async void ClearPlayHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            await Helper.ShowYesNoDialog("ClearPlayHistory", () =>
            {
                Settings.settings.RemoveRecentPlayed();
                PlayedModifed = true;
                SetupPlayed();
            });
        }

        private async void ClearSearchHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            await Helper.ShowYesNoDialog("ClearSearchHistory", () =>
            {
                Settings.settings.RemoveSearchHistory();
                SearchModified = true;
                SetupSearched();
            });
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.Cancel:
                    if (CurrentMultiSelectItem == RecentType.Search)
                        SearchHistoryListView.SelectionMode = ListViewSelectionMode.None;
                    CurrentMultiSelectItem = null;
                    break;
                case MultiSelectEvent.AddTo:
                    BuildMenuFlyoutHelper(args.FlyoutHelper);
                    break;
                case MultiSelectEvent.Remove:
                    switch (CurrentMultiSelectItem)
                    {
                        case RecentType.Play:
                            if (PlayedMusicView.SelectedItemsCount == 0) return;
                            AskRemoveRecentPlayed(PlayedMusicView.SelectedItems.ToList());
                            break;
                        case RecentType.Search:
                            if (SearchHistoryListView.SelectedItems.Count == 0) return;
                            AskRemoveSearchHistory(SearchHistoryListView.SelectedItems.ToList());
                            break;
                    }
                    break;
                case MultiSelectEvent.SelectAll:
                    if (CurrentMultiSelectItem == RecentType.Search)
                        SearchHistoryListView.SelectAll();
                    break;
                case MultiSelectEvent.ClearSelections:
                    if (CurrentMultiSelectItem == RecentType.Search)
                        SearchHistoryListView.ClearSelections();
                    break;
                case MultiSelectEvent.ReverseSelections:
                    if (CurrentMultiSelectItem == RecentType.Search)
                        SearchHistoryListView.ReverseSelections();
                    break;
            }
        }

        private void RemoveRecentPlayed(object items)
        {
            var selected = (List<object>)items;
            foreach (GridViewMusic item in selected)
            {
                PlayedMusicView.RemoveMusic(item.Source);
                Settings.settings.RemoveRecentPlayed(item.Source);
            }
            SetupPlayedButtonState(PlayedMusicView.MusicCollection);
            MainPage.Instance.ShowNotification(Helper.LocalizeMessage("ItemsRemoved", selected.Count));
        }

        private void RemoveRecentSearches(object items)
        {
            var selected = (List<object>)items;
            foreach (string item in selected)
            {
                RecentSearches.Remove(item);
                Settings.settings.RemoveSearchHistory(item);
            }
            ResetSearchHistoryRowColor();
            SetupSearchedButtonState(RecentSearches);
            MainPage.Instance.ShowNotification(Helper.LocalizeMessage("ItemsRemoved", selected.Count));
        }

        private void BuildMenuFlyoutHelper(MenuFlyoutHelper helper)
        {
            string playlistName;
            if (RecentPivot.SelectedItem == RecentAddedItem)
            {
                playlistName = RecentAddedItem.Header as string;
            }
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                playlistName = RecentPlayedItem.Header as string;
            }
            else
            {
                return;
            }
            helper.DefaultPlaylistName = Settings.settings.FindNextPlaylistName(playlistName);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            AskRemoveSearchHistory((sender as Button).DataContext as string);
        }

        void IMenuFlyoutHelperBuildListener.OnBuild(MenuFlyoutHelper helper)
        {
            BuildMenuFlyoutHelper(helper);
        }

        void IMusicEventListener.Execute(Music music, MusicEventArgs args)
        {
            switch (args.EventType)
            {
                case MusicEventType.Add:
                    AddedModified = true;
                    break;
                case MusicEventType.Remove:
                    RecentAdded.Remove(music);
                    break;
                case MusicEventType.Like:
                    break;
                case MusicEventType.Modify:
                    break;
            }
        }

        void IRecentEventListener.Search(string keyword)
        {
            SearchModified = true;
        }

        void IRecentEventListener.Played(Music music)
        {
            if (MainPage.Instance?.CurrentPage == typeof(RecentPage) &&
                RecentPivot.SelectedItem == RecentPlayedItem)
            {
                PlayedMusicView.AddOrMoveToTheFirst(music);
            }
            else
            {
                PlayedModifed = true;
            }
        }
    }

    public interface IInitListener
    {
        void Inited();
    }
}
