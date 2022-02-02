using SMPlayer.Dialogs;
using SMPlayer.Models;
using SMPlayer.Models.VO;
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
            PreferenceSettings settings = PreferenceSettings.settings;
            MainPage.Instance?.SetHeaderText("PreferenceSettings");
            PreferredSongsCheckBox.IsChecked = settings.Songs;
            PreferredArtistsCheckBox.IsChecked = settings.Artists;
            PreferredAlbumsCheckBox.IsChecked = settings.Albums;
            PreferredPlaylistsCheckBox.IsChecked = settings.Playlists;
            PreferredFoldersCheckBox.IsChecked = settings.Folders;

            PreferredOthers.Add(BuildView(PreferenceSettings.FindRecentAdded, EntityType.RecentAdded));
            PreferredOthers.Add(BuildView(PreferenceSettings.FindMyFavorites, EntityType.MyFavorites));
            PreferredOthers.Add(BuildView(PreferenceSettings.FindMostPlayed, EntityType.MostPlayed));
            PreferredOthers.Add(BuildView(PreferenceSettings.FindLeastPlayed, EntityType.LeastPlayed));

            PreferredSongs = ConvertToViews(PreferenceSettings.FindPreferredSongs, EntityType.Song);
            PreferredArtists = ConvertToViews(PreferenceSettings.FindPreferredArtists, EntityType.Artist);
            PreferredAlbums = ConvertToViews(PreferenceSettings.FindPreferredAlbums, EntityType.Album);
            PreferredPlaylists = ConvertToViews(PreferenceSettings.FindPreferredPlaylists, EntityType.Playlist);
            PreferredFolders = ConvertToViews(PreferenceSettings.FindPreferredFolders, EntityType.Folder);

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

        private void SetExpandButtonVisibility(EntityType type)
        {
            GetExpandButton(type).Visibility = GetPreferenceViewByType(type).Count > GetLimitedPreferenceViewByType(type).Count ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetClearInvalidButtonVisibility(EntityType type)
        {
            GetClearInvalidButton(type).Visibility = GetPreferenceViewByType(type).Any(i => !i.IsValid) ? Visibility.Visible : Visibility.Collapsed;
        }

        private ObservableCollection<PreferenceItemView> ConvertToViews(List<PreferenceItem> items, EntityType type)
        {
            return new ObservableCollection<PreferenceItemView>(items.AsParallel().Select(i => BuildView(i, type)).OrderByDescending(i => i.Id));
        }

        private PreferenceItemView BuildView(PreferenceItem item, EntityType type)
        {
            PreferenceItemView view = item.AsView();
            switch (type)
            {
                case EntityType.Song:
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
                case EntityType.Artist:
                    view.ToolTip = view.Name = view.ItemId;
                    view.IsValid = Settings.AllSongs.Any(i => i.Artist == view.ItemId);
                    break;
                case EntityType.Album:
                    view.ToolTip = view.Name;
                    string[] albumId = view.ItemId.Split(Helpers.TileHelper.StringConcatenationFlag);
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
                case EntityType.Playlist:
                    Playlist playlist = Settings.FindPlaylist(item.LongId);
                    view.ToolTip = view.Name = playlist?.Name;
                    view.IsValid = playlist != null;
                    break;
                case EntityType.Folder:
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
                case EntityType.RecentAdded:
                    view.Name = Helper.LocalizeText("RecentAdded");
                    view.ShowRemove = false;
                    break;
                case EntityType.MyFavorites:
                    view.Name = Helper.LocalizeText("MyFavorites");
                    view.ShowRemove = false;
                    break;
                case EntityType.MostPlayed:
                    view.Name = Helper.LocalizeText("MostPlayed");
                    view.ShowRemove = false;
                    break;
                case EntityType.LeastPlayed:
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
            PreferenceSettings.settings.Songs = true;
        }

        private void PreferredSongsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Songs = false;
        }

        private void PreferredArtistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Artists = true;
        }

        private void PreferredArtistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Artists = false;
        }

        private void PreferredAlbumsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Albums = true;
        }

        private void PreferredAlbumsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Albums = false;
        }

        private void PreferredPlaylistsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Playlists = true;
        }

        private void PreferredPlaylistsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Playlists = false;
        }

        private void PreferredFoldersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Folders = true;
        }

        private void PreferredFoldersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PreferenceSettings.settings.Folders = false;
        }

        private void ClearInvalidPreferredSongsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(EntityType.Song);
        }

        private void ClearInvalidPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(EntityType.Artist);
        }

        private void ClearInvalidPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(EntityType.Album);
        }

        private void ClearInvalidPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(EntityType.Playlist);
        }

        private void ClearInvalidPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            ClearInvalid(EntityType.Folder);
        }

        private void ClearInvalid(EntityType type)
        {
            ObservableCollection<PreferenceItemView> views = GetPreferenceViewByType(type);
            
            List<PreferenceItem> models = GetPreferenceByType(type);
            HashSet<string> invalidIds = views.Where(i => !i.IsValid).Select(i => i.ItemId).ToHashSet();
            views.RemoveAll(i => invalidIds.Contains(i.ItemId));
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
            PreferenceSettings.settings.UndoPrefer(view);
            EntityType type = view.PreferType;
            ObservableCollection<PreferenceItemView> views = GetPreferenceViewByType(type);
            views.RemoveAll(i => i.ItemId == view.ItemId);
            // 写成下面这样而不是注释中这样，是为了更好的UI动画效果
            // GetLimitedPreferenceViewByType(type).SetTo(views.Take(GetMaxLimitedPreferenceViewByType(type)));
            ObservableCollection<PreferenceItemView> limitedViews = GetLimitedPreferenceViewByType(type);
            int index = limitedViews.FindIndex(i => i.ItemId == view.ItemId);
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
                                        view => MainPage.Instance.NavigateToPage(typeof(ArtistsPage), view.ItemId));
        }

        private void PreferredAlbumsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(AlbumPage), view.ItemId));
        }

        private void PreferredPlaylistsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        view => MainPage.Instance.NavigateToPage(typeof(PlaylistsPage), view.ItemId));
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

        private void ExpandButton_Click(object sender, EntityType type)
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
            ExpandButton_Click(sender, EntityType.Song);
        }

        private void ExpandPreferredArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, EntityType.Artist);
        }

        private void ExpandPreferredAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, EntityType.Album);
        }

        private void ExpandPreferredPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, EntityType.Playlist);
        }

        private void ExpandPreferredFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            ExpandButton_Click(sender, EntityType.Folder);
        }

        private void IsEnabledToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            toggleSwitch.SetToolTip(toggleSwitch.IsOn ? "IsEnabledToggleSwitchOnToolTip" : "IsEnabledToggleSwitchOffToolTip");
            if (toggleSwitch.DataContext is PreferenceItemView view)
            {
                view.IsEnabled = toggleSwitch.IsOn;
                PreferenceSettings.UpdatePreference(view.AsPreferenceItem());
            }
        }

        private void PreferenceListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = GetRowBackground(args.ItemIndex);
        }

        private void AlternateRowBackgroud(EntityType type, int start, int end)
        {
            ListView listView = GetPreferredListViewByType(type);
            for (int i = start; i < end; i++)
                if (listView.ContainerFromIndex(i) is ListViewItem container)
                    container.Background = GetRowBackground(i);
        }

        private void PreferLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (sender as ComboBox);
            if (comboBox.DataContext is PreferenceItemView item)
            {
                PreferenceSettings.UpdatePreference(item.AsPreferenceItem());
            }
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
                case EntityType.RecentAdded:
                    MainPage.Instance.NavigateToPage(typeof(RecentPage), "RecentAdded");
                    break;
                case EntityType.MyFavorites:
                    MainPage.Instance.NavigateToPage(typeof(MyFavoritesPage));
                    break;
                case EntityType.MostPlayed:
                case EntityType.LeastPlayed:
                    break;
            }
        }

        private ObservableCollection<PreferenceItemView> GetPreferenceViewByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return PreferredSongs;
                case EntityType.Artist:
                    return PreferredArtists;
                case EntityType.Album:
                    return PreferredAlbums;
                case EntityType.Playlist:
                    return PreferredPlaylists;
                case EntityType.Folder:
                    return PreferredFolders;
                default:
                    return new ObservableCollection<PreferenceItemView>();
            }
        }

        private ObservableCollection<PreferenceItemView> GetLimitedPreferenceViewByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return LimitedPreferredSongs;
                case EntityType.Artist:
                    return LimitedPreferredArtists;
                case EntityType.Album:
                    return LimitedPreferredAlbums;
                case EntityType.Playlist:
                    return LimitedPreferredPlaylists;
                case EntityType.Folder:
                    return LimitedPreferredFolders;
                default:
                    return new ObservableCollection<PreferenceItemView>();
            }
        }

        private List<PreferenceItem> GetPreferenceByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return PreferenceSettings.FindPreferredSongs;
                case EntityType.Artist:
                    return PreferenceSettings.FindPreferredArtists;
                case EntityType.Album:
                    return PreferenceSettings.FindPreferredAlbums;
                case EntityType.Playlist:
                    return PreferenceSettings.FindPreferredPlaylists;
                case EntityType.Folder:
                    return PreferenceSettings.FindPreferredFolders;
                default:
                    return new List<PreferenceItem>();
            }
        }

        private PreferenceItem GetOthersPreferenceByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.RecentAdded:
                    return PreferenceSettings.FindRecentAdded;
                case EntityType.MyFavorites:
                    return PreferenceSettings.FindMyFavorites;
                case EntityType.MostPlayed:
                    return PreferenceSettings.FindMostPlayed;
                case EntityType.LeastPlayed:
                    return PreferenceSettings.FindLeastPlayed;
                default:
                    return null;
            }
        }

        private AppBarButton GetExpandButton(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return ExpandPreferredSongsButton;
                case EntityType.Artist:
                    return ExpandPreferredArtistsButton;
                case EntityType.Album:
                    return ExpandPreferredAlbumsButton;
                case EntityType.Playlist:
                    return ExpandPreferredPlaylistsButton;
                case EntityType.Folder:
                    return ExpandPreferredFoldersButton;
                default:
                    return null;
            }
        }

        private Storyboard GetExpandAnimationByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return ExpandSongsSpinArrowAnimation;
                case EntityType.Artist:
                    return ExpandArtistsSpinArrowAnimation;
                case EntityType.Album:
                    return ExpandAlbumsSpinArrowAnimation;
                case EntityType.Playlist:
                    return ExpandPlaylistsSpinArrowAnimation;
                case EntityType.Folder:
                    return ExpandFoldersSpinArrowAnimation;
                default:
                    return null;
            }
        }

        private AppBarButton GetClearInvalidButton(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return ClearInvalidPreferredSongsButton;
                case EntityType.Artist:
                    return ClearInvalidPreferredArtistsButton;
                case EntityType.Album:
                    return ClearInvalidPreferredAlbumsButton;
                case EntityType.Playlist:
                    return ClearInvalidPreferredPlaylistsButton;
                case EntityType.Folder:
                    return ClearInvalidPreferredFoldersButton;
                default:
                    return null;
            }
        }

        private ListView GetPreferredListViewByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return PreferredSongsListView;
                case EntityType.Artist:
                    return PreferredArtistsListView;
                case EntityType.Album:
                    return PreferredAlbumsListView;
                case EntityType.Playlist:
                    return PreferredPlaylistsListView;
                case EntityType.Folder:
                    return PreferredFoldersListView;
                default:
                    return null;
            }
        }

        private int GetMaxLimitedPreferenceViewByType(EntityType type)
        {
            switch (type)
            {
                case EntityType.Song:
                    return MaxLimitedPreferredSongs;
                case EntityType.Artist:
                    return MaxLimitedPreferredArtists;
                case EntityType.Album:
                    return MaxLimitedPreferredAlbums;
                case EntityType.Playlist:
                    return MaxLimitedPreferredPlaylists;
                case EntityType.Folder:
                    return MaxLimitedPreferredFolders;
                default:
                    return 0;
            }
        }
    }
}
