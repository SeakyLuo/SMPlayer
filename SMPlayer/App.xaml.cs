﻿using SMPlayer.Helpers;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SMPlayer
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        public static List<Action> LoadedListeners = new List<Action>();
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnLaunched(e, null);
        }

        private async void OnLaunched(LaunchActivatedEventArgs e, Music music)
        {
            await Settings.Init();
            await MusicLibraryPage.Init();
            if (Settings.settings.LastPage == "Albums")
                await AlbumsPage.Init();
            await Helper.Init();
            await UpdateHelper.Init();

            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e?.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e == null || e.PrelaunchActivated == false)
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch"))
                {
                    Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
                }
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(MainPage), e?.Arguments);
                    if (e != null && e.TileId != "App")
                    {
                        var tileId = System.Net.WebUtility.UrlDecode(e.TileId);
                        MainPage.Instance.NavigateToPage(bool.Parse(e.Arguments) ? typeof(PlaylistsPage) :
                                                         tileId.StartsWith(Helper.Localize(MenuFlyoutHelper.MyFavorites)) ? typeof(MyFavoritesPage) : typeof(AlbumPage), tileId);
                    }
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            MediaHelper.Init(music);
            await AlbumsPage.Init();
            await RecentPage.Init();
            //LaunchVoiceAssistant();
            foreach (var listener in LoadedListeners) listener.Invoke();
            // If background task is already registered, do nothing
            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(Helper.ToastTaskName)))
                return;
            // Otherwise request access
            BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

            // Create the background task
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = Helper.ToastTaskName
            };

            // Assign the toast action trigger
            builder.SetTrigger(new ToastNotificationActionTrigger());

            // And register the task
            BackgroundTaskRegistration registration = builder.Register();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Helper.LogException(e.Exception);
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Helper.ResumeTile();
            Helper.HideToast();
            Save();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        public static async void Save()
        {
            Settings.Save();
            MusicLibraryPage.Save();
            MediaHelper.Save();
            AlbumsPage.Save();
            RecentPage.Save();
            UpdateHelper.Save();
            await Helper.ClearBackups(10);
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            var deferral = args.TaskInstance.GetDeferral();
            switch (args.TaskInstance.Task.Name)
            {
                case Helper.ToastTaskName:
                    if (args.TaskInstance.TriggerDetails is Windows.UI.Notifications.ToastNotificationActionTriggerDetail details)
                    {
                        // Perform tasks
                        switch (details.Argument)
                        {
                            case "Next":
                                MediaHelper.MoveNext();
                                break;
                            case "Pause":
                                MediaHelper.Pause();
                                break;
                            case "Play":
                                MediaHelper.Play();
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
            deferral.Complete();
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            Music music = await Music.GetMusicAsync(args.Files[0].Path);
            OnLaunched(null, music);
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Helper.LogException(e.Exception);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if (args.Kind != ActivationKind.VoiceCommand)
            {
                return;
            }
            var commands = args as VoiceCommandActivatedEventArgs;
            string command = commands.Result.RulePath[0];
        }

        private async void LaunchVoiceAssistant()
        {
            StorageFile commandSet = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx://VoiceAssistantCommandSet.xml"));
            await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(commandSet);
        }
    }
}
