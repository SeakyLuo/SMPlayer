﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\LocalPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6B02EEFD99037C1E5258513096F37FEB"
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
    partial class LocalPage : 
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
            case 1: // LocalPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 2: // LocalPage.xaml line 26
                {
                    this.LocalNavigationView = (global::Windows.UI.Xaml.Controls.NavigationView)(target);
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.LocalNavigationView).BackRequested += this.LocalNavigationView_BackRequested;
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.LocalNavigationView).ItemInvoked += this.LocalNavigationView_ItemInvoked;
                }
                break;
            case 3: // LocalPage.xaml line 35
                {
                    this.LocalFoldersItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 4: // LocalPage.xaml line 40
                {
                    this.LocalSongsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 5: // LocalPage.xaml line 45
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyout element5 = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyout)element5).Opening += this.OpenLocalMusicFlyout;
                }
                break;
            case 6: // LocalPage.xaml line 51
                {
                    this.LocalRefreshItem = (global::SMPlayer.IconTextButton)(target);
                    ((global::SMPlayer.IconTextButton)this.LocalRefreshItem).Tapped += this.LocalRefreshItem_Tapped;
                }
                break;
            case 7: // LocalPage.xaml line 62
                {
                    this.LocalShuffleItem = (global::SMPlayer.IconTextButton)(target);
                    ((global::SMPlayer.IconTextButton)this.LocalShuffleItem).Tapped += this.LocalShuffleItem_Tapped;
                }
                break;
            case 8: // LocalPage.xaml line 73
                {
                    this.LocalListViewItem = (global::SMPlayer.IconTextButton)(target);
                    ((global::SMPlayer.IconTextButton)this.LocalListViewItem).Tapped += this.LocalListViewItem_Tapped;
                }
                break;
            case 9: // LocalPage.xaml line 86
                {
                    this.LocalGridViewItem = (global::SMPlayer.IconTextButton)(target);
                    ((global::SMPlayer.IconTextButton)this.LocalGridViewItem).Tapped += this.LocalGridViewItem_Tapped;
                }
                break;
            case 10: // LocalPage.xaml line 101
                {
                    this.LocalFrame = (global::Windows.UI.Xaml.Controls.Frame)(target);
                }
                break;
            case 11: // LocalPage.xaml line 104
                {
                    this.WidthChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 12: // LocalPage.xaml line 105
                {
                    this.Wide = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 13: // LocalPage.xaml line 110
                {
                    this.Narrow = (global::Windows.UI.Xaml.VisualState)(target);
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

