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
        public MenuFlyout GetAddToMenuFlyout()
        {
            var flyout = new MenuFlyout();
            foreach (var item in GetAddToMenuFlyoutSubItem().Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public MenuFlyoutSubItem GetAddToMenuFlyoutSubItem()
        {
            MenuFlyoutSubItem subItem = new MenuFlyoutSubItem()
            {
                Text = "Add To",
                Name = AddToSubItemName
            };
            var nowPlaying = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uEC4F" },
                Text = "Now Playing"
            };
            nowPlaying.Click += (sender, args) =>
            {
                PlaylistControl.AddMusic(Data);
            };
            subItem.Items.Add(nowPlaying);
            var myFavorites = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uEB51" },
                Text = "My Favorites"
            };
            myFavorites.Click += (sender, args) =>
            {
                //if (target is Music)
                //    PlaylistControl.AddMusic(target as Music);
                //else
                //    PlaylistControl.AddMusic(target as ICollection<Music>);
            };
            subItem.Items.Add(myFavorites);
            subItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var item in GetAddToPlaylistsMenuFlyout("").Items)
                subItem.Items.Add(item);
            return subItem;
        }

        public MenuFlyout GetAddToPlaylistsMenuFlyout(string OldName)
        {
            var flyout = new MenuFlyout();
            var newPlaylist = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Text = "New Playlist"
            };
            newPlaylist.Click += async (sender, args) =>
            {
                var listener = new VirtualRenameActionListener() { Data = Data };
                RenameDialog dialog = new RenameDialog(listener, TitleOption.NewPlaylist, OldName);
                listener.Dialog = dialog;
                await dialog.ShowAsync();
            };
            flyout.Items.Add(newPlaylist);
            foreach (var playlist in Settings.settings.Playlists)
            {
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

        public MenuFlyout GetPlaylistMenuFlyout()
        {
            var flyout = new MenuFlyout();
            var shuffleItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Shuffle),
                Text = "Shuffle",
                Name = PlaylistMenuName
            };
            shuffleItem.Click += (s, args) =>
            {
                PlaylistControl.SetPlaylist(Data as ICollection<Music>);
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem());
            return flyout;
        }
        public MenuFlyout GetMusicMenuFlyout()
        {
            var flyout = new MenuFlyout();
            var playItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Play),
                Text = "Play",
                Name = MusicMenuName
            };
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
            flyout.Items.Add(deleteItem);
            var musicInfoItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.MusicInfo),
                Text = "Music Info"
            };
            musicInfoItem.Click += (s, args) =>
            {

            };
            flyout.Items.Add(musicInfoItem);
            return flyout;
        }
        public MenuFlyout GetRemovableMusicMenuFlyout()
        {
            var flyout = GetMusicMenuFlyout();
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = "Remove From Playlist"
            };
            removeItem.Click += (sender, args) =>
            {

            };
            flyout.Items.Insert(2, removeItem);
            return flyout;
        }

        public static void InsertPlaylistMenu(object sender, int index = 0)
        {
            InsertMenu((helper) => helper.GetPlaylistMenuFlyout(), sender, index);
        }
        public static void InsertMusicMenu(object sender, int index = 0)
        {
            InsertMenu((helper) => helper.GetMusicMenuFlyout(), sender, index);
        }
        public static void InsertRemovableMusicMenu(object sender, int index = 0)
        {
            InsertMenu((helper) => helper.GetRemovableMusicMenuFlyout(), sender, index);
        }
        private static void InsertMenu(Func<MenuFlyoutHelper, MenuFlyout> GetMenu, object sender, int index = 0)
        {
            var flyout = sender as MenuFlyout;
            var helper = new MenuFlyoutHelper() { Data = (flyout.Target as Windows.UI.Xaml.FrameworkElement).DataContext };
            var items = GetMenu(helper).Items;
            if (flyout.Items.Count >= index + items.Count && flyout.Items[index].Name == MusicMenuName)
            {
                for (int i = 0; i < items.Count; i++)
                    flyout.Items[index + i] = items[i];
            }
            else
            {
                foreach (var item in items.Reverse())
                    flyout.Items.Insert(index, item);
            }
        }
    }

    public interface AddToMenuItemClickListener
    {

    }
}
