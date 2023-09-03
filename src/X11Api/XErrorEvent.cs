using System.Runtime.InteropServices;
using System;

namespace CefNet.Avalonia.X11Api;

[StructLayout(LayoutKind.Sequential)]
public struct XErrorEvent
{
    internal XEventName type;
    internal IntPtr display;
    internal IntPtr resourceid;
    internal IntPtr serial;
    internal byte error_code;
    internal XRequest request_code;
    internal byte minor_code;
}