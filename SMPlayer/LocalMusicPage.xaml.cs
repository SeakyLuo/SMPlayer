using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalMusicPage : Page, SwitchMusicListener, ViewModeChangedListener
    {
        private ObservableCollection<Music> Songs = new ObservableCollection<Music>();
        private FolderTree Tree;
        public LocalMusicPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            MediaHelper.SwitchMusicListeners.Add(this as SwitchMusicListener);
            LocalPage.MusicViewModeChangedListener = this as ViewModeChangedListener;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Setup((FolderTree)e.Parameter);
            base.OnNavigatedTo(e);
            ModeChanged(Settings.settings.LocalMusicGridView);
        }

        private async void Setup(FolderTree tree)
        {
            if (Tree == tree) return;
            Tree = tree;
            LoadingProgressBar.Visibility = Visibility.Visible;
            try
            {
                await GridMusicView.Setup(tree.Files);
                Songs.Clear();
                foreach (var music in GridMusicView.MusicCollection)
                    Songs.Add(music);
            }
            catch (InvalidOperationException)
            {
                // Loading while Set New Folder will cause this Exception
                System.Diagnostics.Debug.WriteLine("InvalidOperationException On Local Music Page");
            }
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }

        public async void MusicSwitching(Music current, Music next, MediaPlaybackItemChangedReason reason)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => MediaHelper.FindMusicAndSetPlaying(Songs, current, next));
        }

        public void ModeChanged(bool isGridView)
        {
            if (isGridView)
            {
                GridMusicView.Visibility = Visibility.Visible;
                LocalPlaylist.Visibility = Visibility.Collapsed;
            }
            else
            {
                GridMusicView.Visibility = Visibility.Collapsed;
                LocalPlaylist.Visibility = Visibility.Visible;
            }
        }
    }
}
