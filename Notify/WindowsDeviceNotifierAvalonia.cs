using System.Collections.Generic;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.DeviceNotify.Info;
using LibUsbDotNet.WinUsb;
using LibUsbDotNet.Main;
using Microsoft.Win32;

namespace GuitarConfigurator.NetCore.Notify;

// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Notifies an application of a change to the hardware Configuration of a device or 
/// the computer. See <see cref="IDeviceNotifier"/> or <see cref="DeviceNotifier.OpenDeviceNotifier"/> interface for more information
/// </summary>
/// <remarks>
/// This is the windows implementation of the device notifier.
/// </remarks>
public class WindowsDeviceNotifierAvalonia : IDeviceNotifier
{
    public static readonly Guid SantrollerGUID = new Guid("DF59037D-7C92-4155-AC12-7D700A313D79");
    public static readonly Guid ArdwiinoGUID = new Guid("DF59037D-7C92-4155-AC12-7D700A313D78");

    public static readonly Guid UsbGUID = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
    private readonly DevBroadcastDeviceinterface mDevInterface =
        new DevBroadcastDeviceinterface(UsbGUID);

    private readonly DevBroadcastDeviceinterface mDevInterfaceSantroller =
        new DevBroadcastDeviceinterface(SantrollerGUID);

    private readonly DevBroadcastDeviceinterface mDevInterfaceArdwiino =
        new DevBroadcastDeviceinterface(ArdwiinoGUID);

    private SafeNotifyHandle? mDevInterfaceHandle;
    private SafeNotifyHandle? mDevInterfaceHandleSantroller;
    private SafeNotifyHandle? mDevInterfaceHandleArdwiino;
    private bool mEnabled = true;
    private NotificationWindow? mNotifyWindow;

    ///<summary>
    /// Creates an instance of the <see cref="WindowsDeviceNotifier"/> class.
    /// See the <see cref="IDeviceNotifier"/> interface or <see cref="DeviceNotifier.OpenDeviceNotifier"/> method for more information
    ///</summary>
    ///<remarks>
    ///To make your code platform-independent use the <see cref="DeviceNotifier.OpenDeviceNotifier"/> method for creating instances.
    ///</remarks>
    public WindowsDeviceNotifierAvalonia()
    {
        mNotifyWindow = new NotificationWindow(OnDeviceChange);
        // mNotifyWindow.Show(false, false );
        //TODO: if this all works, then we need to hide the window probably.
        RegisterDeviceInterface(mNotifyWindow.Handle.Handle);
    }
    #region IDeviceNotifier Members

    ///<summary>
    /// Enables/Disables notification events.
    ///</summary>
    public bool Enabled
    {
        get { return mEnabled; }
        set { mEnabled = value; }
    }


    /// <summary>
    /// Main Notify event for all device notifications.
    /// </summary>
    public event EventHandler<DeviceNotifyEventArgs>? OnDeviceNotify;

    #endregion

    [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterDeviceNotificationA", CharSet = CharSet.Ansi)]
    private static extern SafeNotifyHandle RegisterDeviceNotification(IntPtr hRecipient,
        [MarshalAs(UnmanagedType.LPStruct), In]
        DevBroadcastDeviceinterface notificationFilter,
        int flags);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnregisterDeviceNotification(IntPtr handle);

    ///<summary>
    ///Releases the resources associated with this window. 
    ///</summary>
    ///
    ~WindowsDeviceNotifierAvalonia()
    {
        mNotifyWindow?.Dispose();
        mNotifyWindow = null;

        mDevInterfaceHandle?.Dispose();
        mDevInterfaceHandle = null;

        mDevInterfaceHandleSantroller?.Dispose();
        mDevInterfaceHandleSantroller = null;

        mDevInterfaceHandleArdwiino?.Dispose();
        mDevInterfaceHandleArdwiino = null;
    }

    private bool RegisterDeviceInterface(IntPtr windowHandle)
    {
        mDevInterfaceHandle?.Dispose();
        mDevInterfaceHandle = null;
        mDevInterfaceHandleSantroller?.Dispose();
        mDevInterfaceHandleSantroller = null;

        if (windowHandle != IntPtr.Zero)
        {
            mDevInterfaceHandle = RegisterDeviceNotification(windowHandle, mDevInterface, 0);
            mDevInterfaceHandleArdwiino = RegisterDeviceNotification(windowHandle, mDevInterfaceArdwiino, 0);
            mDevInterfaceHandleSantroller = RegisterDeviceNotification(windowHandle, mDevInterfaceSantroller, 0);
            if (mDevInterfaceHandle != null && !mDevInterfaceHandle.IsInvalid && mDevInterfaceHandleArdwiino != null &&
                !mDevInterfaceHandleArdwiino.IsInvalid && mDevInterfaceHandleSantroller != null &&
                !mDevInterfaceHandleSantroller.IsInvalid)
                return true;
            return false;
        }

        return false;
    }

    private void OnDeviceChange(uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (!mEnabled) return;
        if (lParam != IntPtr.Zero)
        {
            EventHandler<DeviceNotifyEventArgs>? temp = OnDeviceNotify;
            if (temp != null)
            {
                DeviceNotifyEventArgs? args;
                DevBroadcastHdr hdr = new DevBroadcastHdr();
                Marshal.PtrToStructure(lParam, hdr);
                switch (hdr.DeviceType)
                {
                    case DeviceType.Port:
                    case DeviceType.Volume:
                    case DeviceType.DeviceInterface:
                        args = new WindowsDeviceNotifyEventArgs(hdr, lParam, (EventType) wParam.ToInt32());
                        break;
                    default:
                        args = null;
                        break;
                }

                if (args != null) temp(this, args);
            }
        }
    }
}