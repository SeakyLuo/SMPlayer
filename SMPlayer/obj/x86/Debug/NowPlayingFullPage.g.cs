﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\NowPlayingFullPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "71009534D37901F52089E59081E87EA4"
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
    partial class NowPlayingFullPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Media_ImageBrush_ImageSource(global::Windows.UI.Xaml.Media.ImageBrush obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.ImageSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBox_Text(global::Windows.UI.Xaml.Controls.TextBox obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
            public static void Set_Windows_UI_Xaml_UIElement_Visibility(global::Windows.UI.Xaml.UIElement obj, global::Windows.UI.Xaml.Visibility value)
            {
                obj.Visibility = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class NowPlayingFullPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            INowPlayingFullPage_Bindings
        {
            private global::SMPlayer.NowPlayingFullPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private global::Windows.UI.Xaml.ResourceDictionary localResources;
            private global::System.WeakReference<global::Windows.UI.Xaml.FrameworkElement> converterLookupRoot;

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Media.ImageBrush obj2;
            private global::Windows.UI.Xaml.Controls.TextBox obj34;
            private global::Windows.UI.Xaml.Controls.Button obj35;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj2ImageSourceDisabled = false;
            private static bool isobj34TextDisabled = false;
            private static bool isobj35VisibilityDisabled = false;

            private NowPlayingFullPage_obj1_BindingsTracking bindingsTracking;

            public NowPlayingFullPage_obj1_Bindings()
            {
                this.bindingsTracking = new NowPlayingFullPage_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 37 && columnNumber == 25)
                {
                    isobj2ImageSourceDisabled = true;
                }
                else if (lineNumber == 244 && columnNumber == 37)
                {
                    isobj34TextDisabled = true;
                }
                else if (lineNumber == 254 && columnNumber == 37)
                {
                    isobj35VisibilityDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 2: // NowPlayingFullPage.xaml line 37
                        this.obj2 = (global::Windows.UI.Xaml.Media.ImageBrush)target;
                        break;
                    case 34: // NowPlayingFullPage.xaml line 240
                        this.obj34 = (global::Windows.UI.Xaml.Controls.TextBox)target;
                        break;
                    case 35: // NowPlayingFullPage.xaml line 245
                        this.obj35 = (global::Windows.UI.Xaml.Controls.Button)target;
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

            // INowPlayingFullPage_Bindings

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
                    this.dataRoot = (global::SMPlayer.NowPlayingFullPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }
            public void SetConverterLookupRoot(global::Windows.UI.Xaml.FrameworkElement rootElement)
            {
                this.converterLookupRoot = new global::System.WeakReference<global::Windows.UI.Xaml.FrameworkElement>(rootElement);
            }

            public global::Windows.UI.Xaml.Data.IValueConverter LookupConverter(string key)
            {
                if (this.localResources == null)
                {
                    global::Windows.UI.Xaml.FrameworkElement rootElement;
                    this.converterLookupRoot.TryGetTarget(out rootElement);
                    this.localResources = rootElement.Resources;
                    this.converterLookupRoot = null;
                }
                return (global::Windows.UI.Xaml.Data.IValueConverter) (this.localResources.ContainsKey(key) ? this.localResources[key] : global::Windows.UI.Xaml.Application.Current.Resources[key]);
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.NowPlayingFullPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_FullMediaControl(obj.FullMediaControl, phase);
                    }
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_CurrentMusic(obj.CurrentMusic, phase);
                    }
                }
            }
            private void Update_FullMediaControl(global::SMPlayer.MediaControl obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_FullMediaControl_AlbumCover(obj.AlbumCover, phase);
                    }
                }
            }
            private void Update_FullMediaControl_AlbumCover(global::Windows.UI.Xaml.Controls.Image obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_FullMediaControl_AlbumCover(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_FullMediaControl_AlbumCover_Source(obj.Source, phase);
                    }
                }
            }
            private void Update_FullMediaControl_AlbumCover_Source(global::Windows.UI.Xaml.Media.ImageSource obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // NowPlayingFullPage.xaml line 37
                    if (!isobj2ImageSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_ImageBrush_ImageSource(this.obj2, obj, null);
                    }
                }
            }
            private void Update_CurrentMusic(global::SMPlayer.Models.Music obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_CurrentMusic_PlayCount(obj.PlayCount, phase);
                    }
                }
            }
            private void Update_CurrentMusic_PlayCount(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // NowPlayingFullPage.xaml line 240
                    if (!isobj34TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBox_Text(this.obj34, (global::System.String)this.LookupConverter("IntConverter").Convert(obj, typeof(global::System.String), null, null), null);
                    }
                    // NowPlayingFullPage.xaml line 245
                    if (!isobj35VisibilityDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_UIElement_Visibility(this.obj35, (global::Windows.UI.Xaml.Visibility)this.LookupConverter("VisibilityConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Visibility), null, null));
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class NowPlayingFullPage_obj1_BindingsTracking
            {
                private global::System.WeakReference<NowPlayingFullPage_obj1_Bindings> weakRefToBindingObj; 

                public NowPlayingFullPage_obj1_BindingsTracking(NowPlayingFullPage_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<NowPlayingFullPage_obj1_Bindings>(obj);
                }

                public NowPlayingFullPage_obj1_Bindings TryGetBindingObject()
                {
                    NowPlayingFullPage_obj1_Bindings bindingObject = null;
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
                    UpdateChildListeners_FullMediaControl_AlbumCover(null);
                }

                public void DependencyPropertyChanged_FullMediaControl_AlbumCover_Source(global::Windows.UI.Xaml.DependencyObject sender, global::Windows.UI.Xaml.DependencyProperty prop)
                {
                    NowPlayingFullPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        global::Windows.UI.Xaml.Controls.Image obj = sender as global::Windows.UI.Xaml.Controls.Image;
                        if (obj != null)
                        {
                            bindings.Update_FullMediaControl_AlbumCover_Source(obj.Source, DATA_CHANGED);
                        }
                    }
                }
                private global::Windows.UI.Xaml.Controls.Image cache_FullMediaControl_AlbumCover = null;
                private long tokenDPC_FullMediaControl_AlbumCover_Source = 0;
                public void UpdateChildListeners_FullMediaControl_AlbumCover(global::Windows.UI.Xaml.Controls.Image obj)
                {
                    if (obj != cache_FullMediaControl_AlbumCover)
                    {
                        if (cache_FullMediaControl_AlbumCover != null)
                        {
                            cache_FullMediaControl_AlbumCover.UnregisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.Image.SourceProperty, tokenDPC_FullMediaControl_AlbumCover_Source);
                            cache_FullMediaControl_AlbumCover = null;
                        }
                        if (obj != null)
                        {
                            cache_FullMediaControl_AlbumCover = obj;
                            tokenDPC_FullMediaControl_AlbumCover_Source = obj.RegisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.Image.SourceProperty, DependencyPropertyChanged_FullMediaControl_AlbumCover_Source);
                        }
                    }
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
            case 1: // NowPlayingFullPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 3: // NowPlayingFullPage.xaml line 52
                {
                    this.AppTitleBar = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 4: // NowPlayingFullPage.xaml line 64
                {
                    this.BackButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BackButton).Click += this.BackButton_Click;
                }
                break;
            case 5: // NowPlayingFullPage.xaml line 73
                {
                    this.FullMediaControl = (global::SMPlayer.MediaControl)(target);
                }
                break;
            case 6: // NowPlayingFullPage.xaml line 77
                {
                    this.NowPlayingFullBladeView = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeView)(target);
                }
                break;
            case 7: // NowPlayingFullPage.xaml line 453
                {
                    this.ShowResultInAppNotification = (global::Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification)(target);
                }
                break;
            case 8: // NowPlayingFullPage.xaml line 81
                {
                    this.PlaylistBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 9: // NowPlayingFullPage.xaml line 110
                {
                    this.MusicPropertyBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 10: // NowPlayingFullPage.xaml line 405
                {
                    this.LyricsBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 11: // NowPlayingFullPage.xaml line 442
                {
                    this.LyricsTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 12: // NowPlayingFullPage.xaml line 419
                {
                    this.SearchLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SearchLyricsButton).Click += this.SearchLyricsButton_Click;
                }
                break;
            case 13: // NowPlayingFullPage.xaml line 426
                {
                    this.SaveLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SaveLyricsButton).Click += this.SaveLyricsButton_Click;
                }
                break;
            case 14: // NowPlayingFullPage.xaml line 433
                {
                    this.ResetLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ResetLyricsButton).Click += this.ResetLyricsButton_Click;
                }
                break;
            case 15: // NowPlayingFullPage.xaml line 177
                {
                    this.TitleTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 16: // NowPlayingFullPage.xaml line 188
                {
                    this.SubtitleTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 17: // NowPlayingFullPage.xaml line 199
                {
                    this.ArtistTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 18: // NowPlayingFullPage.xaml line 210
                {
                    this.AlbumTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 19: // NowPlayingFullPage.xaml line 221
                {
                    this.AlbumArtistTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 20: // NowPlayingFullPage.xaml line 261
                {
                    this.PublisherTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 21: // NowPlayingFullPage.xaml line 272
                {
                    this.TrackNumberTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.TrackNumberTextBox).BeforeTextChanging += this.CheckIfDigit;
                }
                break;
            case 22: // NowPlayingFullPage.xaml line 284
                {
                    this.YearTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.YearTextBox).BeforeTextChanging += this.CheckIfDigit;
                }
                break;
            case 23: // NowPlayingFullPage.xaml line 296
                {
                    this.BitRateTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 24: // NowPlayingFullPage.xaml line 306
                {
                    this.ComposersTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 25: // NowPlayingFullPage.xaml line 316
                {
                    this.ConductorsTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 26: // NowPlayingFullPage.xaml line 326
                {
                    this.DateCreatedTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 27: // NowPlayingFullPage.xaml line 336
                {
                    this.DateModifiedTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 28: // NowPlayingFullPage.xaml line 346
                {
                    this.DurationTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 29: // NowPlayingFullPage.xaml line 356
                {
                    this.FileSizeTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 30: // NowPlayingFullPage.xaml line 366
                {
                    this.FileTypeTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 31: // NowPlayingFullPage.xaml line 376
                {
                    this.GenreTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 32: // NowPlayingFullPage.xaml line 386
                {
                    this.PathTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 33: // NowPlayingFullPage.xaml line 396
                {
                    this.ProducersTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 34: // NowPlayingFullPage.xaml line 240
                {
                    this.PlayCountTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 35: // NowPlayingFullPage.xaml line 245
                {
                    this.ClearPlayCountButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ClearPlayCountButton).Click += this.ClearPlayCountButton_Click;
                }
                break;
            case 36: // NowPlayingFullPage.xaml line 127
                {
                    this.SaveMusicPropertiesButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SaveMusicPropertiesButton).Click += this.SaveMusicPropertiesButton_Click;
                }
                break;
            case 37: // NowPlayingFullPage.xaml line 134
                {
                    this.ResetMusicPropertiesButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ResetMusicPropertiesButton).Click += this.ResetMusicPropertiesButton_Click;
                }
                break;
            case 38: // NowPlayingFullPage.xaml line 84
                {
                    this.FullPlaylistControl = (global::SMPlayer.PlaylistControl)(target);
                }
                break;
            case 39: // NowPlayingFullPage.xaml line 91
                {
                    this.CommonStates = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 40: // NowPlayingFullPage.xaml line 92
                {
                    this.WideLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 41: // NowPlayingFullPage.xaml line 97
                {
                    this.NarrowLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 42: // NowPlayingFullPage.xaml line 59
                {
                    this.LeftPaddingColumn = (global::Windows.UI.Xaml.Controls.ColumnDefinition)(target);
                }
                break;
            case 43: // NowPlayingFullPage.xaml line 61
                {
                    this.RightPaddingColumn = (global::Windows.UI.Xaml.Controls.ColumnDefinition)(target);
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
            case 1: // NowPlayingFullPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    NowPlayingFullPage_obj1_Bindings bindings = new NowPlayingFullPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    bindings.SetConverterLookupRoot(this);
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

