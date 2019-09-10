using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalPage : Page, LocalSetter, AfterPathSetListener
    {
        public static Stack<FolderTree> History = new Stack<FolderTree>();
        public LocalPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            LocalFoldersPage.setter = this;
            SetPage(Settings.settings.Tree);
            SettingsPage.AddAfterPathSetListener(this as AfterPathSetListener);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SetBackButtonVisibility();
        }

        private void LocalNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = (NavigationViewItem)LocalNavigationView.SelectedItem;
            Type page = item.Name == "LocalFoldersItem" ? typeof(LocalFoldersPage) : typeof(LocalMusicPage);
            if (LocalFrame.CurrentSourcePageType != page)
                LocalFrame.Navigate(page, History.Peek());
        }

        private void SetBackButtonVisibility()
        {
            LocalNavigationView.IsBackButtonVisible = History.Count == 1 ? NavigationViewBackButtonVisible.Collapsed : NavigationViewBackButtonVisible.Visible;
        }

        private void LocalNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (History.Count == 1) return;
            var tree = History.Pop();
            int last;
            PageStackEntry page;
            do
            {
                page = LocalFrame.BackStack[last = LocalFrame.BackStack.Count - 1];
                if (page.Parameter == tree) LocalFrame.BackStack.RemoveAt(last);
                else break;
            } while (true);
            LocalFrame.GoBack();
            var info = History.Peek().GetTreeInfo();
            SetText(info);
            switch (page.SourcePageType.Name)
            {
                case "LocalFoldersPage":
                    LocalFoldersItem.IsSelected = true;
                    break;
                case "LocalMusicPage":
                    LocalSongsItem.IsSelected = true;
                    break;
            }
            SetBackButtonVisibility();
        }

        private void LocalListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalListViewItem.Visibility = Visibility.Collapsed;
            LocalGridViewItem.Visibility = Visibility.Visible;
        }

        private void LocalGridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            LocalListViewItem.Visibility = Visibility.Visible;
            LocalGridViewItem.Visibility = Visibility.Collapsed;
        }

        private void LocalShuffleItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tree = History.Peek();
            MediaHelper.ShuffleAndPlay(LocalFrame.SourcePageType == typeof(LocalMusicPage) ? tree.Flatten() : tree.Files);
        }

        private void SetText(TreeInfo info)
        {
            TitleTextBlock.Text = string.IsNullOrEmpty(info.Directory) ? "No Music" : info.Directory;
            LocalFoldersItem.Content = $"Folders ({info.Folders})";
            LocalFoldersItem.IsEnabled = info.Folders != 0;
            LocalSongsItem.Content = $"Songs ({info.Songs})";
            LocalSongsItem.IsEnabled = info.Songs != 0;
        }

        public void SetPage(FolderTree tree)
        {
            if (History.Count > 0 && History.Peek() == tree) return;
            History.Push(tree);
            SetBackButtonVisibility();
            TreeInfo info = tree.GetTreeInfo();
            SetText(info);
            if (IsBackToMusicPage(info))
            {
                LocalNavigationView.SelectedItem = LocalSongsItem;
                LocalFrame.Navigate(typeof(LocalMusicPage), tree);
            }
            else if (info.Folders > 0)
            {
                LocalNavigationView.SelectedItem = LocalFoldersItem;
                LocalFrame.Navigate(typeof(LocalFoldersPage), tree);
            }
        }

        private bool IsBackToMusicPage(TreeInfo info)
        {
            return info.Songs > info.Folders;
        }

        public void PathSet(string path)
        {
            History.Clear();
            SetPage(Settings.settings.Tree);
        }
    }
    public interface LocalSetter
    {
        void SetPage(FolderTree tree);
    }
}