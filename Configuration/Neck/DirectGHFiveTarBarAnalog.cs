using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Neck;

[JsonDiscriminator(nameof(DirectGhFiveTarBarAnalog))]
public class DirectGhFiveTarBarAnalog : Axis
{
    public override string Input => "Guitar Hero 5 Tap Bar";

    public DirectGhFiveTarBarAnalog(Microcontroller.Microcontroller controller, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
    }

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        return "fivetar_buttons[1]";
    }
    public static string TickFiveTar(IEnumerable<Binding> bindings)
    {
        return @"uint8_t fivetar_buttons[2];
                    twi_readFromPointer(GH5NECK_ADDR, GH5NECK_BUTTONS_PTR, 2, fivetar_buttons);";
    }

    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Generate(bindings, xbox);
    }
}