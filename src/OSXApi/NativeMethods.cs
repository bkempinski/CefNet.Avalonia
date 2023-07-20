using System;
using System.Runtime.InteropServices;

namespace CefNet.Avalonia.OSXApi;

public static class NativeMethods
{
    private const string CarbonFramework = "/System/Library/Frameworks/Carbon.framework/Carbon";
    private const uint EventClassKeyboard = 1 << 16;
    private const uint EventGetModifierState = 23;

    [DllImport(CarbonFramework)]
    private static extern int SendEventToEventTarget(IntPtr inEvent, IntPtr inTarget);

    [DllImport(CarbonFramework)]
    private static extern int CreateEvent(IntPtr inAllocator, uint inClass, uint inKind, double inWhen, uint inFlags, out IntPtr outEvent);

    public static bool IsCapsLockToggled()
    {
        IntPtr eventRef;
        int result = CreateEvent(IntPtr.Zero, EventClassKeyboard, EventGetModifierState, 0, 0, out eventRef);

        if (result == 0)
        {
            int sendResult = SendEventToEventTarget(eventRef, IntPtr.Zero);

            if (sendResult == 0)
            {
                const int kEventParamKeyModifiers = 1;
                int capsLockState = Marshal.ReadInt32(eventRef + kEventParamKeyModifiers);
                return (capsLockState & 0x00000001) != 0;
            }
        }

        return false;
    }

    public static bool IsNumLockToggled()
    {
        IntPtr eventRef;
        int result = CreateEvent(IntPtr.Zero, EventClassKeyboard, EventGetModifierState, 0, 0, out eventRef);

        if (result == 0)
        {
            int sendResult = SendEventToEventTarget(eventRef, IntPtr.Zero);

            if (sendResult == 0)
            {
                const int kEventParamKeyModifiers = 1;
                int numLockState = Marshal.ReadInt32(eventRef + kEventParamKeyModifiers);
                return (numLockState & 0x00000002) != 0;
            }
        }

        return false;
    }
}