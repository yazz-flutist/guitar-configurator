using System;
using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.PS2
{
    [JsonDiscriminator(nameof(Ps2Button))]
    public class Ps2Button : GroupableButton, IPs2Input
    {
        public Ps2Button(Microcontroller.Microcontroller controller, Ps2ButtonType button, Ps2Controller ps2Controller, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.PS2, debounce, type, ledOn, ledOff)
        {
            this.Button = button;
            this.Ps2Controller = ps2Controller;
        }

        public Ps2ButtonType Button { get; }
        public Ps2Controller Ps2Controller { get; }

        public override StandardButtonType StandardButton => StandardButtonMap.Ps2ButtonMap[Button];

        public override string Input => Enum.GetName(typeof(Ps2ButtonType), Button)!;

        private readonly Dictionary<Ps2ButtonType, String> _buttonMap = new Dictionary<Ps2ButtonType, string>() {
            {Ps2ButtonType.GuitarGreen, "in[4] >> 1"},
            {Ps2ButtonType.GuitarRed,   "in[4] >> 5"},
            {Ps2ButtonType.GuitarYellow,"in[4] >> 4"},
            {Ps2ButtonType.GuitarBlue,  "in[4] >> 6"},
            {Ps2ButtonType.GuitarOrange,"in[4] >> 7"},
            {Ps2ButtonType.Select,      "in[3] >> 0"},
            {Ps2ButtonType.L3,          "in[3] >> 1"},
            {Ps2ButtonType.R3,          "in[3] >> 2"},
            {Ps2ButtonType.Start,       "in[3] >> 3"},
            {Ps2ButtonType.Up,          "in[3] >> 4"},
            {Ps2ButtonType.Right,       "in[3] >> 5"},
            {Ps2ButtonType.Down,        "in[3] >> 6"},
            {Ps2ButtonType.Left,        "in[3] >> 7"},
            {Ps2ButtonType.L2,          "in[4] >> 0"},
            {Ps2ButtonType.R2,          "in[4] >> 1"},
            {Ps2ButtonType.L1,          "in[4] >> 2"},
            {Ps2ButtonType.R1,          "in[4] >> 3"},
            {Ps2ButtonType.Triangle,    "in[4] >> 4"},
            {Ps2ButtonType.Circle,      "in[4] >> 5"},
            {Ps2ButtonType.Cross,       "in[4] >> 6"},
            {Ps2ButtonType.Square,      "in[4] >> 7"}
        };
        public override string Generate(IEnumerable<Binding> bindings, bool xbox)
        {
            return $"({_buttonMap[Button]} & 1)";
        }
    }
}