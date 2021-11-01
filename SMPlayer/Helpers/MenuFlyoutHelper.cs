using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer
{
    public class MenuFlyoutHelper
    {
        public static List<IMenuFlyoutItemClickListener> ClickListeners = new List<IMenuFlyoutItemClickListener>();
        public const string AddToSubItemName = "AddToSubItem", PlaylistMenuName = "ShuffleAndPlayItem", MusicMenuName = "PlayItem",
                            ShuffleSubItemName = "ShuffleSubItem", SeeAlbumItemName = "SeeAlbumItemName";
        public object Data { get; set; }
        public string DefaultPlaylistName { get; set; } = "";
        public string CurrentPlaylistName { get; set; } = "";
        public static string NowPlaying = Helper.Localize("Now Playing"), MyFavorites = Helper.Localize("My Favorites");
        public static bool IsBadNewPlaylistName(string name) { return name == NowPlaying || name == MyFavorites; }
        public MenuFlyout GetAddToMenuFlyout(IMenuFlyoutItemClickListener listener = null)
        {
            return GetAddToMenuFlyoutSubItem(listener).ToMenuFlyout();
        }
        public MenuFlyoutSubItem GetAddToMenuFlyoutSubItem(IMenuFlyoutItemClickListener listener = null)
        {
            MenuFlyoutSubItem addToItem = new MenuFlyoutSubItem()
            {
                Text = Helper.Localize("Add To"),
                Name = AddToSubItemName
            };
            addToItem.SetToolTip("AddToToolTip");
            if (CurrentPlaylistName != NowPlaying)
            {
                var nowPlayingItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEC4F" },
                    Text = NowPlaying
                };
                nowPlayingItem.Click += async (sender, args) =>
                {
                    if (Data is IMusicable musicable)
                    {
                        Music music = musicable.ToMusic();
                        if (await Helper.FileNotExist(music.Path))
                        {
                            Helper.ShowMusicNotFoundNotification(music.Name);
                            return;
                        }
                        MediaHelper.AddMusic(music);
                        listener?.AddTo(Data, MediaHelper.CurrentPlaylist, MediaHelper.CurrentPlaylist.Count - 1, AddToCollectionType.NowPlaying);
                        foreach (var clickListener in ClickListeners)
                            clickListener.AddTo(Data, MediaHelper.CurrentPlaylist, MediaHelper.CurrentPlaylist.Count - 1, AddToCollectionType.NowPlaying);
                        Helper.ShowCancelableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, NowPlaying), () =>
                        {
                            MediaHelper.RemoveMusic(music);
                        });
                    }
                    else if (Data is IEnumerable<IMusicable> songs)
                    {
                        if (songs.Count() == 0) return;
                        foreach (var song in songs)
                            MediaHelper.AddMusic(song);
                        string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.ElementAt(0).ToMusic().Name, NowPlaying) :
                                                              Helper.LocalizeMessage("SongsAddedTo", songs.Count(), NowPlaying);
                        listener?.AddTo(Data, MediaHelper.CurrentPlaylist, MediaHelper.CurrentPlaylist.Count - songs.Count() - 1, AddToCollectionType.NowPlaying);
                        foreach (var clickListener in ClickListeners)
                            clickListener.AddTo(Data, MediaHelper.CurrentPlaylist, MediaHelper.CurrentPlaylist.Count - songs.Count() - 1, AddToCollectionType.NowPlaying);
                        Helper.ShowCancelableNotificationRaw(message, () =>
                        {
                            foreach (var song in songs)
                                MediaHelper.RemoveMusic(song.ToMusic());
                        });
                    }
                    else
                    {
                        return;
                    }
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (CurrentPlaylistName != MyFavorites && ((Data is IMusicable m && !Settings.settings.MyFavorites.Contains(m.ToMusic())) ||
                                                       (Data is IEnumerable<IMusicable> list &&
                                                        list.Any(music => !Settings.settings.MyFavorites.Contains(music.ToMusic())))))
            {
                var favItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uEB51" },
                    Text = MyFavorites
                };
                favItem.Click += async (sender, args) =>
                {
                    if (Data is IMusicable musicable)
                    {
                        Music music = musicable.ToMusic();
                        if (await Helper.FileNotExist(music.Path))
                        {
                            Helper.ShowMusicNotFoundNotification(music.Name);
                            return;
                        }
                        Settings.settings.LikeMusic(music);
                        Helper.ShowCancelableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, MyFavorites), () =>
                        {
                            Settings.settings.DislikeMusic(music);
                        });
                    }
                    else if (Data is IEnumerable<IMusicable> songs)
                    {
                        Settings.settings.LikeMusic(songs);
                        string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.ElementAt(0).ToMusic().Name, MyFavorites) :
                                                              Helper.LocalizeMessage("SongsAddedTo", songs.Count(), MyFavorites);
                        Helper.ShowCancelableNotificationRaw(message, () => 
                        {
                            foreach(var song in songs)
                                Settings.settings.DislikeMusic(song.ToMusic());
                        });
                    }
                    else
                    {
                        return;
                    }
                    listener?.Favorite(Data);
                };
                addToItem.Items.Add(favItem);
            }
            if (addToItem.Items.Count > 0) addToItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var item in GetAddToPlaylistsMenuFlyout(listener).Items)
                addToItem.Items.Add(item);
            return addToItem;
        }

        internal static MenuFlyoutItemBase GetRenameFolderItem(FolderTree tree, Action<object, object> p)
        {
            throw new NotImplementedException();
        }

        public MenuFlyout GetAddToPlaylistsMenuFlyout(IMenuFlyoutItemClickListener listener = null)
        {
            var flyout = new MenuFlyout();
            var newPlaylistItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Add),
                Text = Helper.Localize("New Playlist")
            };
            newPlaylistItem.Click += async (sender, args) =>
            {
                if (Data is IMusicable musicable)
                {
                    Music music = musicable.ToMusic();
                    if (await Helper.FileNotExist(music.Path))
                    {
                        Helper.ShowMusicNotFoundNotification(music.Name);
                        return;
                    }
                }
                var renameActionListener = new VirtualRenameActionListener
                {
                    Data = Data,
                    ConfirmAction = () => 
                    {
                        listener?.AddTo(Data, null, -1, AddToCollectionType.NewPlaylist);
                        foreach (var clickListener in ClickListeners)
                            clickListener?.AddTo(Data, null, -1, AddToCollectionType.NewPlaylist);
                    }
                };
                var dialog = new RenameDialog(renameActionListener, RenameOption.Create, RenameTarget.Playlist, DefaultPlaylistName);
                renameActionListener.Dialog = dialog;
                await dialog.ShowAsync();
            };
            newPlaylistItem.SetToolTip("NewPlaylistToolTip");
            flyout.Items.Add(newPlaylistItem);
            foreach (var playlist in Settings.settings.Playlists)
            {
                if (playlist.Name == CurrentPlaylistName ||
                    (Data is IMusicable m && playlist.Contains(m.ToMusic()))) continue;
                var item = new MenuFlyoutItem()
                {
                    Icon = new SymbolIcon(Symbol.Audio),
                    Text = playlist.Name
                };
                item.Click += async (sender, args) =>
                {
                    if (Data is IMusicable musicable)
                    {
                        Music music = musicable.ToMusic();
                        if (await Helper.FileNotExist(music.Path))
                        {
                            Helper.ShowMusicNotFoundNotification(music.Name);
                            return;
                        }
                        playlist.Add(music);
                        listener?.AddTo(Data, playlist, playlist.Count - 1, AddToCollectionType.Playlist);
                        foreach (var clickListener in ClickListeners)
                            clickListener.AddTo(Data, playlist, playlist.Count - 1, AddToCollectionType.Playlist);
                        Helper.ShowCancelableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, playlist.Name), () =>
                        {
                            playlist.Remove(music);
                        });
                    }
                    else if (Data is IEnumerable<IMusicable> songs)
                    {
                        if (songs.Count() == 0) return;
                        playlist.Add(Data);
                        string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.ElementAt(0).ToMusic().Name, playlist.Name) :
                                                              Helper.LocalizeMessage("SongsAddedTo", songs.Count(), playlist.Name);
                        listener?.AddTo(Data, playlist, playlist.Count - songs.Count() - 1, AddToCollectionType.Playlist);
                        foreach (var clickListener in ClickListeners)
                            clickListener.AddTo(Data, playlist, playlist.Count - songs.Count() - 1, AddToCollectionType.Playlist);
                        Helper.ShowCancelableNotificationRaw(message, () =>
                        {
                            playlist.Remove(songs);
                        });
                    }
                    else
                    {
                        return;
                    }
                };
                flyout.Items.Add(item);
            }
            return flyout;
        }
        public static MenuFlyoutSubItem GetShuffleSubItem()
        {
            MenuFlyoutSubItem subItem = GetShuffleMenu().ToSubItem();
            subItem.Name = ShuffleSubItemName;
            subItem.Text = Helper.Localize("RandomPlay");
            subItem.Icon = new SymbolIcon(Symbol.Shuffle);
            return subItem;
        }
        public MenuFlyout GetPlaylistMenuFlyout(IMenuFlyoutItemClickListener listener = null, MenuFlyoutOption option = null)
        {
            if (option == null) option = new MenuFlyoutOption() { ShowSelect = false };
            var flyout = new MenuFlyout();
            var shuffleItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Shuffle),
                Text = Helper.Localize("Shuffle"),
                Name = PlaylistMenuName
            };
            shuffleItem.SetToolTip("ShuffleAndPlay");
            shuffleItem.Click += (s, args) =>
            {
                MediaHelper.ShuffleAndPlay(Data as IEnumerable<Music>);
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem(listener));
            if (option.ShowMultiSelect) flyout.Items.Add(GetMultiSelectItem(listener, option.MultiSelectOption));
            else if (option.ShowSelect) flyout.Items.Add(GetSelectItem(listener, option.MultiSelectOption));
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
                if (path.Contains(".") && await Helper.FileNotExist(path))
                {
                    Helper.ShowMusicNotFoundNotification(Music.GetFilename(path));
                    return;
                }
                if (!path.Contains(".") && await Helper.FolderNotExist(path))
                {
                    Helper.ShowPathNotFoundNotification(path);
                    return;
                }
                ShowInExplorerWithLoader(path, type);
            };
            return item;
        }
        public static MenuFlyoutItem GetSelectItem(IMenuFlyoutItemClickListener listener = null, MultiSelectCommandBarOption option = null)
        {
            return GetMultiSelectMenuFlyoutItem("Select", listener, option);
        }
        public static MenuFlyoutItem GetMultiSelectItem(IMenuFlyoutItemClickListener listener = null, MultiSelectCommandBarOption option = null)
        {
            return GetMultiSelectMenuFlyoutItem("MultiSelect", listener, option);
        }
        private static MenuFlyoutItem GetMultiSelectMenuFlyoutItem(string text, IMenuFlyoutItemClickListener listener = null, MultiSelectCommandBarOption option = null)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uE762" },
                Text = Helper.Localize(text)
            };
            item.Click += (s, args) =>
            {
                listener?.Select((s as FrameworkElement).DataContext);
                Helper.ShowMultiSelectCommandBar(option);
            };
            return item;
        }
        public static void ShowInExplorerWithLoader(string path, StorageItemTypes type)
        {
            if (MainPage.Instance == null)
                ShowInExplorer(path, type);
            else 
                MainPage.Instance.Loader.ShowIndeterminant("ProcessRequest", false, () => ShowInExplorer(path, type));
        }
        private static async void ShowInExplorer(string path, StorageItemTypes type)
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

        public static MenuFlyoutItem GetRenameFolderItem(FolderTree tree, Func<string, string, Task<NamingError>> confirmAsync, Action<string, string> afterConfirmation)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Rename),
                Text = Helper.LocalizeText("RenameFolder")
            };
            item.Click += async (s, args) =>
            {
                await new RenameDialog(confirmAsync, RenameOption.Rename, RenameTarget.Folder, tree.Directory)
                {
                    AfterConfirmation = afterConfirmation
                }.ShowAsync();
            };
            return item;
        }

        public static MenuFlyoutItem GetRefreshDirectoryItem(FolderTree tree, Action<FolderTree> afterTreeUpdated = null)
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
            return item;
        }

        public static MenuFlyoutItem GetDeleteFolderItem(FolderTree tree, Action<FolderTree> afterTreeDeleted = null)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Delete),
                Text = Helper.LocalizeText("DeleteFolder")
            };
            item.Click += async (s, args) =>
            {
                await new RemoveDialog()
                {
                    Message = Helper.LocalizeMessage("DeleteFolder", tree.Directory),
                    CheckBoxVisibility = Visibility.Collapsed,
                    Confirm = async () =>
                    {

                        StorageFolder folder = await tree.GetStorageFolderAsync();
                        if (folder != null) await folder.DeleteAsync();
                        Settings.settings.DeleteFolder(tree);
                        MusicLibraryPage.AllSongs.RemoveAll(i => i.Path.StartsWith(tree.Path));
                        RecentPage.RecentAdded.DeleteFolder(tree.Path);
                        afterTreeDeleted?.Invoke(tree);
                        App.Save();
                     
                        Helper.ShowNotificationRaw(Helper.LocalizeMessage("FolderIsDeleted", tree.Directory));
                    }
                }.ShowAsync();
            };
            return item;
        }
        public static MenuFlyoutItem GetSearchDirectoryItem(FolderTree tree)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Find),
                Text = Helper.Localize("Search Directory")
            };
            item.Click += async (s, args) =>
            {
                await new InputDialog()
                {
                    Title = Helper.LocalizeMessage("Search"),
                    PlaceholderText = Helper.LocalizeMessage("SearchDirectoryHint", tree.Directory),
                    Confirm = (inputText) =>
                    {
                        MainPage.Instance.Search(new SearchKeyword()
                        {
                            Text = inputText,
                            Songs = tree.Flatten(),
                            Tree = tree
                        });
                    }
                }.ShowAsync();
            };
            return item;
        }
        public MenuFlyout GetMusicMenuFlyout(IMenuFlyoutItemClickListener listener = null, MenuFlyoutOption option = null)
        {
            if (option == null) option = new MenuFlyoutOption();
            Music music = (Data as IMusicable).ToMusic();
            var flyout = new MenuFlyout();
            if (!MediaHelper.IsPlaying || MediaHelper.CurrentMusic != music)
            {
                var localizedPlay = Helper.Localize("Play");
                var playItem = new MenuFlyoutItem()
                {
                    Icon = new SymbolIcon(Symbol.Play),
                    Text = localizedPlay,
                    Name = MusicMenuName
                };
                playItem.SetToolTip(localizedPlay + Helper.LocalizeMessage("MusicName", music.Name), false);
                playItem.Click += (s, args) =>
                {
                    MediaHelper.SetMusicAndPlay(music);
                };
                flyout.Items.Add(playItem);
            }
            if (MediaHelper.CurrentMusic != null && music != MediaHelper.CurrentMusic)
            {
                var playNextItem = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("PlayNext"),
                    Icon = new SymbolIcon(Symbol.Upload)
                };
                playNextItem.Click += (s, args) =>
                {
                    int index = MediaHelper.CurrentMusic.Index + 1;
                    if (music.Index >= 0)
                    {
                        MediaHelper.MoveMusic(music.Index, index);
                    }
                    else
                    {
                        MediaHelper.AddMusic(music, index);
                        listener?.AddTo(music, MediaHelper.CurrentPlaylist, index, AddToCollectionType.NowPlaying);
                        foreach (var clickListener in ClickListeners)
                            clickListener.AddTo(music, MediaHelper.CurrentPlaylist, index, AddToCollectionType.NowPlaying);
                    }
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("SetPlayNext", music.Name));
                };
                flyout.Items.Add(playNextItem);
            }
            flyout.Items.Add(GetAddToMenuFlyoutSubItem(listener));
            if (option.ShowRemove) flyout.Items.Add(GetRemovableMenuFlyoutItem(music, listener));
            if (option.ShowSelect || option.MultiSelectOption != null) flyout.Items.Add(GetSelectItem(listener, option.MultiSelectOption));
            flyout.Items.Add(GetPreferItem(music));
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
                    Message = Helper.LocalizeMessage("DeleteMusicMessage", music.Name),
                    Confirm = async () =>
                    {
                        MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                        listener?.Delete(music);
                        MusicLibraryPage.AllSongs.Remove(music);
                        Settings.settings.RemoveMusic(music);
                        MediaHelper.DeleteMusic(music);
                        RecentPage.RecentAdded.Remove(music);
                        if (!await Helper.FileNotExist(music.Path))
                        {
                            StorageFile file = await StorageFile.GetFileFromPathAsync(music.Path);
                            await file.DeleteAsync();
                        }
                        MainPage.Instance?.Loader.Hide();
                        Helper.ShowNotification(Helper.LocalizeMessage("MusicDeleted", music.Name));
                    }
                }.ShowAsync();
            };
            deleteItem.SetToolTip(Helper.LocalizeMessage("DeleteMusic", music.Name), false);
            flyout.Items.Add(deleteItem);
            foreach (var item in GetMusicPropertiesMenuFlyout(option.ShowSeeArtistsAndSeeAlbum).Items)
                flyout.Items.Add(item);
            return flyout;
        }

        public static MenuFlyoutItem GetSeeAlbumFlyout(Music music)
        {
            var albumItem = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uE93C" },
                Text = Helper.Localize("See Album"),
                Name = SeeAlbumItemName
            };
            albumItem.Click += async (s, args) =>
            {
                if (await Helper.FileNotExist(music.Path))
                {
                    Helper.ShowMusicNotFoundNotification(music.Name);
                    return;
                }
                string albumNavigationFlag = TileHelper.BuildAlbumNavigationFlag(music.Album, music.Artist);
                if (MainPage.Instance.CurrentPage == typeof(AlbumPage))
                    AlbumPage.Instance.LoadAlbum(albumNavigationFlag);
                else
                    MainPage.Instance.NavigateToPage(typeof(AlbumPage), albumNavigationFlag);
            };
            return albumItem;
        }

        public MenuFlyout GetMusicPropertiesMenuFlyout(bool ShowSeeArtistsAndSeeAlbum = true)
        {
            var music = (Data as IMusicable).ToMusic();
            var flyout = new MenuFlyout();
            if (ShowSeeArtistsAndSeeAlbum)
            {
                var artistItem = new MenuFlyoutItem()
                {
                    Icon = new FontIcon() { Glyph = "\uE8D4" },
                    Text = Helper.Localize("See Artist")
                };
                artistItem.Click += async (s, args) =>
                {
                    if (await Helper.FileNotExist(music.Path))
                    {
                        Helper.ShowMusicNotFoundNotification(music.Name);
                        return;
                    }
                    if (MainPage.Instance.CurrentPage == typeof(ArtistsPage))
                        ArtistsPage.Instance.SelectArtist(music.Artist);
                    else
                        MainPage.Instance.NavigateToPage(typeof(ArtistsPage), music.Artist);
                };
                flyout.Items.Add(artistItem);
                flyout.Items.Add(GetSeeAlbumFlyout(music));
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
                    Helper.ShowMusicNotFoundNotification(music.Name);
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
                    Helper.ShowMusicNotFoundNotification(music.Name);
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
                        Helper.ShowMusicNotFoundNotification(music.Name);
                        return;
                    }
                    await new MusicDialog(MusicDialogOption.AlbumArt, music).ShowAsync();
                };
                flyout.Items.Add(albumArtItem);
            }
            return flyout;
        }

        public static MenuFlyoutItem GetRemovableMenuFlyoutItem(Music music, IMenuFlyoutItemClickListener listener = null)
        {
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = Helper.Localize("Remove From List")
            };
            removeItem.Click += (sender, args) =>
            {
                listener?.Remove(music);
            };
            removeItem.SetToolTip(Helper.LocalizeMessage("RemoveFromList", music.Name), false);
            return removeItem;
        }

        public static MenuFlyoutSubItem GetSortByMenuSubItem(Dictionary<SortBy, Action> actions, Action reverse = null)
        {
            var sortByItem = new MenuFlyoutSubItem() { Text = Helper.Localize("Sort") };
            if (reverse != null)
            {
                var reverseItem = new MenuFlyoutItem() { Text = Helper.LocalizeMessage("Reverse Playlist") };
                reverseItem.Click += (sender, args) => reverse.Invoke();
                sortByItem.Items.Add(reverseItem);
                sortByItem.Items.Add(new MenuFlyoutSeparator());
            }
            foreach (var pair in actions)
            {
                string sortby = Helper.LocalizeMessage("Sort By " + pair.Key.ToStr());
                var item = new MenuFlyoutItem() { Text = sortby };
                item.Click += (sender, args) => pair.Value?.Invoke();
                sortByItem.Items.Add(item);
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
                };
                flyout.Items.Add(radioItem);
            }
            flyout.ShowAt(sender as FrameworkElement);
        }
        public static void SetSortByMenu(object sender, SortBy criterion, SortBy[] criteria, Action<SortBy> onSelected, Action reverse = null)
        {
            var flyout = new MenuFlyout();
            flyout.Items.Clear();
            if (reverse != null)
            {
                var reverseItem = new MenuFlyoutItem() { Text = Helper.LocalizeMessage("ReverseList") };
                reverseItem.Click += (send, args) => reverse.Invoke();
                flyout.Items.Add(reverseItem);
                flyout.Items.Add(new MenuFlyoutSeparator());
            }
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
        public static MenuFlyout GetShuffleMenu(int randomLimit = 100, Action callback = null)
        {
            var flyout = new MenuFlyout();
            var quickPlay = new MenuFlyoutItem()
            {
                Text = Helper.LocalizeText("QuickPlay")
            };
            quickPlay.Click += (sender, args) =>
            {
                MediaHelper.QuickPlay(randomLimit);
                callback?.Invoke();
            };
            quickPlay.SetToolTip("QuickPlayToolTip");
            flyout.Items.Add(quickPlay);
            flyout.Items.Add(new MenuFlyoutSeparator());
            var nowPlaying = new MenuFlyoutItem()
            {
                Text = Helper.Localize("NowPlaying")
            };
            nowPlaying.Click += (sender, args) =>
            {
                MediaHelper.ShuffleAndPlay();
                callback?.Invoke();
            };
            flyout.Items.Add(nowPlaying);
            flyout.Items.Add(new MenuFlyoutSeparator());
            var musicLibrary = new MenuFlyoutItem()
            {
                Text = Helper.Localize("Music Library")
            };
            musicLibrary.Click += (sender, args) =>
            {
                RandomPlayHelper.PlayMusic(randomLimit);
                callback?.Invoke();
            };
            flyout.Items.Add(musicLibrary);
            var artist = new MenuFlyoutItem()
            {
                Text = Helper.Localize("Artist")
            };
            artist.Click += (sender, args) =>
            {
                var rArtist = RandomPlayHelper.PlayArtist(randomLimit);
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomArtist", rArtist.Key));
                callback?.Invoke();
            };
            flyout.Items.Add(artist);
            var album = new MenuFlyoutItem()
            {
                Text = Helper.Localize("Album")
            };
            album.Click += (sender, args) =>
            {
                var rAlbum = RandomPlayHelper.PlayAlbum(randomLimit);
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomAlbum", rAlbum.Name));
                callback?.Invoke();
            };
            flyout.Items.Add(album);
            if (Settings.settings.Playlists.Count > 0)
            {
                var playlist = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Playlist")
                };
                playlist.Click += (sender, args) =>
                {
                    var rPlaylist = RandomPlayHelper.PlayPlaylist(randomLimit);
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomPlaylist", rPlaylist.Name));
                    callback?.Invoke();
                };
                flyout.Items.Add(playlist);
            }
            if (!string.IsNullOrEmpty(Settings.settings.RootPath))
            {
                var localFolder = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Local Folder")
                };
                localFolder.Click += (sender, args) =>
                {
                    var rLocalFolder = RandomPlayHelper.PlayFolder(randomLimit);
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomLocalFolder", rLocalFolder.Directory));
                    callback?.Invoke();
                };
                flyout.Items.Add(localFolder);
            }
            if (RecentPage.RecentAdded?.Count > 0)
            {
                var recentAdded = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Recent Added")
                };
                recentAdded.Click += (sender, args) =>
                {
                    MediaHelper.SetMusicAndPlay(RecentPage.RecentAdded.TimeLine.RandItems(randomLimit));
                    callback?.Invoke();
                };
                flyout.Items.Add(recentAdded);
            }
            if (Settings.settings.RecentPlayed.Count > 0)
            {
                var recentPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Recent Played")
                };
                recentPlayed.Click += (sender, args) =>
                {
                    MediaHelper.SetMusicAndPlay(Settings.settings.RecentPlayed.ToMusicList());
                    callback?.Invoke();
                };
                flyout.Items.Add(recentPlayed);
            }
            if (MusicLibraryPage.SongCount > randomLimit)
            {
                flyout.Items.Add(new MenuFlyoutSeparator());
                var mostPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Most Played")
                };
                mostPlayed.Click += (sender, args) =>
                {
                    MediaHelper.SetPlaylistAndPlay(MusicLibraryPage.GetMostPlayed(randomLimit).Shuffle().Take(randomLimit));
                    callback?.Invoke();
                };
                flyout.Items.Add(mostPlayed);
                var leastPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Least Played")
                };
                leastPlayed.Click += (sender, args) =>
                {
                    MediaHelper.SetPlaylistAndPlay(MusicLibraryPage.GetLeastPlayed(randomLimit).Shuffle().Take(randomLimit));
                    callback?.Invoke();
                };
                flyout.Items.Add(leastPlayed);
            }
            return flyout;
        }
        public static MenuFlyoutSubItem AppendRecentAddedItem(MenuFlyoutSubItem parent, object title, List<Music> songs, int limit)
        {
            if (songs.Count > 0)
            {
                var recenItem = new MenuFlyoutItem()
                {
                    Text = title is string stringTitle ? Helper.LocalizeMessage(stringTitle) : title.ToString()
                };
                recenItem.Click += (sender, args) =>
                {
                    MediaHelper.SetPlaylistAndPlay(songs.RandItems(limit));
                };
                parent.Items.Add(recenItem);
            }
            return parent;
        }
        public static MenuFlyoutItem GetPreferItem(IPreferable data)
        {
            bool isPreferred = Settings.settings.Preference.IsPreferred(data);
            MenuFlyoutItem item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(isPreferred ? Symbol.UnFavorite : Symbol.Favorite),
                Text = Helper.LocalizeText(isPreferred ? "UndoPrefer" : "SetAsPreferred")
            };
            item.Click += (sender, args) =>
            {
                string name = data.AsPreferenceItem().Name;
                if (isPreferred)
                {
                    Settings.settings.Preference.UndoPrefer(data);
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("UndoPreferItem", name));
                }
                else
                {
                    if (Settings.settings.Preference.Prefer(data))
                    {
                        Helper.ShowNotificationRaw(Helper.LocalizeMessage("SetItemAsPreferred", name));
                    }
                    else
                    {
                        Helper.ShowNotification("SetAsPreferredFailed");
                    }
                }
            };
            return item;
        }
        public static MenuFlyout SetPlaylistMenu(object sender, IMenuFlyoutItemClickListener clickListener = null, IMenuFlyoutHelperBuildListener buildListener = null, MenuFlyoutOption option = null)
        {
            return SetMenu(helper => helper.GetPlaylistMenuFlyout(clickListener, option), sender, buildListener);
        }
        public static MenuFlyout SetMusicMenu(object sender, IMenuFlyoutItemClickListener clickListener = null, IMenuFlyoutHelperBuildListener buildListener = null, MenuFlyoutOption option = null)
        {
            return SetMenu(helper => helper.GetMusicMenuFlyout(clickListener, option), sender, buildListener);
        }
        private static MenuFlyout SetMenu(Func<MenuFlyoutHelper, MenuFlyout> GetMenu, object sender, IMenuFlyoutHelperBuildListener buildListener = null)
        {
            MenuFlyout flyout;
            MenuFlyoutHelper helper;
            object data;
            if (sender is MenuFlyout)
            {
                flyout = sender as MenuFlyout;
                data = flyout.Target.DataContext;
                flyout.Items.Clear();
            }
            else
            {
                flyout = new MenuFlyout();
                data = (sender as FrameworkElement).DataContext;
            }
            helper = new MenuFlyoutHelper()
            {
                Data = FindMusic(data),
                DefaultPlaylistName = Settings.settings.FindNextPlaylistName(FindPlaylistName(data))
            };
            buildListener?.OnBuild(helper);
            var items = GetMenu(helper).Items;
            flyout.Items.Clear();
            foreach (var item in items)
                flyout.Items.Add(item);
            return flyout;
        }

        private static object FindMusic(object obj)
        {
            if (obj is Music || obj is IEnumerable<Music>) return obj;
            else if (obj is IMusicable || obj is IEnumerable<IMusicable>) return obj;
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

    public interface IMenuFlyoutItemClickListener
    {
        void AddTo(object data, object collection, int index, AddToCollectionType type);
        void Favorite(object data);
        void Delete(Music music);
        void UndoDelete(Music music);
        void Remove(Music music);
        void Select(object data);
    }

    public interface IMenuFlyoutHelperBuildListener
    {
        void OnBuild(MenuFlyoutHelper helper);
    }

    public enum AddToCollectionType
    {
        NowPlaying, MyFavorites, Playlist, NewPlaylist
    }
}
