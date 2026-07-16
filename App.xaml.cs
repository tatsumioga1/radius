using Microsoft.UI.Xaml;
using Roundly.Services;
using Roundly.Interop;
using WinRT.Interop;

namespace Roundly;

public sealed partial class App : Application
{
    private readonly AppSettings _settings;
    private readonly StartupTaskService _startupTasks = new();
    private CornerOverlayService? _overlays;
    private TrayIconService? _trayIcon;
    private MainWindow? _mainWindow;
    private bool _startupEnabled;

    public App()
    {
        AppLog.Write("App constructor");
        InitializeComponent();
        _settings = AppSettings.Load();
        AppLog.Write("App initialized");
    }

    public Window SettingsWindow => EnsureMainWindow();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppLog.Write("OnLaunched");
        ShowSettings();
        TryStartNativeServices();
        _ = RefreshStartupStateAsync();
    }

    public void UpdateOverlay()
    {
        _settings.Save();
        TryApplyOverlay();
        _trayIcon?.Update(_settings.Enabled);
    }

    private MainWindow EnsureMainWindow()
    {
        if (_mainWindow is null)
        {
            AppLog.Write("Creating MainWindow");
            _mainWindow = new MainWindow(_settings, UpdateOverlay, SetStartupEnabledAsync);
            _mainWindow.Closed += (_, _) => _mainWindow = null;
            _mainWindow.SyncStartup(_startupEnabled);
            AppLog.Write("MainWindow created");
        }

        return _mainWindow;
    }

    private void ShowSettings()
    {
        var window = EnsureMainWindow();
        NativeMethods.ShowWindow(WindowNative.GetWindowHandle(window), NativeMethods.SwRestore);
        NativeMethods.ShowWindow(WindowNative.GetWindowHandle(window), NativeMethods.SwShow);
        window.Activate();
        AppLog.Write("Settings window activated");
    }

    private void TryStartNativeServices()
    {
        var messages = new List<string>();

        try
        {
            _trayIcon = new TrayIconService(
                showSettings: ShowSettings,
                toggleEnabled: ToggleEnabled,
                toggleStartup: ToggleStartup,
                exit: ExitApplication,
                isEnabled: () => _settings.Enabled,
                isStartupEnabled: () => _startupEnabled);
            _trayIcon.Update(_settings.Enabled);
            messages.Add("Tray icon ready.");
            AppLog.Write("Tray icon ready");
        }
        catch (Exception ex)
        {
            messages.Add($"Tray icon failed: {ex.Message}");
            AppLog.Write("Tray icon failed");
            AppLog.Write(ex);
        }

        try
        {
            _overlays = new CornerOverlayService(_settings);
            _overlays.Apply();
            messages.Add(_settings.Enabled ? "Corner overlays active." : "Corner overlays are off.");
            AppLog.Write("Corner overlays started");
        }
        catch (Exception ex)
        {
            messages.Add($"Corner overlays failed: {ex.Message}");
            AppLog.Write("Corner overlays failed");
            AppLog.Write(ex);
        }

        _mainWindow?.SetStatus(string.Join(Environment.NewLine, messages));
    }

    private void TryApplyOverlay()
    {
        try
        {
            _overlays?.Apply();
            _mainWindow?.SetStatus(_settings.Enabled ? "Corner overlays active." : "Corner overlays are off.");
        }
        catch (Exception ex)
        {
            _mainWindow?.SetStatus($"Corner overlays failed: {ex.Message}");
        }
    }

    private void ToggleEnabled()
    {
        _settings.Enabled = !_settings.Enabled;
        UpdateOverlay();
        _mainWindow?.SyncFromSettings();
    }

    private async Task RefreshStartupStateAsync()
    {
        _startupEnabled = await _startupTasks.IsEnabledAsync();
        _mainWindow?.SyncStartup(_startupEnabled);
    }

    private async Task SetStartupEnabledAsync(bool enabled)
    {
        _startupEnabled = await _startupTasks.SetEnabledAsync(enabled);
        _mainWindow?.SyncStartup(_startupEnabled);
    }

    private void ToggleStartup()
    {
        _ = SetStartupEnabledAsync(!_startupEnabled);
    }

    private void ExitApplication()
    {
        _trayIcon?.Dispose();
        _overlays?.Dispose();
        _settings.Save();
        Environment.Exit(0);
    }
}
