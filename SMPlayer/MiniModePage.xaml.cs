using SMPlayer.Models;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SMPlayer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MiniModePage : Page
    {
        public static Size PageSize { get => new Size(300, 300); }
        //public static Size PageSize { get => new Size(300, Settings.settings.MiniModeWithDropdown ? 900 : 300); }
        public static ViewModePreferences ViewModePreferences
        {
            get
            {
                var pref = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                pref.CustomSize = PageSize;
                return pref;
            }
        }
        public MiniModePage()
        {
            this.InitializeComponent();
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += (sender, args) => UpdateTitleBarLayout(sender);
            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += (sender, args) => AppTitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleBarHelper.SetDarkTitleBar();
            Window.Current.SetTitleBar(AppTitleBar);
            UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar);

            //DropdownButton.Content = Settings.settings.MiniModeWithDropdown ? "\uE70E" : "\uE70D";
        }
        private void UpdateTitleBarLayout(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar coreTitleBar)
        {
            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SpinArrowAnimation.Begin();
            Settings.settings.MiniModeWithDropdown = !Settings.settings.MiniModeWithDropdown;
            var size = PageSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(size);
            ApplicationView.GetForCurrentView().TryResizeView(size);
        }
    }
}
