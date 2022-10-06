using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("ps2c")]
public class JsonPs2CombinedOutput : JsonOutput
{
    public override JsonInput? Input => null;
    public override Color LedOn { get; }
    public override Color LedOff { get; }
    public int Miso { get; }
    public int Mosi { get; }
    public int Sck { get; }

    public JsonPs2CombinedOutput(Color ledOn, Color ledOff, int miso, int mosi, int sck)
    {
        LedOn = ledOn;
        LedOff = ledOff;
        Miso = miso;
        Mosi = mosi;
        Sck = sck;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new Ps2CombinedOutput(model, microcontroller, Miso, Mosi, Sck);
    }
}