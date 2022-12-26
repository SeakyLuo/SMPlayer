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
        private readonly ObservableCollection<FolderTree> HiddenFolders = new ObservableCollection<FolderTree>();

        public HiddenFoldersPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void IStorageItemEventListener.ExecuteFileEvent(FolderFile file, StorageItemEventArgs args)
        {
            throw new NotImplementedException();
        }

        void IStorageItemEventListener.ExecuteFolderEvent(FolderTree folder, StorageItemEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void HiddenFoldersView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {

        }

    }
}