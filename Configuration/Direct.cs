using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.Utils;

namespace GuitarConfiguratorSharp.Configuration
{

    [JsonDiscriminator(nameof(DirectDigital))]
    public class DirectDigital : Button
    {
        public DirectDigital(Microcontroller controller, DevicePinMode pinmode, int Pin, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.PinMode = pinmode;
            this.Pin = Pin;
        }

        public DevicePinMode PinMode { get; }
        public int Pin { get; }

        public override string Input => $"Pin {Controller.GetPin(Pin)}";

        public override string generate(IEnumerable<Binding> bindings)
        {
            return Controller.generateDigitalRead(Pin, PinMode == DevicePinMode.VCC);
        }
    }
    [JsonDiscriminator(nameof(DirectAnalog))]
    public class DirectAnalog : Axis
    {
        // TODO: can we set trigger based on outputaxis now?
        public DirectAnalog(Microcontroller controller, int Pin, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct,type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
            this.Pin = Pin;
        }
        public int Pin { get; }

        public override string Input => $"Pin {Pin}";

        public override string generate(IEnumerable<Binding> bindings)
        {
            var pins = bindings.FilterCast<Binding, DirectAnalog>().OrderBy(b => b.Pin).ToArray();
            if (Trigger)
            {
                return Controller.generateAnalogTriggerRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone);
            }
            return Controller.generateAnalogRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone);
        }
        internal override string generateRaw(IEnumerable<Binding> bindings)
        {
            return Controller.generateAnalogReadRaw(bindings, Pin);
        }
    }

}