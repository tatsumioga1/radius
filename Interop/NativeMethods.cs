using System.Runtime.InteropServices;

namespace Roundly.Interop;

internal static partial class NativeMethods
{
    public const int WmAppTray = 0x8001;
    public const int WmContextMenu = 0x007B;
    public const int WmLButtonUp = 0x0202;
    public const int WmLButtonDblClk = 0x0203;
    public const int WmRButtonUp = 0x0205;
    public const int NinSelect = 0x0400;
    public const int NinKeySelect = 0x0401;

    public const int NimAdd = 0x00000000;
    public const int NimModify = 0x00000001;
    public const int NimDelete = 0x00000002;
    public const int NimSetVersion = 0x00000004;
    public const int NifMessage = 0x00000001;
    public const int NifIcon = 0x00000002;
    public const int NifTip = 0x00000004;
    public const int NifShowTip = 0x00000080;
    public const int NotifyIconVersion4 = 4;

    public const int ImageIcon = 1;
    public const int IdiApplication = 32512;
    public const int LrLoadFromFile = 0x00000010;
    public const int LrDefaultSize = 0x00000040;
    public const int MfString = 0x00000000;
    public const int MfChecked = 0x00000008;
    public const int MfSeparator = 0x00000800;
    public const int TpmReturNcmd = 0x00000100;
    public const int TpmRightButton = 0x00000002;

    public const uint EventSystemForeground = 0x0003;
    public const uint WineventOutofcontext = 0x0000;

    public const int AcSrcOver = 0x00;
    public const int AcSrcAlpha = 0x01;
    public const int BiRgb = 0;
    public const int DibRgbColors = 0;
    public const int UlwAlpha = 0x00000002;
    public const int SwHide = 0;
    public const int SwShow = 5;
    public const int SwRestore = 9;
    public const int SwShownoactivate = 4;

    public const int WsPopup = unchecked((int)0x80000000);
    public const int WsExLayered = 0x00080000;
    public const int WsExTransparent = 0x00000020;
    public const int WsExToolWindow = 0x00000080;
    public const int WsExTopmost = 0x00000008;
    public const int WsExNoActivate = 0x08000000;

    public const int CwUseDefault = unchecked((int)0x80000000);

    public static readonly nint HwndTopmost = new(-1);

    public static IReadOnlyList<MonitorBounds> GetMonitors()
    {
        var monitors = new List<MonitorBounds>();
        EnumDisplayMonitors(
            nint.Zero,
            nint.Zero,
            (monitor, _, _, _) =>
            {
                var info = new MonitorInfo { cbSize = Marshal.SizeOf<MonitorInfo>() };
                if (GetMonitorInfo(monitor, ref info))
                {
                    monitors.Add(new MonitorBounds(
                        info.rcMonitor.left,
                        info.rcMonitor.top,
                        info.rcMonitor.right,
                        info.rcMonitor.bottom));
                }

                return true;
            },
            nint.Zero);

        return monitors;
    }

    [DllImport("user32.dll", EntryPoint = "RegisterClassExW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern ushort RegisterClassEx(ref WndClassEx lpwcx);

    [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShellNotifyIcon(uint dwMessage, ref NotifyIconData lpData);

    [DllImport("user32.dll", EntryPoint = "LoadIconW", SetLastError = true)]
    public static extern nint LoadIcon(nint hInstance, nint lpIconName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(nint hIcon);

    [DllImport("user32.dll", EntryPoint = "LoadImageW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint LoadImage(
        nint hinst,
        string lpszName,
        uint uType,
        int cxDesired,
        int cyDesired,
        uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint CreatePopupMenu();

    [DllImport("user32.dll", EntryPoint = "AppendMenuW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AppendMenu(nint hMenu, uint uFlags, nuint uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyMenu(nint hMenu);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int TrackPopupMenu(
        nint hMenu,
        uint uFlags,
        int x,
        int y,
        int nReserved,
        nint hWnd,
        nint prcRect);

    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern nint CreateWindowEx(
        int dwExStyle,
        string lpClassName,
        string lpWindowName,
        int dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        nint hWndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint GetDC(nint hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int ReleaseDC(nint hWnd, nint hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern nint CreateCompatibleDC(nint hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteDC(nint hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern nint SelectObject(nint hdc, nint h);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject(nint ho);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern nint CreateDIBSection(
        nint hdc,
        ref BitmapInfo pbmi,
        uint usage,
        out nint ppvBits,
        nint hSection,
        uint offset);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UpdateLayeredWindow(
        nint hwnd,
        nint hdcDst,
        ref Point pptDst,
        ref Size psize,
        nint hdcSrc,
        ref Point pptSrc,
        uint crKey,
        ref BlendFunction pblend,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetWinEventHook(
        uint eventMin,
        uint eventMax,
        nint hmodWinEventProc,
        WinEventProc pfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumDisplayMonitors(
        nint hdc,
        nint lprcClip,
        MonitorEnumProc lpfnEnum,
        nint dwData);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint hMonitor, ref MonitorInfo lpmi);

    public delegate nint WindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    public delegate void WinEventProc(
        nint hWinEventHook,
        uint eventType,
        nint hwnd,
        int idObject,
        int idChild,
        uint idEventThread,
        uint dwmsEventTime);

    private delegate bool MonitorEnumProc(nint hMonitor, nint hdcMonitor, nint lprcMonitor, nint dwData);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WndClassEx
    {
        public int cbSize;
        public int style;
        public WindowProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public string? lpszMenuName;
        public string lpszClassName;
        public nint hIconSm;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NotifyIconData
    {
        public int cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;

        public uint dwState;
        public uint dwStateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;

        public uint uTimeoutOrVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;

        public uint dwInfoFlags;
        public Guid guidItem;
        public nint hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfo
    {
        public BitmapInfoHeader bmiHeader;
        public uint bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
