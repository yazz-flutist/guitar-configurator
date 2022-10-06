using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Conversions;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("adc")]
public class JsonAnalogToDigital : JsonInput
{
    private JsonInput Child { get; }
    private AnalogToDigitalType Type { get; }
    private int Threshold { get; }

    public JsonAnalogToDigital(JsonInput child, AnalogToDigitalType type, int threshold)
    {
        Child = child;
        Type = type;
        Threshold = threshold;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new AnalogToDigital(Child.Generate(microcontroller), Type, Threshold);
    }
}