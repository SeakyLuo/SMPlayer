﻿#pragma checksum "C:\Users\Seaky\source\repos\SMPlayer\SMPlayer\LocalFoldersPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "0E2E86E45345EEE541FA928A2932E271"
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
    partial class LocalFoldersPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
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
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
            public static void Set_Windows_UI_Xaml_Controls_Image_Source(global::Windows.UI.Xaml.Controls.Image obj, global::Windows.UI.Xaml.Media.ImageSource value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::Windows.UI.Xaml.Media.ImageSource) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::Windows.UI.Xaml.Media.ImageSource), targetNullValue);
                }
                obj.Source = value;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class LocalFoldersPage_obj42_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ILocalFoldersPage_Bindings
        {
            private global::SMPlayer.Models.GridFolderView dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj42;
            private global::Windows.UI.Xaml.Controls.TextBlock obj45;
            private global::Windows.UI.Xaml.Controls.TextBlock obj46;
            private global::Windows.UI.Xaml.Controls.Image obj47;
            private global::Windows.UI.Xaml.Controls.Image obj48;
            private global::Windows.UI.Xaml.Controls.Image obj49;
            private global::Windows.UI.Xaml.Controls.Image obj50;
            private global::Windows.UI.Xaml.Controls.Image obj51;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj45TextDisabled = false;
            private static bool isobj46TextDisabled = false;
            private static bool isobj47SourceDisabled = false;
            private static bool isobj48SourceDisabled = false;
            private static bool isobj49SourceDisabled = false;
            private static bool isobj50SourceDisabled = false;
            private static bool isobj51SourceDisabled = false;

            public LocalFoldersPage_obj42_Bindings()
            {
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 358 && columnNumber == 33)
                {
                    isobj45TextDisabled = true;
                }
                else if (lineNumber == 366 && columnNumber == 33)
                {
                    isobj46TextDisabled = true;
                }
                else if (lineNumber == 313 && columnNumber == 41)
                {
                    isobj47SourceDisabled = true;
                }
                else if (lineNumber == 317 && columnNumber == 41)
                {
                    isobj48SourceDisabled = true;
                }
                else if (lineNumber == 321 && columnNumber == 41)
                {
                    isobj49SourceDisabled = true;
                }
                else if (lineNumber == 325 && columnNumber == 41)
                {
                    isobj50SourceDisabled = true;
                }
                else if (lineNumber == 329 && columnNumber == 41)
                {
                    isobj51SourceDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 42: // LocalFoldersPage.xaml line 285
                        this.obj42 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.UserControl)target);
                        break;
                    case 45: // LocalFoldersPage.xaml line 353
                        this.obj45 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 46: // LocalFoldersPage.xaml line 360
                        this.obj46 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 47: // LocalFoldersPage.xaml line 310
                        this.obj47 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 48: // LocalFoldersPage.xaml line 314
                        this.obj48 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 49: // LocalFoldersPage.xaml line 318
                        this.obj49 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 50: // LocalFoldersPage.xaml line 322
                        this.obj50 = (global::Windows.UI.Xaml.Controls.Image)target;
                        break;
                    case 51: // LocalFoldersPage.xaml line 326
                        this.obj51 = (global::Windows.UI.Xaml.Controls.Image)target;
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
                            (this.obj42.Target as global::Windows.UI.Xaml.Controls.UserControl).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::SMPlayer.Models.GridFolderView) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // ILocalFoldersPage_Bindings

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
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::SMPlayer.Models.GridFolderView)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.Models.GridFolderView obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Name(obj.Name, phase);
                        this.Update_FolderInfo(obj.FolderInfo, phase);
                        this.Update_First(obj.First, phase);
                        this.Update_Second(obj.Second, phase);
                        this.Update_Third(obj.Third, phase);
                        this.Update_Fourth(obj.Fourth, phase);
                        this.Update_LargeThumbnail(obj.LargeThumbnail, phase);
                    }
                }
            }
            private void Update_Name(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 353
                    if (!isobj45TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj45, obj, null);
                    }
                }
            }
            private void Update_FolderInfo(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 360
                    if (!isobj46TextDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj46, obj, null);
                    }
                }
            }
            private void Update_First(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 310
                    if (!isobj47SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj47, obj, null);
                    }
                }
            }
            private void Update_Second(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 314
                    if (!isobj48SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj48, obj, null);
                    }
                }
            }
            private void Update_Third(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 318
                    if (!isobj49SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj49, obj, null);
                    }
                }
            }
            private void Update_Fourth(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 322
                    if (!isobj50SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj50, obj, null);
                    }
                }
            }
            private void Update_LargeThumbnail(global::Windows.UI.Xaml.Media.Imaging.BitmapImage obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // LocalFoldersPage.xaml line 326
                    if (!isobj51SourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_Image_Source(this.obj51, obj, null);
                    }
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class LocalFoldersPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IXamlBindScopeDiagnostics,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            ILocalFoldersPage_Bindings
        {
            private global::SMPlayer.LocalFoldersPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.GridView obj39;

            // Static fields for each binding's enabled/disabled state
            private static bool isobj39ItemsSourceDisabled = false;

            private LocalFoldersPage_obj1_BindingsTracking bindingsTracking;

            public LocalFoldersPage_obj1_Bindings()
            {
                this.bindingsTracking = new LocalFoldersPage_obj1_BindingsTracking(this);
            }

            public void Disable(int lineNumber, int columnNumber)
            {
                if (lineNumber == 281 && columnNumber == 13)
                {
                    isobj39ItemsSourceDisabled = true;
                }
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 39: // LocalFoldersPage.xaml line 274
                        this.obj39 = (global::Windows.UI.Xaml.Controls.GridView)target;
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

            // ILocalFoldersPage_Bindings

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
                    this.dataRoot = (global::SMPlayer.LocalFoldersPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::SMPlayer.LocalFoldersPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | DATA_CHANGED | (1 << 0))) != 0)
                    {
                        this.Update_GridItems(obj.GridItems, phase);
                    }
                }
            }
            private void Update_GridItems(global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView> obj, int phase)
            {
                this.bindingsTracking.UpdateChildListeners_GridItems(obj);
                if ((phase & ((1 << 0) | NOT_PHASED | DATA_CHANGED)) != 0)
                {
                    // LocalFoldersPage.xaml line 274
                    if (!isobj39ItemsSourceDisabled)
                    {
                        XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj39, obj, null);
                    }
                }
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
            [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
            private class LocalFoldersPage_obj1_BindingsTracking
            {
                private global::System.WeakReference<LocalFoldersPage_obj1_Bindings> weakRefToBindingObj; 

                public LocalFoldersPage_obj1_BindingsTracking(LocalFoldersPage_obj1_Bindings obj)
                {
                    weakRefToBindingObj = new global::System.WeakReference<LocalFoldersPage_obj1_Bindings>(obj);
                }

                public LocalFoldersPage_obj1_Bindings TryGetBindingObject()
                {
                    LocalFoldersPage_obj1_Bindings bindingObject = null;
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
                    UpdateChildListeners_GridItems(null);
                }

                public void PropertyChanged_GridItems(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
                {
                    LocalFoldersPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        string propName = e.PropertyName;
                        global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView>;
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
                public void CollectionChanged_GridItems(object sender, global::System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    LocalFoldersPage_obj1_Bindings bindings = TryGetBindingObject();
                    if (bindings != null)
                    {
                        global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView> obj = sender as global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView>;
                    }
                }
                private global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView> cache_GridItems = null;
                public void UpdateChildListeners_GridItems(global::System.Collections.ObjectModel.ObservableCollection<global::SMPlayer.Models.GridFolderView> obj)
                {
                    if (obj != cache_GridItems)
                    {
                        if (cache_GridItems != null)
                        {
                            ((global::System.ComponentModel.INotifyPropertyChanged)cache_GridItems).PropertyChanged -= PropertyChanged_GridItems;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)cache_GridItems).CollectionChanged -= CollectionChanged_GridItems;
                            cache_GridItems = null;
                        }
                        if (obj != null)
                        {
                            cache_GridItems = obj;
                            ((global::System.ComponentModel.INotifyPropertyChanged)obj).PropertyChanged += PropertyChanged_GridItems;
                            ((global::System.Collections.Specialized.INotifyCollectionChanged)obj).CollectionChanged += CollectionChanged_GridItems;
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
            case 1: // LocalFoldersPage.xaml line 1
                {
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)(target);
                    ((global::Windows.UI.Xaml.Controls.Page)element1).Loaded += this.Page_Loaded;
                }
                break;
            case 29: // LocalFoldersPage.xaml line 39
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyout element29 = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyout)element29).Opening += this.OpenMusicFlyout;
                }
                break;
            case 35: // LocalFoldersPage.xaml line 18
                {
                    global::Windows.UI.Xaml.Controls.Grid element35 = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    ((global::Windows.UI.Xaml.Controls.Grid)element35).DoubleTapped += this.FolderTemplate_DoubleTapped;
                }
                break;
            case 36: // LocalFoldersPage.xaml line 23
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyout element36 = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyout)element36).Opening += this.OpenPlaylistFlyout;
                }
                break;
            case 37: // LocalFoldersPage.xaml line 269
                {
                    this.LoadingControlPlaceHolder = (global::Windows.UI.Xaml.Shapes.Rectangle)(target);
                }
                break;
            case 38: // LocalFoldersPage.xaml line 270
                {
                    this.LocalLoadingControl = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 39: // LocalFoldersPage.xaml line 274
                {
                    this.LocalFoldersGridView = (global::Windows.UI.Xaml.Controls.GridView)(target);
                    ((global::Windows.UI.Xaml.Controls.GridView)this.LocalFoldersGridView).ItemClick += this.LocalFoldersGridView_ItemClick;
                }
                break;
            case 40: // LocalFoldersPage.xaml line 387
                {
                    this.LocalFolderTreeView = (global::Windows.UI.Xaml.Controls.TreeView)(target);
                    ((global::Windows.UI.Xaml.Controls.TreeView)this.LocalFolderTreeView).Collapsed += this.LocalFolderTreeView_Collapsed;
                    ((global::Windows.UI.Xaml.Controls.TreeView)this.LocalFolderTreeView).Expanding += this.LocalFolderTreeView_Expanding;
                    ((global::Windows.UI.Xaml.Controls.TreeView)this.LocalFolderTreeView).ItemInvoked += this.LocalFolderTreeView_ItemInvoked;
                }
                break;
            case 42: // LocalFoldersPage.xaml line 285
                {
                    global::Windows.UI.Xaml.Controls.UserControl element42 = (global::Windows.UI.Xaml.Controls.UserControl)(target);
                    ((global::Windows.UI.Xaml.Controls.UserControl)element42).PointerEntered += this.GridViewItem_PointerEntered;
                    ((global::Windows.UI.Xaml.Controls.UserControl)element42).PointerExited += this.GridViewItem_PointerExited;
                }
                break;
            case 43: // LocalFoldersPage.xaml line 288
                {
                    global::Windows.UI.Xaml.Controls.MenuFlyout element43 = (global::Windows.UI.Xaml.Controls.MenuFlyout)(target);
                    ((global::Windows.UI.Xaml.Controls.MenuFlyout)element43).Opening += this.OpenPlaylistFlyout;
                }
                break;
            case 53: // LocalFoldersPage.xaml line 338
                {
                    global::Windows.UI.Xaml.Controls.Button element53 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element53).Click += this.PlayAllButton_Click;
                }
                break;
            case 54: // LocalFoldersPage.xaml line 344
                {
                    global::Windows.UI.Xaml.Controls.Button element54 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element54).Click += this.AddToButton_Click;
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
            case 1: // LocalFoldersPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    LocalFoldersPage_obj1_Bindings bindings = new LocalFoldersPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element1, bindings);
                }
                break;
            case 42: // LocalFoldersPage.xaml line 285
                {                    
                    global::Windows.UI.Xaml.Controls.UserControl element42 = (global::Windows.UI.Xaml.Controls.UserControl)target;
                    LocalFoldersPage_obj42_Bindings bindings = new LocalFoldersPage_obj42_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element42.DataContext);
                    element42.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element42, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element42, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}

