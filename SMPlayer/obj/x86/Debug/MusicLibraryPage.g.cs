﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MusicLibraryPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "58936CFC7FE5D1C2907A577F91A61A7B"
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
    partial class MusicLibraryPage : 
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
            case 1: // MusicLibraryPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 2: // MusicLibraryPage.xaml line 18
                {
                    this.MusicLibraryTitleTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3: // MusicLibraryPage.xaml line 22
                {
                    this.MusicLibraryDataGrid = (global::Microsoft.Toolkit.Uwp.UI.Controls.DataGrid)(target);
                    ((global::Microsoft.Toolkit.Uwp.UI.Controls.DataGrid)this.MusicLibraryDataGrid).DoubleTapped += this.MusicLibraryDataGrid_DoubleTapped;
                    ((global::Microsoft.Toolkit.Uwp.UI.Controls.DataGrid)this.MusicLibraryDataGrid).Sorting += this.MusicLibraryDataGrid_Sorting;
                }
                break;
            case 4: // MusicLibraryPage.xaml line 155
                {
                    this.MusicLibraryProgressRing = (global::Windows.UI.Xaml.Controls.ProgressRing)(target);
                }
                break;
            case 5: // MusicLibraryPage.xaml line 122
                {
                    this.PlayItem = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.PlayItem).Click += this.PlayItem_Click;
                }
                break;
            case 6: // MusicLibraryPage.xaml line 127
                {
                    this.AddToPlaylistsItem = (global::Windows.UI.Xaml.Controls.MenuFlyoutSubItem)(target);
                }
                break;
            case 7: // MusicLibraryPage.xaml line 138
                {
                    this.DeleteItem = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.DeleteItem).Click += this.DeleteItem_Click;
                }
                break;
            case 8: // MusicLibraryPage.xaml line 143
                {
                    this.MusicInfoItem = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)this.MusicInfoItem).Click += this.MusicInfoItem_Click;
                }
                break;
            case 9: // MusicLibraryPage.xaml line 133
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyoutItem element9 = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)element9).Click += this.AddToMyFavorites_Click;
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

