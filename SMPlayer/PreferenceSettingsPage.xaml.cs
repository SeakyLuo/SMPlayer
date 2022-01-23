using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PreferenceSettingsPage : Page
    {
        private const int MaxLimitedPreferredSongs = 5,
                          MaxLimitedPreferredArtists = 5,
                          MaxLimitedPreferredAlbums = 5,
                          MaxLimitedPreferredPlaylists = 5,
                          MaxLimitedPreferredFolders = 5;


        private static bool ShowAddPreferredSongsToolTip = true,
                            ShowAddPreferredArtistsToolTip = true,
                            ShowAddPreferredAlbumsToolTip = true,
                            ShowAddPreferredPlaylistsToolTip = true,
                            ShowAddPreferredFoldersToolTip = true;

        private readonly ObservableCollection<PreferenceItemView> LimitedPreferredSongs;
        private readonly ObservableCollection<PreferenceItemView> LimitedPreferredArtists;
        private readonly ObservableCollection<PreferenceItemView> LimitedPreferredAlbums;
        private readonly ObservableCollection<PreferenceItemView> LimitedPreferredPlaylists;
        private readonly ObservableCollection<PreferenceItemView> LimitedPreferredFolders;
        private readonly ObservableCollection<PreferenceItemView> PreferredSongs;
        private readonly ObservableCollection<PreferenceItemView> PreferredArtists;
        private readonly ObservableCollection<PreferenceItemView> PreferredAlbums;
        private readonly ObservableCollection<PreferenceItemView> PreferredPlaylists;
        private readonly ObservableCollection<PreferenceItemView> PreferredFolders;
        private readonly ObservableCollection<PreferenceItemView> PreferredOthers = new ObservableCollection<PreferenceItemView>();
        private static RemoveDialog removeDialog;

        public PreferenceSettingsPage()
        {
            this.InitializeComponent();
            MainPage.Instance?.SetHeaderText("PreferenceSettings");
            PreferredSongsCheckBox.IsChecked = Settings.settings.Preference.Songs;
            PreferredArtistsCheckBox.IsChecked = Settings.settings.Preference.Artists;
            PreferredAlbumsCheckBox.IsChecked = Settings.settings.Preference.Albums;
            PreferredPlaylistsCheckBox.IsChecked = Settings.settings.Preference.Playlists;
            PreferredFoldersCheckBox.IsChecked = Settings.settings.Preference.Folders;

            PreferredOthers.Add(BuildView(Settings.settings.Preference.RecentAdded, PreferType.RecentAdded));
            PreferredOthers.Add(BuildView(Settings.settings.Preference.MyFavorites, PreferType.MyFavorites));
            PreferredOthers.Add(BuildView(Settings.settings.Preference.MostPlayed, PreferType.MostPlayed));
            PreferredOthers.Add(BuildView(Settings.settings.Preference.LeastPlayed, PreferType.LeastPlayed));

            PreferredSongs = ConvertToViews(Settings.settings.Preference.PreferredSongs, PreferType.Song);
            PreferredArtists = ConvertToViews(Settings.settings.Preference.PreferredArtists, PreferType.Artist);
            PreferredAlbums = ConvertToViews(Settings.settings.Preference.PreferredAlbums, PreferType.Album);
            PreferredPlaylists = ConvertToViews(Settings.settings.Preference.PreferredPlaylists, PreferType.Playlist);
            PreferredFolders = ConvertToViews(Settings.settings.Preference.PreferredFolders, PreferType.Folder);

            LimitedPreferredSongs = new ObservableCollection<PreferenceItemView>(PreferredSongs.Take(MaxLimitedPreferredSongs));
            LimitedPreferredArtists = new ObservableCollection<PreferenceItemView>(PreferredArtists.Take(MaxLimitedPreferredArtists));
            LimitedPreferredAlbums = new ObservableCollection<PreferenceItemView>(PreferredAlbums.Take(MaxLimitedPreferredAlbums));
            LimitedPreferredPlaylists = new ObservableCollection<PreferenceItemView>(PreferredPlaylists.Take(MaxLimitedPreferredPlaylists));
            LimitedPreferredFolders = new ObservableCollection<PreferenceItemView>(PreferredFolders.Take(MaxLimitedPreferredFolders));

            GoToAddPreferredSongsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredSongsToolTip"), false);
            GoToAddPreferredArtistsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredArtistsToolTip"), false);
            GoToAddPreferredAlbumsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredAlbumsToolTip"), false);
            GoToAddPreferredPlaylistsButton.SetToolTip(Helper.LocalizeMessage("AddPreferredPlaylistsToolTip"), false);
            GoToAddPreferredFoldersButton.SetToolTip(Helper.LocalizeMessage("AddPreferredFoldersToolTip"), false);

            ClearInvalidPreferredSongsButton.Visibility = PreferredSongs.Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
            ClearInvalidPreferredArtistsButton.Visibility = PreferredArtists.Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
            ClearInvalidPreferredAlbumsButton.Visibility = PreferredAlbums.Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
            ClearInvalidPreferredPlaylistsButton.Visibility = PreferredPlaylists.Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
            ClearInvalidPreferredFoldersButton.Visibility = PreferredFolders.Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;

            ExpandPreferredSongsButton.Visibility = PreferredSongs.Count > LimitedPreferredSongs.Count ? Visibility.Visible : Visibility.Collapsed;
            ExpandPreferredArtistsButton.Visibility = PreferredArtists.Count > LimitedPreferredArtists.Count ? Visibility.Visible : Visibility.Collapsed;
            ExpandPreferredAlbumsButton.Visibility = PreferredAlbums.Count > LimitedPreferredAlbums.Count ? Visibility.Visible : Visibility.Collapsed;
            ExpandPreferredPlaylistsButton.Visibility = PreferredPlaylists.Count > LimitedPreferredPlaylists.Count ? Visibility.Visible : Visibility.Collapsed;
            ExpandPreferredFoldersButton.Visibility = PreferredFolders.Count > LimitedPreferredFolders.Count ? Visibility.Visible : Visibility.Collapsed;
            ExpandPreferredSongsButton.Label = ExpandPreferredArtistsButton.Label = ExpandPreferredAlbumsButton.Label
                                             = ExpandPreferredPlaylistsButton.Label = ExpandPreferredFoldersButton.Label
                                             = Helper.LocalizeText("ExpandList");
        }

        private void SetExpandButtonVisibility(PreferType type)
        {
            GetExpandButton(type).Visibility = GetPreferenceViewByType(type).Count > GetLimitedPreferenceViewByType(type).Count ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetClearInvalidButtonVisibility(PreferType type)
        {
            GetClearInvalidButton(type).Visibility = GetPreferenceViewByType(type).Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
        }

        private ObservableCollection<PreferenceItemView> ConvertToViews(List<PreferenceItem> items, PreferType type)
        {
            return new ObservableCollection<PreferenceItemView>(items.AsParallel().AsOrdered().Select(i => BuildView(i, type)));
        }

        private PreferenceItemView BuildView(PreferenceItem item, PreferType type)
        {
            PreferenceItemView view = item.AsView();
            view.PreferType = type;
            switch (type)
            {
                case PreferType.Song:
                    Music music = Settings.FindMusic(item.LongId);
                    if (music == null)
                    {
                        view.IsValid = false;
                    }
                    else
                    {
                        view.Name = music.Name;
                        view.ToolTip = music.Path;
                    }
                    break;
                case PreferType.Artist:
                    view.ToolTip = view.Name = view.Id;
                    view.IsValid = Settings.AllSongs.Any(i => i.Artist == view.Id);
                    break;
                case PreferType.Album:
                    view.ToolTip = view.Name;
                    string[] albumId = view.Id.Split(Helpers.TileHelper.StringConcatenationFlag);
                    if (albumId.Length > 1)
                    {
                        string album = albumId[0], artist = albumId[1];
                        view.IsValid = Settings.AllSongs.Any(i => i.Album == album && i.Artist == artist);
                    }
                    else
                    {
                        view.IsValid = false;
                    }
                    break;
                case PreferType.Playlist:
                    Playlist playlist = Settings.FindPlaylist(item.LongId);
                    view.ToolTip = view.Name = playlist?.Name;
                    view.IsValid = playlist != null;
                    break;
                case PreferType.Folder:
                    FolderTree tree = Settings.FindFolderInfo(item.LongId);
                    if (tree == null)
                    {
                        view.IsValid = false;
                    }
                    else
                    {
                        view.Name = tree.Name;
                        view.ToolTip = tree.Path;
                    }
                    break;
                case PreferType.RecentAdded:
                    view.Name = Helper.LocalizeText("RecentAdded");
                    view.ShowRemove = false;
                    break;
                case PreferType.MyFavorites:
                    view.Name = Helper.LocalizeText("MyFavorites");
                    view.ShowRemove = false;
                    break;
                case PreferType.MostPlayed:
                    view.Name = Helper.LocalizeText("MostPlayed");
                    view.ShowRemove = false;
                    break;
                case PreferType.LeastPlayed:
                    view.Name = Helper.LocalizeText("LeastPlayed");
                    view.ShowRemove = false;
                    break;
            }
            if (!view.IsValid)
            {
                view.ToolTip = Helper.LocalizeText("InvalidItemToolTip", view.Name);
            }
            return view;
        }

        private void GoToAddPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(MusicLibraryPage));
            if (ShowAddPreferredSongsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredSongsToolTip", 5000);
                ShowAddPreferredSongsToolTip = false;
            }
        }

        private void GoToAddPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(ArtistsPage));
            if (ShowAddPreferredArtistsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredArtistsToolTip", 5000);
                ShowAddPreferredArtistsToolTip = false;
            }
        }

        private void GoToAddPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(AlbumsPage));
            if (ShowAddPreferredAlbumsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredAlbumsToolTip", 5000);
                ShowAddPreferredAlbumsToolTip = false;
            }
        }

        private void GoToAddPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(PlaylistsPage));
            if (ShowAddPreferredPlaylistsToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredPlaylistsToolTip", 5000);
                ShowAddPreferredPlaylistsToolTip = false;
            }
        }

        private void GoToAddPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.NavigateToPage(typeof(LocalPage));
            if (ShowAddPreferredFoldersToolTip)
            {
                MainPage.Instance.ShowLocalizedNotification("AddPreferredFoldersToolTip", 5000);
                ShowAddPreferredFoldersToolTip = true;
            }
        }

        private void PreferredSongsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Songs = true;
        }

        private void PreferredSongsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Songs = false;
        }

        private void PreferredArtistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Artists = true;
        }

        private void PreferredArtistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Artists = false;
        }

        private void PreferredAlbumsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Albums = true;
        }

        private void PreferredAlbumsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Albums = false;
        }

        private void PreferredPlaylistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Playlists = true;
        }

        private void PreferredPlaylistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Playlists = false;
        }

        private void PreferredFoldersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Folders = true;
        }

        private void PreferredFoldersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.Folders = false;
        }

        private void ClearInvalidPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(PreferType.Song);
        }

        private void ClearInvalidPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(PreferType.Artist);
        }

        private void ClearInvalidPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(PreferType.Album);
        }

        private void ClearInvalidPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(PreferType.Playlist);
        }

        private void ClearInvalidPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(PreferType.Folder);
        }

        private void ClearInvalid(PreferType type)
        {
            ObservableCollection<PreferenceItemView> views = GetPreferenceViewByType(type);
            
            List<PreferenceItem> models = GetPreferenceByType(type);
            HashSet<string> invalidIds = views.Where(i => !i.IsValid).Select(i => i.Id).ToHashSet();
            views.RemoveAll(i => invalidIds.Contains(i.Id));
            models.RemoveAll(i => invalidIds.Contains(i.Id));

            ObservableCollection<PreferenceItemView> limitedViews = GetLimitedPreferenceViewByType(type);
            limitedViews.SetTo(views.Take(GetMaxLimitedPreferenceViewByType(type)));

            AlternateRowBackgroud(type, 0, limitedViews.Count);
            SetExpandButtonVisibility(type);
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            PreferenceItemView view = (sender as Button).DataContext as PreferenceItemView;
            if (removeDialog == null)
            {
                removeDialog = new RemoveDialog();
            }
            if (removeDialog.IsChecked)
            {
                RemovePreference(view);
            }
            else
            {
                removeDialog.Confirm = () => RemovePreference(view);
                removeDialog.Message = Helper.LocalizeMessage("RemoveItem", view.Name);
                await removeDialog.ShowAsync();
            }
        }

        private void RemovePreference(PreferenceItemView view)
        {
            PreferType type = view.PreferType;
            GetPreferenceByType(type).RemoveAll(i => i.Id == view.Id);
            ObservableCollection<PreferenceItemView> views = GetPreferenceViewByType(type);
            views.RemoveAll(i => i.Id == view.Id);
            // 写成下面这样而不是注释中这样，是为了更好的UI动画效果
            // GetLimitedPreferenceViewByType(type).SetTo(views.Take(GetMaxLimitedPreferenceViewByType(type)));
            ObservableCollection<PreferenceItemView> limitedViews = GetLimitedPreferenceViewByType(type);
            int index = limitedViews.FindIndex(i => i.Id == view.Id);
            limitedViews.RemoveAt(index);
            limitedViews.AddRange(views.GetRange(limitedViews.Count, GetMaxLimitedPreferenceViewByType(type)));

            AlternateRowBackgroud(type, index, limitedViews.Count);
            SetExpandButtonVisibility(type);
            SetClearInvalidButtonVisibility(type);
        }

        private void PreferredSongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        async (view) =>
                                        {
                                            MusicDialog dialog = new MusicDialog(MusicDialogOption.Properties, Settings.FindMusic(view.LongId));
                                            await dialog.ShowAsync();
                                        });
        }

        private void PreferredArtistsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(ArtistsPage), view.Id));
        }

        private void PreferredAlbumsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(AlbumPage), view.Id));
        }

        private void PreferredPlaylistsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(PlaylistsPage), view.Id));
        }

        private void PreferredFoldersListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(LocalPage), Settings.FindFolderInfo(view.LongId)));
        }

        private void PreferredListView_ItemClick(object sender, ItemClickEventArgs e, Action<PreferenceItemView> action)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectionMode != ListViewSelectionMode.None) return;
            PreferenceItemView view = e.ClickedItem as PreferenceItemView;
            if (view.IsValid)
            {
                action.Invoke(view);
            }
            else
            {
                Helper.ShowNotificationRaw(Helper.LocalizeText("InvalidItemToolTip", view.Name));
            }
        }

        private void ExpandButton_Click(object sender, PreferType type)
        {
            // 开始动画
            GetExpandAnimationByType(type).Begin();
            // 改按钮文字
            AppBarButton button = (sender as AppBarButton);
            string expandText = Helper.LocalizeText("ExpandList");
            bool isCollapsed = button.Label.ToString() == expandText;
            button.Label = isCollapsed ? Helper.LocalizeText("CollapseList") : expandText;
            // 调整列表
            ObservableCollection<PreferenceItemView> list = GetPreferenceViewByType(type);
            ObservableCollection<PreferenceItemView> limitedList = GetLimitedPreferenceViewByType(type);
            if (isCollapsed)
            {
                limitedList.AddRange(list.Skip(limitedList.Count));
            }
            else
            {
                limitedList.Clear();
            }
        }

        private void ExpandPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, PreferType.Song);
        }

        private void ExpandPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, PreferType.Artist);
        }

        private void ExpandPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, PreferType.Album);
        }

        private void ExpandPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, PreferType.Playlist);
        }

        private void ExpandPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, PreferType.Folder);
        }

        private void IsEnabledToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            toggleSwitch.SetToolTip(toggleSwitch.IsOn ? "IsEnabledToggleSwitchOnToolTip" : "IsEnabledToggleSwitchOffToolTip");
            if (toggleSwitch.DataContext is PreferenceItemView view)
            {
                if (GetOthersPreferenceByType(view.PreferType) is PreferenceItem other)
                {
                    other.IsEnabled = !other.IsEnabled;
                }
                else
                {
                    GetPreferenceByType(view.PreferType).Find(i => i.Id == view.Id).IsEnabled = view.IsEnabled = !view.IsEnabled;
                }
            }
        }

        private void PreferenceListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = GetRowBackground(args.ItemIndex);
        }

        private void AlternateRowBackgroud(PreferType type, int start, int end)
        {
            ListView listView = GetPreferredListViewByType(type);
            for (int i = start; i < end; i++)
                if (listView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }

        private List<PreferLevel> preferLevels = new List<PreferLevel>() { PreferLevel.Low, PreferLevel.Normal, PreferLevel.High, PreferLevel.Higher };
        private void PreferLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
             PreferLevel preferLevel = preferLevels[comboBox.SelectedIndex];

            PreferenceItemView item = comboBox.DataContext as PreferenceItemView;
            item.Level = preferLevel;

            switch (item.PreferType)
            {
                case PreferType.RecentAdded:
                case PreferType.MyFavorites:
                case PreferType.MostPlayed:
                case PreferType.LeastPlayed:
                    GetOthersPreferenceByType(item.PreferType).Level = preferLevel;
                    break;
                default:
                    GetPreferenceByType(item.PreferType).Find(i => i.Id == item.Id).Level = preferLevel;
                    break;
            }
        }

        private void PreferLevelComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            comboBox.SelectedIndex = preferLevels.IndexOf((comboBox.DataContext as PreferenceItemView).Level);
        }

        public static Brush GetRowBackground(int index)
        {
            return index % 2 == 0 ? ColorHelper.WhiteSmokeBrush : ColorHelper.WhiteBrush;
        }

        private void PreferredOthersListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferenceItemView view = e.ClickedItem as PreferenceItemView;
            switch (view.PreferType)
            {
                case PreferType.RecentAdded:
                    MainPage.Instance.NavigateToPage(typeof(RecentPage), "RecentAdded");
                    break;
                case PreferType.MyFavorites:
                    MainPage.Instance.NavigateToPage(typeof(MyFavoritesPage));
                    break;
                case PreferType.MostPlayed:
                case PreferType.LeastPlayed:
                    break;
            }
        }

        private ObservableCollection<PreferenceItemView> GetPreferenceViewByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return PreferredSongs;
                case PreferType.Artist:
                    return PreferredArtists;
                case PreferType.Album:
                    return PreferredAlbums;
                case PreferType.Playlist:
                    return PreferredPlaylists;
                case PreferType.Folder:
                    return PreferredFolders;
                default:
                    return new ObservableCollection<PreferenceItemView>();
            }
        }

        private ObservableCollection<PreferenceItemView> GetLimitedPreferenceViewByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return LimitedPreferredSongs;
                case PreferType.Artist:
                    return LimitedPreferredArtists;
                case PreferType.Album:
                    return LimitedPreferredAlbums;
                case PreferType.Playlist:
                    return LimitedPreferredPlaylists;
                case PreferType.Folder:
                    return LimitedPreferredFolders;
                default:
                    return new ObservableCollection<PreferenceItemView>();
            }
        }

        private List<PreferenceItem> GetPreferenceByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return Settings.settings.Preference.PreferredSongs;
                case PreferType.Artist:
                    return Settings.settings.Preference.PreferredArtists;
                case PreferType.Album:
                    return Settings.settings.Preference.PreferredAlbums;
                case PreferType.Playlist:
                    return Settings.settings.Preference.PreferredPlaylists;
                case PreferType.Folder:
                    return Settings.settings.Preference.PreferredFolders;
                default:
                    return new List<PreferenceItem>();
            }
        }

        private PreferenceItem GetOthersPreferenceByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.RecentAdded:
                    return Settings.settings.Preference.RecentAdded;
                case PreferType.MyFavorites:
                    return Settings.settings.Preference.MyFavorites;
                case PreferType.MostPlayed:
                    return Settings.settings.Preference.MostPlayed;
                case PreferType.LeastPlayed:
                    return Settings.settings.Preference.LeastPlayed;
                default:
                    return null;
            }
        }

        private AppBarButton GetExpandButton(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return ExpandPreferredSongsButton;
                case PreferType.Artist:
                    return ExpandPreferredArtistsButton;
                case PreferType.Album:
                    return ExpandPreferredAlbumsButton;
                case PreferType.Playlist:
                    return ExpandPreferredPlaylistsButton;
                case PreferType.Folder:
                    return ExpandPreferredFoldersButton;
                default:
                    return null;
            }
        }

        private Storyboard GetExpandAnimationByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return ExpandSongsSpinArrowAnimation;
                case PreferType.Artist:
                    return ExpandArtistsSpinArrowAnimation;
                case PreferType.Album:
                    return ExpandAlbumsSpinArrowAnimation;
                case PreferType.Playlist:
                    return ExpandPlaylistsSpinArrowAnimation;
                case PreferType.Folder:
                    return ExpandFoldersSpinArrowAnimation;
                default:
                    return null;
            }
        }

        private AppBarButton GetClearInvalidButton(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return ClearInvalidPreferredSongsButton;
                case PreferType.Artist:
                    return ClearInvalidPreferredArtistsButton;
                case PreferType.Album:
                    return ClearInvalidPreferredAlbumsButton;
                case PreferType.Playlist:
                    return ClearInvalidPreferredPlaylistsButton;
                case PreferType.Folder:
                    return ClearInvalidPreferredFoldersButton;
                default:
                    return null;
            }
        }

        private ListView GetPreferredListViewByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return PreferredSongsListView;
                case PreferType.Artist:
                    return PreferredArtistsListView;
                case PreferType.Album:
                    return PreferredAlbumsListView;
                case PreferType.Playlist:
                    return PreferredPlaylistsListView;
                case PreferType.Folder:
                    return PreferredFoldersListView;
                default:
                    return null;
            }
        }

        private int GetMaxLimitedPreferenceViewByType(PreferType type)
        {
            switch (type)
            {
                case PreferType.Song:
                    return MaxLimitedPreferredSongs;
                case PreferType.Artist:
                    return MaxLimitedPreferredArtists;
                case PreferType.Album:
                    return MaxLimitedPreferredAlbums;
                case PreferType.Playlist:
                    return MaxLimitedPreferredPlaylists;
                case PreferType.Folder:
                    return MaxLimitedPreferredFolders;
                default:
                    return 0;
            }
        }
    }
}
