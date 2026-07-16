using System.Runtime.InteropServices;
using Radius.Interop;

namespace Radius.Services;

public sealed class TrayIconService : IDisposable
{
    private const uint IconId = 1;
    private const int CommandSettings = 100;
    private const int CommandEnabled = 101;
    private const int CommandStartup = 102;
    private const int CommandExit = 103;
    private const string WindowClassName = "RadiusTrayMessageWindow";

    private static readonly NativeMethods.WindowProc WndProc = OnWindowMessage;
    private static TrayIconService? _current;
    private static bool _classRegistered;

    private readonly Action _showSettings;
    private readonly Action _toggleEnabled;
    private readonly Action _toggleStartup;
    private readonly Action _exit;
    private readonly Func<bool> _isEnabled;
    private readonly Func<bool> _isStartupEnabled;
    private readonly nint _windowHandle;
    private readonly nint _iconHandle;
    private readonly bool _ownsIconHandle;
    private bool _disposed;

    public TrayIconService(
        Action showSettings,
        Action toggleEnabled,
        Action toggleStartup,
        Action exit,
        Func<bool> isEnabled,
        Func<bool> isStartupEnabled)
    {
        _showSettings = showSettings;
        _toggleEnabled = toggleEnabled;
        _toggleStartup = toggleStartup;
        _exit = exit;
        _isEnabled = isEnabled;
        _isStartupEnabled = isStartupEnabled;

        EnsureWindowClass();
        _windowHandle = NativeMethods.CreateWindowEx(
            0,
            WindowClassName,
            "Radius tray",
            0,
            0,
            0,
            0,
            0,
            nint.Zero,
            nint.Zero,
            nint.Zero,
            nint.Zero);

        if (_windowHandle == nint.Zero)
        {
            throw new InvalidOperationException("Could not create the tray message window.");
        }

        _current = this;
        _iconHandle = LoadTrayIcon(out _ownsIconHandle);
        AddIcon();
    }

    public void Update(bool enabled)
    {
        var data = CreateIconData();
        NativeMethods.ShellNotifyIcon(NativeMethods.NimModify, ref data);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var data = CreateIconData();
        NativeMethods.ShellNotifyIcon(NativeMethods.NimDelete, ref data);
        NativeMethods.DestroyWindow(_windowHandle);
        if (_ownsIconHandle && _iconHandle != nint.Zero)
        {
            NativeMethods.DestroyIcon(_iconHandle);
        }

        if (ReferenceEquals(_current, this))
        {
            _current = null;
        }

        _disposed = true;
    }

    private static nint OnWindowMessage(nint hWnd, uint message, nint wParam, nint lParam)
    {
        if (message == NativeMethods.WmAppTray && _current is not null)
        {
            var mouseMessage = lParam.ToInt32() & 0xFFFF;
            if (mouseMessage is NativeMethods.WmLButtonUp
                or NativeMethods.WmLButtonDblClk
                or NativeMethods.NinSelect
                or NativeMethods.NinKeySelect)
            {
                _current._showSettings();
                return nint.Zero;
            }

            if (mouseMessage is NativeMethods.WmRButtonUp or NativeMethods.WmContextMenu)
            {
                _current.ShowContextMenu();
                return nint.Zero;
            }
        }

        return NativeMethods.DefWindowProc(hWnd, message, wParam, lParam);
    }

    private static void EnsureWindowClass()
    {
        if (_classRegistered)
        {
            return;
        }

        var windowClass = new NativeMethods.WndClassEx
        {
            cbSize = Marshal.SizeOf<NativeMethods.WndClassEx>(),
            lpfnWndProc = WndProc,
            lpszClassName = WindowClassName
        };

        var atom = NativeMethods.RegisterClassEx(ref windowClass);
        if (atom == 0)
        {
            throw new InvalidOperationException("Could not register the tray message window class.");
        }

        _classRegistered = true;
    }

    private static nint LoadTrayIcon(out bool ownsIconHandle)
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            var icon = NativeMethods.LoadImage(
                nint.Zero,
                iconPath,
                NativeMethods.ImageIcon,
                0,
                0,
                NativeMethods.LrLoadFromFile | NativeMethods.LrDefaultSize);

            if (icon != nint.Zero)
            {
                ownsIconHandle = true;
                return icon;
            }
        }

        ownsIconHandle = false;
        return NativeMethods.LoadIcon(nint.Zero, NativeMethods.IdiApplication);
    }

    private void AddIcon()
    {
        var data = CreateIconData();
        NativeMethods.ShellNotifyIcon(NativeMethods.NimAdd, ref data);
        data.uTimeoutOrVersion = NativeMethods.NotifyIconVersion4;
        NativeMethods.ShellNotifyIcon(NativeMethods.NimSetVersion, ref data);
    }

    private NativeMethods.NotifyIconData CreateIconData()
    {
        return new NativeMethods.NotifyIconData
        {
            cbSize = Marshal.SizeOf<NativeMethods.NotifyIconData>(),
            hWnd = _windowHandle,
            uID = IconId,
            uFlags = NativeMethods.NifMessage | NativeMethods.NifIcon | NativeMethods.NifTip | NativeMethods.NifShowTip,
            uCallbackMessage = NativeMethods.WmAppTray,
            hIcon = _iconHandle,
            szTip = "Radius",
            szInfo = string.Empty,
            szInfoTitle = string.Empty
        };
    }

    private void ShowContextMenu()
    {
        var menu = NativeMethods.CreatePopupMenu();
        if (menu == nint.Zero)
        {
            return;
        }

        try
        {
            NativeMethods.AppendMenu(menu, NativeMethods.MfString, CommandSettings, "Open Radius");
            var enabledFlags = NativeMethods.MfString | (_isEnabled() ? NativeMethods.MfChecked : 0);
            NativeMethods.AppendMenu(menu, (uint)enabledFlags, CommandEnabled, "Enabled");
            var startupFlags = NativeMethods.MfString | (_isStartupEnabled() ? NativeMethods.MfChecked : 0);
            NativeMethods.AppendMenu(menu, (uint)startupFlags, CommandStartup, "Open on startup");
            NativeMethods.AppendMenu(menu, NativeMethods.MfSeparator, 0, null);
            NativeMethods.AppendMenu(menu, NativeMethods.MfString, CommandExit, "Exit Radius");

            NativeMethods.GetCursorPos(out var cursor);
            NativeMethods.SetForegroundWindow(_windowHandle);
            var command = NativeMethods.TrackPopupMenu(
                menu,
                NativeMethods.TpmReturNcmd | NativeMethods.TpmRightButton,
                cursor.x,
                cursor.y,
                0,
                _windowHandle,
                nint.Zero);

            switch (command)
            {
                case CommandSettings:
                    _showSettings();
                    break;
                case CommandEnabled:
                    _toggleEnabled();
                    break;
                case CommandStartup:
                    _toggleStartup();
                    break;
                case CommandExit:
                    _exit();
                    break;
            }
        }
        finally
        {
            NativeMethods.DestroyMenu(menu);
        }
    }
}
