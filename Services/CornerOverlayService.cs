using Microsoft.UI.Dispatching;
using Roundly.Interop;

namespace Roundly.Services;

public sealed class CornerOverlayService : IDisposable
{
    private readonly AppSettings _settings;
    private readonly List<CornerOverlayWindow> _windows = [];
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly NativeMethods.WinEventProc _foregroundChanged;
    private readonly nint _foregroundHook;
    private long _lastKeepVisibleTicks;
    private string _lastSignature = string.Empty;
    private bool _disposed;

    public CornerOverlayService(AppSettings settings)
    {
        _settings = settings;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        _foregroundChanged = OnForegroundChanged;
        _foregroundHook = NativeMethods.SetWinEventHook(
            NativeMethods.EventSystemForeground,
            NativeMethods.EventSystemForeground,
            nint.Zero,
            _foregroundChanged,
            0,
            0,
            NativeMethods.WineventOutofcontext);
    }

    public void Apply()
    {
        if (_disposed)
        {
            return;
        }

        var monitors = NativeMethods.GetMonitors();
        var signature = CreateSignature(monitors);
        if (signature == _lastSignature)
        {
            return;
        }

        Clear();
        _lastSignature = signature;

        if (!_settings.Enabled || _settings.Radius <= 0)
        {
            return;
        }

        foreach (var monitor in monitors)
        {
            var radius = Math.Min(_settings.Radius, Math.Min(monitor.Width, monitor.Height) / 4);
            if (radius <= 0)
            {
                continue;
            }

            try
            {
                _windows.Add(CornerOverlayWindow.Create(monitor.Left, monitor.Top, radius, Corner.TopLeft));
                _windows.Add(CornerOverlayWindow.Create(monitor.Right - radius, monitor.Top, radius, Corner.TopRight));
                _windows.Add(CornerOverlayWindow.Create(monitor.Left, monitor.Bottom - radius, radius, Corner.BottomLeft));
                _windows.Add(CornerOverlayWindow.Create(monitor.Right - radius, monitor.Bottom - radius, radius, Corner.BottomRight));
            }
            catch
            {
                Clear();
                _lastSignature = string.Empty;
                throw;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_foregroundHook != nint.Zero)
        {
            NativeMethods.UnhookWinEvent(_foregroundHook);
        }

        Clear();
    }

    private void OnForegroundChanged(
        nint hWinEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime)
    {
        var now = Environment.TickCount64;
        if (now - _lastKeepVisibleTicks < 250)
        {
            return;
        }

        _lastKeepVisibleTicks = now;
        _dispatcherQueue.TryEnqueue(KeepVisible);
    }

    private void Clear()
    {
        foreach (var window in _windows)
        {
            window.Dispose();
        }

        _windows.Clear();
    }

    private void KeepVisible()
    {
        foreach (var window in _windows)
        {
            window.KeepTopmost();
        }
    }

    private string CreateSignature(IReadOnlyList<MonitorBounds> monitors)
    {
        var monitorSignature = string.Join(
            "|",
            monitors.Select(monitor => $"{monitor.Left},{monitor.Top},{monitor.Right},{monitor.Bottom}"));

        return $"{_settings.Enabled}:{_settings.Radius}:{monitorSignature}";
    }
}
