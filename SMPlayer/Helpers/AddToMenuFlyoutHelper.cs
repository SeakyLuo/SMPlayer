using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    class AddToMenuFlyout
    {
        public object Data;
        public const string AddToSubItemName = "AddToSubItem";
        public MenuFlyout GetMenuFlyout()
        {
            var flyout = new MenuFlyout();
            foreach (var item in GetMenuFlyoutSubItem().Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public MenuFlyoutSubItem GetMenuFlyoutSubItem()
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
            foreach (var item in GetPlaylistsMenuFlyout("").Items)
                subItem.Items.Add(item);
            return subItem;
        }

        public MenuFlyout GetPlaylistsMenuFlyout(string OldName)
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
    }

    public interface AddToMenuItemClickListener
    {

    }
}
