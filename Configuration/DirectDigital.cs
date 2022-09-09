using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration
{

    [JsonDiscriminator(nameof(DirectDigital))]
    public class DirectDigital : Button
    {
        public DirectDigital(Microcontroller.Microcontroller controller, DevicePinMode pinmode, int pin, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.PinMode = pinmode;
            this.Pin = pin;
        }

        public DevicePinMode PinMode { get; }
        public int Pin { get; }

        public override string Input => $"Pin {Controller.GetPin(Pin)}";

        public override string Generate(IEnumerable<Binding> bindings, bool xbox)
        {
            return Controller.GenerateDigitalRead(Pin, PinMode == DevicePinMode.VCC);
        }
    }
}