﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\SettingsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "04A66D76079BDB507C89F7F515BF5D0B"
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
    partial class SettingsPage : 
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
            case 1: // SettingsPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 4: // SettingsPage.xaml line 197
                {
                    this.SettingsLoadingControl = (global::SMPlayer.LoadingControl)(target);
                }
                break;
            case 5: // SettingsPage.xaml line 122
                {
                    this.PathBox = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.PathBox).QuerySubmitted += this.PathBox_QuerySubmitted;
                }
                break;
            case 6: // SettingsPage.xaml line 137
                {
                    this.LanguageComboBox = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                }
                break;
            case 7: // SettingsPage.xaml line 150
                {
                    this.NotificationComboBox = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                }
                break;
            case 8: // SettingsPage.xaml line 174
                {
                    this.ThemeColorPicker = (global::Windows.UI.Xaml.Controls.ColorPicker)(target);
                }
                break;
            case 9: // SettingsPage.xaml line 180
                {
                    this.ConfirmColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ConfirmColorButton).Click += this.ConfirmColorButton_Click;
                }
                break;
            case 10: // SettingsPage.xaml line 185
                {
                    this.CancelColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
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

