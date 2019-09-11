using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer
{
    public static class TitleBarHelper
    {
        public static void SetMainTitleBar()
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

            // Set active window colors
            titleBar.ForegroundColor = Windows.UI.Colors.Black;
            titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.DarkGray;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.Gray;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.Black;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Black;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }
        public static void SetFullTitleBar()
        {
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

            // Set active window colors
            titleBar.ForegroundColor = Windows.UI.Colors.White;
            titleBar.BackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = Models.Settings.settings.ThemeColor;
            //titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            //titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.MediumBlue;
            //titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            //titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.DarkBlue;

            // Set inactive window colors
            titleBar.InactiveForegroundColor = Windows.UI.Colors.White;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
        }
    }
}
