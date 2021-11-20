using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public sealed partial class RecentPage : Page, IInitListener, IMultiSelectListener, IMenuFlyoutItemClickListener, IMenuFlyoutHelperBuildListener
    {
        public static RecentTimeLine RecentAdded;
        private const string JsonFileName = "RecentAddedTimeLine";
        private static bool AddedModified = true, PlayedModifed = true, SearchedModified = true;
        private ObservableCollection<string> recentSearches { get => Settings.settings.RecentSearches; }
        private RemoveDialog recentPlayedRemoveDialog, recentSearchesRemoveDialog;
        private object CurrentCategory;
        private static List<IInitListener> InitListeners = new List<IInitListener>();

        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            if (RecentAdded == null)
            {
                InitListeners.Add(this);
            }
            else
            {
                Inited();
            }
            Settings.settings.RecentPlayed.CollectionChanged += (sender, args) => PlayedModifed = true;
            Settings.settings.RecentSearches.CollectionChanged += (sender, args) => SearchedModified = true;
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

        public void Inited()
        {
            RecentAdded.CollectionChanged += () => AddedModified = true;
        }

        public static async Task Init()
        {
            if (Settings.settings.RecentAdded.IsEmpty())
            {
                ObservableCollection<Music> recentAdded = JsonFileHelper.Convert<ObservableCollection<Music>>(await JsonFileHelper.ReadAsync(JsonFileName));
                foreach (var music in recentAdded)
                {
                    music.Id = Settings.FindMusic(music.Path).Id;
                }
                RecentAdded = RecentTimeLine.FromMusicList(recentAdded ?? Settings.settings.AllSongs);
            }
            else
            {
                RecentAdded = RecentTimeLine.FromMusicList(Settings.settings.SelectMusicByIds(Settings.settings.RecentAdded));
            }
        }

        public static void Save()
        {
            if (RecentAdded == null) return;
            Settings.settings.RecentAdded = RecentAdded.TimeLine.Select(i => i.Id).ToList();
            JsonFileHelper.SaveAsync(JsonFileName, RecentAdded.TimeLine);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetMultiSelectListener();
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
            if (RecentPivot.SelectedItem == RecentAddedItem)
            {
                MainPage.Instance.SetMultiSelectListener(AddedMusicView);
            }
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                MainPage.Instance.SetMultiSelectListener(PlayedMusicView);
            }
            else
            {
                MainPage.Instance.SetMultiSelectListener(this);
            }
        }

        public void SetupAdded(IEnumerable<Music> list)
        {
            if (!AddedModified) return;
            RecentAddedProgressRing.IsActive = true;
            try
            {
                AddedMusicView.Setup(list);
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                Debug.WriteLine("InvalidOperationException On Recent Added");
            }
            RecentAddedProgressRing.IsActive = AddedModified = false;
        }

        public void SetupPlayed(IEnumerable<long> list)
        {
            if (PlayedModifed)
            {
                RecentPlayedProgressRing.IsActive = true;
                try
                {
                    PlayedMusicView.Setup(list);
                    ClearPlayHistoryAppButton.IsEnabled = list.Count() != 0;
                }
                catch (InvalidOperationException)
                {
                    // Loading while Set New Folder will cause this Exception
                    Debug.WriteLine("InvalidOperationException On Recent Played");
                }
            }
            RecentPlayedProgressRing.IsActive = PlayedModifed = false;
        }

        private void ResetColor(int start = 0)
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
                ResetColor();
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
            ShowYesNoDialog("ClearPlayHistory", () =>
            {
                Settings.settings.RecentPlayed.Clear();
                SetupPlayed(Settings.settings.RecentPlayedSongs);
            });
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
            if (item is GridMusicView)
            {
                music = (GridMusicView) item;
            }
            else if (item is IEnumerable<GridMusicView> list && list.Count() == 1)
            {
                music = list.ElementAt(0);
            }
            if (recentPlayedRemoveDialog.IsChecked)
            {
                if (music == null)
                {
                    RemoveRecentPlayed();
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
                    recentPlayedRemoveDialog.Confirm = RemoveRecentPlayed;
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
            if (item is string @string)
            {
                keyword = @string;
            }
            else if (item is IList<string> list && list.Count() == 1)
            {
                keyword = list.ElementAt(0);
            }
            if (recentSearchesRemoveDialog.IsChecked)
            {
                if (keyword == null)
                {
                    RemoveRecentSearches();
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
                    recentSearchesRemoveDialog.Confirm = RemoveRecentSearches;
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
            MainPage.Instance.ShowUndoNotification(Helper.LocalizeMessage("ItemRemoved", item), () =>
            {
                PlayedMusicView.UndoDelete(item.Source);
            });
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

        private void RecentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (RecentPivot.SelectedItem == RecentAddedItem)
            {
                SetupAdded(RecentAdded.TimeLine);
            }
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                SetupPlayed(Settings.settings.RecentPlayedSongs);
            }
            else
            {
                SetupSearched(recentSearches);
            }
            SetMultiSelectListener();
        }

        private void RecentAddedItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (RecentAdded != null)
            {
                SetupAdded(RecentAdded.TimeLine);
            }
        }

        private void RecentAddedMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            AddedMusicView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption()
            {
                ShowRemove = false
            });
        }

        private void RecentSearchesMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            SearchHistoryListView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar(new MultiSelectCommandBarOption()
            {
                ShowPlay = false,
                ShowAdd = false,
                ShowRemove = true
            });
        }

        private void RecentPlayedMultiSelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            PlayedMusicView.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar();
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

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            BuildMenuFlyoutHelper(helper);
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar)
        {
            if (RecentPivot.SelectedItem != RecentSearchesItem) return;
            SearchHistoryListView.SelectionMode = ListViewSelectionMode.None;
        }
        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar)
        {
            if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                if (PlayedMusicView.SelectedItemsCount == 0) return;
                AskRemoveRecentPlayed(PlayedMusicView.SelectedItems);
                return;
            }
            else if (RecentPivot.SelectedItem == RecentSearchesItem)
            {
                if (SearchHistoryListView.SelectedItems.Count == 0) return;
                AskRemoveSearchHistory(SearchHistoryListView.SelectedItems);
            }
        }

        private void RemoveRecentPlayed()
        {
            foreach (GridMusicView item in PlayedMusicView.SelectedItems.ToList())
            {
                PlayedMusicView.RemoveMusic(item.Source);
                Settings.settings.RecentPlayed.Remove(item.Source.Path);
            }
        }

        private void RemoveRecentSearches()
        {
            var selected = SearchHistoryListView.SelectedItems.ToList();
            foreach (string item in selected)
            {
                recentSearches.Remove(item);
            }
            ResetColor();
            MainPage.Instance.ShowNotification(Helper.LocalizeMessage("ItemsRemoved", selected.Count));
        }

        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar) 
        {
            if (RecentPivot.SelectedItem != RecentSearchesItem) return;
            SearchHistoryListView.SelectAll();
        }

        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar) 
        {
            if (RecentPivot.SelectedItem != RecentSearchesItem) return;
            SearchHistoryListView.ClearSelections();
        }
        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar) 
        {
            if (RecentPivot.SelectedItem != RecentSearchesItem) return;
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
    }

    public interface IInitListener
    {
        void Inited();
    }
}
