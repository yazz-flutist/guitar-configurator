using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Neck;

[JsonDiscriminator(nameof(DirectGhwtBarAnalog))]

public class DirectGhwtBarAnalog : Axis
{
    public DirectGhwtBarAnalog(Microcontroller.Microcontroller controller, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
    }
    public override string Input => "Guitar Hero WT Tap Bar";

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        return "lastTap";
    }
    public static string TickWt()
    {
        return @"long pulse = digitalReadPulse(&wtPin, LOW, 50);
                    if (pulse == digitalReadPulse(&wtPin, LOW, 50)) {
                        lastTap = wttapbindings[pulse >> 1];
                    }";
    }
    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Generate(bindings, xbox);
    }
}