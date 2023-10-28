using SMPlayer.Controls;
using SMPlayer.Dialogs;
using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HiddenFoldersPage : Page, IStorageItemEventListener
    {
        private readonly ObservableCollection<GridViewStorageItem> HiddenStorageItems = new ObservableCollection<GridViewStorageItem>();

        public HiddenFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            StorageService.AddStorageItemEventListener(this);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HiddenStorageItems.Clear();
            HiddenStorageItems.AddRange(StorageService.FindHiddenFolders().Select(i => new GridViewFolder(i)));
            HiddenStorageItems.AddRange(StorageService.FindHiddenFiles()    
                                                      .Select(i => MusicService.FindMusicIncludeHidden(i.FileId))
                                                      .Select(i => new GridViewMusic(i.ToVO())));
        }

        async void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
            switch (args.EventType)
            {
                case StorageItemEventType.ResumeFile:
                    await Helper.RunInMainUIThread(Dispatcher, () =>
                    {
                        HiddenStorageItems.RemoveAll(i => i.Path == file.Path);
                    });
                    break;
            }
        }

        async void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            switch (args.EventType)
            {
                case StorageItemEventType.ResumeFolder:
                    await Helper.RunInMainUIThread(Dispatcher, () =>
                    {
                        HiddenStorageItems.RemoveAll(i => i.Path == folder.Path);
                    });
                    break;
            }
        }

        private void HiddenFoldersView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = ColorHelper.GetRowBackground(args.ItemIndex);
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            GridViewStorageItem storageItem = (sender as FrameworkElement).DataContext as GridViewStorageItem;
            if (storageItem is GridViewFolder f)
            {
                StorageService.ResumeFolder(f.Source);
            }
            else if (storageItem is GridViewMusic m)
            {
                StorageService.ResumeMusic(m.Source.FromVO());
            }
        }
    }
}