using Device.Net;
using GuitarConfiguratorSharp.Utils;
public interface ConfigurableDevice {
    public bool IsSameDevice(IDevice device);
    public bool IsSameDevice(PlatformIOPort port);
    public bool IsSameDevice(string path);
}