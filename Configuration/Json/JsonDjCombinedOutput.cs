using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("djc")]
public class JsonDjCombinedOutput : JsonOutput
{
    public override JsonInput? Input => null;
    public override Color LedOn { get; }
    public override Color LedOff { get; }
    public int Sda { get; }
    public int Scl { get; }

    public JsonDjCombinedOutput(Color ledOn, Color ledOff, int sda, int scl)
    {
        LedOn = ledOn;
        LedOff = ledOff;
        Sda = sda;
        Scl = scl;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new DjCombinedOutput(model, microcontroller, Sda, Scl);
    }
}