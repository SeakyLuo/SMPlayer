﻿using ExpressionBuilder;
using Microsoft.Graphics.Canvas.Effects;
using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using EF = ExpressionBuilder.ExpressionFunctions;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class HeaderedPlaylistControl : UserControl, IImageSavedListener, IMultiSelectListener, IPlaylistEventListener
    {
        public PlaylistControl HeaderedPlaylist { get => HeaderedPlaylistController; }
        public PlaylistView CurrentPlaylist { get; private set; }
        public Brush HeaderBackground
        {
            get => headerBackground;
            set => OverlayRectangle.Fill = headerBackground = value;
        }
        private Brush headerBackground = ColorHelper.HighlightBrush;
        private bool IsPlaylist => PlaylistType == HeaderedPlaylistType.Playlist;
        private bool IsMyFavorites => PlaylistType == HeaderedPlaylistType.MyFavorites;
        public HeaderedPlaylistType PlaylistType { get; set; } = HeaderedPlaylistType.Playlist;

        private static RenameDialog dialog;
        private static RemoveDialog removeDialog;
        private bool doNotListenToPlaylistEvent = false;
        public HeaderedPlaylistControl()
        {
            this.InitializeComponent();
            HeaderedPlaylistController.PlaylistEventListener = this;
            HeaderedPlaylistController.MultiSelectListener = this;
            PlaylistService.AddPlaylistEventListener(this);
            AlbumArtControl.ImageSavedListeners.Add(this);
        }

        public async Task SetPlaylist(PlaylistView playlist)
        {
            if (playlist == null) return;
            HidePlaylistCover();
            MusicPlayer.SetMusicPlaying(playlist.Songs, MusicPlayer.CurrentMusic);
            CurrentPlaylist = playlist;
            playlist.Sort();
            HeaderedPlaylist.CurrentPlaylist = playlist.Songs;
            SetPlaylistInfo(playlist.Songs);
            switch (PlaylistType)
            {
                case HeaderedPlaylistType.Playlist:
                    PlaylistNameTextBlock.Text = playlist.Name;
                    HeaderedPlaylistController.ShowAlbumText = true;
                    HeaderedPlaylistController.Removable = true;
                    RenameButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Visible;
                    EditAlbumArtButton.Visibility = Visibility.Collapsed;
                    ClearButton.Visibility = Visibility.Visible;
                    break;
                case HeaderedPlaylistType.MyFavorites:
                    PlaylistNameTextBlock.Text = playlist.Name;
                    HeaderedPlaylistController.ShowAlbumText = true;
                    HeaderedPlaylistController.Removable = true;
                    RenameButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    EditAlbumArtButton.Visibility = Visibility.Collapsed;
                    ClearButton.Visibility = Visibility.Visible;
                    break;
                case HeaderedPlaylistType.Album:
                    PlaylistNameTextBlock.Text = string.IsNullOrEmpty(playlist.Name) ? Helper.LocalizeMessage("UnknownAlbum") : playlist.Name;
                    HeaderedPlaylistController.ShowAlbumText = false;
                    HeaderedPlaylistController.Removable = false;
                    RenameButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    EditAlbumArtButton.Visibility = Visibility.Visible;
                    ClearButton.Visibility = Visibility.Collapsed;
                    break;
            }
            SetPinState(Windows.UI.StartScreen.SecondaryTile.Exists(TileHelper.FormatTileId(playlist)));
            if (MusicDisplayItem.IsNullOrEmpty(playlist.DisplayItem))
            {
                await playlist.LoadDisplayItemAsync();
            }
            await SetMusicDisplayItem(playlist.DisplayItem);
            ShowPlaylistCover();
        }

        public void HidePlaylistCover()
        {
            PlaylistCover.Visibility = Visibility.Collapsed;
            PlaylistCoverProgressRing.IsActive = true;
        }

        public void ShowPlaylistCover()
        {
            PlaylistCover.Visibility = Visibility.Visible;
            PlaylistCoverProgressRing.IsActive = false;
        }

        public async Task SetMusicDisplayItem(MusicDisplayItem item)
        {
            if (item == null)
            {
                HeaderBackground = MusicDisplayItem.DefaultItem.Color;
            }
            else
            {
                HeaderBackground = item.Color;
                PlaylistCover.Source = await item.GetThumbnailAsync();
            }
        }

        public void SetPlaylistInfo(IEnumerable<IMusicable> songs)
        {
            PlaylistInfoTextBlock.Text = SongCountConverter.ToStr(songs);
            ShuffleButton.IsEnabled = !songs.IsEmpty();
            MultiSelectButton.IsEnabled = !songs.IsEmpty();
            ClearButton.Visibility = songs.IsEmpty() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.ShuffleAndPlay(CurrentPlaylist.Songs);
        }

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            dialog = new RenameDialog(RenameOption.Rename, RenameTarget.Playlist, CurrentPlaylist.Name)
            {
                Validate = PlaylistService.ValidatePlaylistName,
                Confirmed = Confirmed
            };
            await dialog.ShowAsync();
        }

        public void Confirmed(string newName)
        {
            PlaylistNameTextBlock.Text = newName;
            PlaylistService.RenamePlaylist(CurrentPlaylist, newName);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeletePlaylist(CurrentPlaylist);
        }

        public async void DeletePlaylist(PlaylistView playlist)
        {
            if (removeDialog == null)
            {
                removeDialog = new RemoveDialog();
            }
            if (removeDialog.IsChecked)
            {
                ExecutePlaylistDeletion(playlist);
            }
            else
            {
                removeDialog.Confirm = () => ExecutePlaylistDeletion(playlist);
                removeDialog.Message = Helper.LocalizeMessage("RemovePlaylist", playlist.Name);
                await removeDialog.ShowAsync();
            }
        }
        private void ExecutePlaylistDeletion(PlaylistView playlist)
        {
            PlaylistService.RemovePlaylist(playlist.FromVO());
            Helper.ShowUndoableNotificationRaw(Helper.LocalizeMessage("PlaylistRemoved", playlist.Name), () =>
            {
                PlaylistService.AddPlaylist(playlist.FromVO());
            });
        }

        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            SetPinState(await TileHelper.PinToStartAsync(CurrentPlaylist));
        }

        public void SetPinState(bool isPinned)
        {
            if (isPinned)
            {
                PinToStartButton.Icon = new SymbolIcon(Symbol.UnPin);
                PinToStartButton.Label = Helper.Localize("UnPin");
                PinToStartButton.SetToolTip("UnPinToolTip");
            }
            else
            {
                PinToStartButton.Icon = new SymbolIcon(Symbol.Pin);
                PinToStartButton.Label = Helper.Localize("Pin To Start");
                PinToStartButton.SetToolTip("PinToStartToolTip");
            }
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (removeDialog == null) removeDialog = new RemoveDialog();
            removeDialog.Message = Helper.LocalizeMessage("ClearPlaylist", CurrentPlaylist.Name);
            removeDialog.Confirm = () =>
            {
                PlaylistService.ClearPlaylist(CurrentPlaylist);
                HeaderedPlaylistController.CurrentPlaylist.Clear();
                SetPlaylistInfo(CurrentPlaylist.Songs);
            };
            await removeDialog.ShowAsync();
        }

        private async void EditAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await new AlbumDialog(AlbumDialogOption.AlbumArt, CurrentPlaylist.ToAlbumView()).ShowAsync();
            }
            catch (Exception ex)
            {
                Helper.ShowOperationFailedNotification(ex);
            }
        }

        private void MultiSelectButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderedPlaylistController.SelectionMode = ListViewSelectionMode.Multiple;
            MainPage.Instance.ShowMultiSelectCommandBar();
        }

        public void ScrollToTop()
        {
            HeaderedPlaylistController.ScrollToTop();
        }


        public async void SaveAlbum(AlbumView album, BitmapImage image)
        {
            // IsAlbum
            if (!IsPlaylist && CurrentPlaylist.Name == album.Name)
            {
                MusicDisplayItem item = await album.Songs[0].GetMusicDisplayItemAsync();
                await SetMusicDisplayItem(CurrentPlaylist.DisplayItem = item);
            }
        }

        public async void SaveMusic(MusicView music, BitmapImage image)
        {
            // IsAlbum
            if (!IsPlaylist && CurrentPlaylist.Name == music.Album && CurrentPlaylist.Count == 1)
            {
                MusicDisplayItem item = await music.GetMusicDisplayItemAsync();
                await SetMusicDisplayItem(CurrentPlaylist.DisplayItem = item);
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            SortBy[] criteria = new SortBy[] { SortBy.Reverse, SortBy.Title, SortBy.Artist, SortBy.Album, SortBy.Duration, SortBy.PlayCount, SortBy.DateAdded };
            MenuFlyoutHelper.SetSortByMenu(sender, CurrentPlaylist.Criterion, criteria, (criterion) =>
            {
                PlaylistService.SortPlaylist(CurrentPlaylist, criterion);
                HeaderedPlaylist.CurrentPlaylist = CurrentPlaylist.Songs;
            });
        }

        private void SetAsPreferredButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPlaylist == null)
            {
                Helper.ShowOperationFailedNotification("");
                return;
            }
            IPreferable preferable;
            if (IsPlaylist || IsMyFavorites)
            {
                preferable = CurrentPlaylist;
            }
            else
            {
                preferable = CurrentPlaylist.ToAlbumView();
            }
            MenuFlyoutSubItem subItem = MenuFlyoutHelper.GetPreferItem(preferable);
            subItem.ToMenuFlyout().ShowAt(sender as FrameworkElement);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentPlaylist == null) return;
            if (CurrentPlaylist.DisplayItem == null || CurrentPlaylist.DisplayItem.IsDefault)
            {
                HidePlaylistCover();
                await CurrentPlaylist.LoadDisplayItemAsync();
                await SetMusicDisplayItem(CurrentPlaylist.DisplayItem);
                ShowPlaylistCover();
            }
        }

        public void AddMusic(Music music)
        {
            CurrentPlaylist.Add(music.ToVO());
            HeaderedPlaylistController.CurrentPlaylist.SetTo(CurrentPlaylist.Songs);
        }

        public void RemoveMusic(Music music)
        {
            HeaderedPlaylistController.CurrentPlaylist.Remove(music.ToVO());
        }

        void IMultiSelectListener.Execute(MultiSelectCommandBar commandBar, MultiSelectEventArgs args)
        {
            switch (args.Event)
            {
                case MultiSelectEvent.AddTo:
                    args.FlyoutHelper.DefaultPlaylistName = MenuFlyoutHelper.IsBadNewPlaylistName(CurrentPlaylist.Name) ? "" : PlaylistService.FindNextPlaylistName(CurrentPlaylist.Name);
                    args.FlyoutHelper.CurrentPlaylistName = CurrentPlaylist.Name;
                    args.FlyoutHelper.OriginalData = CurrentPlaylist;
                    break;
            }
        }

        void IPlaylistEventListener.Execute(Playlist playlist, PlaylistEventArgs args)
        {
            if (doNotListenToPlaylistEvent || (playlist != null && CurrentPlaylist.Id != playlist.Id)) return;
            switch (args.EventType)
            {
                case PlaylistEventType.AddMusic:
                    if (playlist == null)
                    {
                        doNotListenToPlaylistEvent = true;
                        PlaylistService.AddMusic(CurrentPlaylist.FromVO(), args.Music);
                        doNotListenToPlaylistEvent = false;
                    }
                    else
                    {
                        HeaderedPlaylistController.AddMusic(args.Music, CurrentPlaylist.Criterion);
                    }
                    SetPlaylistInfo(HeaderedPlaylistController.CurrentPlaylist);
                    break;
                case PlaylistEventType.RemoveMusic:
                    if (playlist == null)
                    {
                        doNotListenToPlaylistEvent = true;
                        PlaylistService.RemoveMusic(CurrentPlaylist.FromVO(), args.Music);
                        doNotListenToPlaylistEvent = false;
                    }
                    else
                    {
                        // 如果通知监听器会导致无限递归
                        HeaderedPlaylistController.RemoveMusic(args.Music.ToVO(), notifyListener: false);
                    }
                    SetPlaylistInfo(HeaderedPlaylistController.CurrentPlaylist);
                    break;
                case PlaylistEventType.Rename:
                    Confirmed(playlist.Name);
                    break;
            }
        }

        CompositionPropertySet _props;
        CompositionPropertySet _scrollerPropertySet;
        Compositor _compositor;
        private SpriteVisual _blurredBackgroundImageVisual;

        public void SetShyHeader()
        {
            // Retrieve the ScrollViewer that the GridView is using internally
            var scrollViewer = HeaderedPlaylist.ScrollViewer;

            // Update the ZIndex of the header container so that the header is above the items when scrolling
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent(HeaderedPlaylist);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            // Get the PropertySet that contains the scroll values from the ScrollViewer
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            _compositor = _scrollerPropertySet.Compositor;

            // Create a PropertySet that has values to be referenced in the ExpressionAnimations below
            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", 150);
            _props.InsertScalar("scaleFactor", 0.7f);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");

            // Create a blur effect to be animated based on scroll position
            var blurEffect = new GaussianBlurEffect()
            {
                Name = "blur",
                BlurAmount = 0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };

            var blurBrush = _compositor.CreateEffectFactory(
                blurEffect,
                new[] { "blur.BlurAmount" })
                .CreateBrush();

            blurBrush.SetSourceParameter("source", _compositor.CreateBackdropBrush());

            // Create a Visual for applying the blur effect
            _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();
            _blurredBackgroundImageVisual.Brush = blurBrush;
            _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);

            // Insert the blur visual at the right point in the Visual Tree
            ElementCompositionPreview.SetElementChildVisual(OverlayRectangle, _blurredBackgroundImageVisual);

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            // Create and start an ExpressionAnimation to animate blur radius between 0 and 15 based on progress
            ExpressionNode blurAnimation = EF.Lerp(0, 15, progressNode);
            _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

            // Get the backing visual for the header so that its properties can be animated
            Visual headerVisual = ElementCompositionPreview.GetElementVisual(PlaylistInfoGrid);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionNode headerTranslationAnimation = EF.Conditional(progressNode < 1, 0, -scrollingProperties.Translation.Y - clampSizeNode);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            // Create and start an ExpressionAnimation to scale the header during overpan
            ExpressionNode headerScaleAnimation = EF.Lerp(1, 1.25f, EF.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
            headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
            headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

            //Set the header's CenterPoint to ensure the overpan scale looks as desired
            headerVisual.CenterPoint = new Vector3((float)(PlaylistInfoGrid.ActualWidth / 2), (float)PlaylistInfoGrid.ActualHeight, 0);

            // Get the backing visual for the photo in the header so that its properties can be animated
            Visual photoVisual = ElementCompositionPreview.GetElementVisual(PlaylistInfoGrid);

            // Create and start an ExpressionAnimation to opacity fade out the image behind the header
            ExpressionNode imageOpacityAnimation = 1 - progressNode;
            photoVisual.StartAnimation("opacity", imageOpacityAnimation);

            // Get the backing visual for the profile picture visual so that its properties can be animated
            Visual profileVisual = ElementCompositionPreview.GetElementVisual(PlaylistCover);

            // Create and start an ExpressionAnimation to scale the profile image with scroll position
            ExpressionNode scaleAnimation = EF.Lerp(1, scaleFactorNode, progressNode);
            profileVisual.StartAnimation("Scale.X", scaleAnimation);
            profileVisual.StartAnimation("Scale.Y", scaleAnimation);

            // Get backing visuals for the text blocks so that their properties can be animated
            Visual subtitleVisual = ElementCompositionPreview.GetElementVisual(PlaylistInfoTextBlock);

            // Create an ExpressionAnimation that moves between 1 and 0 with scroll progress, to be used for text block opacity
            ExpressionNode textOpacityAnimation = EF.Clamp(1 - (progressNode * 2), 0, 1);

            // Start opacity and scale animations on the text block visuals
            subtitleVisual.StartAnimation("Opacity", textOpacityAnimation);
            subtitleVisual.StartAnimation("Scale.X", scaleAnimation);
            subtitleVisual.StartAnimation("Scale.Y", scaleAnimation);

            // Get the backing visuals for the text and button containers so that their properites can be animated
            Visual textVisual = ElementCompositionPreview.GetElementVisual(PlaylistInfoGrid);
            Visual buttonVisual = ElementCompositionPreview.GetElementVisual(PlaylistCommandBar);

            // When the header stops scrolling it is 150 pixels offscreen.  We want the text header to end up with 50 pixels of its content
            // offscreen which means it needs to go from offset 0 to 100 as we traverse through the scrollable region
            ExpressionNode contentOffsetAnimation = progressNode * 100;
            textVisual.StartAnimation("Offset.Y", contentOffsetAnimation);

            //ExpressionNode buttonOffsetAnimation = progressNode * -100;
            //buttonVisual.StartAnimation("Offset.Y", buttonOffsetAnimation);
        }

        private void PlaylistInfoGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetShyHeader();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_blurredBackgroundImageVisual != null)
            {
                _blurredBackgroundImageVisual.Size = new Vector2((float)OverlayRectangle.ActualWidth, (float)OverlayRectangle.ActualHeight);
            }
        }
    }

    public enum HeaderedPlaylistType
    {
        Playlist, Album, MyFavorites
    }
}
