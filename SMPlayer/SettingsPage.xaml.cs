using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using SMPlayer.Models;
using Windows.Storage;
using System.Threading.Tasks;
using System.Threading;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static List<AfterPathSetListener> listeners = new List<AfterPathSetListener>();
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null || folder.Path == Settings.settings.RootPath) return;
            UpdatePopup.IsOpen = true;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            Settings.settings.RootPath = folder.Path;
            Settings.Save();
            await Settings.SetTreeFolder(folder);
            foreach (var listener in listeners) listener.PathSet(folder.Path);
            await MediaControl.SetPlayList(MusicLibraryPage.AllSongs);
            PathBox.Text = folder.Path;
            UpdatePopup.IsOpen = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PathBox.Text = Settings.settings.RootPath;
            LanguageComboBox.SelectedItem = Settings.settings.Language;
        }

        public static void AddAfterPathSetListener(AfterPathSetListener listener)
        {
            listeners.Add(listener);
        }
    }

    public interface AfterPathSetListener
    {
        void PathSet(string path);
    }
}
