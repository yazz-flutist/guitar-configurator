using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using GuitarConfiguratorSharp.Utils;

namespace GuitarConfiguratorSharp.Configuration
{

    public class DirectDigital : Button
    {
        public DirectDigital(DevicePinMode pinmode, int Pin, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.PinMode = pinmode;
            this.Pin = Pin;
        }

        public DevicePinMode PinMode { get; }
        public int Pin { get; }

        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            return controller.generateDigitalRead(Pin, PinMode == DevicePinMode.VCC);
        }
    }
    public class DirectAnalog : Axis
    {
        // TODO: can we set trigger based on outputaxis now?
        public DirectAnalog(int Pin, OutputAxis type, Color ledOn, Color ledOff, int multiplier, int offset, int deadzone, bool trigger) : base(InputControllerType.Direct,type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
            this.Pin = Pin;
        }
        public int Pin { get; }

        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            var pins = bindings.FilterCast<Binding, DirectAnalog>().OrderBy(b => b.Pin).ToArray();
            if (Trigger)
            {
                return controller.generateAnalogTriggerRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone);
            }
            return controller.generateAnalogRead(Pin, Array.IndexOf(pins, this), Offset, Multiplier, Deadzone);
        }
    }

}