using System;
using LibUsbDotNet.DeviceNotify;

namespace GuitarConfigurator.NetCore.Notify;

public class WindowsDeviceNotifyEventArgs : DeviceNotifyEventArgs
{
    private readonly DevBroadcastHdr mBaseHdr;

    internal WindowsDeviceNotifyEventArgs(DevBroadcastHdr hdr, IntPtr ptrHdr, EventType eventType)
    {
        mBaseHdr = hdr;
        EventType = eventType;
        DeviceType = mBaseHdr.DeviceType;
        switch (DeviceType)
        {
            case DeviceType.Volume:
                Volume = new VolumeNotifyInfo(ptrHdr);
                Object = Volume;
                break;
            case DeviceType.Port:
                Port = new PortNotifyInfo(ptrHdr);
                Object = Port;
                break;
            case DeviceType.DeviceInterface:
                Device = new UsbDeviceNotifyInfo(ptrHdr);
                Object = Device;
                break;
        }
    }
}