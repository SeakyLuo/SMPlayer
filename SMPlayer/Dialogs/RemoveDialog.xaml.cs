using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class RemoveDialog : ContentDialog
    {
        public bool IsChecked
        {
            get => (bool)DisplayCheckBox.IsChecked;
            set => DisplayCheckBox.IsChecked = value;
        }
        public string Message
        {
            get => MessageTextBlock.Text;
            set => MessageTextBlock.Text = value;
        }
        public Visibility CheckBoxVisibility
        {
            get => DisplayCheckBox.Visibility;
            set => DisplayCheckBox.Visibility = value;
        }

        public RemoveDialog(string message = "")
        {
            this.InitializeComponent();
            this.Message = message;
        }

        public Action Confirm { get; set; }
        public Action Cancel { get; set; }

        public static RemoveDialog BuildDeleteMusicDialog(Music music, Action<Music> onDeletion = null)
        {
            return new RemoveDialog
            {
                Message = Helper.LocalizeMessage("DeleteItem", music.Name),
                Confirm = async () =>
                {
                    MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                    onDeletion?.Invoke(music);
                    await StorageService.DeleteFile(music.ToFolderFile());
                    MainPage.Instance?.Loader.Hide();
                    Helper.ShowNotification(Helper.LocalizeMessage("ItemDeleted", music.Name));
                }
            };
        }

        public static RemoveDialog BuildDeleteFolderDialog(FolderTree tree, Action<FolderTree> onDeleted = null)
        {
            return new RemoveDialog
            {
                Message = Helper.LocalizeMessage("DeleteFolder", tree.Name),
                CheckBoxVisibility = Visibility.Collapsed,
                Confirm = async () =>
                {
                    MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                    await StorageService.DeleteFolder(tree);
                    onDeleted?.Invoke(tree);
                    MainPage.Instance?.Loader.Hide();
                    Helper.ShowNotification(Helper.LocalizeMessage("FolderIsDeleted", tree.Name));
                }
            };
        }

        public static RemoveDialog BuildDeleteGridViewStorageItemsDialog(List<GridViewStorageItem> items, Action<List<GridViewStorageItem>> onDeleted = null)
        {
            if (items.Count == 1)
            {
                GridViewStorageItem item = items[0];
                if (item is GridViewMusic music)
                {
                    return BuildDeleteMusicDialog(music.Source.FromVO(), m => onDeleted?.Invoke(items));
                }
                if (item is GridViewFolder folder)
                {
                    return BuildDeleteFolderDialog(folder.Source, m => onDeleted?.Invoke(items));
                }
            }
            return new RemoveDialog
            {
                Message = Helper.LocalizeMessage("DeleteItems", items.Count),
                CheckBoxVisibility = Visibility.Collapsed,
                Confirm = async () =>
                {
                    MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
                    foreach (var item in items)
                    {
                        if (item is GridViewMusic music)
                        {
                            await StorageService.DeleteFile(music.ToFolderFile());
                        }
                        else if (item is GridViewFolder folder)
                        {
                            await StorageService.DeleteFolder(folder.Source);
                        }
                    }
                    onDeleted?.Invoke(items);
                    MainPage.Instance?.Loader.Hide();
                    Helper.ShowNotification(Helper.LocalizeMessage("ItemsDeleted", items.Count));
                }
            };
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainPage.Instance?.Loader.ShowIndeterminant("ProcessRequest");
            Confirm?.Invoke();
            MainPage.Instance?.Loader.Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke();
            this.Hide();
        }
    }
}
