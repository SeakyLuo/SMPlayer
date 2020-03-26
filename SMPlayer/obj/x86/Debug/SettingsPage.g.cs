﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\SettingsPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "873060964942DC216F6617802863BF17"
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
            case 9: // SettingsPage.xaml line 164
                {
                    this.PathBox = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.PathBox).QuerySubmitted += this.PathBox_QuerySubmitted;
                }
                break;
            case 10: // SettingsPage.xaml line 176
                {
                    this.NotificationComboBox = (global::Windows.UI.Xaml.Controls.ComboBox)(target);
                    ((global::Windows.UI.Xaml.Controls.ComboBox)this.NotificationComboBox).SelectionChanged += this.NotificationComboBox_SelectionChanged;
                }
                break;
            case 11: // SettingsPage.xaml line 225
                {
                    this.KeepRecentCheckBox = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.KeepRecentCheckBox).Checked += this.KeepRecentCheckBox_Checked;
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.KeepRecentCheckBox).Unchecked += this.KeepRecentCheckBox_Unchecked;
                }
                break;
            case 12: // SettingsPage.xaml line 232
                {
                    this.AutoPlayCheckBox = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.AutoPlayCheckBox).Checked += this.AutoPlayCheckBox_Checked;
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.AutoPlayCheckBox).Unchecked += this.AutoPlayCheckBox_Unchecked;
                }
                break;
            case 13: // SettingsPage.xaml line 239
                {
                    this.SaveProgressCheckBox = (global::Windows.UI.Xaml.Controls.CheckBox)(target);
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.SaveProgressCheckBox).Checked += this.SaveProgressCheckBox_Checked;
                    ((global::Windows.UI.Xaml.Controls.CheckBox)this.SaveProgressCheckBox).Unchecked += this.SaveProgressCheckBox_Unchecked;
                }
                break;
            case 14: // SettingsPage.xaml line 246
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element14 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element14).Click += this.ClearHistoryButton_Click;
                }
                break;
            case 15: // SettingsPage.xaml line 252
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element15 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element15).Click += this.UpdateMusicLibrary_Click;
                }
                break;
            case 16: // SettingsPage.xaml line 258
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element16 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element16).Click += this.SaveChanges_Click;
                }
                break;
            case 17: // SettingsPage.xaml line 264
                {
                    global::Windows.UI.Xaml.Controls.HyperlinkButton element17 = (global::Windows.UI.Xaml.Controls.HyperlinkButton)(target);
                    ((global::Windows.UI.Xaml.Controls.HyperlinkButton)element17).Click += this.BugReport_Click;
                }
                break;
            case 18: // SettingsPage.xaml line 193
                {
                    this.ColorPickerFlyout = (global::Windows.UI.Xaml.Controls.Flyout)(target);
                }
                break;
            case 19: // SettingsPage.xaml line 195
                {
                    this.ThemeColorPicker = (global::Windows.UI.Xaml.Controls.ColorPicker)(target);
                }
                break;
            case 20: // SettingsPage.xaml line 201
                {
                    this.ConfirmColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ConfirmColorButton).Click += this.ConfirmColorButton_Click;
                }
                break;
            case 21: // SettingsPage.xaml line 210
                {
                    this.CancelColorButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.CancelColorButton).Click += this.CancelColorButton_Click;
                }
                break;
            case 22: // SettingsPage.xaml line 132
                {
                    this.MusicFolderTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 23: // SettingsPage.xaml line 137
                {
                    this.NotificationTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 24: // SettingsPage.xaml line 142
                {
                    this.ThemeTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 25: // SettingsPage.xaml line 147
                {
                    this.RecentTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 26: // SettingsPage.xaml line 152
                {
                    this.PlayTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 27: // SettingsPage.xaml line 157
                {
                    this.SaveTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 28: // SettingsPage.xaml line 272
                {
                    this.WidthChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 29: // SettingsPage.xaml line 273
                {
                    this.Wide = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 30: // SettingsPage.xaml line 278
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

