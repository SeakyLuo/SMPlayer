﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\NowPlayingPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "34FC6536C14ABCEDB4C460117E409FC6"
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
    partial class NowPlayingPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(global::Windows.UI.Xaml.Controls.TextBlock obj, global::Windows.UI.Xaml.Media.Brush value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.Brush) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.Brush), targetNullValue);
                }
                obj.Foreground = value;
            }
            public static void Set_Windows_UI_Xaml_UIElement_Visibility(global::Windows.UI.Xaml.UIElement obj, global::Windows.UI.Xaml.Visibility value)
            {
                obj.Visibility = value;
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

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class NowPlayingPage_obj4_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            INowPlayingPage_Bindings
        {
            private global::SMPlayer.Music dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private global::Windows.UI.Xaml.ResourceDictionary localResources;
            private global::System.WeakReference<global::Windows.UI.Xaml.FrameworkElement> converterLookupRoot;
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj4;
            private global::Windows.UI.Xaml.Controls.TextBlock obj8;
            private global::Windows.UI.Xaml.Controls.TextBlock obj9;
            private global::Windows.UI.Xaml.Controls.TextBlock obj10;
            private global::Windows.UI.Xaml.Controls.FontIcon obj11;
            private global::Windows.UI.Xaml.Controls.TextBlock obj12;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj8ForegroundDisabled = false;
            private static bool isobj8TextDisabled = false;
            private static bool isobj9ForegroundDisabled = false;
            private static bool isobj9TextDisabled = false;
            private static bool isobj10ForegroundDisabled = false;
            private static bool isobj10TextDisabled = false;
            private static bool isobj11VisibilityDisabled = false;
            private static bool isobj12ForegroundDisabled = false;
            private static bool isobj12TextDisabled = false;

            private NowPlayingPage_obj4_BindingsTracking bindingsTracking;

            public NowPlayingPage_obj4_Bindings()
            {
                this.bindingsTracking = new NowPlayingPage_obj4_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 121 && columnNumber == 29)
                {
                    isobj8ForegroundDisabled = true;
                }
                else if (lineNumber == 122 && columnNumber == 29)
                {
                    isobj8TextDisabled = true;
                }
                else if (lineNumber == 125 && columnNumber == 29)
                {
                    isobj9ForegroundDisabled = true;
                }
                else if (lineNumber == 126 && columnNumber == 29)
                {
                    isobj9TextDisabled = true;
                }
                else if (lineNumber == 130 && columnNumber == 29)
                {
                    isobj10ForegroundDisabled = true;
                }
                else if (lineNumber == 131 && columnNumber == 29)
                {
                    isobj10TextDisabled = true;
                }
                else if (lineNumber == 116 && columnNumber == 33)
                {
                    isobj11VisibilityDisabled = true;
                }
                else if (lineNumber == 117 && columnNumber == 40)
                {
                    isobj12ForegroundDisabled = true;
                }
                else if (lineNumber == 117 && columnNumber == 131)
                {
                    isobj12TextDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 4: // NowPlayingPage.xaml line 70
                        this.obj4 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.Grid)target);
                        break;
                    case 8: // NowPlayingPage.xaml line 119
                        this.obj8 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 9: // NowPlayingPage.xaml line 123
                        this.obj9 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 10: // NowPlayingPage.xaml line 127
                        this.obj10 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 11: // NowPlayingPage.xaml line 109
                        this.obj11 = (global::Windows.UI.Xaml.Controls.FontIcon)target;
                        break;
                    case 12: // NowPlayingPage.xaml line 117
                        this.obj12 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj4.Target as global::Windows.UI.Xaml.Controls.Grid).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::SMPlayer.Music) item, 1 << phase);
            }

            public void Recycle()
            {
                this.bindingsTracking.ReleaseAllListeners();
            }

            // INowPlayingPage_Bindings

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
                    this.dataRoot = (global::SMPlayer.Music)newDataRoot;
                    return true;
                }
                return false;
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
            private void Update_(global::SMPlayer.Music obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_(obj);
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_IsPlaying(obj.IsPlaying, phase);
                    }
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Artist(obj.Artist, phase);
                        this.Update_Album(obj.Album, phase);
                        this.Update_Duration(obj.Duration, phase);
                        this.Update_Name(obj.Name, phase);
                    }
                }
            }
            private void Update_IsPlaying(global::System.Boolean obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // NowPlayingPage.xaml line 119
                    if (!isobj8ForegroundDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(this.obj8, (global::Windows.UI.Xaml.Media.Brush)this.LookupConverter("RowColorConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Media.Brush), null, null), null);
                    }
                    // NowPlayingPage.xaml line 123
                    if (!isobj9ForegroundDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(this.obj9, (global::Windows.UI.Xaml.Media.Brush)this.LookupConverter("RowColorConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Media.Brush), null, null), null);
                    }
                    // NowPlayingPage.xaml line 127
                    if (!isobj10ForegroundDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(this.obj10, (global::Windows.UI.Xaml.Media.Brush)this.LookupConverter("RowColorConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Media.Brush), null, null), null);
                    }
                    // NowPlayingPage.xaml line 109
                    if (!isobj11VisibilityDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_UIElement_Visibility(this.obj11, (global::Windows.UI.Xaml.Visibility)this.LookupConverter("MusicVisibilityConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Visibility), null, null));
                    }
                    // NowPlayingPage.xaml line 117
                    if (!isobj12ForegroundDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Foreground(this.obj12, (global::Windows.UI.Xaml.Media.Brush)this.LookupConverter("RowColorConverter").Convert(obj, typeof(global::Windows.UI.Xaml.Media.Brush), null, null), null);
                    }
                }
            }
            private void Update_Artist(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // NowPlayingPage.xaml line 119
                    if (!isobj8TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj8, obj, null);
                    }
                }
            }
            private void Update_Album(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // NowPlayingPage.xaml line 123
                    if (!isobj9TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj9, obj, null);
                    }
                }
            }
            private void Update_Duration(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // NowPlayingPage.xaml line 127
                    if (!isobj10TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj10, (global::System.String)this.LookupConverter("MusicDurationConverter").Convert(obj, typeof(global::System.String), null, null), null);
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // NowPlayingPage.xaml line 117
                    if (!isobj12TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj12, obj, null);
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class NowPlayingPage_obj4_BindingsTracking
            {
                private global::System.WeakReference<NowPlayingPage_obj4_Bindings> weakRefToBindingObj; 

                public NowPlayingPage_obj4_BindingsTracking(NowPlayingPage_obj4_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<NowPlayingPage_obj4_Bindings>(obj);
                }

                public NowPlayingPage_obj4_Bindings TryGetBindingObject()
                {
                    NowPlayingPage_obj4_Bindings bindingObject = null;
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
                    UpdateChildListeners_(null);
                }

                public void PropertyChanged_(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    NowPlayingPage_obj4_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::SMPlayer.Music obj = sender as global::SMPlayer.Music;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                            if (obj != null)
                            {
                                bindings.Update_IsPlaying(obj.IsPlaying, DATA_CHANGED);
                            }
                        }
                        else
                        {
                            switch (propName)
                            {
                                case "IsPlaying":
                                {
                                    if (obj != null)
                                    {
                                        bindings.Update_IsPlaying(obj.IsPlaying, DATA_CHANGED);
                                    }
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                    }
                }
                public void UpdateChildListeners_(global::SMPlayer.Music obj)
                {
                    NowPlayingPage_obj4_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        if (bindings.dataRoot != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)bindings.dataRoot).PropertyChanged -= PropertyChanged_;
                        }
                        if (obj != null)
                        {
                            bindings.dataRoot = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_;
                        }
                    }
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class NowPlayingPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            INowPlayingPage_Bindings
        {
            private global::SMPlayer.NowPlayingPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.ListView obj3;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj3ItemsSourceDisabled = false;

            private NowPlayingPage_obj1_BindingsTracking bindingsTracking;

            public NowPlayingPage_obj1_Bindings()
            {
                this.bindingsTracking = new NowPlayingPage_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 60 && columnNumber == 13)
                {
                    isobj3ItemsSourceDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3: // NowPlayingPage.xaml line 49
                        this.obj3 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        this.bindingsTracking.RegisterTwoWayListener_3(this.obj3);
                        break;
                    default:
                        break;
                }
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                throw new global::System.NotImplementedException();
            }

            public void Recycle()
            {
                throw new global::System.NotImplementedException();
            }

            // INowPlayingPage_Bindings

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
                    this.dataRoot = (global::SMPlayer.NowPlayingPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.NowPlayingPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_Songs(obj.Songs, phase);
                    }
                }
            }
            private void Update_Songs(global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music> obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_Songs(obj);
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // NowPlayingPage.xaml line 49
                    if (!isobj3ItemsSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj3, obj, null);
                    }
                }
            }
            private void UpdateTwoWay_3_ItemsSource()
            {
                if (this.initialized)
                {
                    if (this.dataRoot != null)
                    {
                        this.dataRoot.Songs = (global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music>)this.obj3.ItemsSource;
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class NowPlayingPage_obj1_BindingsTracking
            {
                private global::System.WeakReference<NowPlayingPage_obj1_Bindings> weakRefToBindingObj; 

                public NowPlayingPage_obj1_BindingsTracking(NowPlayingPage_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<NowPlayingPage_obj1_Bindings>(obj);
                }

                public NowPlayingPage_obj1_Bindings TryGetBindingObject()
                {
                    NowPlayingPage_obj1_Bindings bindingObject = null;
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
                    UpdateChildListeners_Songs(null);
                }

                public void PropertyChanged_Songs(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    NowPlayingPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music>;
                        if (global::System.String.IsNullOrEmpty(propName))
                        {
                        }
                        else
                        {
                            switch (propName)
                            {
                                default:
                                    break;
                            }
                        }
                    }
                }
                public void CollectionChanged_Songs(object sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    NowPlayingPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music>;
                    }
                }
                private global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music> cache_Songs = null;
                public void UpdateChildListeners_Songs(global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Music> obj)
                {
                    if (obj != cache_Songs)
                    {
                        if (cache_Songs != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_Songs).PropertyChanged -= PropertyChanged_Songs;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)cache_Songs).CollectionChanged -= CollectionChanged_Songs;
                            cache_Songs = null;
                        }
                        if (obj != null)
                        {
                            cache_Songs = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_Songs;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)obj).CollectionChanged += CollectionChanged_Songs;
                        }
                    }
                }
                public void RegisterTwoWayListener_3(global::Windows.UI.Xaml.Controls.ListView sourceObject)
                {
                    sourceObject.RegisterPropertyChangedCallback(global::Windows.UI.Xaml.Controls.ItemsControl.ItemsSourceProperty, (sender, prop) =>
                    {
                        var bindingObj = this.TryGetBindingObject();
                        if (bindingObj != null)
                        {
                            bindingObj.UpdateTwoWay_3_ItemsSource();
                        }
                    });
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // NowPlayingPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 2: // NowPlayingPage.xaml line 19
                {
                    this.MusicLibraryTitleTextBlock = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 3: // NowPlayingPage.xaml line 49
                {
                    this.SongsListView = (global::Windows.UI.Xaml.Controls.ListView)(target);
                    ((global::Windows.UI.Xaml.Controls.ListView)this.SongsListView).ContainerContentChanging += this.SongsListView_ContainerContentChanging;
                    ((global::Windows.UI.Xaml.Controls.ListView)this.SongsListView).ItemClick += this.SongsListView_ItemClick;
                }
                break;
            case 5: // NowPlayingPage.xaml line 79
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyoutItem element5 = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)element5).Click += this.PlayItem_Click;
                }
                break;
            case 6: // NowPlayingPage.xaml line 92
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyoutItem element6 = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)element6).Click += this.DeleteItem_Click;
                }
                break;
            case 7: // NowPlayingPage.xaml line 101
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyoutItem element7 = (global::Windows.UI.Xaml.Controls.MenuFlyoutItem)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyoutItem)element7).Click += this.MoveToTopItem_Click;
                }
                break;
            case 13: // NowPlayingPage.xaml line 28
                {
                    this.NewListButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.NewListButton).Click += this.NewListButton_Click;
                }
                break;
            case 14: // NowPlayingPage.xaml line 33
                {
                    this.MusicInfoButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.MusicInfoButton).Click += this.MusicInfoButton_Click;
                }
                break;
            case 15: // NowPlayingPage.xaml line 38
                {
                    this.LyricsButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.LyricsButton).Click += this.LyricsButton_Click;
                }
                break;
            case 16: // NowPlayingPage.xaml line 43
                {
                    this.FullScreenButton = (global::Windows.UI.Xaml.Controls.AppBarButton)(target);
                    ((global::Windows.UI.Xaml.Controls.AppBarButton)this.FullScreenButton).Click += this.FullScreenButton_Click;
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
            switch(connectionId)
            {
            case 1: // NowPlayingPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    NowPlayingPage_obj1_Bindings bindings = new NowPlayingPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element1, bindings);
                }
                break;
            case 4: // NowPlayingPage.xaml line 70
                {                    
                    global::Windows.UI.Xaml.Controls.Grid element4 = (global::Windows.UI.Xaml.Controls.Grid)target;
                    NowPlayingPage_obj4_Bindings bindings = new NowPlayingPage_obj4_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element4.DataContext);
                    bindings.SetConverterLookupRoot(this);
                    element4.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element4, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element4, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

