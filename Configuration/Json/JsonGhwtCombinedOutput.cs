using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("ghwtc")]
public class JsonGhwtCombinedOutput : JsonOutput
{
    public override JsonInput? Input => null;
    public override Color LedOn { get; }
    public override Color LedOff { get; }
    public int Pin { get; }
    public bool MapTapBarToFrets { get; set; }
    public bool MapTapBarToAxis { get; set; }

    public JsonGhwtCombinedOutput(Color ledOn, Color ledOff, int pin, bool mapTapBarToFrets, bool mapTapBarToAxis)
    {
        LedOn = ledOn;
        LedOff = ledOff;
        Pin = pin;
        MapTapBarToFrets = mapTapBarToFrets;
        MapTapBarToAxis = mapTapBarToAxis;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new GHWTCombinedOutput(model, microcontroller, Pin, MapTapBarToFrets, MapTapBarToAxis);
    }
}