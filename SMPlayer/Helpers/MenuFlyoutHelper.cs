using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    public class MenuFlyoutHelper
    {
        public object Data { get; set; }
        public string DefaultPlaylistName { get; set; } = "";
        public const string AddToSubItemName = "AddToSubItem", PlaylistMenuName = "ShuffleItem", MusicMenuName = "PlayItem";
        public static string NowPlaying = Helper.Localize("Now Playing"), MyFavorites = Helper.Localize("My Favorites");
        public static bool IsBadNewPlaylistName(string name) { return name == NowPlaying || name == MyFavorites; }
        public MenuFlyout GetAddToMenuFlyout(string playlistName = "", MenuFlyoutItemClickListener listener = null)
        {
            var flyout = new MenuFlyout();
            foreach (var item in GetAddToMenuFlyoutSubItem(playlistName, listener).Items)
                flyout.Items.Add(item);
            return flyout;
        }
        public MenuFlyoutSubItem GetAddToMenuFlyoutSubItem(string playlistName = "", MenuFlyoutItemClickListener listener = null)
        {
            MenuFlyoutSubItem addToItem = new MenuFlyoutSubItem()
            {
                Text = Helper.Localize("Add To"),
                Name = AddToSubItemName
            };
            addToItem.SetToolTip("Add To Playlist");
            if (playlistName != NowPlaying)
            {
                var nowPlayingItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEC4F" },
                    Text = Helper.Localize("Now Playing")
                };
                nowPlayingItem.Click += (sender, args) =>
                {
                    if (Data is ICollection<Music> playlist)
                        foreach (var music in playlist)
                            MediaHelper.AddMusic(music);
                    else if (Data is Music music)
                        MediaHelper.AddMusic(music);
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (playlistName != MyFavorites && ((Data is Music m && !Settings.settings.MyFavorites.Contains(m)) ||
                                                (Data is ICollection<Music> list && list.Any(music => !Settings.settings.MyFavorites.Contains(music)))))
            {
                var favItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEB51" },
                    Text = Helper.Localize("My Favorites")
                };
                favItem.Click += (sender, args) =>
                {
                    if (Data is Music music)
                        Settings.settings.LikeMusic(music);
                    else if (Data is ICollection<Music>)
                        Settings.settings.LikeMusic(Data as ICollection<Music>);
                    listener?.Favorite(Data);
                };
                addToItem.Items.Add(favItem);
            }
            if (addToItem.Items.Count > 0) addToItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var item in GetAddToPlaylistsMenuFlyout(playlistName).Items)
                addToItem.Items.Add(item);
            return addToItem;
        }
        public MenuFlyout GetAddToPlaylistsMenuFlyout(string CurrentPlaylistName = "")
        {
            var flyout = new MenuFlyout();
            var newPlaylistItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Text = Helper.Localize("New Playlist")
            };
            newPlaylistItem.Click += async (sender, args) =>
            {
                var listener = new VirtualRenameActionListener() { Data = Data };
                var dialog = new RenameDialog(listener, RenameOption.New, DefaultPlaylistName);
                listener.Dialog = dialog;
                await dialog.ShowAsync();
            };
            newPlaylistItem.SetToolTip("Create a New Playlist");
            flyout.Items.Add(newPlaylistItem);
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (playlist.Name == CurrentPlaylistName ||
                    Data is Music music && playlist.Contains(music)) continue;
                var item = new MenuFlyoutItem()
                {
                    Icon = new SymbolIcon(Symbol.Audio),
                    Text = playlist.Name
                };
                item.Click += (sender, args) =>
                {
                    playlist.Add(Data);
                };
                flyout.Items.Add(item);
            }
            return flyout;
        }

        public MenuFlyout GetPlaylistMenuFlyout(MenuFlyoutItemClickListener listener = null)
        {
            var flyout = new MenuFlyout();
            var shuffleItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Shuffle),
                Text = Helper.Localize("Shuffle"),
                Name = PlaylistMenuName
            };
            shuffleItem.SetToolTip("Shuffle and Play");
            shuffleItem.Click += (s, args) =>
            {
                MediaHelper.ShuffleAndPlay(Data as ICollection<Music>);
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem("", listener));
            return flyout;
        }
        public static MenuFlyoutItem GetShowInExplorerItem(string path, StorageItemTypes type)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uE838" },
                Text = Helper.Localize("Show In Explorer")
            };
            item.Click += async (s, args) =>
            {
                if (await Helper.FileNotExist(path))
                {
                    Helper.ShowAddMusicResultNotification(Music.GetFilename(path));
                    return;
                }
                ShowInExplorer(path, type);
            };
            item.SetToolTip("Show In Explorer");
            return item;
        }
        public static async void ShowInExplorer(string path, StorageItemTypes type)
        {
            var options = new Windows.System.FolderLauncherOptions();
            StorageFolder folder;
            switch (type)
            {
                case StorageItemTypes.File:
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    options.ItemsToSelect.Add(file);
                    folder = await file.GetParentAsync();
                    break;
                case StorageItemTypes.Folder:
                    folder = await StorageFolder.GetFolderFromPathAsync(path);
                    options.ItemsToSelect.Add(folder);
                    break;
                case StorageItemTypes.None:
                default:
                    return;
            }
            await Windows.System.Launcher.LaunchFolderAsync(folder, options);
        }
        public static MenuFlyoutItem GetRefreshDirectoryMenuFlyout(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Refresh),
                Text = Helper.Localize("Refresh Directory")
            };
            item.Click += (s, args) =>
            {
                SettingsPage.CheckNewMusic(tree, afterTreeUpdated);
            };
            item.SetToolTip("Refresh Directory");
            return item;
        }
        public MenuFlyout GetMusicMenuFlyout(MenuFlyoutItemClickListener listener = null, bool withNavigation = true)
        {
            var music = Data as Music;
            var flyout = new MenuFlyout();
            var localizedPlay = Helper.Localize("Play");
            var playItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Play),
                Text = localizedPlay,
                Name = MusicMenuName
            };
            playItem.SetToolTip(localizedPlay + Helper.LocalizeMessage("MusicName", music.Name));
            playItem.Click += (s, args) =>
            {
                MediaHelper.SetMusicAndPlay(music);
            };
            flyout.Items.Add(playItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem());
            flyout.Items.Add(GetShowInExplorerItem(music.Path, StorageItemTypes.File));
            var deleteItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Text = Helper.Localize("Delete From Disk")
            };
            deleteItem.Click += async (s, args) =>
            {
                if (await Helper.FileNotExist(music.Path))
                {
                    Helper.ShowAddMusicResultNotification(music.Name);
                    return;
                }
                await new RemoveDialog()
                {
                    Message = Helper.LocalizeMessage("DeleteMusicMessage", music.Name),
                    Confirm = async () =>
                    {
                        MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                        StorageFile file = await StorageFile.GetFileFromPathAsync(music.Path);
                        await file.DeleteAsync();
                        MusicLibraryPage.AllSongs.Remove(music);
                        Settings.settings.RemoveMusic(music);
                        MediaHelper.DeleteMusic(music);
                        listener?.Delete(music);
                        MainPage.Instance?.Loader.Hide();
                        Helper.ShowNotificationWithoutLocalization(Helper.LocalizeMessage("MusicDeleted", music.Name));
                    }
                }.ShowAsync();
            };
            deleteItem.SetToolTip(Helper.LocalizeMessage("DeleteMusic", music.Name), false);
            flyout.Items.Add(deleteItem);
            foreach (var item in GetMusicPropertiesMenuFlyout(withNavigation).Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public MenuFlyout GetMusicPropertiesMenuFlyout(bool withNavigation = true)
        {
            var music = Settings.FindMusic((Music)Data) ?? (Music)Data;
            var flyout = new MenuFlyout();
            if (withNavigation)
            {
                var artistItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uE8D4" },
                    Text = Helper.Localize("See Artist")
                };
                artistItem.Click += (s, args) =>
                {
                    MainPage.Instance.NavigateToPage(typeof(ArtistsPage), music.Artist);
                };
                flyout.Items.Add(artistItem);
                var albumItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uE93C" },
                    Text = Helper.Localize("See Album")
                };
                albumItem.Click += (s, args) =>
                {
                    MainPage.Instance.NavigateToPage(typeof(AlbumPage), music.GetAlbumNavigationString());
                };
                flyout.Items.Add(albumItem);
            }
            var musicInfoItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.MusicInfo),
                Text = Helper.Localize("See Music Info")
            };
            musicInfoItem.Click += async (s, args) =>
            {
                if (await Helper.FileNotExist(music.Path))
                {
                    Helper.ShowAddMusicResultNotification(music.Name);
                    return;
                }
                if (NowPlayingFullPage.Instance == null) await new MusicDialog(MusicDialogOption.Properties, music).ShowAsync();
                else NowPlayingFullPage.Instance.MusicInfoRequested(music);
            };
            flyout.Items.Add(musicInfoItem);
            var lyricsItem = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uEC42" },
                Text = Helper.Localize("See Lyrics")
            };
            lyricsItem.Click += async (s, args) =>
            {
                if (await Helper.FileNotExist(music.Path))
                {
                    Helper.ShowAddMusicResultNotification(music.Name);
                    return;
                }
                if (NowPlayingFullPage.Instance == null) await new MusicDialog(MusicDialogOption.Lyrics, music).ShowAsync();
                else NowPlayingFullPage.Instance.LyricsRequested(music);
            };
            flyout.Items.Add(lyricsItem);
            if (NowPlayingFullPage.Instance == null)
            {
                var albumArtItem = new MenuFlyoutItem()
                {
                    Icon = new SymbolIcon(Symbol.Pictures),
                    Text = Helper.Localize("See Album Art")
                };
                albumArtItem.Click += async (s, args) =>
                {
                    if (await Helper.FileNotExist(music.Path))
                    {
                        Helper.ShowAddMusicResultNotification(music.Name);
                        return;
                    }
                    await new MusicDialog(MusicDialogOption.AlbumArt, music).ShowAsync();
                };
                flyout.Items.Add(albumArtItem);
            }
            return flyout;
        }

        public MenuFlyout GetRemovableMusicMenuFlyout(MenuFlyoutItemClickListener listener = null)
        {
            var music = Data as Music;
            var flyout = GetMusicMenuFlyout(listener, false);
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = Helper.Localize("Remove From Playlist")
            };
            removeItem.Click += (sender, args) =>
            {
                listener?.Remove(music);
            };
            removeItem.SetToolTip(Helper.LocalizeMessage("RemoveFromPlaylist", music.Name), false);
            flyout.Items.Insert(2, removeItem);
            return flyout;
        }
        public static MenuFlyoutSubItem GetSortByMenu(Dictionary<SortBy, Action> actions, Action reverse = null)
        {
            var sortByItem = new MenuFlyoutSubItem() { Text = Helper.Localize("Sort") };
            var reverseItem = new MenuFlyoutItem() { Text = Helper.Localize("Reverse Playlist") };
            reverseItem.Click += (send, args) => reverse?.Invoke();
            sortByItem.Items.Add(reverseItem);
            sortByItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var criterion in Playlist.Criteria)
            {
                if (actions.TryGetValue(criterion, out Action action))
                {
                    string sortby = Helper.Localize("Sort By " + criterion.ToStr());
                    var item = new MenuFlyoutItem()
                    {
                        Text = sortby,
                    };
                    item.Click += (send, args) => action.Invoke();
                    sortByItem.Items.Add(item);
                }
            }
            return sortByItem;
        }
        public static void SetPlaylistSortByMenu(object sender, Playlist playlist)
        {
            var flyout = new MenuFlyout();
            var reverseItem = new MenuFlyoutItem() { Text = Helper.LocalizeMessage("Reverse Playlist") };
            reverseItem.Click += (send, args) => playlist.Reverse();
            flyout.Items.Add(reverseItem);
            flyout.Items.Add(new MenuFlyoutSeparator());
            foreach (var criterion in Playlist.Criteria)
            {
                string sortby = Helper.LocalizeMessage("Sort By " + criterion.ToStr());
                var radioItem = new ToggleMenuFlyoutItem()
                {
                    Text = sortby,
                    IsChecked = playlist.Criterion == criterion
                };
                radioItem.Click += (send, args) =>
                {
                    playlist.SetCriterionAndSort(criterion);
                    (sender as IconTextButton).Label = sortby;
                };
                flyout.Items.Add(radioItem);
            }
            flyout.ShowAt(sender as IconTextButton);
        }
        public static void SetSearchSortByMenu(object sender, SortBy criterion, SortBy[] criteria, Action<SortBy> onSelected)
        {
            var flyout = new MenuFlyout();
            flyout.Items.Clear();
            foreach (var item in criteria)
            {
                string sortby = Helper.LocalizeMessage("Sort By " + item.ToStr());
                var radioItem = new ToggleMenuFlyoutItem()
                {
                    Text = sortby,
                    IsChecked = item == criterion
                };
                radioItem.Click += (send, args) => onSelected.Invoke(item);
                flyout.Items.Add(radioItem);
            }
            flyout.ShowAt(sender as FrameworkElement);
        }
        public static MenuFlyout SetPlaylistMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu(helper => helper.GetPlaylistMenuFlyout(listener), sender);
        }
        public static MenuFlyout SetMusicMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu(helper => helper.GetMusicMenuFlyout(listener), sender);
        }
        public static MenuFlyout SetRemovableMusicMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu(helper => helper.GetRemovableMusicMenuFlyout(listener), sender);
        }
        private static MenuFlyout SetMenu(Func<MenuFlyoutHelper, MenuFlyout> GetMenu, object sender)
        {
            MenuFlyout flyout;
            MenuFlyoutHelper helper;
            object data;
            if (sender is MenuFlyout)
            {
                flyout = sender as MenuFlyout;
                data = (flyout.Target as Windows.UI.Xaml.FrameworkElement).DataContext;
                flyout.Items.Clear();
            }
            else
            {
                flyout = new MenuFlyout();
                data = (sender as Windows.UI.Xaml.FrameworkElement).DataContext;
            }
            helper = new MenuFlyoutHelper()
            {
                Data = FindMusic(data),
                DefaultPlaylistName = Settings.settings.FindNextPlaylistName(FindPlaylistName(data))
            };
            var items = GetMenu(helper).Items;
            flyout.Items.Clear();
            foreach (var item in items)
                flyout.Items.Add(item);
            return flyout;
        }

        private static object FindMusic(object obj)
        {
            if (obj is Music || obj is ICollection<Music>) return obj;
            else if (obj is ArtistView artist) return artist.Songs;
            else if (obj is AlbumView album) return album.Songs;
            else if (obj is Playlist playlist) return playlist.Songs;
            else if (obj is GridFolderView folder) return folder.Songs;
            else if (obj is GridMusicView gridMusic) return gridMusic.Source;
            else if (obj is TreeViewNode node)
            {
                if (node.Content is FolderTree tree) return tree.Files;
                return node.Content as Music;
            }
            return null;
        }

        private static string FindPlaylistName(object obj)
        {
            if (obj is ArtistView artist) return artist.Name;
            else if (obj is AlbumView album) return album.Name;
            else if (obj is Playlist playlist) return playlist.Name;
            else if (obj is GridFolderView folder) return folder.Name;
            else if (obj is TreeViewNode node && node.Content is FolderTree tree) return tree.Directory;
            return "";
        }
    }

    public interface MenuFlyoutItemClickListener
    {
        void Favorite(object data);
        void Delete(Music music);
        void Remove(Music music);
    }
}
