using Microsoft.Toolkit.Uwp.Notifications;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace SMPlayer.Helpers
{
    public class TileHelper
    {
        public const string StringConcatenationFlag = "+++++";
        private static StorageFolder SecondaryTileFolder;

        public static TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

        public static string BuildAlbumNavigationFlag(string albumName, string artist)
        {
            return albumName;
        }

        public static async Task UpdateTile(StorageItemThumbnail itemThumbnail, MusicView music)
        {
            if (music == null) return;
            string uri = MusicImage.DefaultImagePath;
            if (itemThumbnail != null)
            {
                try
                {
                    var file = await itemThumbnail.SaveAsync(await Helper.GetThumbnailFolder(), string.IsNullOrEmpty(music.Album) ? music.Name : music.Album, true);
                    uri = file.Path;
                }
                catch (Exception)
                {

                }
            }
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Branding = TileBranding.None,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Body,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Artist,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.Name,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage() { Source = uri },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = music.Album,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Name,
                                    HintStyle = AdaptiveTextStyle.Subtitle,
                                    HintWrap = true
                                },
                                new AdaptiveText()
                                {
                                    Text = music.Artist,
                                    HintStyle = AdaptiveTextStyle.Base
                                }
                            }
                        }
                    }
                }
            };
            try
            {
                // Create the tile notification
                var tileNotification = new TileNotification(tileContent.GetXml());

                // And send the notification to the primary tile
                tileUpdater.Update(tileNotification);
            }
            catch (Exception)
            {
                // ArgumentException: Value does not fall within the expected range.
                // 不知道为什么就变成了null
            }
        }

        public static void ResumeTile()
        {
            var tile = new TileBinding()
            {
                DisplayName = Windows.ApplicationModel.Package.Current.DisplayName,
                Branding = TileBranding.Name,
                Content = new TileBindingContentAdaptive()
                {
                    BackgroundImage = new TileBackgroundImage()
                    {
                        Source = MusicImage.DefaultImagePath
                    },
                }
            };
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = tile,
                    TileWide = tile,
                    TileLarge = tile
                }
            };

            // Create the tile notification
            var tileNotification = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            tileUpdater.Update(tileNotification);
        }

        public static async Task<StorageFolder> GetSecondaryTileFolder()
        {
            return SecondaryTileFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("SecondaryTiles", CreationCollisionOption.OpenIfExists);
        }

        public static async Task<bool> PinToStartAsync(PlaylistView playlist)
        {
            var tilename = playlist.Name;
            var tileid = FormatTileId(playlist);
            var filename = tileid + ".png";
            var uri = MusicImage.DefaultImagePath;
            if (playlist?.DisplayItem?.Source != null)
            {
                if (await (await GetSecondaryTileFolder()).Contains(filename))
                {
                    uri = "ms-appdata:///local/SecondaryTiles/" + WebUtility.UrlEncode(filename);
                }
                else
                {
                    var thumbnail = await ImageHelper.LoadThumbnail(playlist.DisplayItem.Source);
                    if (thumbnail.IsThumbnail())
                    {
                        await thumbnail.SaveAsync(SecondaryTileFolder, tileid);
                        uri = "ms-appdata:///local/SecondaryTiles/" + WebUtility.UrlEncode(filename);
                    }
                    else
                    {
                        uri = Helper.LogoPath;
                    }
                }
            }
            var tile = new SecondaryTile(tileid, tilename, playlist.EntityType.ToString(), new Uri(uri), Windows.UI.StartScreen.TileSize.Default);
            tile.VisualElements.ShowNameOnSquare150x150Logo = tile.VisualElements.ShowNameOnSquare310x310Logo = tile.VisualElements.ShowNameOnWide310x150Logo = true;
            if (SecondaryTile.Exists(tileid)) await tile.RequestDeleteAsync();
            else await tile.RequestCreateAsync();
            return SecondaryTile.Exists(tileid);
        }

        public static string FormatTileId(PlaylistView playlist)
        {
            return playlist.EntityType == EntityType.Album ? playlist.Name : playlist.Id.ToString();
        }
    }
}
