using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
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
        public const string AddToSubItemName = "AddToSubItem", PlaylistMenuName = "ShuffleAndPlayItem", MusicMenuName = "PlayItem",
                            ShuffleSubItemName = "ShuffleSubItem", SeeAlbumItemName = "SeeAlbumItemName";
        public object Data { get; set; }
        public string DefaultPlaylistName { get; set; } = "";
        public string CurrentPlaylistName { get; set; } = "";
        public object SelectedItems { get; set; }
        public object OriginalData { get; set; }

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
                        if (await StorageHelper.FileNotExist(music.Path))
                        {
                            Helper.ShowMusicNotFoundNotification(music.Name);
                            return;
                        }
                        int index = MusicPlayer.AddMusic(music);
                        Helper.ShowUndoableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, NowPlaying), () =>
                        {
                            MusicPlayer.RemoveMusic(index);
                        });
                    }
                    else if (Data is IEnumerable<IMusicable> songs)
                    {
                        if (songs.IsEmpty()) return;
                        var indices = songs.ToDictionary(i => MusicPlayer.AddMusic(i)).OrderByDescending(i => i.Key);
                        string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.ElementAt(0).ToMusic().Name, NowPlaying) :
                                                              Helper.LocalizeMessage("SongsAddedTo", songs.Count(), NowPlaying);
                        Helper.ShowUndoableNotificationRaw(message, () =>
                        {
                            foreach (var pair in indices)
                                MusicPlayer.RemoveMusic(pair.Key);
                        });
                    }
                    else
                    {
                        return;
                    }
                };
                addToItem.Items.Add(nowPlayingItem);
            }
            if (CurrentPlaylistName != MyFavorites)
            {
                Playlist myFavorites = PlaylistService.MyFavorites;
                if ((Data is IMusicable m && !myFavorites.Contains(m)) ||
                     Data is IEnumerable<IMusicable> list)
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
                            if (await StorageHelper.FileNotExist(music.Path))
                            {
                                Helper.ShowMusicNotFoundNotification(music.Name);
                                return;
                            }
                            MusicService.LikeMusic(music);
                            Helper.ShowUndoableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, MyFavorites), () =>
                            {
                                MusicService.DislikeMusic(music);
                            });
                        }
                        else if (Data is IEnumerable<IMusicable> songs)
                        {
                            MusicService.LikeMusic(songs);
                            string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.First().ToMusic().Name, MyFavorites) :
                                                                  Helper.LocalizeMessage("SongsAddedTo", songs.Count(), MyFavorites);
                            Helper.ShowUndoableNotificationRaw(message, () =>
                            {
                                foreach (var song in songs)
                                    MusicService.DislikeMusic(song.ToMusic());
                            });
                        }
                        else
                        {
                            return;
                        }
                        listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Favorite) { Data = Data });
                    };
                    addToItem.Items.Add(favItem);
                }
            }
            if (addToItem.Items.IsNotEmpty()) addToItem.Items.Add(new MenuFlyoutSeparator());
            foreach (var item in GetAddToPlaylistsMenuFlyout(listener).Items)
                addToItem.Items.Add(item);
            return addToItem;
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
                    if (await StorageHelper.FileNotExist(music.Path))
                    {
                        Helper.ShowMusicNotFoundNotification(music.Name);
                        return;
                    }
                }
                var dialog = new RenameDialog(RenameOption.Create, RenameTarget.Playlist, DefaultPlaylistName)
                {
                    Validate = PlaylistService.ValidatePlaylistName,
                    Confirmed = newName => PlaylistService.AddPlaylist(newName, Data),
                };
                await dialog.ShowAsync();
            };
            newPlaylistItem.SetToolTip("NewPlaylistToolTip");
            flyout.Items.Add(newPlaylistItem);
            foreach (var playlist in PlaylistService.AllPlaylists)
            {
                if ((OriginalData is PlaylistView playlistView && playlistView.EntityType == EntityType.Playlist && playlist.Id == playlistView.Id) ||
                    (Data is IMusicable m && PlaylistService.FindPlaylistItems(playlist.Id).Contains(m.ToMusic())))
                {
                    continue;
                }
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
                        if (await StorageHelper.FileNotExist(music.Path))
                        {
                            Helper.ShowMusicNotFoundNotification(music.Name);
                            return;
                        }
                        PlaylistService.AddMusic(playlist, music);
                        Helper.ShowUndoableNotificationRaw(Helper.LocalizeMessage("SongAddedTo", music.Name, playlist.Name), () =>
                        {
                            PlaylistService.RemoveMusic(playlist, music);
                        });
                    }
                    else if (Data is IEnumerable<IMusicable> songs)
                    {
                        if (songs.IsEmpty()) return;
                        PlaylistService.AddMusic(playlist, songs);
                        string message = songs.Count() == 1 ? Helper.LocalizeMessage("SongAddedTo", songs.First().ToMusic().Name, playlist.Name) :
                                                              Helper.LocalizeMessage("SongsAddedTo", songs.Count(), playlist.Name);
                        Helper.ShowUndoableNotificationRaw(message, () =>
                        {
                            PlaylistService.RemoveMusic(playlist, songs);
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

        public MenuFlyoutSubItem GetGetMoveToFolderSubItem(List<ILocalStorageItem> storageItems = null, IMenuFlyoutItemClickListener listener = null)
        {
            return GetMoveToFolderFlyout(storageItems, listener).ToSubItem("MoveToFolder", new SymbolIcon(Symbol.MoveToFolder));
        }

        public MenuFlyout GetMoveToFolderFlyout(List<ILocalStorageItem> storageItems = null, IMenuFlyoutItemClickListener listener = null)
        {
            MenuFlyout flyout = new MenuFlyout();
            List<StorageItem> data = storageItems?.Select(i => i.AsStorageItem()).ToList() ?? (List<StorageItem>)Data;
            if (data.IsEmpty()) return flyout;
            MenuFlyoutItemBase itemBase = GetMoveToFolderMenuTree(Settings.settings.Tree, data, listener);
            if (itemBase is MenuFlyoutItem item)
            {
                flyout.Items.Add(item);
            }
            else if (itemBase is MenuFlyoutSubItem subItem)
            {
                flyout.Items.AddRange(subItem.Items);
            }
            return flyout;
        }

        private static MenuFlyoutItemBase GetMoveToFolderMenuTree(FolderTree folder, List<StorageItem> items, IMenuFlyoutItemClickListener listener = null)
        {
            List<FolderTree> branches = StorageService.FindSubFolders(folder);
            if (branches.IsEmpty())
            {
                return GetMoveToFolderMenuFlyoutItem(folder, items, listener);
            }
            MenuFlyoutSubItem item = new MenuFlyoutSubItem()
            {
                Text = folder.Name
            };
            item.SetToolTip(Helper.LocalizeText("MoveToFolderOfPath", folder.Path), false);
            if (IsTargetFolder(folder, items))
            {
                item.Items.Add(GetMoveToFolderMenuFlyoutItem(folder, items, listener));
                item.Items.Add(new MenuFlyoutSeparator());
            }
            foreach (var branch in branches.OrderBy(i => i.Name))
            {
                MenuFlyoutItemBase branchItem = GetMoveToFolderMenuTree(branch, items, listener);
                if (branchItem is MenuFlyoutSubItem subItem && subItem.Items.IsEmpty()) continue;
                else if (branchItem is MenuFlyoutItem && !IsTargetFolder(branch, items)) continue;
                item.Items.Add(branchItem);
            }
            return item;
        }

        // 文件夹不能移动到其本身或者父文件夹，文件也不需要移动到他的父文件夹
        private static bool IsTargetFolder(FolderTree folder, List<StorageItem> items)
        {
            return !items.Any(i => i.Path == folder.Path || i.ParentPath == folder.Path);
        }

        private static MenuFlyoutItem GetMoveToFolderMenuFlyoutItem(FolderTree folder, List<StorageItem> items, IMenuFlyoutItemClickListener listener = null)
        {
            MenuFlyoutItem item = new MenuFlyoutItem()
            {
                Text = folder.Name
            };
            item.SetToolTip(Helper.LocalizeText("MoveToFolderOfPath", folder.Path), false);
            item.Click += async (s, e) =>
            {
                MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                foreach (var storageItem in items)
                {
                    if (storageItem is FolderTree folderTree)
                    {
                        await StorageService.MoveFolderAsync(folderTree, folder);
                    }
                    else if (storageItem is FolderFile folderFile)
                    {
                        await StorageService.MoveFileAsync(StorageService.FindFile(folderFile.Path), folder);
                    }
                }
                listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.MoveToFolder));
                MainPage.Instance?.Loader.Hide();
            };
            return item;
        }

        public static MenuFlyoutSubItem GetShuffleSubItem()
        {
            MenuFlyoutSubItem subItem = GetShuffleMenu().ToSubItem("RandomPlay", new SymbolIcon(Symbol.Shuffle));
            subItem.Name = ShuffleSubItemName;
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
                MusicPlayer.ShuffleAndPlay(Data as IEnumerable<IMusicable>);
                listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Shuffle));
            };
            flyout.Items.Add(shuffleItem);
            flyout.Items.Add(GetAddToMenuFlyoutSubItem(listener));
            if (option.ShowMultiSelect) flyout.Items.Add(GetMultiSelectItem(listener, option.MultiSelectOption));
            else if (option.ShowSelect) flyout.Items.Add(GetSelectItem(listener, option.MultiSelectOption));
            if (option.ShowMoveToFolder) flyout.Items.Add(GetGetMoveToFolderSubItem(new List<ILocalStorageItem>() { OriginalData as ILocalStorageItem }, listener));
            return flyout;
        }
        public static MenuFlyoutItem GetShowInExplorerItem(string path, StorageItemTypes type, IMenuFlyoutItemClickListener clickListener = null)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new FontIcon() { Glyph = "\uE838" },
                Text = Helper.LocalizeText("ShowInExplorer")
            };
            item.SetToolTip(Helper.LocalizeText("ShowInExplorerToolTip", path), false);
            item.Click += async (s, args) =>
            {
                clickListener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.ShowInExplorer));
                if (path.Contains(".") && await StorageHelper.FileNotExist(path))
                {
                    Helper.ShowMusicNotFoundNotification(System.IO.Path.GetFileName(path));
                    return;
                }
                if (!path.Contains(".") && await StorageHelper.FolderNotExist(path))
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
                listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Select)
                {
                    Data = (s as FrameworkElement).DataContext
                });
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

        public static MenuFlyoutItem GetRenameFolderItem(FolderTree tree, Func<string, Task<NamingError>> validateAsync, Action<string> confirmed)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Rename),
                Text = Helper.LocalizeText("RenameFolder")
            };
            item.Click += async (s, args) =>
            {
                await new RenameDialog(RenameOption.Rename, RenameTarget.Folder, tree.Name)
                {
                    ValidateAsync = validateAsync,
                    Confirmed = confirmed
                }.ShowAsync();
            };
            return item;
        }

        public static MenuFlyoutItem GetRefreshDirectoryItem(FolderTree tree)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Refresh),
                Text = Helper.Localize("Refresh Directory")
            };
            item.Click += (s, args) => UpdateHelper.RefreshFolder(tree);
            return item;
        }

        public static MenuFlyoutItem GetNewFolderItem(FolderTree folder)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.NewFolder),
                Text = Helper.LocalizeText("NewFolder"),
            };
            item.Click += async (sender, args) => await StorageHelper.AddFolder(folder);
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
                    Message = Helper.LocalizeMessage("DeleteFolder", tree.Name),
                    CheckBoxVisibility = Visibility.Collapsed,
                    Confirm = async () =>
                    {
                        StorageFolder folder = await tree.GetStorageFolderAsync();
                        if (folder != null) await folder.DeleteAsync();
                        StorageService.DeleteFolder(tree);
                        afterTreeDeleted?.Invoke(tree);
                        Helper.ShowNotificationRaw(Helper.LocalizeMessage("FolderIsDeleted", tree.Name));
                    }
                }.ShowAsync();
            };
            return item;
        }
        public static MenuFlyoutItem GetSearchDirectoryItem(FolderTree tree, IMenuFlyoutItemClickListener clickListener = null)
        {
            var item = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Find),
                Text = Helper.Localize("Search Directory")
            };
            item.Click += async (s, args) =>
            {
                clickListener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.ShowInExplorer));
                await new InputDialog()
                {
                    Title = Helper.LocalizeMessage("Search"),
                    PlaceholderText = Helper.LocalizeMessage("SearchDirectoryHint", tree.Name),
                    Confirm = (inputText) =>
                    {
                        MainPage.Instance.Search(new SearchKeyword()
                        {
                            Text = inputText,
                            Songs = tree.Flatten(),
                            Folder = tree
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
            int index = Data is MusicView musicView ? musicView.Index : -1;
            int currentIndex = MusicPlayer.CurrentIndex;
            var flyout = new MenuFlyout();
            var playItem = new MenuFlyoutItem()
            {
                Text = Helper.LocalizeText("Play"),
                Icon = new SymbolIcon(Symbol.Play)
            };
            playItem.SetToolTip(Helper.LocalizeText("PlayMusicOfName", music.Name));
            playItem.Click += (s, e) =>
            {
                if (index >= 0)
                {
                    MusicPlayer.MoveToMusic(index);
                }
                else
                {
                    MusicPlayer.SetMusicAndPlay(music);
                }
            };
            flyout.Items.Add(playItem);
            if (currentIndex != -1 && currentIndex != index && currentIndex != index - 1)
            {
                var playNextItem = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("PlayNext"),
                    Icon = new SymbolIcon(Symbol.Upload)
                };
                playNextItem.Click += (s, args) =>
                {
                    if (index >= 0)
                    {
                        MusicPlayer.MoveMusic(index, currentIndex);
                    }
                    else
                    {
                        MusicPlayer.AddMusic(music, currentIndex + 1);
                    }
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("SetPlayNext", music.Name));
                };
                flyout.Items.Add(playNextItem);
            }
            flyout.Items.Add(GetAddToMenuFlyoutSubItem(listener));
            if (option.ShowRemove) flyout.Items.Add(GetRemovableMenuFlyoutItem(music, index, listener));
            if (option.ShowSelect || option.MultiSelectOption != null) flyout.Items.Add(GetSelectItem(listener, option.MultiSelectOption));
            flyout.Items.Add(GetPreferItem(music));
            if (option.ShowMoveToFolder) flyout.Items.Add(GetGetMoveToFolderSubItem(new List<ILocalStorageItem>() { OriginalData as ILocalStorageItem }, listener));
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
                        listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Delete) { Data = music });
                        await StorageService.DeleteFile(music.ToFolderFile());
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
                if (await StorageHelper.FileNotExist(music.Path))
                {
                    Helper.ShowMusicNotFoundNotification(music.Name);
                    return;
                }
                if (MainPage.Instance?.CurrentPage == typeof(AlbumPage))
                    AlbumPage.Instance.LoadAlbum(music.Album);
                else
                    MainPage.Instance.NavigateToPage(typeof(AlbumPage), music.Album);
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
                    if (await StorageHelper.FileNotExist(music.Path))
                    {
                        Helper.ShowMusicNotFoundNotification(music.Name);
                        return;
                    }
                    if (MainPage.Instance?.CurrentPage == typeof(ArtistsPage))
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
                if (await StorageHelper.FileNotExist(music.Path))
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
                if (await StorageHelper.FileNotExist(music.Path))
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
                    if (await StorageHelper.FileNotExist(music.Path))
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

        public static MenuFlyoutItem GetRemovableMenuFlyoutItem(Music music, int index, IMenuFlyoutItemClickListener listener = null)
        {
            var removeItem = new MenuFlyoutItem
            {
                Icon = new SymbolIcon(Symbol.Remove),
                Text = Helper.Localize("Remove From List")
            };
            removeItem.Click += (sender, args) =>
            {
                listener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Remove) { Data = music, Index = index });
            };
            removeItem.SetToolTip(Helper.LocalizeMessage("RemoveFromList", music.Name), false);
            return removeItem;
        }

        public static MenuFlyout GetSortByMenu(SortBy criterion, SortBy[] criteria, Action<SortBy> onSelected)
        {
            var flyout = new MenuFlyout();
            foreach (var item in criteria)
            {
                if (item == SortBy.Reverse)
                {
                    var reverseItem = new MenuFlyoutItem() { Text = Helper.LocalizeMessage("ReverseList") };
                    reverseItem.Click += (send, args) => onSelected.Invoke(item);
                    flyout.Items.Add(reverseItem);
                    flyout.Items.Add(new MenuFlyoutSeparator());
                    continue;
                }
                string sortby = Helper.LocalizeMessage("Sort By " + item.ToStr());
                var radioItem = new ToggleMenuFlyoutItem()
                {
                    Text = sortby,
                    IsChecked = item == criterion
                };
                radioItem.Click += (send, args) => onSelected.Invoke(item);
                flyout.Items.Add(radioItem);
            }
            return flyout;
        }
        public static void SetSortByMenu(object sender, SortBy criterion, SortBy[] criteria, Action<SortBy> onSelected)
        {
            GetSortByMenu(criterion, criteria, onSelected).ShowAt(sender as FrameworkElement);
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
                MusicPlayer.QuickPlay(randomLimit);
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
                MusicPlayer.ShuffleAndPlay();
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
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomArtist", rArtist));
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
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomAlbum", rAlbum));
                callback?.Invoke();
            };
            flyout.Items.Add(album);
            if (PlaylistService.AllPlaylists.IsNotEmpty())
            {
                var playlist = new MenuFlyoutItem()
                {
                    Text = Helper.Localize("Playlist")
                };
                playlist.Click += (sender, args) =>
                {
                    var rPlaylist = RandomPlayHelper.PlayPlaylist(randomLimit);
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomPlaylist", rPlaylist));
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
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("PlayRandomLocalFolder", rLocalFolder));
                    callback?.Invoke();
                };
                flyout.Items.Add(localFolder);
            }
            if (RecentPage.RecentAdded?.Count > 0)
            {
                var recentAdded = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("RecentAdded")
                };
                recentAdded.Click += (sender, args) =>
                {
                    MusicPlayer.SetMusicAndPlay(RecentPage.RecentAdded.TimeLine.RandItems(randomLimit));
                    callback?.Invoke();
                };
                flyout.Items.Add(recentAdded);
            }
            if (SettingsService.RecentPlayed.IsNotEmpty())
            {
                var recentPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("RecentPlayed")
                };
                recentPlayed.Click += (sender, args) =>
                {
                    MusicPlayer.ShuffleAndPlay(SettingsService.RecentPlayed);
                    callback?.Invoke();
                };
                flyout.Items.Add(recentPlayed);
            }
            if (MusicService.AllSongs.Count() > randomLimit)
            {
                flyout.Items.Add(new MenuFlyoutSeparator());
                var mostPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("MostPlayed")
                };
                mostPlayed.Click += (sender, args) =>
                {
                    MusicPlayer.SetPlaylistAndPlay(MusicService.GetMostPlayed(randomLimit).Shuffle().Take(randomLimit));
                    callback?.Invoke();
                };
                flyout.Items.Add(mostPlayed);
                var leastPlayed = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("LeastPlayed")
                };
                leastPlayed.Click += (sender, args) =>
                {
                    MusicPlayer.SetPlaylistAndPlay(MusicService.GetLeastPlayed(randomLimit).Shuffle().Take(randomLimit));
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
                    MusicPlayer.SetPlaylistAndPlay(songs.RandItems(limit));
                };
                parent.Items.Add(recenItem);
            }
            return parent;
        }

        public static MenuFlyout SetFolderMenu(object sender, FolderTree folder, IMenuFlyoutItemClickListener clickListener = null, IMenuFlyoutHelperBuildListener buildListener = null, MenuFlyoutOption option = null)
        {
            MenuFlyout flyout = SetMenu(helper => helper.GetPlaylistMenuFlyout(clickListener, option), sender, buildListener);
            flyout.Items.Add(GetPreferItem(folder, clickListener));
            flyout.Items.Add(GetShowInExplorerItem(folder.Path, StorageItemTypes.Folder, clickListener));
            flyout.Items.Add(GetNewFolderItem(folder));
            flyout.Items.Add(GetDeleteFolderItem(folder));
            flyout.Items.Add(GetRefreshDirectoryItem(folder));
            flyout.Items.Add(GetRenameFolderItem(folder,
                                                async newName => await StorageService.ValidateFolderName(folder.ParentPath, newName),
                                                async newName => await StorageService.RenameFolder(folder, newName)));
            flyout.Items.Add(GetFolderSortByMenu(folder, clickListener));
            flyout.Items.Add(GetSearchDirectoryItem(folder, clickListener));
            return flyout;
        }

        public static MenuFlyoutSubItem GetFolderSortByMenu(FolderTree folder, IMenuFlyoutItemClickListener clickListener = null)
        {
            SortBy[] criteria = new SortBy[] { SortBy.Reverse, SortBy.Title, SortBy.Artist, SortBy.Album };
            MenuFlyoutSubItem ret = new MenuFlyoutSubItem
            {
                Text = Helper.LocalizeText("Sort"),
                Icon = new SymbolIcon(Symbol.Sort),
            };
            MenuFlyout flyout = GetSortByMenu(folder.Criterion, criteria, criterion => clickListener.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Sort) { Data = criterion }));
            ret.Items.AddRange(flyout.Items);
            return ret;
        }

        public static MenuFlyout SetSimpleFolderMenu(object sender, FolderTree folder, IMenuFlyoutItemClickListener clickListener = null, IMenuFlyoutHelperBuildListener buildListener = null, MenuFlyoutOption option = null)
        {
            MenuFlyout flyout = SetMenu(helper => helper.GetPlaylistMenuFlyout(clickListener, option), sender, buildListener);
            flyout.Items.Add(GetPreferItem(folder, clickListener));
            flyout.Items.Add(GetShowInExplorerItem(folder.Path, StorageItemTypes.Folder, clickListener));
            flyout.Items.Add(GetSearchDirectoryItem(folder, clickListener));
            return flyout;
        }

        public static MenuFlyout SetAlbumMenu(object sender, AlbumView album, IMenuFlyoutItemClickListener clickListener = null, IMenuFlyoutHelperBuildListener buildListener = null, MenuFlyoutOption option = null)
        {
            MenuFlyout flyout = SetMenu(helper => helper.GetPlaylistMenuFlyout(clickListener, option), sender, buildListener);
            flyout.Items.Add(GetPreferItem(album));
            MenuFlyoutItem albumArtItem = new MenuFlyoutItem()
            {
                Icon = new SymbolIcon(Symbol.Pictures),
                Text = Helper.Localize("See Album Art")
            };
            albumArtItem.Click += async (s, args) =>
            {
                await new AlbumDialog(AlbumDialogOption.AlbumArt, album).ShowAsync();
            };
            flyout.Items.Add(albumArtItem);
            return flyout;
        }

        public static MenuFlyoutSubItem GetPreferItem(IPreferable data, IMenuFlyoutItemClickListener clickListener = null)
        {
            MenuFlyoutSubItem parent = new MenuFlyoutSubItem()
            {
                Icon = new SymbolIcon(Symbol.Favorite),
                Text = Helper.LocalizeText("PreferenceSettings")
            };
            PreferenceItem preferenceItem = PreferenceSettings.GetPreferenceItem(data);
            string name;
            if (preferenceItem == null)
            {
                preferenceItem = data.AsPreferenceItem();
                name = preferenceItem.Name;
            }
            else if (preferenceItem.Type == EntityType.MyFavorites && !preferenceItem.IsEnabled)
            {
                name = preferenceItem.Name;
            }
            else
            {
                name = preferenceItem.Name;
                MenuFlyoutItem undoPrefer = new MenuFlyoutItem()
                {
                    Text = Helper.LocalizeText("UndoPrefer"),
                };
                undoPrefer.Click += (sender, args) =>
                {
                    PreferenceSettings.settings.UndoPrefer(preferenceItem);
                    clickListener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.UndoPrefer));
                    Helper.ShowNotificationRaw(Helper.LocalizeMessage("UndoPreferItem", name));
                };
                parent.Items.Add(undoPrefer);
                parent.Items.Add(new MenuFlyoutSeparator());
            }
            foreach (PreferLevel level in EnumHelper.GetOrderedValues(typeof(PreferLevel)))
            {
                MenuFlyoutItem item = new ToggleMenuFlyoutItem()
                {
                    Text = Helper.LocalizeText(level.GetDescription()),
                    IsChecked = preferenceItem.ThisId > 0 && preferenceItem.Level == level,
                };
                item.SetToolTip(level.GetToolTip());
                item.Click += (sender, args) =>
                {
                    if (!PreferenceSettings.settings.Prefer(preferenceItem, level))
                    {
                        Helper.ShowNotification("SetAsPreferredFailed", 4000);
                    }
                    clickListener?.Execute(new MenuFlyoutEventArgs(MenuFlyoutEvent.Prefer));
                };
                parent.Items.Add(item);
            }
            return parent;
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
            MenuFlyoutHelper helper = new MenuFlyoutHelper()
            {
                Data = FindMusic(data),
                OriginalData = data,
                SelectedItems = data,
                DefaultPlaylistName = PlaylistService.FindNextPlaylistName(FindPlaylistName(data))
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
            else if (obj is ArtistView artist) return artist.Songs;
            else if (obj is AlbumView album) return album.Songs;
            else if (obj is PlaylistView playlist) return playlist.Songs;
            else if (obj is GridViewFolder gridFolder) return gridFolder.Songs;
            else if (obj is GridViewMusic gridMusic) return gridMusic.Source;
            else if (obj is FolderChainItem folderChainItem) return StorageService.FindFolder(folderChainItem.Id).Songs;
            else if (obj is IMusicable || obj is IEnumerable<IMusicable>) return obj;
            return null;
        }

        private static string FindPlaylistName(object obj)
        {
            if (obj is ArtistView artist) return artist.Name;
            else if (obj is AlbumView album) return album.Name;
            else if (obj is PlaylistView playlist) return playlist.Name;
            else if (obj is GridViewFolder gridFolder) return gridFolder.Name;
            return "";
        }
    }

    public interface IMenuFlyoutItemClickListener
    {
        void Execute(MenuFlyoutEventArgs args);
    }

    public enum MenuFlyoutEvent
    {
        AddTo, Favorite, Delete, Remove, Select, MoveToFolder,
        Sort, Shuffle, Prefer, UndoPrefer, ShowInExplorer, SearchDirectory
    }

    public class MenuFlyoutEventArgs
    {
        public MenuFlyoutEvent Event { get; set; }
        public object Data { get; set; }
        public Music Music { get => (Music)Data; }
        public MusicView MusicView { get => Music.ToVO(Index); }
        public int Index { get; set; }

        public MenuFlyoutEventArgs() { }

        public MenuFlyoutEventArgs(MenuFlyoutEvent Event)
        {
            this.Event = Event;
        }
    }

    public interface IMenuFlyoutHelperBuildListener
    {
        void OnBuild(MenuFlyoutHelper helper);
    }
}
