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
    public sealed partial class SettingsPage : Page, TreeInitProgressListener
    {
        private static List<AfterPathSetListener> listeners = new List<AfterPathSetListener>();
        public static ShowToast[] NotificationOptions = { ShowToast.Always, ShowToast.MusicChanged, ShowToast.Never };
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PathBox.Text = Settings.settings.RootPath;
            NotificationComboBox.SelectedIndex = (int)Settings.settings.Toast;
            ThemeColorPicker.Color = Settings.settings.ThemeColor;
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                UpdateMusicLibrary(folder);
            }
        }

        public async void UpdateMusicLibrary(StorageFolder folder)
        {
            if (folder == null) return;
            MainPage.Instance.Loader.Text = Helper.LocalizeMessage("LoadMusicLibrary");
            MainPage.Instance.Loader.StartLoading();
            Helper.CurrentFolder = folder;
            await Settings.settings.Tree.Init(folder, this);
            Settings.settings.RootPath = folder.Path;
            MainPage.Instance.Loader.Text = Helper.LocalizeMessage("UpdateMusicLibrary");
            MusicLibraryPage.SetAllSongs(Settings.settings.Tree.Flatten());
            MainPage.Instance.Loader.Max = listeners.Count;
            for (int i = 0; i < listeners.Count; i++)
            {
                var listener = listeners[i];
                listener.PathSet(folder.Path);
                MainPage.Instance.Loader.Progress = i;
            }
            MediaHelper.Clear();
            Settings.Save();
            PathBox.Text = folder.Path;
            MainPage.Instance.Loader.FinishLoading();
        }

        public static void AddAfterPathSetListener(AfterPathSetListener listener)
        {
            listeners.Add(listener);
        }

        private void ConfirmColorButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.settings.ThemeColor = ThemeColorPicker.Color;
            ColorPickerFlyout.Hide();
        }

        private void CancelColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerFlyout.Hide();
        }

        private void NotificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.Toast = NotificationOptions[(sender as ComboBox).SelectedIndex];
        }

        public void Update(string folder, string file, int progress, int max)
        {
            bool isDeterminant = max != 0;
            MainPage.Instance.Loader.Max = max;
            MainPage.Instance.Loader.Progress = progress;
            MainPage.Instance.Loader.IsDeterminant = isDeterminant;
            if (isDeterminant)
            {
                MainPage.Instance.Loader.Text = file;
            }
        }

        private void UpdateMusicLibrary_Click(object sender, RoutedEventArgs e)
        {
            UpdateMusicLibrary(Helper.CurrentFolder);
        }

        private async void BugReport_Click(object sender, RoutedEventArgs e)
        {
            if (await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/SeakyLuo/SMPlayer/issues")))
            {

            }
            else
            {
                MainPage.Instance.ShowNotification(Helper.LocalizeMessage("FailToOpenBrowser"));
            }
        }
    }

    public interface AfterPathSetListener
    {
        void PathSet(string path);
    }
}
