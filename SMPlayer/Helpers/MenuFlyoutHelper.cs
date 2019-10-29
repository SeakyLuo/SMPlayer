using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    class MenuFlyoutHelper
    {
        public object Data { get; set; }
        public string DefaultPlaylistName { get; set; }
        public const string AddToSubItemName = "AddToSubItem", PlaylistMenuName = "ShuffleItem", MusicMenuName = "PlayItem",
                            NowPlaying = "Now Playing", MyFavorites = "My Favorites";
        public static bool IsBadNewPlaylistName(string name) { return name == NowPlaying || name == MyFavorites; }
        public MenuFlyout GetAddToMenuFlyout(string playlistName = "")
        {
            var flyout = new MenuFlyout();
            foreach (var item in GetAddToMenuFlyoutSubItem(playlistName).Items)
                flyout.Items.Add(item);
            return flyout;
        }
        public MenuFlyoutSubItem GetAddToMenuFlyoutSubItem(string playlistName = "")
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
                nowPlayingItem.Click += async (sender, args) =>
                {
                    if (Data is ICollection<Music> playlist)
                        foreach (var music in playlist)
                            await MediaHelper.AddMusic(music);
                    else if (Data is Music music)
                        await MediaHelper.AddMusic(music);
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (playlistName != MyFavorites)
            {
                var favItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEB51" },
                    Text = Helper.Localize("My Favorites")
                };
                favItem.Click += (sender, args) =>
                {
                    if (Data is Music)
                        Settings.settings.LikeMusic(Data as Music);
                    else if (Data is ICollection<Music>)
                        Settings.settings.LikeMusic(Data as ICollection<Music>);
                };
                addToItem.Items.Add(favItem);
            }
            addToItem.Items.Add(new MenuFlyoutSeparator());
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
                if (playlist.Name == CurrentPlaylistName) continue;
                var item = new MenuFlyoutItem()
                {
                    Icon = new SymbolIcon(Symbol.Audio),
                    Text = playlist.Name
                };
                item.Click += (sender, args) =>
                {
                    var target = Settings.settings.Playlists.Find((p) => p.Name == item.Text);
                    target.Add(Data);
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
                //MediaHelper.ShuffleAndPlay(Data as ICollection<Music>);
                MainPage.Instance.ShowAddMusicResultNotification(Data as ICollection<Music>);
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem());
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
            };
            item.SetToolTip("Show In Explorer");
            return item;
        }
        public MenuFlyout GetMusicMenuFlyout(MenuFlyoutItemClickListener listener = null)
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
            playItem.Click += async (s, args) =>
            {
                await MediaHelper.SetMusicAndPlay(music);
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
                await new RemoveDialog()
                {
                    Message = Helper.LocalizeMessage("DeleteMusic", music.Name),
                    Confirm = async () =>
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(music.Path);
                        await file.DeleteAsync();
                        MusicLibraryPage.AllSongs.Remove(music);
                        Settings.settings.DeleteMusic(music);
                        MediaHelper.RemoveMusic(music);
                        var notification = Helper.LocalizeMessage("MusicDeleted", music.Name);
                        MainPage.Instance?.ShowNotification(notification);
                        NowPlayingFullPage.Instance?.ShowNotification(notification);
                    }
                }.ShowAsync();
            };
            deleteItem.SetToolTip($"Delete {music.Name}");
            flyout.Items.Add(deleteItem);
            foreach (var item in GetMusicPropertiesMenuFlyout().Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public MenuFlyout GetMusicPropertiesMenuFlyout()
        {
            var music = Data as Music;
            var flyout = new MenuFlyout();
            var musicInfoItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.MusicInfo),
                Text = Helper.Localize("Show Music Info")
            };
            musicInfoItem.Click += async (s, args) =>
            {
                if (NowPlayingFullPage.Instance == null) await new MusicDialog(MusicDialogOption.Properties, music).ShowAsync();
                else NowPlayingFullPage.Instance.MusicInfoRequested(music);
            };
            flyout.Items.Add(musicInfoItem);
            var lyricsItem = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uEC42" },
                Text = Helper.Localize("Show Lyrics")
            };
            lyricsItem.Click += async (s, args) =>
            {
                if (NowPlayingFullPage.Instance == null) await new MusicDialog(MusicDialogOption.Lyrics, music).ShowAsync();
                else NowPlayingFullPage.Instance.LyricsRequested(music);
            };
            flyout.Items.Add(lyricsItem);
            return flyout;
        }

        public MenuFlyout GetRemovableMusicMenuFlyout(MenuFlyoutItemClickListener listener = null)
        {
            var music = Data as Music;
            var flyout = GetMusicMenuFlyout();
            var localizedRemove = Helper.Localize("Remove From Playlist");
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = localizedRemove
            };
            removeItem.Click += (sender, args) =>
            {
                if (music == MediaHelper.CurrentMusic)
                    MediaHelper.NextMusic();
                MediaHelper.RemoveMusic(music);
            };
            removeItem.SetToolTip(localizedRemove, false);
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
            var reverseItem = new MenuFlyoutItem() { Text = Helper.Localize("Reverse Playlist") };
            reverseItem.Click += (send, args) => playlist.Reverse();
            flyout.Items.Add(reverseItem);
            flyout.Items.Add(new MenuFlyoutSeparator());
            foreach (var criterion in Playlist.Criteria)
            {
                string sortby = Helper.Localize("Sort By " + criterion.ToStr());
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
        public static MenuFlyout SetPlaylistMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu((helper) => helper.GetPlaylistMenuFlyout(listener), sender);
        }
        public static MenuFlyout SetMusicMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu((helper) => helper.GetMusicMenuFlyout(listener), sender);
        }
        public static MenuFlyout SetRemovableMusicMenu(object sender, MenuFlyoutItemClickListener listener = null)
        {
            return SetMenu((helper) => helper.GetRemovableMusicMenuFlyout(listener), sender);
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
            foreach (var item in items)
                flyout.Items.Add(item);
            return flyout;
        }

        private static object FindMusic(object obj)
        {
            if (obj is Music || obj is ICollection<Music>) return obj;
            else if (obj is ArtistView) return (obj as ArtistView).Songs;
            else if (obj is AlbumView) return (obj as AlbumView).Songs;
            else if (obj is Playlist) return (obj as Playlist).Songs;
            else if (obj is GridFolderView) return (obj as GridFolderView).Songs;
            else if (obj is GridMusicView) return (obj as GridMusicView).Source;
            else if (obj is TreeViewNode node)
            {
                if (node.Content is FolderTree tree) return tree.Files;
                return node.Content as Music;
            }
            return null;
        }

        private static string FindPlaylistName(object obj)
        {
            if (obj is ArtistView) return (obj as ArtistView).Name;
            else if (obj is AlbumView) return (obj as AlbumView).Name;
            else if (obj is Playlist) return (obj as Playlist).Name;
            else if (obj is GridFolderView) return (obj as GridFolderView).Name;
            else if (obj is TreeViewNode node && node.Content is FolderTree tree) return tree.Directory;
            return "";
        }
    }

    public interface MenuFlyoutItemClickListener
    {
        void Play();
        void Delete();

    }
}
