using System;
using System.Runtime.InteropServices;

namespace CefNet.Avalonia.X11Api;

public static class NativeMethods
{
    public const int XkbUseCoreKbd = 0x0100;
    public const int Success = 1;

    [DllImport("X11", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("X11", CallingConvention = CallingConvention.Cdecl)]
    public static extern int XCloseDisplay(IntPtr display);

    [DllImport("X11", CallingConvention = CallingConvention.Cdecl)]
    public static extern int XkbGetIndicatorState(IntPtr display, uint deviceSpec, out uint stateReturn);

    public static bool IsCapsLockToggled()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);

        try
        {
            if (XkbGetIndicatorState(display, XkbUseCoreKbd, out var stateReturn) == Success)
                return (stateReturn & 0x01) == 0x01;
        }
        finally
        {
            XCloseDisplay(display);
        }

        return false;
    }

    public static bool IsNumLockToggled()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);

        try
        {
            if (XkbGetIndicatorState(display, XkbUseCoreKbd, out var stateReturn) == Success)
                return (stateReturn & 0x02) == 0x02;
        }
        finally
        {
            XCloseDisplay(display);
        }

        return false;
    }
}