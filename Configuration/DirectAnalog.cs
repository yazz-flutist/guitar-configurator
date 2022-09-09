using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Utils;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

[JsonDiscriminator(nameof(DirectAnalog))]
public class DirectAnalog : Axis
{
    // TODO: can we set trigger based on outputaxis now?
    public DirectAnalog(Microcontroller.Microcontroller controller, int pin, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct,type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
        this.Pin = pin;
    }
    public int Pin { get; }

    public override string Input => $"Pin {Pin}";

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        var pins = bindings.FilterCast<Binding, DirectAnalog>().OrderBy(b => b.Pin).ToArray();
        if (Trigger)
        {
            return Controller.GenerateAnalogTriggerRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone, xbox);
        }
        return Controller.GenerateAnalogRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone, xbox);
    }
    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Controller.GenerateAnalogReadRaw(bindings, Pin);
    }
}