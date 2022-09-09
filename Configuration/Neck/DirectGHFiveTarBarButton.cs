using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Utils;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Neck
{
    [JsonDiscriminator(nameof(DirectGhFiveTarBarButton))]
    public class DirectGhFiveTarBarButton : Button
    {
        public DirectGhFiveTarBarButton(Microcontroller.Microcontroller controller, GhFiveTarButton button, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.Button = button;
        }

        public GhFiveTarButton Button { get; }

        public override string Input => Enum.GetName(typeof(GhFiveTarButton), Button)!;
        static readonly Dictionary<int, BarButton> _mappings = new Dictionary<int, BarButton>() {
            {0x19, BarButton.Green | BarButton.Yellow},
            {0x1A, BarButton.Yellow},
            {0x2C, BarButton.Green | BarButton.Red | BarButton.Yellow | BarButton.Blue},
            {0x2D, BarButton.Green | BarButton.Yellow | BarButton.Blue},
            {0x2E, BarButton.Red | BarButton.Yellow | BarButton.Blue},
            {0x2F, BarButton.Yellow | BarButton.Blue},
            {0x46, BarButton.Green | BarButton.Red | BarButton.Blue},
            {0x47, BarButton.Green | BarButton.Blue},
            {0x48, BarButton.Red | BarButton.Blue},
            {0x49, BarButton.Blue},
            {0x5F, BarButton.Green | BarButton.Red | BarButton.Yellow | BarButton.Blue |  BarButton.Orange},
            {0x60, BarButton.Green | BarButton.Red | BarButton.Blue | BarButton.Orange},
            {0x61, BarButton.Green | BarButton.Yellow | BarButton.Blue | BarButton.Orange},
            {0x62, BarButton.Green | BarButton.Blue | BarButton.Orange},
            {0x63, BarButton.Red | BarButton.Yellow | BarButton.Blue | BarButton.Orange},
            {0x64, BarButton.Red | BarButton.Blue | BarButton.Orange},
            {0x65, BarButton.Yellow | BarButton.Blue | BarButton.Orange},
            {0x66, BarButton.Blue | BarButton.Orange},
            {0x78, BarButton.Green | BarButton.Red | BarButton.Yellow | BarButton.Orange},
            {0x79, BarButton.Green | BarButton.Red | BarButton.Orange},
            {0x7A, BarButton.Green | BarButton.Yellow | BarButton.Orange},
            {0x7B, BarButton.Green | BarButton.Orange},
            {0x7C, BarButton.Red | BarButton.Yellow | BarButton.Orange},
            {0x7D, BarButton.Red | BarButton.Orange},
            {0x7E, BarButton.Yellow | BarButton.Orange},
            {0x7F, BarButton.Orange},
            {0x95, BarButton.Green},
            {0xB0, BarButton.Green | BarButton.Red},
            {0xCD, BarButton.Red},
            {0xE5, BarButton.Green | BarButton.Red | BarButton.Yellow},
            {0xE6, BarButton.Red | BarButton.Yellow},
        };

        private static readonly List<GhFiveTarButton> _frets = new List<GhFiveTarButton>() {
            GhFiveTarButton.Yellow,
            GhFiveTarButton.Blue,
            GhFiveTarButton.Red,
            GhFiveTarButton.Green,
            GhFiveTarButton.None,
            GhFiveTarButton.None,
            GhFiveTarButton.None,
            GhFiveTarButton.Orange
        };
        private static readonly List<GhFiveTarButton> _tap = new List<GhFiveTarButton>() {
            GhFiveTarButton.TapGreen,
            GhFiveTarButton.TapRed,
            GhFiveTarButton.TapYellow,
            GhFiveTarButton.TapBlue,
            GhFiveTarButton.TapOrange
        };
        public override string Generate(IEnumerable<Binding> bindings, bool xbox)
        {
            if (_frets.Contains(this.Button))
            {
                return $"(fivetar_buttons[0] >> {_frets.IndexOf(this.Button)}) & 1";
            }
            return $"(fivetartapbindings[fivetar_buttons[1]] >> {_tap.IndexOf(this.Button)}) & 1";
        }

        public static string TickFiveTar(IEnumerable<Binding> bindings)
        {
            // If we aren't using tap bar stuff, we can skip reading that byte of data
            if (bindings.FilterCast<Binding, DirectGhFiveTarBarButton>().All(binding => _frets.Contains(binding.Button)))
            {
                return @"uint8_t fivetar_buttons[2];
                    twi_readFromPointer(GH5NECK_ADDR, GH5NECK_BUTTONS_PTR, 1, fivetar_buttons);
                    uint8_t fivetartapbindings = {" + String.Join(" ", _mappings.Keys.Select(key => $"[{key}] = {(int)_mappings[key]},")) + "};";

            }
            return DirectGhFiveTarBarAnalog.TickFiveTar(bindings);
        }
    }
}