using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("di")]
public class JsonDirectInput : JsonInput
{
    private int Pin { get; }
    private DevicePinMode PinMode { get; }

    public JsonDirectInput(int pin, DevicePinMode pinMode)
    {
        Pin = pin;
        PinMode = pinMode;
    }

    public override Input Generate(Microcontroller microcontroller)
    {
        return new DirectInput(Pin, PinMode, microcontroller);
    }
}