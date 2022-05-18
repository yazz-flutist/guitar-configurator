
using Device.Net;
using Usb.Net;
using System;
using GuitarConfiguratorSharp.Utils;

public class Pico : ConfigurableDevice
{
    private string path;

    public Pico(string path)
    {
        this.path = path;

    }

    public bool IsSameDevice(IDevice device)
    {
        return false;
    }

    public bool IsSameDevice(PlatformIOPort port)
    {
        return false;
    }

    public bool IsSameDevice(string path)
    {
        return path == this.path;
    }

    public string GetPath() {
        return path;
    }

    public override String ToString()
    {
        return $"Pico ({this.path})";
    }
}