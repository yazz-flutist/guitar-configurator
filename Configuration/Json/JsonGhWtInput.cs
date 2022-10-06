using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Neck;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("ghwt")]
public class JsonGhWtInput : JsonInput
{
    private int Pin { get; }

    private GhWtInputType Type { get; }

    public JsonGhWtInput(int pin, GhWtInputType type)
    {
        Pin = pin;
        Type = type;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new GhWtTapInput(Type, microcontroller, Pin);
    }
}