﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F3BCB67344570A524A68353ACD110E6F"
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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_FrameworkElement_Width(global::Windows.UI.Xaml.FrameworkElement obj, global::System.Double value)
            {
                obj.Width = value;
            }
            public static void Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_From(global::Windows.UI.Xaml.Media.Animation.DoubleAnimation obj, global::System.Nullable<global::System.Double> value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Double) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Double), targetNullValue);
                }
                obj.From = value;
            }
            public static void Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_To(global::Windows.UI.Xaml.Media.Animation.DoubleAnimation obj, global::System.Nullable<global::System.Double> value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Double) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Double), targetNullValue);
                }
                obj.To = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class MainPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IMainPage_Bindings
        {
            private global::SMPlayer.MainPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.Border obj24;
            private global::Windows.UI.Xaml.Controls.TextBlock obj25;
            private global::Windows.UI.Xaml.Media.Animation.DoubleAnimation obj30;
            private global::Windows.UI.Xaml.Media.Animation.DoubleAnimation obj31;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj24WidthDisabled = false;
            private static bool isobj25TextDisabled = false;
            private static bool isobj30FromDisabled = false;
            private static bool isobj30ToDisabled = false;
            private static bool isobj31FromDisabled = false;
            private static bool isobj31ToDisabled = false;

            private MainPage_obj1_BindingsTracking bindingsTracking;

            public MainPage_obj1_Bindings()
            {
                this.bindingsTracking = new MainPage_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 37 && columnNumber == 17)
                {
                    isobj24WidthDisabled = true;
                }
                else if (lineNumber == 47 && columnNumber == 17)
                {
                    isobj25TextDisabled = true;
                }
                else if (lineNumber == 268 && columnNumber == 33)
                {
                    isobj30FromDisabled = true;
                }
                else if (lineNumber == 269 && columnNumber == 33)
                {
                    isobj30ToDisabled = true;
                }
                else if (lineNumber == 248 && columnNumber == 33)
                {
                    isobj31FromDisabled = true;
                }
                else if (lineNumber == 249 && columnNumber == 33)
                {
                    isobj31ToDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 24: // MainPage.xaml line 34
                        this.obj24 = (global::Windows.UI.Xaml.Controls.Border)target;
                        break;
                    case 25: // MainPage.xaml line 40
                        this.obj25 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 30: // MainPage.xaml line 264
                        this.obj30 = (global::Windows.UI.Xaml.Media.Animation.DoubleAnimation)target;
                        break;
                    case 31: // MainPage.xaml line 244
                        this.obj31 = (global::Windows.UI.Xaml.Media.Animation.DoubleAnimation)target;
                        break;
                    default:
                        break;
                }
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
            }

            public void Recycle()
            {
                return;
            }

            // IMainPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
                this.bindingsTracking.ReleaseAllListeners();
                this.initialized = false;
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                this.bindingsTracking.ReleaseAllListeners();
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::SMPlayer.MainPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.MainPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_MainNavigationView(obj.MainNavigationView, phase);
                    }
                }
                this.Update_Windows_ApplicationModel_Package_Current(global::Windows.ApplicationModel.Package.Current, phase);
            }
            private void Update_MainNavigationView(global::Windows.UI.Xaml.Controls.NavigationView obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_MainNavigationView_OpenPaneLength(obj.OpenPaneLength, phase);
                        this.Update_MainNavigationView_CompactPaneLength(obj.CompactPaneLength, phase);
                    }
                }
            }
            private void Update_MainNavigationView_OpenPaneLength(global::System.Double obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // MainPage.xaml line 34
                    if (!isobj24WidthDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_Width(this.obj24, obj);
                    }
                    // MainPage.xaml line 264
                    if (!isobj30FromDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_From(this.obj30, obj, null);
                    }
                    // MainPage.xaml line 244
                    if (!isobj31ToDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_To(this.obj31, obj, null);
                    }
                }
            }
            private void Update_Windows_ApplicationModel_Package_Current(global::Windows.ApplicationModel.Package obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Windows_ApplicationModel_Package_Current_DisplayName(obj.DisplayName, phase);
                    }
                }
            }
            private void Update_Windows_ApplicationModel_Package_Current_DisplayName(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // MainPage.xaml line 40
                    if (!isobj25TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj25, obj, null);
                    }
                }
            }
            private void Update_MainNavigationView_CompactPaneLength(global::System.Double obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // MainPage.xaml line 264
                    if (!isobj30ToDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_To(this.obj30, obj, null);
                    }
                    // MainPage.xaml line 244
                    if (!isobj31FromDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_Animation_DoubleAnimation_From(this.obj31, obj, null);
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class MainPage_obj1_BindingsTracking
            {
                private global::System.WeakReference<MainPage_obj1_Bindings> weakRefToBindingObj; 

                public MainPage_obj1_BindingsTracking(MainPage_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<MainPage_obj1_Bindings>(obj);
                }

                public MainPage_obj1_Bindings TryGetBindingObject()
                {
                    MainPage_obj1_Bindings bindingObject = null;
                    if (weakRefToBindingObj != null)
                    {
                        weakRefToBindingObj.TryGetTarget(out bindingObject);
                        if (bindingObject == null)
                        {
                            weakRefToBindingObj = null;
                            ReleaseAllListeners();
                        }
                    }
                    return bindingObject;
                }

                public void ReleaseAllListeners()
                {
                }

            }
        }
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
            case 2: // MainPage.xaml line 22
                {
                    this.AppTitleBar = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 3: // MainPage.xaml line 50
                {
                    this.MainNavigationView = (global::Windows.UI.Xaml.Controls.NavigationView)(target);
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).ItemInvoked += this.MainNavigationView_ItemInvoked;
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).PaneClosing += this.MainNavigationView_PaneClosing;
                    ((global::Windows.UI.Xaml.Controls.NavigationView)this.MainNavigationView).PaneOpening += this.MainNavigationView_PaneOpening;
                }
                break;
            case 4: // MainPage.xaml line 185
                {
                    this.BackButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BackButton).Click += this.BackButton_Click;
                }
                break;
            case 5: // MainPage.xaml line 194
                {
                    this.FakeTogglePaneButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.FakeTogglePaneButton).Click += this.FakeTogglePaneButton_Click;
                }
                break;
            case 6: // MainPage.xaml line 201
                {
                    this.MainMediaControl = (global::SMPlayer.MediaControl)(target);
                }
                break;
            case 7: // MainPage.xaml line 205
                {
                    this.MainLoadingControl = (global::SMPlayer.LoadingControl)(target);
                }
                break;
            case 8: // MainPage.xaml line 61
                {
                    this.HeaderGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 9: // MainPage.xaml line 66
                {
                    this.MainNavigationViewHeader = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 10: // MainPage.xaml line 71
                {
                    this.HeaderSearchButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.HeaderSearchButton).Click += this.HeaderSearchButton_Click;
                }
                break;
            case 11: // MainPage.xaml line 84
                {
                    this.HeaderSearchBar = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.HeaderSearchBar).FocusDisengaged += this.HeaderNaviSearchBar_FocusDisengaged;
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.HeaderSearchBar).LosingFocus += this.HeaderNaviSearchBar_LosingFocus;
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.HeaderSearchBar).NoFocusCandidateFound += this.HeaderNaviSearchBar_NoFocusCandidateFound;
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.HeaderSearchBar).QuerySubmitted += this.SearchBar_QuerySubmitted;
                }
                break;
            case 12: // MainPage.xaml line 101
                {
                    this.NaviSearchBar = (global::Windows.UI.Xaml.Controls.AutoSuggestBox)(target);
                    ((global::Windows.UI.Xaml.Controls.AutoSuggestBox)this.NaviSearchBar).QuerySubmitted += this.SearchBar_QuerySubmitted;
                }
                break;
            case 13: // MainPage.xaml line 110
                {
                    this.MusicLibraryItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 14: // MainPage.xaml line 116
                {
                    this.ArtistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 15: // MainPage.xaml line 125
                {
                    this.AlbumsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 16: // MainPage.xaml line 135
                {
                    this.NowPlayingItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 17: // MainPage.xaml line 143
                {
                    this.RecentItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 18: // MainPage.xaml line 152
                {
                    this.LocalItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 19: // MainPage.xaml line 161
                {
                    this.PlaylistsItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 20: // MainPage.xaml line 170
                {
                    this.MyFavoritesItem = (global::Windows.UI.Xaml.Controls.NavigationViewItem)(target);
                }
                break;
            case 21: // MainPage.xaml line 180
                {
                    this.NaviFrame = (global::Windows.UI.Xaml.Controls.Frame)(target);
                    ((global::Windows.UI.Xaml.Controls.Frame)this.NaviFrame).Navigated += this.NaviFrame_Navigated;
                }
                break;
            case 22: // MainPage.xaml line 30
                {
                    this.LeftPaddingColumn = (global::Windows.UI.Xaml.Controls.ColumnDefinition)(target);
                }
                break;
            case 23: // MainPage.xaml line 32
                {
                    this.RightPaddingColumn = (global::Windows.UI.Xaml.Controls.ColumnDefinition)(target);
                }
                break;
            case 24: // MainPage.xaml line 34
                {
                    this.AppTitleBorder = (global::Windows.UI.Xaml.Controls.Border)(target);
                }
                break;
            case 25: // MainPage.xaml line 40
                {
                    this.AppTitle = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 26: // MainPage.xaml line 210
                {
                    this.WindowWidthChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 27: // MainPage.xaml line 234
                {
                    this.NavigationViewPaneStateChange = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 28: // MainPage.xaml line 235
                {
                    this.Open = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 29: // MainPage.xaml line 258
                {
                    this.Close = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 32: // MainPage.xaml line 211
                {
                    this.MinimumLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 33: // MainPage.xaml line 228
                {
                    this.WideLayout = (global::Windows.UI.Xaml.VisualState)(target);
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
            switch(connectionId)
            {
            case 1: // MainPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    MainPage_obj1_Bindings bindings = new MainPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element1, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

