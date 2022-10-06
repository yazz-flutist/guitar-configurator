using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("wii")]
public class JsonWiiInput : JsonInput
{
    private int Sda { get; }
    private int Scl { get; }
    private WiiInputType Type { get; }

    public JsonWiiInput(int sda, int scl, WiiInputType type)
    {
        Sda = sda;
        Scl = scl;
        Type = type;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new WiiInput(Type, microcontroller, Sda, Scl);
    }
}