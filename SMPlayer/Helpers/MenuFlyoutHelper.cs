using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    class MenuFlyoutHelper
    {
        public object Data { get; set; }
        public string DefaultPlaylistName { get; set; }
        public const string AddToSubItemName = "AddToSubItem", PlaylistMenuName = "ShuffleItem", MusicMenuName = "PlayItem",
                            NowPlaying = "Now Playing", MyFavorites = "My Favorites";
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
                Text = "Add To",
                Name = AddToSubItemName
            };
            addToItem.SetToolTip("Add To Playlist");
            if (playlistName != NowPlaying)
            {
                var nowPlayingItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEC4F" },
                    Text = "Now Playing"
                };
                nowPlayingItem.Click += async (sender, args) =>
                {
                    if (Data is Music)
                        await MediaHelper.AddMusic(Data as Music);
                    else if (Data is ICollection<Music>)
                        MediaHelper.AddMusic(Data as ICollection<Music>);
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (playlistName != MyFavorites)
            {
                var favItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEB51" },
                    Text = "My Favorites"
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
                Text = "New Playlist"
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
                Text = "Shuffle",
                Name = PlaylistMenuName
            };
            shuffleItem.SetToolTip("Shuffle and Play");
            shuffleItem.Click += (s, args) =>
            {
                MediaHelper.ShuffleAndPlay(Data as ICollection<Music>);
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
                Text = "Show In Explorer"
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
            var playItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Play),
                Text = "Play",
                Name = MusicMenuName
            };
            playItem.SetToolTip($"Play {music.Name}");
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
                Text = "Delete From Disk"
            };
            deleteItem.Click += async (s, args) =>
            {
                await new RemoveDialog()
                {
                    Message = $"You are deleting {music.Name} from your device.\nThis operation is irrevertible.\nDo you want to continue?",
                    Confirm = async () =>
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync(music.Path);
                        await file.DeleteAsync();
                        MusicLibraryPage.AllSongs.Remove(music);
                        Settings.settings.DeleteMusic(music);
                        MediaHelper.RemoveMusic(music);
                        MainPage.Instance?.ShowNotification($"{music.Name} is deleted!");
                        NowPlayingFullPage.Instance?.ShowNotification($"{music.Name} is deleted!");
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
                Text = "Music Info"
            };
            musicInfoItem.Click += (s, args) =>
            {
                if (MediaHelper.CurrentMusic == null) return;
                MainPage.Instance.Frame.Navigate(typeof(NowPlayingFullPage));
                NowPlayingFullPage.Instance.MusicInfoRequested(MediaHelper.CurrentMusic);
            };
            musicInfoItem.SetToolTip("Show Music Info");
            flyout.Items.Add(musicInfoItem);
            var lyricsItem = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uEC42" },
                Text = "Show Lyrics"
            };
            lyricsItem.Click += (s, args) =>
            {
                if (MediaHelper.CurrentMusic == null) return;
                MainPage.Instance.Frame.Navigate(typeof(NowPlayingFullPage));
                NowPlayingFullPage.Instance.LyricsRequested(MediaHelper.CurrentMusic);
            };
            lyricsItem.SetToolTip("Show Music Lyrics");
            flyout.Items.Add(lyricsItem);
            return flyout;
        }

        public MenuFlyout GetRemovableMusicMenuFlyout(MenuFlyoutItemClickListener listener = null)
        {
            var music = Data as Music;
            var flyout = GetMusicMenuFlyout();
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = "Remove From Playlist"
            };
            removeItem.Click += (sender, args) =>
            {
                if (music.Equals(MediaHelper.CurrentMusic))
                {
                    MediaHelper.NextMusic();
                    MediaHelper.RemoveMusic(music);
                }
            };
            removeItem.SetToolTip("Remove From Playlist");
            flyout.Items.Insert(2, removeItem);
            return flyout;
        }
        public static void InsertMusicItem(object sender, int index = 0)
        {
            var flyout = sender as MenuFlyout;
            var helper = new MenuFlyoutHelper();
            var addToItem = helper.GetAddToMenuFlyoutSubItem();
            var propertyItems = helper.GetMusicPropertiesMenuFlyout().Items;
            propertyItems.Insert(0, addToItem);
            foreach (var item in propertyItems)
                System.Diagnostics.Debug.WriteLine(item);
            if (flyout.Items[index].Name == AddToSubItemName)
            {
                for (int i = 0; i < propertyItems.Count; i++)
                    flyout.Items[i] = propertyItems[i];
            }
            else
            {
                foreach (var item in propertyItems.Reverse())
                    flyout.Items.Insert(index, item);
            }
        }
        public static MenuFlyout SetAddToMenu(object sender, string playlistName = "")
        {
            return SetMenu((helper) => helper.GetAddToMenuFlyout(playlistName), sender);
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
