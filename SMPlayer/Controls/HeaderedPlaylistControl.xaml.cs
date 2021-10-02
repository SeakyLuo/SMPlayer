using ExpressionBuilder;
using Microsoft.Graphics.Canvas.Effects;
using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Models;
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
    public sealed partial class HeaderedPlaylistControl : UserControl, IRemoveMusicListener, IImageSavedListener, IMultiSelectListener
    {
        public PlaylistControl HeaderedPlaylist { get => HeaderedPlaylistController; }
        public Playlist CurrentPlaylist { get; private set; }
        public Brush HeaderBackground
        {
            get => headerBackground;
            set => OverlayRectangle.Fill = headerBackground = value;
        }
        private Brush headerBackground = ColorHelper.HighlightBrush;
        public bool ShowAlbumText
        {
            get => HeaderedPlaylistController.ShowAlbumText;
            set => HeaderedPlaylistController.ShowAlbumText = value;
        }
        public bool IsPlaylist 
        {
            get => isPlaylist;
            set
            {
                isPlaylist = value;
                EditAlbumArtButton.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        private bool isPlaylist = true;
        public bool AllowClear
        {
            get => allowClear;
            set
            {
                allowClear = value;
                ClearButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private bool allowClear = true;
        public bool Removable
        {
            get => HeaderedPlaylistController.Removable;
            set => HeaderedPlaylistController.Removable = value;
        }

        public bool IsMyFavorites
        {
            get => CurrentPlaylist.Name == Settings.settings.MyFavorites.Name;
        }

        private static RenameDialog dialog;
        private static RemoveDialog removeDialog;
        public HeaderedPlaylistControl()
        {
            this.InitializeComponent();
            HeaderedPlaylistController.RemoveListeners.Add(this);
            HeaderedPlaylistController.MultiSelectListener = this;
            AlbumArtControl.ImageSavedListeners.Add(this);
        }

        public async Task SetPlaylist(Playlist playlist)
        {
            HidePlaylistCover();
            MediaHelper.FindMusicAndSetPlaying(playlist.Songs, null, MediaHelper.CurrentMusic);
            CurrentPlaylist = playlist;
            HeaderedPlaylist.ItemsSource = playlist.Songs;
            PlaylistNameTextBlock.Text = string.IsNullOrEmpty(playlist.Name) && !IsPlaylist ? Helper.LocalizeMessage("UnknownAlbum") : playlist.Name;
            SetPlaylistInfo(SongCountConverter.ToStr(playlist.Songs));
            ShuffleButton.IsEnabled = playlist.Count != 0;
            RenameButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            bool isPreferred = IsPlaylist ? Settings.settings.Preference.IsPreferred(playlist) :
                                            IsMyFavorites ? Settings.settings.Preference.MyFavorites :
                                                            Settings.settings.Preference.IsPreferred(playlist.ToAlbumView());
            SetAsPreferredButton.Visibility = isPreferred ? Visibility.Collapsed : Visibility.Visible;
            UndoPreferButton.Visibility = isPreferred ? Visibility.Visible : Visibility.Collapsed;
            SetPinState(Windows.UI.StartScreen.SecondaryTile.Exists(TileHelper.FormatTileId(playlist, IsPlaylist)));
            if (playlist.DisplayItem == null)
            {
                await playlist.SetDisplayItemAsync();
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
            PlaylistCover.Source = await item.GetThumbnailAsync();
            HeaderBackground = item.Color;
        }

        public void SetPlaylistInfo(string info)
        {
            PlaylistInfoTextBlock.Text = info;
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.ShuffleAndPlay(CurrentPlaylist.Songs);
        }

        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            dialog = new RenameDialog(Confirm, RenameOption.Rename, CurrentPlaylist.Name);
            await dialog.ShowAsync();
        }

        public bool Confirm(string OldName, string NewName)
        {
            bool successful = PlaylistsPage.ConfirmRenaming(dialog, OldName, NewName);
            if (successful)
            {
                PlaylistNameTextBlock.Text = NewName;
            }
            return successful;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            DeletePlaylist(CurrentPlaylist);
        }

        public async void DeletePlaylist(Playlist playlist)
        {
            if (removeDialog == null)
            {
                removeDialog = new RemoveDialog()
                {
                    Confirm = () => ExecutePlaylistDeletion(playlist)
                };
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
        private void ExecutePlaylistDeletion(Playlist playlist)
        {
            int index = Settings.settings.Playlists.IndexOf(playlist);
            PlaylistsPage.Playlists.RemoveAt(index);
            Settings.settings.Playlists.RemoveAt(index);
            MainPage.Instance.ShowUndoNotification(Helper.LocalizeMessage("PlaylistRemoved", playlist.Name), () =>
            {
                PlaylistsPage.Playlists.Insert(index, playlist);
                Settings.settings.Playlists.Insert(index, playlist);
            });
        }

        public void MusicRemoved(int index, Music music, IEnumerable<Music> newCollection)
        {
            if (AllowClear) SetPlaylistInfo(SongCountConverter.ToStr(newCollection));
        }

        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            SetPinState(await TileHelper.PinToStartAsync(CurrentPlaylist, IsPlaylist));
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
                CurrentPlaylist.Clear();
                SetPlaylistInfo(SongCountConverter.ToStr(CurrentPlaylist.Songs));
            };
            await removeDialog.ShowAsync();
        }

        private async void EditAlbumArtButton_Click(object sender, RoutedEventArgs e)
        {
            await new AlbumDialog(AlbumDialogOption.AlbumArt, CurrentPlaylist.ToAlbumView()).ShowAsync();
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

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.FindMusicAndSetPlaying(CurrentPlaylist.Songs, current, next);
            });
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

        public async void SaveAlbum(AlbumView album, BitmapImage image)
        {
            // IsAlbum
            if (!IsPlaylist && CurrentPlaylist.Name == album.Name)
            {
                MusicDisplayItem item = await album.Songs[0].GetMusicDisplayItemAsync();
                await SetMusicDisplayItem(CurrentPlaylist.DisplayItem = item);
            }
        }

        public async void SaveMusic(Music music, BitmapImage image)
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
            MenuFlyoutHelper.SetPlaylistSortByMenu(sender, CurrentPlaylist);
        }

        void IMultiSelectListener.AddTo(MultiSelectCommandBar commandBar, MenuFlyoutHelper helper)
        {
            helper.DefaultPlaylistName = MenuFlyoutHelper.IsBadNewPlaylistName(CurrentPlaylist.Name) ? "" : Settings.settings.FindNextPlaylistName(CurrentPlaylist.Name);
            helper.CurrentPlaylistName = CurrentPlaylist.Name;
        }

        void IMultiSelectListener.Cancel(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.Play(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.Remove(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.SelectAll(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.ClearSelections(MultiSelectCommandBar commandBar) { }
        void IMultiSelectListener.ReverseSelections(MultiSelectCommandBar commandBar) { }

        private void SetAsPreferredButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaylist)
            {
                Settings.settings.Preference.Prefer(CurrentPlaylist);
            }
            else
            {
                if (IsMyFavorites)
                {
                    Settings.settings.Preference.MyFavorites = true;
                }
                else
                {
                    Settings.settings.Preference.Prefer(CurrentPlaylist.ToAlbumView());
                }
            }
            (sender as AppBarButton).Visibility = Visibility.Collapsed;
            UndoPreferButton.Visibility = Visibility.Visible;
        }

        private void UndoPreferButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlaylist)
            {
                Settings.settings.Preference.UndoPrefer(CurrentPlaylist);
            }
            else
            {
                if (IsMyFavorites)
                {
                    Settings.settings.Preference.MyFavorites = false;
                }
                else
                {
                    Settings.settings.Preference.UndoPrefer(CurrentPlaylist.ToAlbumView());
                }
            }
            (sender as AppBarButton).Visibility = Visibility.Collapsed;
            SetAsPreferredButton.Visibility = Visibility.Visible;
        }
    }
}
