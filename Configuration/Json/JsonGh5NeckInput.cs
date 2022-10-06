using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Neck;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("gh5")]
public class JsonGh5NeckInput : JsonInput
{
    private int Sda { get; }
    private int Scl { get; }
    private Gh5NeckInputType Type { get; }

    public JsonGh5NeckInput(int sda, int scl, Gh5NeckInputType type)
    {
        Sda = sda;
        Scl = scl;
        Type = type;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new Gh5NeckInput(Type, microcontroller, Sda, Scl);
    }
}