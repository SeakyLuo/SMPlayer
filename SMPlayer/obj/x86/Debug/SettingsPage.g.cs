﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\SettingsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "361952574B049BAFF55A64DEAFAE3A52"
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
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
            case 9: // SettingsPage.xaml line 146
                {
                    this.PathBox = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.PathBox).QuerySubmitted += this.PathBox_QuerySubmitted;
                }
                break;
            case 10: // SettingsPage.xaml line 161
                {
                    this.NotificationComboBox = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.NotificationComboBox).SelectionChanged += this.NotificationComboBox_SelectionChanged;
                }
                break;
            case 11: // SettingsPage.xaml line 211
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element11 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element11).Click += this.UpdateMusicLibrary_Click;
                }
                break;
            case 12: // SettingsPage.xaml line 217
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element12 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element12).Click += this.BugReport_Click;
                }
                break;
            case 13: // SettingsPage.xaml line 179
                {
                    this.ColorPickerFlyout = (global::Windows.UI.Xaml.Controls.Flyout)(target);
                }
                break;
            case 14: // SettingsPage.xaml line 181
                {
                    this.ThemeColorPicker = (global::Windows.UI.Xaml.Controls.ColorPicker)(target);
                }
                break;
            case 15: // SettingsPage.xaml line 187
                {
                    this.ConfirmColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ConfirmColorButton).Click += this.ConfirmColorButton_Click;
                }
                break;
            case 16: // SettingsPage.xaml line 196
                {
                    this.CancelColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.CancelColorButton).Click += this.CancelColorButton_Click;
                }
                break;
            case 17: // SettingsPage.xaml line 128
                {
                    this.MusicFolderTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 18: // SettingsPage.xaml line 133
                {
                    this.NotificationTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 19: // SettingsPage.xaml line 138
                {
                    this.ThemeTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 20: // SettingsPage.xaml line 225
                {
                    this.WidthChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 21: // SettingsPage.xaml line 226
                {
                    this.Wide = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 22: // SettingsPage.xaml line 231
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

