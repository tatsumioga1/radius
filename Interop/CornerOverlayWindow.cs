using System.Runtime.InteropServices;

namespace Roundly.Interop;

internal sealed class CornerOverlayWindow : IDisposable
{
    private const string WindowClassName = "RoundlyCornerOverlayWindow";
    private static readonly NativeMethods.WindowProc WndProc = (hWnd, message, wParam, lParam) =>
        NativeMethods.DefWindowProc(hWnd, message, wParam, lParam);

    private static bool _classRegistered;
    private readonly nint _handle;
    private bool _disposed;

    private CornerOverlayWindow(nint handle)
    {
        _handle = handle;
    }

    public static CornerOverlayWindow Create(int x, int y, int radius, Corner corner)
    {
        EnsureWindowClass();

        var handle = NativeMethods.CreateWindowEx(
            NativeMethods.WsExLayered
            | NativeMethods.WsExTransparent
            | NativeMethods.WsExToolWindow
            | NativeMethods.WsExTopmost
            | NativeMethods.WsExNoActivate,
            WindowClassName,
            "Roundly corner",
            NativeMethods.WsPopup,
            x,
            y,
            radius,
            radius,
            nint.Zero,
            nint.Zero,
            nint.Zero,
            nint.Zero);

        if (handle == nint.Zero)
        {
            throw new InvalidOperationException("Could not create a corner overlay window.");
        }

        Paint(handle, x, y, radius, corner);
        NativeMethods.SetWindowPos(handle, NativeMethods.HwndTopmost, x, y, radius, radius, 0x0010);
        NativeMethods.ShowWindow(handle, NativeMethods.SwShownoactivate);

        return new CornerOverlayWindow(handle);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        NativeMethods.DestroyWindow(_handle);
        _disposed = true;
    }

    public void KeepTopmost()
    {
        if (_disposed)
        {
            return;
        }

        NativeMethods.SetWindowPos(
            _handle,
            NativeMethods.HwndTopmost,
            0,
            0,
            0,
            0,
            0x0001 | 0x0002 | 0x0010);
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
            throw new InvalidOperationException("Could not register the corner overlay window class.");
        }

        _classRegistered = true;
    }

    private static void Paint(nint handle, int x, int y, int radius, Corner corner)
    {
        var screenDc = NativeMethods.GetDC(nint.Zero);
        var memoryDc = NativeMethods.CreateCompatibleDC(screenDc);
        nint bitmap = nint.Zero;
        nint previous = nint.Zero;

        try
        {
            var info = new NativeMethods.BitmapInfo
            {
                bmiHeader = new NativeMethods.BitmapInfoHeader
                {
                    biSize = (uint)Marshal.SizeOf<NativeMethods.BitmapInfoHeader>(),
                    biWidth = radius,
                    biHeight = -radius,
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = NativeMethods.BiRgb,
                    biSizeImage = (uint)(radius * radius * 4)
                }
            };

            bitmap = NativeMethods.CreateDIBSection(
                screenDc,
                ref info,
                NativeMethods.DibRgbColors,
                out var bits,
                nint.Zero,
                0);

            if (bitmap == nint.Zero || bits == nint.Zero)
            {
                throw new InvalidOperationException("Could not create the corner overlay bitmap.");
            }

            var pixels = CreateMaskPixels(radius, corner);
            Marshal.Copy(pixels, 0, bits, pixels.Length);
            previous = NativeMethods.SelectObject(memoryDc, bitmap);

            var destination = new NativeMethods.Point { x = x, y = y };
            var size = new NativeMethods.Size { cx = radius, cy = radius };
            var source = new NativeMethods.Point { x = 0, y = 0 };
            var blend = new NativeMethods.BlendFunction
            {
                BlendOp = NativeMethods.AcSrcOver,
                SourceConstantAlpha = 255,
                AlphaFormat = NativeMethods.AcSrcAlpha
            };

            NativeMethods.UpdateLayeredWindow(
                handle,
                screenDc,
                ref destination,
                ref size,
                memoryDc,
                ref source,
                0,
                ref blend,
                NativeMethods.UlwAlpha);
        }
        finally
        {
            if (previous != nint.Zero)
            {
                NativeMethods.SelectObject(memoryDc, previous);
            }

            if (bitmap != nint.Zero)
            {
                NativeMethods.DeleteObject(bitmap);
            }

            if (memoryDc != nint.Zero)
            {
                NativeMethods.DeleteDC(memoryDc);
            }

            if (screenDc != nint.Zero)
            {
                NativeMethods.ReleaseDC(nint.Zero, screenDc);
            }
        }
    }

    private static byte[] CreateMaskPixels(int radius, Corner corner)
    {
        var pixels = new byte[radius * radius * 4];
        var centerX = corner is Corner.TopLeft or Corner.BottomLeft ? radius : -1;
        var centerY = corner is Corner.TopLeft or Corner.TopRight ? radius : -1;
        var radiusSquared = radius * radius;

        for (var y = 0; y < radius; y++)
        {
            for (var x = 0; x < radius; x++)
            {
                var dx = x - centerX;
                var dy = y - centerY;
                var insideVisibleCorner = (dx * dx) + (dy * dy) <= radiusSquared;
                if (insideVisibleCorner)
                {
                    continue;
                }

                var index = ((y * radius) + x) * 4;
                pixels[index] = 0;
                pixels[index + 1] = 0;
                pixels[index + 2] = 0;
                pixels[index + 3] = 255;
            }
        }

        return pixels;
    }
}
