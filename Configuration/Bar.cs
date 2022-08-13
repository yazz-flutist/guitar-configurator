using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.Utils;

namespace GuitarConfiguratorSharp.Configuration
{
    [Flags]
    public enum BarButton
    {
        Green = 1,
        Red = 2,
        Yellow = 4,
        Blue = 8,
        Orange = 16
    }
    [JsonDiscriminator(nameof(DirectGHFiveTarBarButton))]
    public class DirectGHFiveTarBarButton : Button
    {
        public DirectGHFiveTarBarButton(Microcontroller controller, GHFiveTarButton Button, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.Button = Button;
        }

        public GHFiveTarButton Button { get; }

        public override string Input => Enum.GetName(typeof(GHFiveTarButton), Button)!;
        static Dictionary<int, BarButton> mappings = new Dictionary<int, BarButton>() {
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

        private static List<GHFiveTarButton> frets = new List<GHFiveTarButton>() {
            GHFiveTarButton.Yellow,
            GHFiveTarButton.Blue,
            GHFiveTarButton.Red,
            GHFiveTarButton.Green,
            GHFiveTarButton.None,
            GHFiveTarButton.None,
            GHFiveTarButton.None,
            GHFiveTarButton.Orange
        };
        private static List<GHFiveTarButton> tap = new List<GHFiveTarButton>() {
            GHFiveTarButton.TapGreen,
            GHFiveTarButton.TapRed,
            GHFiveTarButton.TapYellow,
            GHFiveTarButton.TapBlue,
            GHFiveTarButton.TapOrange
        };
        public override string generate(IEnumerable<Binding> bindings)
        {
            if (frets.Contains(this.Button))
            {
                return $"(fivetar_buttons[0] >> {frets.IndexOf(this.Button)}) & 1";
            }
            return $"(fivetartapbindings[fivetar_buttons[1]] >> {tap.IndexOf(this.Button)}) & 1";
        }

        public static string tickFiveTar(IEnumerable<Binding> bindings)
        {
            // If we aren't using tap bar stuff, we can skip reading that byte of data
            if (bindings.FilterCast<Binding, DirectGHFiveTarBarButton>().All(binding => frets.Contains(binding.Button)))
            {
                return @"uint8_t fivetar_buttons[2];
                    twi_readFromPointer(GH5NECK_ADDR, GH5NECK_BUTTONS_PTR, 1, fivetar_buttons);
                    uint8_t fivetartapbindings = {" + String.Join(" ", mappings.Keys.Select(key => $"[{key}] = {(int)mappings[key]},")) + "};";

            }
            return DirectGHFiveTarBarAnalog.tickFiveTar(bindings);
        }
    }

    [JsonDiscriminator(nameof(DirectGHFiveTarBarAnalog))]
    public class DirectGHFiveTarBarAnalog : Axis
    {
        public override string Input => "Guitar Hero 5 Tap Bar";

        public DirectGHFiveTarBarAnalog(Microcontroller controller, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
        }

        public override string generate(IEnumerable<Binding> bindings)
        {
            return "fivetar_buttons[1]";
        }
        public static string tickFiveTar(IEnumerable<Binding> bindings)
        {
            return @"uint8_t fivetar_buttons[2];
                    twi_readFromPointer(GH5NECK_ADDR, GH5NECK_BUTTONS_PTR, 2, fivetar_buttons);";
        }

        internal override string generateRaw(IEnumerable<Binding> bindings)
        {
            return generate(bindings);
        }
    }
    [JsonDiscriminator(nameof(DirectGHWTBarButton))]
    public class DirectGHWTBarButton : Button
    {
        public DirectGHWTBarButton(Microcontroller controller, GHWTTarButton Button, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
        {
            this.Button = Button;
        }

        public override string Input => Enum.GetName(typeof(GHWTTarButton), Button)!;
        public GHWTTarButton Button { get; }
        private static List<GHWTTarButton> tap = new List<GHWTTarButton>() {
            GHWTTarButton.TapGreen,
            GHWTTarButton.TapRed,
            GHWTTarButton.TapYellow,
            GHWTTarButton.TapBlue,
            GHWTTarButton.TapOrange
        };
        public override string generate(IEnumerable<Binding> bindings)
        {
            return $"(wttapbindings[fivetar_buttons[1]] >> {tap.IndexOf(this.Button)}) & 1";
        }
        static Dictionary<int, BarButton> mappings = new Dictionary<int, BarButton>() {
            {0x17, BarButton.Green},
            {0x16, BarButton.Green},
            {0x14, BarButton.Green | BarButton.Red},
            {0x11, BarButton.Red},
            {0x12, BarButton.Red},
            {0xf, BarButton.Red | BarButton.Yellow},
            {0xa, BarButton.Yellow},
            {0xb, BarButton.Yellow},
            {0x9, BarButton.Yellow | BarButton.Blue},
            {0x7, BarButton.Blue},
            {0x5, BarButton.Blue | BarButton.Orange},
            {0x4, BarButton.Blue | BarButton.Orange},
            {0x3, BarButton.Blue | BarButton.Orange},
            {0x0, BarButton.Orange},
        };
        public static string initWT()
        {
            return "uint8_t wttapbindings = {" + String.Join(" ", mappings.Keys.Select(key => $"[{key}] = {(int)mappings[key]},")) + "};";
        }
    }
    [JsonDiscriminator(nameof(DirectGHWTBarAnalog))]

    public class DirectGHWTBarAnalog : Axis
    {
        public DirectGHWTBarAnalog(Microcontroller controller, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Direct, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
        }
        public override string Input => "Guitar Hero WT Tap Bar";

        public override string generate(IEnumerable<Binding> bindings)
        {
            return "lastTap";
        }
        public static string tickWT()
        {
            return @"long pulse = digitalReadPulse(&wtPin, LOW, 50);
                    if (pulse == digitalReadPulse(&wtPin, LOW, 50)) {
                        lastTap = wttapbindings[pulse >> 1];
                    }";
        }
        internal override string generateRaw(IEnumerable<Binding> bindings)
        {
            return generate(bindings);
        }
    }
}