using SMPlayer.Dialogs;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using ExpressionBuilder;
using EF = ExpressionBuilder.ExpressionFunctions;
using System.Threading.Tasks;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SMPlayer
{
    public sealed partial class HeaderedPlaylistControl : UserControl
    {
        public Playlist MusicCollection { get; set; }
        public Brush HeaderBackground
        {
            get => headerBackground;
            set => OverlayRectangle.Fill = headerBackground = value;
        }
        private Brush headerBackground = ColorHelper.HighlightBrush;
        public bool ShowAlbumText
        {
            get => HeaderedPlaylist.ShowAlbumText;
            set => HeaderedPlaylist.ShowAlbumText = value;
        }

        public bool IsPlaylist { get; set; }
        private static Dictionary<string, List<MusicDisplayItem>> PlaylistDisplayDict = new Dictionary<string, List<MusicDisplayItem>>();
        private static readonly Random random = new Random();
        private static RenameDialog dialog;
        public PlaylistControl Playlist { get => HeaderedPlaylist; }
        public HeaderedPlaylistControl()
        {
            this.InitializeComponent();
        }

        public async Task SetMusicCollection(Playlist playlist)
        {
            MusicCollection = playlist;
            HeaderedPlaylist.ItemsSource = playlist.Songs;
            PlaylistNameTextBlock.Text = playlist.Name;
            SetPlaylistInfo(SongCountConverter.ToStr(playlist.Songs));
            ShuffleButton.IsEnabled = playlist.Songs.Count != 0;
            AddToButton.IsEnabled = playlist.Songs.Count != 0;
            RenameButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            DeleteButton.Visibility = IsPlaylist ? Visibility.Visible : Visibility.Collapsed;
            MusicDisplayItem item;
            if (PlaylistDisplayDict.TryGetValue(playlist.Name, out List<MusicDisplayItem> MusicDisplayItems))
            {
                item = MusicDisplayItems[random.Next(MusicDisplayItems.Count)];
            }
            else
            {
                if (MusicDisplayItem.IsNullOrEmpty(playlist.DisplayItem))
                    await playlist.SetDisplayItemAsync();
                item = playlist.DisplayItem;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var items = await playlist.GetMusicDisplayItemsAsync();
                    if (items.Count > 0) PlaylistDisplayDict[playlist.Name] = items;
                });
            }
            PlaylistCover.Source = item.Thumbnail;
            HeaderBackground = item.Color;
        }

        public void SetPlaylistInfo(string info)
        {
            PlaylistInfoTextBlock.Text = info;
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            MediaHelper.ShuffleAndPlay(MusicCollection.Songs);
        }
        private void AddTo_Click(object sender, RoutedEventArgs e)
        {
            var helper = new MenuFlyoutHelper()
            {
                Data = MusicCollection.Songs,
                DefaultPlaylistName = Settings.settings.FindNextPlaylistName(MusicCollection.Name)
            };
            helper.GetAddToMenuFlyout(MusicCollection.Name).ShowAt(sender as FrameworkElement);
        }
        private async void Rename_Click(object sender, RoutedEventArgs e)
        {
            dialog = new RenameDialog(Confirm, RenameOption.Rename, MusicCollection.Name);
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
            PlaylistsPage.DeletePlaylist(MusicCollection);
        }

        private SymbolIcon PinIcon = new SymbolIcon(Symbol.Pin);
        private SymbolIcon UnPinIcon = new SymbolIcon(Symbol.UnPin);
        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            bool isPinned = await Helper.PinToStartAsync(MusicCollection, IsPlaylist);
            if (isPinned)
            {
                PinToStartButton.Icon = PinIcon;
                PinToStartButton.Content = "Pin To Start";
            }
            else
            {
                PinToStartButton.Icon = UnPinIcon;
                PinToStartButton.Content = "UnPin";
            }
        }

        public async void MusicSwitching(Music current, Music next, Windows.Media.Playback.MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaHelper.FindMusicAndSetPlaying(MusicCollection.Songs, current, next);
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
    }
}
