﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4E7B09EF21CA7EB10DD6D7A21FFE76FD"
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
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
            case 3: // MainPage.xaml line 141
                {
                    this.MainMediaControl = (global::SMPlayer.MediaControl)(target);
                }
                break;
            case 4: // MainPage.xaml line 61
                {
                    this.NaviSearchItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 5: // MainPage.xaml line 66
                {
                    this.NaviSearchBarItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 6: // MainPage.xaml line 75
                {
                    this.MusicLibraryItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 7: // MainPage.xaml line 80
                {
                    this.ArtistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 8: // MainPage.xaml line 88
                {
                    this.AlbumsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 9: // MainPage.xaml line 97
                {
                    this.NowPlayingItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 10: // MainPage.xaml line 102
                {
                    this.RecentItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 11: // MainPage.xaml line 110
                {
                    this.LocalItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 12: // MainPage.xaml line 118
                {
                    this.PlaylistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 13: // MainPage.xaml line 126
                {
                    this.MyFavoritesItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 14: // MainPage.xaml line 67
                {
                    this.NaviSearchBar = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.NaviSearchBar).QuerySubmitted += this.NaviSearchBar_QuerySubmitted;
                }
                break;
            case 15: // MainPage.xaml line 135
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

