using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Conversions;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("dac")]
public class JsonDigitalToAnalog : JsonInput
{
    private JsonInput Child { get; }
    private int Value { get; }

    public JsonDigitalToAnalog(JsonInput child, int value)
    {
        Child = child;
        Value = value;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new DigitalToAnalog(Child.Generate(microcontroller), Value);
    }
}