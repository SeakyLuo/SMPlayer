﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MusicLibraryPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8A04212372E84809CB4DDC8ED6046991"
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
            case 2: // MusicLibraryPage.xaml line 44
                {
                    this.MusicLibraryNavigationView = (global::Windows.UI.Xaml.Controls.NavigationView)(target);
                }
                break;
            case 3: // MusicLibraryPage.xaml line 50
                {
                    this.SongsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 4: // MusicLibraryPage.xaml line 58
                {
                    this.ArtistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 5: // MusicLibraryPage.xaml line 66
                {
                    this.AlbumsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 6: // MusicLibraryPage.xaml line 75
                {
                    this.MusicLibraryFrame = (global::Windows.UI.Xaml.Controls.Frame)(target);
                }
                break;
            case 7: // MusicLibraryPage.xaml line 30
                {
                    global::Windows.UI.Xaml.Controls.Button element7 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element7).Click += this.OpenFolder;
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

