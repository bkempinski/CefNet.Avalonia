using System.Runtime.InteropServices;
using System;

namespace CefNet.Avalonia.X11Api;

[StructLayout(LayoutKind.Sequential)]
public struct XErrorEvent
{
    public XEventName type;
    public IntPtr display;
    public IntPtr resourceid;
    public IntPtr serial;
    public byte error_code;
    public XRequest request_code;
    public byte minor_code;
}