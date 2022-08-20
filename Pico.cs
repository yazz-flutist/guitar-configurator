
using LibUsbDotNet;
using System;
using GuitarConfiguratorSharp.Utils;
using GuitarConfiguratorSharp.Configuration;
using System.Threading.Tasks;

public class PicoDevice : ConfigurableDevice
{
    private string path;

    public bool MigrationSupported => true;

    private DeviceConfiguration? _config;

    public DeviceConfiguration? Configuration => _config;

    public PicoDevice(PlatformIO pio, string path)
    {
        this.path = path;
        this._config = new DeviceConfiguration(Board.findMicrocontroller(Board.findBoard("pico",0)));
    }

    public bool IsSameDevice(PlatformIOPort port)
    {
        return false;
    }

    public bool IsSameDevice(string serial_or_path)
    {
        return serial_or_path == this.path;
    }

    public string GetPath()
    {
        return path;
    }

    public override String ToString()
    {
        return $"Pico ({this.path})";
    }

    public void Bootloader()
    {
    }

    public void BootloaderUSB()
    {
    }

    bool ConfigurableDevice.DeviceAdded(ConfigurableDevice device)
    {
        return false;
    }

    public Task<string?> getUploadPort()
    {
        return Task.FromResult((string?)path);
    }
}