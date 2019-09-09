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
        public static AppLanguage[] LanguageOptions = { AppLanguage.FollowSystem, AppLanguage.SimplifiedChinese, AppLanguage.TraditionalChinese, AppLanguage.English, AppLanguage.Japanese };
        public static ShowNotification[] NotificationOptions = { ShowNotification.Always, ShowNotification.MusicChanged, ShowNotification.Never };
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void PathBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder == null || folder.Path == Settings.settings.RootPath) return;
            SettingsLoadingControl.IsLoading = true;
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            Settings.settings.RootPath = folder.Path;
            await Settings.SetTreeFolder(folder);
            foreach (var listener in listeners) listener.PathSet(folder.Path);
            MediaHelper.Clear();
            Settings.Save();
            PathBox.Text = folder.Path;
            SettingsLoadingControl.IsLoading = false;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PathBox.Text = Settings.settings.RootPath;
            LanguageComboBox.SelectedItem = Settings.settings.Language;
            NotificationComboBox.SelectedItem = Settings.settings.Notification;
            ThemeColorPicker.Color = Settings.settings.ThemeColor;
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

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.Language = LanguageOptions[(sender as ComboBox).SelectedIndex];
        }

        private void NotificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.settings.Notification = NotificationOptions[(sender as ComboBox).SelectedIndex];
        }
    }

    public interface AfterPathSetListener
    {
        void PathSet(string path);
    }
}
