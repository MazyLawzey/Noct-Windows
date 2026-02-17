using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Media.Core;
using Windows.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Noct
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private IntPtr hwnd;
        private AppWindow appWindow;
        private SizeInt32 windowSize = new SizeInt32(400, 700);

        // P/Invoke declarations
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_MINIMIZEBOX = 0x00020000;

        public MainWindow()
        {
            this.InitializeComponent();

            // Get the window handle (HWND)
            hwnd = WindowNative.GetWindowHandle(this);

            // Get the AppWindow from the HWND
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(windowSize);

            // Disable window resizing and hide system caption buttons
            var presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(true, false);
            }

            // Remove maximize and minimize boxes from window style
            RemoveWindowStyle(hwnd, WS_MAXIMIZEBOX | WS_MINIMIZEBOX);

            // Hide the default title bar and use custom one
            ExtendsContentIntoTitleBar = true;

            // Set the custom title bar as the drag area
            SetTitleBar(CustomTitleBar);

            // Listen for window changes and prevent maximize
            appWindow.Changed += AppWindow_Changed;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange || args.DidSizeChange)
            {
                // If somehow the window got maximized, restore it immediately
                if (sender.Presenter is OverlappedPresenter p && p.State == OverlappedPresenterState.Maximized)
                {
                    p.Restore();
                    sender.Resize(windowSize);
                }
            }
        }

        private void RemoveWindowStyle(IntPtr hwnd, uint stylesToRemove)
        {
            try
            {
                IntPtr currentStyle = GetWindowLongPtr(hwnd, GWL_STYLE);
                IntPtr newStyle = new IntPtr(currentStyle.ToInt64() & ~stylesToRemove);
                SetWindowLongPtr(hwnd, GWL_STYLE, newStyle);
            }
            catch
            {
                // Ignore if P/Invoke fails
            }
        }

        private void NavigateTo(string section)
        {
            // Hide all sections
            HomeSection.Visibility = Visibility.Collapsed;
            ServerSection.Visibility = Visibility.Collapsed;
            SettingsSection.Visibility = Visibility.Collapsed;

            // Reset all icon opacity to inactive
            HomeIcon.Opacity = 0.6;
            ServerIcon.Opacity = 0.6;
            SettingsIcon.Opacity = 0.6;

            // Activate selected section and icon
            switch (section)
            {
                case "home":
                    HomeSection.Visibility = Visibility.Visible;
                    HomeIcon.Opacity = 1.0;
                    break;
                case "server":
                    ServerSection.Visibility = Visibility.Visible;
                    ServerIcon.Opacity = 1.0;
                    break;
                case "settings":
                    SettingsSection.Content = new Settings();
                    SettingsSection.Visibility = Visibility.Visible;
                    SettingsIcon.Opacity = 1.0;
                    break;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e) => NavigateTo("home");
        private void ServerButton_Click(object sender, RoutedEventArgs e) => NavigateTo("server");
        private void SettingsButton_Click(object sender, RoutedEventArgs e) => NavigateTo("settings");

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Minimize();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(new Windows.UI.Color { A = 255, R = 0x80, G = 0x80, B = 0x80 });
            }
        }

        private void MinimizeButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            }
        }

        private void CloseButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(new Windows.UI.Color { A = 255, R = 0x80, G = 0x80, B = 0x80 });
            }
        }

        private void CloseButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            }
        }

        private void CustomTitleBar_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Flyout is MenuFlyout flyout)
            {
                flyout.ShowAt(button);
            }
        }

        private void AddConnection_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать добавление подключения
        }

        private void ShareConnection_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать поделиться подключением
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo("settings");
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var videoUri = new Uri("ms-appx:///Assets/App/Noct_connected.mov");
            var mediaSource = MediaSource.CreateFromUri(videoUri);

            var mediaPlayerElement = new MediaPlayerElement
            {
                Source = mediaSource,
                AutoPlay = true,
                Width = 200,
                Height = 200,
            };

            mediaPlayerElement.MediaPlayer.IsLoopingEnabled = true;

            HomeSection.Children.Clear();
            HomeSection.Children.Add(mediaPlayerElement);
        }
    }
}