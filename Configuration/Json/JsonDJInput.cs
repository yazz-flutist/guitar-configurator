using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.DJ;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("dj")]
public class JsonDjInput : JsonInput
{
    private int Sda { get; }
    private int Scl { get; }
    private DjInputType Type { get; }

    public JsonDjInput(int sda, int scl, DjInputType type)
    {
        Sda = sda;
        Scl = scl;
        Type = type;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new DjInput(Type, microcontroller, Sda, Scl);
    }
}