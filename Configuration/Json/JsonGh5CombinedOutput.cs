using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("gh5c")]
public class JsonGh5CombinedOutput : JsonOutput
{
    public override JsonInput? Input => null;
    public override Color LedOn { get; }
    public override Color LedOff { get; }
    public int Sda { get; }
    public int Scl { get; }
    public bool MapTapBarToFrets { get; }
    public bool MapTapBarToAxis { get; }
    public bool FretsEnabled { get; }

    public JsonGh5CombinedOutput(Color ledOn, Color ledOff, int sda, int scl, bool fretsEnabled, bool mapTapBarToFrets,
        bool mapTapBarToAxis)
    {
        LedOn = ledOn;
        LedOff = ledOff;
        Sda = sda;
        Scl = scl;
        FretsEnabled = fretsEnabled;
        MapTapBarToFrets = mapTapBarToFrets;
        MapTapBarToAxis = mapTapBarToAxis;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new GH5CombinedOutput(model, microcontroller, Sda, Scl, FretsEnabled, MapTapBarToFrets, MapTapBarToAxis);
    }
}