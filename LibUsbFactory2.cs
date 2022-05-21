#if !Windows
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public static class LibUsbFactoryExtensions
    {
        public static IDeviceFactory CreateLibUsbDeviceFactory2(
            this FilterDeviceDefinition filterDeviceDefinition,
            ILoggerFactory loggerFactory = null,
            int? timeout = null,
            ushort? writeBufferSize = null,
            ushort? readBufferSize = null,
            Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> supportsDevice = null
            )
             => CreateLibUsbDeviceFactory2
                    (
                        new ReadOnlyCollection<FilterDeviceDefinition>(new List<FilterDeviceDefinition> { filterDeviceDefinition }),
                        loggerFactory,
                        timeout,
                        writeBufferSize,
                        readBufferSize,
                        supportsDevice
                 );

        public static IDeviceFactory CreateLibUsbDeviceFactory2(
            this IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions,
            ILoggerFactory loggerFactory = null,
            int? timeout = null,
            ushort? writeBufferSize = null,
            ushort? readBufferSize = null,
            Func<ConnectedDeviceDefinition, CancellationToken, Task<bool>> supportsDevice = null
            )
             => new DeviceFactory(
                loggerFactory,
                cancellationToken => GetConnectedDeviceDefinitionsAsync2(filterDeviceDefinitions, cancellationToken),
                (c, cancellationToken) =>
                Task.FromResult<IDevice>(
                    new Usb.Net.UsbDevice
                    (
                        c.DeviceId,
                        new LibUsbInterfaceManager(
                            GetDevice2(c),
                            timeout ?? 1000,
                            loggerFactory,
                            writeBufferSize,
                            readBufferSize), loggerFactory
                    )
                ),
                supportsDevice ??
                ((c, cancellationToken) => Task.FromResult(c.DeviceType == DeviceType.Usb))
            );

        public static Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync2(
            IReadOnlyList<FilterDeviceDefinition> filterDeviceDefinitions,
            CancellationToken cancellationToken = default)
        =>
            Task.Run<IEnumerable<ConnectedDeviceDefinition>>(
                () =>
           {
               IEnumerable<UsbRegistry> devices = UsbDevice.AllDevices;

               return filterDeviceDefinitions == null || filterDeviceDefinitions.Count == 0
                   ? devices.Select(usbRegistry
                   =>
                   new ConnectedDeviceDefinition(
                    usbRegistry.DevicePath,
                    vendorId: (uint)usbRegistry.Vid,
                    productId: (uint)usbRegistry.Pid,
                    deviceType: DeviceType.Usb
                   )
                   ).ToList()
                   : devices
               .Where(d => filterDeviceDefinitions.FirstOrDefault(f
                   =>
                   (f.VendorId == null || f.VendorId == d.Vid) &&
                   (f.ProductId == null || f.ProductId == d.Pid)
                   )
               != null)
               .Select(usbRegistry => new ConnectedDeviceDefinition
               (
                   usbRegistry.DevicePath,
                   vendorId: (uint)usbRegistry.Vid,
                   productId: (uint)usbRegistry.Pid,
                   deviceType: DeviceType.Usb
               )).ToList();
           }, cancellationToken);

        public static UsbDevice GetDevice2(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));
#pragma warning disable CA2208
            if (deviceDefinition.VendorId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.VendorId));
            if (deviceDefinition.ProductId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.ProductId));
#pragma warning restore CA2208 
            IEnumerable<UsbRegistry> devices = UsbDevice.AllDevices;
            var dev = devices.First(dev => dev.DevicePath == deviceDefinition.DeviceId).Device;
            dev.Open();
            return dev;
        }
    }
}
#endif