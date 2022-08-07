using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.Configuration
{
    public interface PS2Input {
        public PS2Controller ps2Controller {get;}
        static public Dictionary<PS2Controller, string> caseStatements = new Dictionary<PS2Controller, string>() {
            {PS2Controller.Digital, "PSPROTO_DIGITAL"}
        };
    }

    [JsonDiscriminator(nameof(PS2Button))]
    public class PS2Button : GroupableButton, PS2Input
    {
        public PS2Button(PS2ButtonType button, PS2Controller controller, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(InputControllerType.PS2, debounce, type, ledOn, ledOff)
        {
            this.button = button;
            this.ps2Controller = controller;
        }

        public PS2ButtonType button { get; }
        public PS2Controller ps2Controller { get; }

        public override StandardButtonType standardButton => StandardButtonMap.ps2ButtonMap[button];

        private Dictionary<PS2ButtonType, String> buttonMap = new Dictionary<PS2ButtonType, string>() {
            {PS2ButtonType.GuitarGreen, "in[4] >> 1"},
            {PS2ButtonType.GuitarRed,   "in[4] >> 5"},
            {PS2ButtonType.GuitarYellow,"in[4] >> 4"},
            {PS2ButtonType.GuitarBlue,  "in[4] >> 6"},
            {PS2ButtonType.GuitarOrange,"in[4] >> 7"},
            {PS2ButtonType.Select,      "in[3] >> 0"},
            {PS2ButtonType.L3,          "in[3] >> 1"},
            {PS2ButtonType.R3,          "in[3] >> 2"},
            {PS2ButtonType.Start,       "in[3] >> 3"},
            {PS2ButtonType.Up,          "in[3] >> 4"},
            {PS2ButtonType.Right,       "in[3] >> 5"},
            {PS2ButtonType.Down,        "in[3] >> 6"},
            {PS2ButtonType.Left,        "in[3] >> 7"},
            {PS2ButtonType.L2,          "in[4] >> 0"},
            {PS2ButtonType.R2,          "in[4] >> 1"},
            {PS2ButtonType.L1,          "in[4] >> 2"},
            {PS2ButtonType.R1,          "in[4] >> 3"},
            {PS2ButtonType.Triangle,    "in[4] >> 4"},
            {PS2ButtonType.Circle,      "in[4] >> 5"},
            {PS2ButtonType.Cross,       "in[4] >> 6"},
            {PS2ButtonType.Square,      "in[4] >> 7"}
        };
        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            return $"({buttonMap[button]} & 1)";
        }
    }


    [JsonDiscriminator(nameof(PS2Analog))]
    public class PS2Analog : GroupableAxis, PS2Input
    {
        public PS2Analog(PS2Axis axis, PS2Controller controller, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(InputControllerType.PS2, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
            this.axis = axis;
            this.ps2Controller = controller;
        }

        private Dictionary<PS2Axis, String> analogMap = new Dictionary<PS2Axis, string>() {
            { PS2Axis.DualshockLeftX,   "(in[7] - 128) << 8"  },
            { PS2Axis.DualshockLeftY,   "-(in[8] - 127) << 8" },
            { PS2Axis.DualshockRightX,  "(in[5] - 128) << 8"  },
            { PS2Axis.DualshockRightY,  "-(in[6] - 127) << 8" },
            { PS2Axis.NegConTwist,      "(in[5] - 128) << 8"  },
            { PS2Axis.NegConTwistI,     "in[6]"               },
            { PS2Axis.NegConTwistII,    "in[7]"               },
            { PS2Axis.NegConTwistL,     "in[8]"               },
            { PS2Axis.GunconHSync,      "(in[6] << 8) | in[5]"},
            { PS2Axis.GunconVSync,      "(in[8] << 8) | in[7]"},
            { PS2Axis.JogConWheel,      "(in[6] << 8) | in[5]"},
            { PS2Axis.GuitarWhammy,     "-(in[8] - 127) << 8" }
        };

        public static List<PS2Axis> dualshock2Order = new List<PS2Axis>(){
            PS2Axis.Dualshock2RightX,
            PS2Axis.Dualshock2RightY,
            PS2Axis.Dualshock2LeftX,
            PS2Axis.Dualshock2LeftY,
            PS2Axis.Dualshock2RightButton,
            PS2Axis.Dualshock2LeftButton,
            PS2Axis.Dualshock2UpButton,
            PS2Axis.Dualshock2DownButton,
            PS2Axis.Dualshock2Triangle,
            PS2Axis.Dualshock2Circle,
            PS2Axis.Dualshock2Cross,
            PS2Axis.Dualshock2Square,
            PS2Axis.Dualshock2L1,
            PS2Axis.Dualshock2R1,
            PS2Axis.Dualshock2L2,
            PS2Axis.Dualshock2R2,
        };

        public PS2Axis axis { get; }

        public PS2Controller ps2Controller { get; }

        public override StandardAxisType standardAxis => StandardAxisMap.ps2AxisMap[axis];

        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            if (this.ps2Controller == PS2Controller.Dualshock2)
            {
                return $"in[{GetAxes(bindings).IndexOf(axis)}]";
            }
            else
            {
                return analogMap[axis];
            }
        }

        private static List<PS2Axis> GetAxes(IEnumerable<Binding> bindings)
        {
            return bindings
                   .Select(binding => binding as PS2Analog)
                   .Where(binding => binding != null)
                   .Select(binding => binding!.axis)
                   .OrderBy(axis => dualshock2Order.IndexOf(axis))
                   .ToList();
        }
        public static string generatePressures(IEnumerable<Binding> bindings)
        {
            var axes = GetAxes(bindings);
            var bits = String.Join("", PS2Analog.dualshock2Order.Select(axis => axes.Contains(axis) ? 1 : 0));
            // Generate binary 0b ints, remembering to split every 8 bits to form bytes
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bits.Length; i++)
            {
                if (i % 8 == 0)
                    sb.Append(", 0b");
                sb.Append(bits[i]);
            }
            // Slice off extra ,
            return sb.ToString().Substring(2);
        }

    }
}