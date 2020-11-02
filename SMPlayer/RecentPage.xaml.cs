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
    public sealed partial class RecentPage : Page, IInitListener
    {
        private const string JsonFileName = "RecentAddedTimeLine";
        public static bool Inited { get; private set; } = false;
        public static RecentTimeLine AddedTimeLine;
        private static bool AddedModified = true, PlayedModifed = true, SearchedModified = true;
        private Dialogs.RemoveDialog dialog;
        private ObservableCollection<string> recentSearches { get => Settings.settings.RecentSearches; }
        private readonly ObservableCollection<MusicTimeLine> RecentAddedTimeLineList = new ObservableCollection<MusicTimeLine>();
        private static readonly List<IInitListener> InitListeners = new List<IInitListener>();
        private Dictionary<object, GridMusicControl> GridMusicControlDict = new Dictionary<object, GridMusicControl>();

        public RecentPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings.settings.RecentPlayed.CollectionChanged += (sender, args) => PlayedModifed = true;
            Settings.settings.RecentSearches.CollectionChanged += (sender, args) => SearchedModified = true;
            InitListeners.Add(this);
        }

        public static async Task Init()
        {
            foreach (var listener in InitListeners) listener.BeforeInit();
            string json = await JsonFileHelper.ReadAsync(JsonFileName);
            AddedTimeLine = RecentTimeLine.FromMusicList(JsonFileHelper.Convert<List<Music>>(json)) ??
                            RecentTimeLine.FromMusicList(MusicLibraryPage.AllSongs);
            Inited = true;
            foreach (var listener in InitListeners) listener.Inited();
        }

        public static void Save()
        {
            if (AddedTimeLine.Count > 0)
            {
                JsonFileHelper.SaveAsync(JsonFileName, AddedTimeLine.All);
            }
        }

        void IInitListener.BeforeInit()
        {
            RecentAddedProgressRing.IsActive = true;
        }

        void IInitListener.Inited()
        {
            AddedTimeLine.CollectionChanged += AddedTimeLineChanged;
            if (IsLoaded)
            {
                SetupAdded(AddedTimeLine);
            }
        }

        private void AddedTimeLineChanged(RecentTimeLineCategory category, Music music)
        {
            if (category == RecentTimeLineCategory.Today)
            {
                if (RecentAddedTimeLineList.Count == 0 || RecentAddedTimeLineList[0].Category != RecentTimeLineCategory.Today)
                {
                    AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.Today, AddedTimeLine.Today);
                }
                else
                {
                    GridMusicControlDict[RecentTimeLineCategory.Today].AddMusic(music);
                }
            }
            if (category == RecentTimeLineCategory.Remove)
            {
                foreach (var control in GridMusicControlDict.Values)
                {
                    if (control.RemoveMusic(music))
                    {
                        break;
                    }
                }
            }
            else
            {
                GridMusicControl control = category == RecentTimeLineCategory.Year ? GridMusicControlDict[music.DateAdded.Year] :
                                                                                     GridMusicControlDict[category.ToString()];
                control.AddMusic(music);
            }
        }

        private static void AddTimeLine(ICollection<MusicTimeLine> timeLine, object title, List<Music> list)
        {
            if (list.Count == 0) return;
            timeLine.Add(new MusicTimeLine(title, list));
        }

        public void SetupAdded(RecentTimeLine AddedTimeLine)
        {
            if (!AddedModified) return;
            RecentAddedProgressRing.IsActive = true;
            try
            {
                RecentAddedTimeLineList.Clear();
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.Today, AddedTimeLine.Today);
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.ThisWeek, AddedTimeLine.ThisWeek);
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.ThisMonth, AddedTimeLine.ThisMonth);
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.Recent3Months, AddedTimeLine.Recent3Months);
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.Recent6Months, AddedTimeLine.Recent6Months);
                AddTimeLine(RecentAddedTimeLineList, RecentTimeLineCategory.ThisYear, AddedTimeLine.ThisYear);
                foreach (var item in AddedTimeLine.Years)
                {
                    AddTimeLine(RecentAddedTimeLineList, item.Key, item.Value);
                }
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                Debug.WriteLine("InvalidOperationException On Recent Added");
            }
            RecentAddedProgressRing.IsActive = AddedModified = false;
        }

        public void SetupPlayed(IEnumerable<string> list)
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
            GridMusicControl control = (GridMusicControl)sender;
            MusicTimeLine data = (MusicTimeLine)control.DataContext;
            if (control.GridMusicCollection.Count == 0)
            {
                control.GridItemClickedListener += (s, a) =>
                {
                    MediaHelper.SetPlaylistAndPlay(AddedTimeLine.All, (s as GridMusicView).Source);
                };
                control.Setup(data.Items);
                GridMusicControlDict[data.Title] = control;
            }
        }

        private void RecentPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Inited || !IsLoaded) return;
            if (RecentPivot.SelectedItem == RecentAddedItem)
            {
                SetupAdded(AddedTimeLine);
            }
            else if (RecentPivot.SelectedItem == RecentPlayedItem)
            {
                SetupPlayed(Settings.settings.RecentPlayed);
            }
            else
            {
                SetupSearched(recentSearches);
            }
        }

        private void RecentAddedItem_Loaded(object sender, RoutedEventArgs e)
        {
            SetupAdded(AddedTimeLine);
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

    public interface IInitListener
    {
        void BeforeInit();
        void Inited();
    }
}
