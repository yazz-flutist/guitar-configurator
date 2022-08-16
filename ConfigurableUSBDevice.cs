
using Device.Net;
using Usb.Net;
using System;
using GuitarConfiguratorSharp.Utils;
using GuitarConfiguratorSharp.Configuration;

#if !Windows
using Device.Net.LibUsb;
using LibUsbDotNet.Main;
#endif

public abstract class ConfigurableUSBDevice : ConfigurableDevice
{
    protected readonly UsbDevice device;
    protected readonly string product;
    protected readonly string serial;
    protected readonly Version version;

    public ConfigurableUSBDevice(IDevice device, string product, string serial, ushort version)
    {
        this.device = (UsbDevice)device;
        this.product = product;
        this.serial = serial;
        this.version = new Version((version >> 8) & 0xff, (version >> 4) & 0xf, (version) & 0xf);
    }

    public abstract bool MigrationSupported { get; }
    public abstract DeviceConfiguration Configuration { get; }

    public abstract void bootloader();
    public abstract void bootloaderUSB();

    public bool IsSameDevice(IDevice device)
    {
        return this.device == device;
    }
    public bool IsSameDevice(PlatformIOPort port)
    {
        return false;
    }

    public bool IsSameDevice(string serial_or_path)
    {
        return this.serial == serial_or_path;
    }

    public byte[] ReadData(ushort wValue, byte bRequest, ushort size=128)
    {
        byte[] buffer = new byte[size];
        SetupPacket packet = new SetupPacket(new UsbDeviceRequestType(RequestDirection.In, RequestType.Class, RequestRecipient.Interface), bRequest, wValue, 2, (ushort)buffer.Length);
#if Windows
        TransferResult tr = device.PerformControlTransferAsync(packet, buffer).Result;
        Array.Resize(ref tr.Data, tr.BytesTransferred);
        return tr.Data;
#else
        // TODO: if the libary is every fixed, then we won't need this patch
        LibUsbInterfaceManager luim = (LibUsbInterfaceManager)device.UsbInterfaceManager;
        var sp = new UsbSetupPacket(
            packet.RequestType.ToByte(),
            packet.Request,
            packet.Value,
            packet.Index,
            packet.Length);
        luim.UsbDevice.ControlTransfer(ref sp, buffer, buffer.Length, out var length);
        Array.Resize(ref buffer, length);
        return buffer;
#endif 

    }


    public uint WriteData(ushort id, byte bRequest, byte[] buffer)
    {
        SetupPacket packet = new SetupPacket(new UsbDeviceRequestType(RequestDirection.Out, RequestType.Class, RequestRecipient.Interface), bRequest, id, 2, (ushort)buffer.Length);
#if Windows
        TransferResult tr = device.PerformControlTransferAsync(packet, buffer).Result;
        return tr.BytesTransferred;
#else
        
        // TODO: if the libary is ever fixed, then we won't need this patch
        LibUsbInterfaceManager luim = (LibUsbInterfaceManager)device.UsbInterfaceManager;
        var sp = new UsbSetupPacket(
            packet.RequestType.ToByte(),
            packet.Request,
            packet.Value,
            packet.Index,
            packet.Length);
        luim.UsbDevice.ControlTransfer(ref sp, buffer, buffer.Length, out var length);
        return (uint)length;
#endif 
    }



}