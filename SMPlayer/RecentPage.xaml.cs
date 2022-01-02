using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
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
    public sealed partial class RecentPage : Page, IMultiSelectListener, IMenuFlyoutItemClickListener, IMenuFlyoutHelperBuildListener, IMusicEventListener, IFolderTreeEventListener, IRecentEventListener
    {
        public static RecentTimeLine RecentAdded;
        private const string JsonFileName = "RecentAddedTimeLine";
        private static bool AddedModified = true, PlayedModifed = true, SearchModified = true;
        private readonly ObservableCollection<string> RecentSearches = new ObservableCollection<string>();
        private RemoveDialog recentPlayedRemoveDialog, recentSearchesRemoveDialog;
        private object CurrentCategory;
        private RecentType? CurrentMultiSelectItem;

        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.AddRecentEventListener(this);

            AddedMusicView.TopItemEffectiveViewportChanged += (sender, args) =>
            {
                if (sender.DataContext is GridMusicView music)
                {
                    object category = RecentTimeLine.Categorize(music.Source.DateAdded);
                    if (category != CurrentCategory)
                    {
                        CurrentCategory = category;
                        RecentAddedHeader.Text = Helper.LocalizeMessage(category.ToString());
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
            //if (RecentAdded == null) return;
            //JsonFileHelper.SaveAsync(JsonFileName, RecentAdded.TimeLine);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (RecentAdded == null)
            {
                //List<Music> recentAdded = await JsonFileHelper.ReadObjectAsync<List<Music>>(JsonFileName);
                //if (recentAdded.IsEmpty()) recentAdded = Settings.AllSongs.OrderByDescending(m => m.DateAdded).ToList();
                List<Music> recentAdded = Settings.AllSongs.OrderByDescending(m => m.DateAdded).ToList();
                RecentAdded = RecentTimeLine.FromMusicList(recentAdded);
                SetupAdded();
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
            GridMusicView music = null;
            if (item is GridMusicView gridMusicView)
            {
                music = gridMusicView;
            }
            else if (item is List<object> list && list.Count() == 1)
            {
                music = list.ElementAt(0) as GridMusicView;
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

        private void RemoveRecentPlayed(GridMusicView item)
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

        private void ClearPlayHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearPlayHistory", () =>
            {
                Settings.settings.RemoveRecentPlayed();
                PlayedModifed = true;
                SetupPlayed();
            });
        }

        private void ClearSearchHistoryAppButton_Click(object sender, RoutedEventArgs e)
        {
            ShowYesNoDialog("ClearSearchHistory", () =>
            {
                Settings.settings.RemoveSearchHistory();
                SearchModified = true;
                SetupSearched();
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

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            BuildMenuFlyoutHelper(helper);
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            if (CurrentMultiSelectItem == RecentType.Search)
                SearchHistoryListView.SelectionMode = ListViewSelectionMode.None;
            CurrentMultiSelectItem = null;
        }
        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar)
        {
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
        }

        private void RemoveRecentPlayed(object items)
        {
            var selected = (List<object>)items;
            foreach (GridMusicView item in selected)
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

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar)
        {
            if (CurrentMultiSelectItem == RecentType.Search)
                SearchHistoryListView.SelectAll();
        }
        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar) 
        {
            if (CurrentMultiSelectItem == RecentType.Search)
                SearchHistoryListView.ClearSelections();
        }
        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar) 
        {
            if (CurrentMultiSelectItem == RecentType.Search)
                SearchHistoryListView.ReverseSelections();
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

        void IMenuFlyoutItemClickListener.Favorite(object data) { }
        void IMenuFlyoutItemClickListener.Delete(Music music) { }
        void IMenuFlyoutItemClickListener.UndoDelete(Music music) { }
        void IMenuFlyoutItemClickListener.Remove(Music music) { }
        void IMenuFlyoutItemClickListener.Select(object data) { }
        void IMenuFlyoutItemClickListener.AddTo(object data, object collection, int index, AddToCollectionType type) { }
        void IFolderTreeEventListener.Added(FolderTree branch, FolderTree root) { }
        void IFolderTreeEventListener.Renamed(FolderTree folder, string newPath) { }
        void IFolderTreeEventListener.Removed(FolderTree folder)
        {
            AddedModified = true;
        }
        void IMusicEventListener.Liked(Music music, bool isFavorite) { }
        void IMusicEventListener.Added(Music music)
        {
            AddedModified = true;
        }
        void IMusicEventListener.Removed(Music music)
        {
            RecentAdded.Remove(music);
        }
        void IMusicEventListener.Modified(Music before, Music after) { }

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
