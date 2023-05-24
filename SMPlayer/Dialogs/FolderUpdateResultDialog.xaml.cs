using SMPlayer.Helpers;
using SMPlayer.Interfaces;
using SMPlayer.Models;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace SMPlayer.Dialogs
{
    public sealed partial class FolderUpdateResultDialog : ContentDialog, IMusicPlayerEventListener
    {
        private readonly ObservableCollection<FolderUpdateResultGroup> Groups = new ObservableCollection<FolderUpdateResultGroup>();
        public FolderUpdateResultDialog()
        {
            this.InitializeComponent();
            MusicPlayer.AddMusicPlayerEventListener(this);
        }

        public async Task ShowAsync(FolderUpdateResult result)
        {
            try
            {
                FolderUpdateResultTextBlock.Text = string.IsNullOrEmpty(result.Path) ? "" : Helper.LocalizeText("UpdateResultOfFolder", Path.GetFileName(result.Path));
                Groups.AddRange(result.ToGroups());
                await ShowAsync();
            } 
            catch (Exception e)
            {
                Log.Warn($"FolderUpdateResultDialog.ShowAsync failed {e}");
                Helper.ShowNotificationRaw(Helper.LocalizeMessage("OperationFailed", e.Message), 5000);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        async void IMusicPlayerEventListener.Execute(MusicPlayerEventArgs args)
        {
            await Helper.RunInMainUIThread(Dispatcher, () =>
            {
                switch (args.EventType)
                {
                    case MusicPlayerEventType.Switch:
                        if (args.Music == null) return;
                        foreach (var group in Groups)
                        {
                            foreach (var item in group.Items)
                            {
                                item.IsPlaying = args.Music.Path == item.Path;
                            }
                        }
                        break;
                }
            });
        }

        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = ColorHelper.GetRowBackground(args.ItemIndex);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FolderUpdateResultGroupItem item = (sender as FrameworkElement).DataContext as FolderUpdateResultGroupItem;
            Music music = MusicService.FindMusic(item.Path);
            MusicPlayer.AddNextAndPlay(music);
        }


        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FolderUpdateResultGroupItem item = (sender as FrameworkElement).DataContext as FolderUpdateResultGroupItem;
            if (item.ShowFlyout)
            {
                MenuFlyoutHelper.SetMusicMenu(sender, option: new MenuFlyoutOption
                {
                    ShowMusicProperties = false,
                    ShowSelect = false,
                    ShowDelete = false,
                }).ShowAt(sender as FrameworkElement);
            }
        }
    }
}
