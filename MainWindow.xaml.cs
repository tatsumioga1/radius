using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Roundly.Interop;
using Roundly.Services;
using WinRT.Interop;

namespace Roundly;

public sealed partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Action _settingsChanged;
    private readonly Func<bool, Task> _startupChanged;
    private AppWindow? _appWindow;
    private bool _syncing;

    public MainWindow(AppSettings settings, Action settingsChanged, Func<bool, Task> startupChanged)
    {
        InitializeComponent();
        _settings = settings;
        _settingsChanged = settingsChanged;
        _startupChanged = startupChanged;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarDragRegion);
        SystemBackdrop = new MicaBackdrop();

        ConfigureWindow();
        SyncFromSettings();
    }

    public void SyncFromSettings()
    {
        _syncing = true;
        EnabledSwitch.IsOn = _settings.Enabled;
        RadiusSlider.Value = _settings.Radius;
        RadiusBox.Value = _settings.Radius;
        _syncing = false;
    }

    public void SyncStartup(bool enabled)
    {
        _syncing = true;
        StartupSwitch.IsOn = enabled;
        _syncing = false;
    }

    public void SetStatus(string message)
    {
        StatusText.Text = message;
    }

    private void ConfigureWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow = appWindow;
        appWindow.Resize(new Windows.Graphics.SizeInt32(560, 460));
        appWindow.Title = "Roundly";
        appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        appWindow.Closing += AppWindow_Closing;
        ApplyTitleBarTheme();

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsMaximizable = false;
            presenter.IsResizable = false;
        }
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true;
        NativeMethods.ShowWindow(WindowNative.GetWindowHandle(this), NativeMethods.SwHide);
    }

    private void RootGrid_ActualThemeChanged(FrameworkElement sender, object args)
    {
        ApplyTitleBarTheme();
    }

    private void ApplyTitleBarTheme()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        var titleBar = appWindow.TitleBar;
        var isDark = RootGrid.ActualTheme == ElementTheme.Dark;
        var foreground = isDark ? Colors.White : Colors.Black;
        var subtleForeground = isDark ? ColorHelper.FromArgb(255, 220, 220, 220) : ColorHelper.FromArgb(255, 32, 32, 32);
        var hover = isDark ? ColorHelper.FromArgb(48, 255, 255, 255) : ColorHelper.FromArgb(34, 0, 0, 0);
        var pressed = isDark ? ColorHelper.FromArgb(72, 255, 255, 255) : ColorHelper.FromArgb(54, 0, 0, 0);

        titleBar.BackgroundColor = Colors.Transparent;
        titleBar.ForegroundColor = foreground;
        titleBar.InactiveBackgroundColor = Colors.Transparent;
        titleBar.InactiveForegroundColor = subtleForeground;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = foreground;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveForegroundColor = subtleForeground;
        titleBar.ButtonHoverBackgroundColor = hover;
        titleBar.ButtonHoverForegroundColor = foreground;
        titleBar.ButtonPressedBackgroundColor = pressed;
        titleBar.ButtonPressedForegroundColor = foreground;
    }

    private void EnabledSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        _settings.Enabled = EnabledSwitch.IsOn;
        _settingsChanged();
    }

    private async void StartupSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        await _startupChanged(StartupSwitch.IsOn);
    }

    private void RadiusSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        var radius = (int)Math.Round(e.NewValue);
        _settings.Radius = radius;
        _syncing = true;
        RadiusBox.Value = radius;
        _syncing = false;
        _settingsChanged();
    }

    private void RadiusBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_syncing || double.IsNaN(args.NewValue))
        {
            return;
        }

        var radius = (int)Math.Clamp(Math.Round(args.NewValue), 0, 96);
        _settings.Radius = radius;
        _syncing = true;
        RadiusSlider.Value = radius;
        _syncing = false;
        _settingsChanged();
    }

    private void HideButton_Click(object sender, RoutedEventArgs e)
    {
        NativeMethods.ShowWindow(WindowNative.GetWindowHandle(this), NativeMethods.SwHide);
    }
}
