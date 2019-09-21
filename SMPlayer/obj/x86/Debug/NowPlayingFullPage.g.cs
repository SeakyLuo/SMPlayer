﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\NowPlayingFullPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B73A37D979B26AB10DBB374FDA6E1A6D"
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

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Media.ImageBrush obj2;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj2ImageSourceDisabled = false;

            private NowPlayingFullPage_obj1_BindingsTracking bindingsTracking;

            public NowPlayingFullPage_obj1_Bindings()
            {
                this.bindingsTracking = new NowPlayingFullPage_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 61 && columnNumber == 25)
                {
                    isobj2ImageSourceDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 2: // NowPlayingFullPage.xaml line 61
                        this.obj2 = (global::Windows.UI.Xaml.Media.ImageBrush)target;
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

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.NowPlayingFullPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_FullMediaControl(obj.FullMediaControl, phase);
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
                    // NowPlayingFullPage.xaml line 61
                    if (!isobj2ImageSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Media_ImageBrush_ImageSource(this.obj2, obj, null);
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
            case 3: // NowPlayingFullPage.xaml line 77
                {
                    this.AppTitleBar = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 4: // NowPlayingFullPage.xaml line 84
                {
                    this.BackButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.BackButton).Click += this.BackButton_Click;
                }
                break;
            case 5: // NowPlayingFullPage.xaml line 91
                {
                    this.FullMediaControl = (global::SMPlayer.MediaControl)(target);
                }
                break;
            case 6: // NowPlayingFullPage.xaml line 95
                {
                    this.NowPlayingFullBladeView = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeView)(target);
                }
                break;
            case 7: // NowPlayingFullPage.xaml line 301
                {
                    this.ShowResultInAppNotification = (global::Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification)(target);
                }
                break;
            case 8: // NowPlayingFullPage.xaml line 99
                {
                    this.PlaylistBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 9: // NowPlayingFullPage.xaml line 128
                {
                    this.MusicPropertyBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 10: // NowPlayingFullPage.xaml line 241
                {
                    this.LyricsBladeItem = (global::Microsoft.Toolkit.Uwp.UI.Controls.BladeItem)(target);
                }
                break;
            case 11: // NowPlayingFullPage.xaml line 278
                {
                    this.SavingLyricsProgressRing = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 12: // NowPlayingFullPage.xaml line 288
                {
                    this.LyricsTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 13: // NowPlayingFullPage.xaml line 255
                {
                    this.SearchLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SearchLyricsButton).Click += this.SearchLyricsButton_Click;
                }
                break;
            case 14: // NowPlayingFullPage.xaml line 262
                {
                    this.SaveLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SaveLyricsButton).Click += this.SaveLyricsButton_Click;
                }
                break;
            case 15: // NowPlayingFullPage.xaml line 269
                {
                    this.ResetLyricsButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ResetLyricsButton).Click += this.ResetLyricsButton_Click;
                }
                break;
            case 16: // NowPlayingFullPage.xaml line 190
                {
                    this.TitleTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 17: // NowPlayingFullPage.xaml line 191
                {
                    this.SubtitleTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 18: // NowPlayingFullPage.xaml line 192
                {
                    this.ArtistTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 19: // NowPlayingFullPage.xaml line 193
                {
                    this.AlbumTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 20: // NowPlayingFullPage.xaml line 194
                {
                    this.AlbumArtistTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 21: // NowPlayingFullPage.xaml line 216
                {
                    this.PublisherTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 22: // NowPlayingFullPage.xaml line 217
                {
                    this.TrackNumberTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.TrackNumberTextBox).BeforeTextChanging += this.CheckIfDigit;
                }
                break;
            case 23: // NowPlayingFullPage.xaml line 221
                {
                    this.YearTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                    ((global::Windows.UI.Xaml.Controls.TextBox)this.YearTextBox).BeforeTextChanging += this.CheckIfDigit;
                }
                break;
            case 24: // NowPlayingFullPage.xaml line 225
                {
                    this.BitRateTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 25: // NowPlayingFullPage.xaml line 226
                {
                    this.ComposersTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 26: // NowPlayingFullPage.xaml line 227
                {
                    this.ConductorsTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 27: // NowPlayingFullPage.xaml line 228
                {
                    this.DateCreatedTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 28: // NowPlayingFullPage.xaml line 229
                {
                    this.DateModifiedTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 29: // NowPlayingFullPage.xaml line 230
                {
                    this.DurationTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 30: // NowPlayingFullPage.xaml line 231
                {
                    this.FileSizeTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 31: // NowPlayingFullPage.xaml line 232
                {
                    this.FileTypeTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 32: // NowPlayingFullPage.xaml line 233
                {
                    this.GenreTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 33: // NowPlayingFullPage.xaml line 234
                {
                    this.PathTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 34: // NowPlayingFullPage.xaml line 235
                {
                    this.ProducersTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 35: // NowPlayingFullPage.xaml line 200
                {
                    this.PlayCountTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 36: // NowPlayingFullPage.xaml line 206
                {
                    this.ClearPlayCountButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ClearPlayCountButton).Click += this.ClearPlayCountButton_Click;
                }
                break;
            case 37: // NowPlayingFullPage.xaml line 145
                {
                    this.SaveMusicPropertiesButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.SaveMusicPropertiesButton).Click += this.SaveMusicPropertiesButton_Click;
                }
                break;
            case 38: // NowPlayingFullPage.xaml line 152
                {
                    this.ResetMusicPropertiesButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ResetMusicPropertiesButton).Click += this.ResetMusicPropertiesButton_Click;
                }
                break;
            case 39: // NowPlayingFullPage.xaml line 102
                {
                    this.FullPlaylistControl = (global::SMPlayer.PlaylistControl)(target);
                }
                break;
            case 40: // NowPlayingFullPage.xaml line 109
                {
                    this.CommonStates = (global::Windows.UI.Xaml.VisualStateGroup)(target);
                }
                break;
            case 41: // NowPlayingFullPage.xaml line 110
                {
                    this.WideLayout = (global::Windows.UI.Xaml.VisualState)(target);
                }
                break;
            case 42: // NowPlayingFullPage.xaml line 115
                {
                    this.NarrowLayout = (global::Windows.UI.Xaml.VisualState)(target);
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

