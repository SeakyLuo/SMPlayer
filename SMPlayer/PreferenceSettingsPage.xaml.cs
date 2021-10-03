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
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PreferenceSettingsPage : Page
    {
        private static bool ShowAddPreferredSongsToolTip = true,
                            ShowAddPreferredArtistsToolTip = true,
                            ShowAddPreferredAlbumsToolTip = true,
                            ShowAddPreferredPlaylistsToolTip = true,
                            ShowAddPreferredFoldersToolTip = true;

        private readonly ObservableCollection<PreferenceItemView> PreferredSongs;
        private readonly ObservableCollection<PreferenceItemView> PreferredArtists;
        private readonly ObservableCollection<PreferenceItemView> PreferredAlbums;
        private readonly ObservableCollection<PreferenceItemView> PreferredPlaylists;
        private readonly ObservableCollection<PreferenceItemView> PreferredFolders;
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
            RecentAddedPreferenceCheckBox.IsChecked = Settings.settings.Preference.RecentAdded;
            MyFavoritePreferenceCheckBox.IsChecked = Settings.settings.Preference.MyFavorites;
            MostPlayedPreferenceCheckBox.IsChecked = Settings.settings.Preference.MostPlayed;
            LeastPlayedPreferenceCheckBox.IsChecked = Settings.settings.Preference.LeastPlayed;

            PreferredSongs = ConvertToViews(Settings.settings.Preference.PreferredSongs, PreferType.Song);
            PreferredArtists = ConvertToViews(Settings.settings.Preference.PreferredArtists, PreferType.Artist);
            PreferredAlbums = ConvertToViews(Settings.settings.Preference.PreferredAlbums, PreferType.Album);
            PreferredPlaylists = ConvertToViews(Settings.settings.Preference.PreferredPlaylists, PreferType.Playlist);
            PreferredFolders = ConvertToViews(Settings.settings.Preference.PreferredFolders, PreferType.Folder);

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
        }

        private ObservableCollection<PreferenceItemView> ConvertToViews(List<PreferenceItem> items, PreferType preferType)
        {
            return new ObservableCollection<PreferenceItemView>(items.AsParallel().Select(i => BuildView(i, preferType)));
        }

        private PreferenceItemView BuildView(PreferenceItem item, PreferType type)
        {
            PreferenceItemView view = item.AsView();
            view.PreferType = type;
            switch (type)
            {
                case PreferType.Song:
                    Music music = Settings.settings.Tree.FindMusic(view.Id);
                    if (music == null)
                    {
                        view.IsValid = false;
                    }
                    else
                    {
                        view.Name = music.Name;
                    }
                    break;
                case PreferType.Artist:
                    view.Name = view.Id;
                    view.IsValid = MusicLibraryPage.AllSongs.Any(i => i.Artist == view.Id);
                    break;
                case PreferType.Album:
                    string[] albumId = view.Id.Split(Helpers.TileHelper.StringConcatenationFlag);
                    if (albumId.Length > 1)
                    {
                        string album = albumId[0], artist = albumId[1];
                        view.IsValid = MusicLibraryPage.AllSongs.Any(i => i.Album == album && i.Artist == artist);
                    }
                    break;
                case PreferType.Playlist:
                    view.Name = view.Id;
                    view.IsValid = Settings.settings.Playlists.Any(i => i.Name == view.Id);
                    break;
                case PreferType.Folder:
                    FolderTree tree = Settings.settings.Tree.FindTree(view.Id);
                    if (tree == null)
                    {
                        view.IsValid = false;
                    }
                    else
                    {
                        view.Name = tree.Directory;
                    }
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

        private void MyFavoritePreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MyFavorites = true;
        }

        private void MyFavoritePreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MyFavorites = false;
        }

        private void MostPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MostPlayed = true;
        }

        private void MostPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.MostPlayed = false;
        }

        private void LeastPlayedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.LeastPlayed = true;
        }

        private void LeastPlayedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.LeastPlayed = false;
        }

        private void RecentAddedPreferenceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.RecentAdded = true;
        }

        private void RecentAddedPreferenceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.settings.Preference.RecentAdded = false;
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

        private void ClearInvalid(PreferType preferType)
        {
            ObservableCollection<PreferenceItemView> views = GetPreferenceViewByType(preferType);
            List<PreferenceItem> models = GetPreferenceByType(preferType);
            HashSet<string> invalidIds = views.Where(i => !i.IsValid).Select(i => i.Id).ToHashSet();
            views.RemoveAll(i => invalidIds.Contains(i.Id));
            models.RemoveAll(i => invalidIds.Contains(i.Id));
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
            GetPreferenceByType(view.PreferType).RemoveAll(i => i.Id == view.Id);
            GetPreferenceViewByType(view.PreferType).RemoveAll(i => i.Id == view.Id);
        }

        private ObservableCollection<PreferenceItemView> GetPreferenceViewByType(PreferType preferType)
        {
            switch (preferType)
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

        private void PreferredSongsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PreferredListView_ItemClick(sender, e,
                                        async (view) =>
                                        {
                                            MusicDialog dialog = new MusicDialog(MusicDialogOption.Properties, Settings.settings.Tree.FindMusic(view.Id));
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
                                        view => MainPage.Instance.NavigateToPage(typeof(LocalPage), Settings.settings.Tree.FindTree(view.Id)));
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
                Helper.ShowNotificationWithoutLocalization(Helper.LocalizeText("InvalidItemToolTip", view.Name));
            }
        }

        private List<PreferenceItem> GetPreferenceByType(PreferType preferType)
        {
            switch (preferType)
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

        private void IsEnabledToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            toggleSwitch.SetToolTip(toggleSwitch.IsOn ? "IsEnabledToggleSwitchOnToolTip" : "IsEnabledToggleSwitchOffToolTip");
            if (toggleSwitch.DataContext is PreferenceItem preferenceItem)
            {
                preferenceItem.IsEnabled = !preferenceItem.IsEnabled;
            }
        }

        private void PreferenceListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = GetRowBackground(args.ItemIndex);
        }

        public static Brush GetRowBackground(int index)
        {
            return index % 2 == 0 ? ColorHelper.WhiteSmokeBrush : ColorHelper.WhiteBrush;
        }
    }
}
