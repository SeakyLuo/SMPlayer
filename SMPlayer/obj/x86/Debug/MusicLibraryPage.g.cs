﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MusicLibraryPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D417273FD109AD01500B0D2924A08F3D"
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
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
            case 4: // MusicLibraryPage.xaml line 114
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyout element4 = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyout)element4).Opening += this.MenuFlyout_Opening;
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

