﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\NowPlayingPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CB4FBD2746D09570B4BD07912B51F954"
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
    partial class NowPlayingPage : 
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
            case 1: // NowPlayingPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 2: // NowPlayingPage.xaml line 19
                {
                    this.NowPlayingCommandBar = (global::Windows.UI.Xaml.Controls.CommandBar)(target);
                }
                break;
            case 3: // NowPlayingPage.xaml line 74
                {
                    this.NowPlayingPlaylistControl = (global::SMPlayer.PlaylistControl)(target);
                }
                break;
            case 4: // NowPlayingPage.xaml line 27
                {
                    this.CommandBarHeaderTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 5: // NowPlayingPage.xaml line 36
                {
                    this.RandomPlayButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.RandomPlayButton).Click += this.RandomPlayButton_Click;
                }
                break;
            case 6: // NowPlayingPage.xaml line 42
                {
                    this.ScrollToCurrentButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.ScrollToCurrentButton).Click += this.ScrollToCurrentButton_Click;
                }
                break;
            case 7: // NowPlayingPage.xaml line 52
                {
                    this.SaveToButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.SaveToButton).Click += this.SaveToButton_Click;
                }
                break;
            case 8: // NowPlayingPage.xaml line 59
                {
                    this.ClearButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.ClearButton).Click += this.ClearButton_Click;
                }
                break;
            case 9: // NowPlayingPage.xaml line 66
                {
                    this.FullScreenButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.FullScreenButton).Click += this.FullScreenButton_Click;
                }
                break;
            case 10: // NowPlayingPage.xaml line 82
                {
                    this.WidthChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 11: // NowPlayingPage.xaml line 83
                {
                    this.WideLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 12: // NowPlayingPage.xaml line 88
                {
                    this.Minimal = (global::Windows.UI.Xaml.VisualState)(target);
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

