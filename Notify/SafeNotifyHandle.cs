namespace GuitarConfigurator.NetCore.Notify;

using System;
using Microsoft.Win32.SafeHandles;

internal class SafeNotifyHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeNotifyHandle()
        : base(true)
    {
    }

    public SafeNotifyHandle(IntPtr pHandle)
        : base(true)
    {
        SetHandle(pHandle);
    }

    protected override bool ReleaseHandle()
    {
        if (handle != IntPtr.Zero)
        {
            bool bSuccess = WindowsDeviceNotifierAvalonia.UnregisterDeviceNotification(handle);
            handle = IntPtr.Zero;
        }

        return true;
    }
}