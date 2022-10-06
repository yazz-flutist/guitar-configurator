using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("ps2")]
public class JsonPs2Input : JsonInput
{
    private int Miso { get; }
    private int Mosi { get; }
    private int Sck { get; }

    private int? Att { get; }
    private int? Ack { get; }
    private Ps2InputType Type { get; }

    public JsonPs2Input(int miso, int mosi, int sck, int? att, int? ack, Ps2InputType type)
    {
        Miso = miso;
        Mosi = mosi;
        Sck = sck;
        Att = att;
        Ack = ack;
        Type = type;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new Ps2Input(Type, microcontroller, Miso, Mosi, Sck, Att, Ack);
    }
}