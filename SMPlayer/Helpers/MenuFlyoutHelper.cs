using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    class MenuFlyoutHelper
    {
        public object Data;
        public const string AddToSubItemName = "AddToSubItem";
        public const string PlaylistMenuName = "ShuffleItem";
        public const string MusicMenuName = "PlayItem";
        public const string NowPlaying = "Now Playing";
        public const string MyFavorites = "My Favorites";
        public MenuFlyout GetAddToMenuFlyout(string playlistName = "")
        {
            var flyout = new MenuFlyout();
            foreach (var item in GetAddToMenuFlyoutSubItem(playlistName).Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public MenuFlyoutSubItem GetAddToMenuFlyoutSubItem(string PlaylistName = "")
        {
            MenuFlyoutSubItem addToItem = new MenuFlyoutSubItem()
            {
                Text = "Add To",
                Name = AddToSubItemName
            };
            ToolTipService.SetToolTip(addToItem, new ToolTip() { Content = "Add To Playlist" });
            if (PlaylistName != NowPlaying)
            {
                var nowPlayingItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEC4F" },
                    Text = "Now Playing"
                };
                nowPlayingItem.Click += (sender, args) =>
                {
                    PlaylistControl.AddMusic(Data);
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (PlaylistName != MyFavorites)
            {
                var favItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEB51" },
                    Text = "My Favorites"
                };
                favItem.Click += (sender, args) =>
                {
                    //if (target is Music)
                    //    PlaylistControl.AddMusic(target as Music);
                    //else
                    //    PlaylistControl.AddMusic(target as ICollection<Music>);
                };
                addToItem.Items.Add(favItem);

            }
            addToItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var item in GetAddToPlaylistsMenuFlyout("", PlaylistName).Items)
                addToItem.Items.Add(item);
            return addToItem;
        }

        public MenuFlyout GetAddToPlaylistsMenuFlyout(string DefaultName, string PlaylistName = "")
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
                var dialog = new RenameDialog(listener, TitleOption.NewPlaylist, DefaultName);
                listener.Dialog = dialog;
                await dialog.ShowAsync();
            };
            flyout.Items.Add(newPlaylistItem);
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (playlist.Name == PlaylistName) continue;
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
            ToolTipService.SetToolTip(shuffleItem, new ToolTip() { Content = "Shuffle and Play" });
            shuffleItem.Click += (s, args) =>
            {
                MediaHelper.SetPlaylist(Data as ICollection<Music>);
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem());
            return flyout;
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
            ToolTipService.SetToolTip(playItem, new ToolTip() { Content = $"Play {music.Name}" });
            playItem.Click += (s, args) =>
            {

            };
            flyout.Items.Add(playItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem());
            var deleteItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Text = "Delete From Disk"
            };
            deleteItem.Click += (s, args) =>
            {

            };
            ToolTipService.SetToolTip(deleteItem, new ToolTip() { Content = $"Delete {music.Name}" });
            flyout.Items.Add(deleteItem);
            var musicInfoItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.MusicInfo),
                Text = "Music Info"
            };
            musicInfoItem.Click += (s, args) =>
            {

            };
            ToolTipService.SetToolTip(deleteItem, new ToolTip() { Content = "Show Music Info" });
            flyout.Items.Add(musicInfoItem);
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
                    PlaylistControl.NowPlayingPlaylist.Remove(music);
                    MediaHelper.RemoveMusic(music);
                }
            };
            ToolTipService.SetToolTip(removeItem, new ToolTip() { Content = "Remove From Playlist" });
            flyout.Items.Insert(2, removeItem);
            return flyout;
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
            object dataContext;
            if (sender is MenuFlyout)
            {
                flyout = sender as MenuFlyout;
                dataContext = (flyout.Target as Windows.UI.Xaml.FrameworkElement).DataContext;
                flyout.Items.Clear();
            }
            else
            {
                flyout = new MenuFlyout();
                dataContext = (sender as Windows.UI.Xaml.FrameworkElement).DataContext;
            }
            helper = new MenuFlyoutHelper() { Data = FindMusic(dataContext) };
            var items = GetMenu(helper).Items;
            foreach (var item in items)
                flyout.Items.Add(item);
            return flyout;
        }

        private static object FindMusic(object obj)
        {
            if (obj is Music || obj is ICollection<Music>) return obj;
            else if (obj is ArtistView) return (obj as ArtistView).GetSongs();
            else if (obj is AlbumView) return (obj as AlbumView).Songs;
            else if (obj is Playlist) return (obj as Playlist).Songs;
            else if (obj is GridFolderView) return (obj as GridFolderView).GetSongs();
            else if (obj is GridMusicView) return (obj as GridMusicView).Source;
            return null;
        }
    }

    public interface MenuFlyoutItemClickListener
    {
        void Play();
        void Delete();

    }
}
