﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "AB67AD9D2C706B3F6106704A46344B88"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMPlayer
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // MainPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 2: // MainPage.xaml line 51
                {
                    this.MainNavigationView = (global::Windows.UI.Xaml.Controls.NavigationView)(target);
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).ItemInvoked += this.MainNavigationView_ItemInvoked;
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).PaneClosing += this.MainNavigationView_PaneClosing;
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).PaneOpening += this.MainNavigationView_PaneOpening;
                }
                break;
            case 3: // MainPage.xaml line 280
                {
                    this.ShuffleButton = (global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)(target);
                    ((global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)this.ShuffleButton).Click += this.ShuffleButton_Click;
                }
                break;
            case 4: // MainPage.xaml line 285
                {
                    this.RepeatButton = (global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)(target);
                    ((global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)this.RepeatButton).Click += this.RepeatButton_Click;
                }
                break;
            case 5: // MainPage.xaml line 290
                {
                    this.RepeatOneButton = (global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)(target);
                    ((global::Windows.UI.Xaml.Controls.Primitives.ToggleButton)this.RepeatOneButton).Click += this.RepeatOneButton_Click;
                }
                break;
            case 6: // MainPage.xaml line 256
                {
                    this.VolumeButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.VolumeButton).Click += this.VolumeButton_Click;
                }
                break;
            case 7: // MainPage.xaml line 261
                {
                    this.VolumeSlider = (global::Windows.UI.Xaml.Controls.Slider)(target);
                    ((global::Windows.UI.Xaml.Controls.Slider)this.VolumeSlider).ValueChanged += this.VolumeSlider_ValueChanged;
                }
                break;
            case 8: // MainPage.xaml line 270
                {
                    this.LikeButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.LikeButton).Click += this.LikeButton_Click;
                }
                break;
            case 9: // MainPage.xaml line 275
                {
                    this.LikeButtonIcon = (global::Windows.UI.Xaml.Controls.FontIcon)(target);
                }
                break;
            case 10: // MainPage.xaml line 220
                {
                    this.LeftTimeTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 11: // MainPage.xaml line 227
                {
                    this.MediaSlider = (global::Windows.UI.Xaml.Controls.Slider)(target);
                    ((global::Windows.UI.Xaml.Controls.Slider)this.MediaSlider).ManipulationCompleted += this.MediaSlider_ManipulationCompleted;
                    ((global::Windows.UI.Xaml.Controls.Slider)this.MediaSlider).ManipulationStarted += this.MediaSlider_ManipulationStarted;
                    ((global::Windows.UI.Xaml.Controls.Slider)this.MediaSlider).ManipulationStarting += this.MediaSlider_ManipulationStarting;
                    ((global::Windows.UI.Xaml.Controls.Slider)this.MediaSlider).ValueChanged += this.MediaSlider_ValueChanged;
                }
                break;
            case 12: // MainPage.xaml line 241
                {
                    this.RightTimeTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 13: // MainPage.xaml line 189
                {
                    this.PrevButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.PrevButton).Click += this.PrevButton_Click;
                }
                break;
            case 14: // MainPage.xaml line 194
                {
                    this.PlayButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.PlayButton).Click += this.PlayButton_Click;
                }
                break;
            case 15: // MainPage.xaml line 208
                {
                    this.NextButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.NextButton).Click += this.NextButton_Click;
                }
                break;
            case 16: // MainPage.xaml line 199
                {
                    this.PlayButtonIcon = (global::Windows.UI.Xaml.Controls.FontIcon)(target);
                }
                break;
            case 17: // MainPage.xaml line 159
                {
                    this.AlbumCover = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 18: // MainPage.xaml line 168
                {
                    this.TitleTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 19: // MainPage.xaml line 176
                {
                    this.ArtistTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 20: // MainPage.xaml line 61
                {
                    this.NaviSearchItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 21: // MainPage.xaml line 65
                {
                    this.NaviSearchBarItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 22: // MainPage.xaml line 75
                {
                    this.MusicLibraryItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 23: // MainPage.xaml line 80
                {
                    this.ArtistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 24: // MainPage.xaml line 88
                {
                    this.AlbumsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 25: // MainPage.xaml line 97
                {
                    this.NowPlayingItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 26: // MainPage.xaml line 102
                {
                    this.RecentItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 27: // MainPage.xaml line 110
                {
                    this.LocalItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 28: // MainPage.xaml line 118
                {
                    this.PlaylistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 29: // MainPage.xaml line 126
                {
                    this.MyFavoritesItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 30: // MainPage.xaml line 66
                {
                    this.NaviSearchBar = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.NaviSearchBar).QuerySubmitted += this.NaviSearchBar_QuerySubmitted;
                }
                break;
            case 31: // MainPage.xaml line 135
                {
                    this.NaviFrame = (global::Windows.UI.Xaml.Controls.Frame)(target);
                    ((global::Windows.UI.Xaml.Controls.Frame)this.NaviFrame).Navigated += this.NaviFrame_Navigated;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

