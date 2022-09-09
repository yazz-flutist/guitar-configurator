using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.PS2;

[JsonDiscriminator(nameof(Ps2Analog))]
public class Ps2Analog : GroupableAxis, IPs2Input
{
    public Ps2Analog(Microcontroller.Microcontroller controller, Ps2Axis axis, Ps2Controller ps2Controller, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.PS2, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
        this.Axis = axis;
        this.Ps2Controller = ps2Controller;
    }
    public override string Input => Enum.GetName(typeof(Ps2Axis), Axis)!;
    private readonly Dictionary<Ps2Axis, String> _analogMap = new Dictionary<Ps2Axis, string>() {
        { Ps2Axis.DualshockLeftX,   "(in[7] - 128) << 8"  },
        { Ps2Axis.DualshockLeftY,   "-(in[8] - 127) << 8" },
        { Ps2Axis.DualshockRightX,  "(in[5] - 128) << 8"  },
        { Ps2Axis.DualshockRightY,  "-(in[6] - 127) << 8" },
        { Ps2Axis.NegConTwist,      "(in[5] - 128) << 8"  },
        { Ps2Axis.NegConTwistI,     "in[6]"               },
        { Ps2Axis.NegConTwistIi,    "in[7]"               },
        { Ps2Axis.NegConTwistL,     "in[8]"               },
        { Ps2Axis.GunconHSync,      "(in[6] << 8) | in[5]"},
        { Ps2Axis.GunconVSync,      "(in[8] << 8) | in[7]"},
        { Ps2Axis.JogConWheel,      "(in[6] << 8) | in[5]"},
        { Ps2Axis.GuitarWhammy,     "-(in[8] - 127) << 8" }
    };

    public static List<Ps2Axis> dualshock2Order = new List<Ps2Axis>(){
        Ps2Axis.Dualshock2RightX,
        Ps2Axis.Dualshock2RightY,
        Ps2Axis.Dualshock2LeftX,
        Ps2Axis.Dualshock2LeftY,
        Ps2Axis.Dualshock2RightButton,
        Ps2Axis.Dualshock2LeftButton,
        Ps2Axis.Dualshock2UpButton,
        Ps2Axis.Dualshock2DownButton,
        Ps2Axis.Dualshock2Triangle,
        Ps2Axis.Dualshock2Circle,
        Ps2Axis.Dualshock2Cross,
        Ps2Axis.Dualshock2Square,
        Ps2Axis.Dualshock2L1,
        Ps2Axis.Dualshock2R1,
        Ps2Axis.Dualshock2L2,
        Ps2Axis.Dualshock2R2,
    };

    public Ps2Axis Axis { get; }

    public Ps2Controller Ps2Controller { get; }

    public override StandardAxisType StandardAxis => StandardAxisMap.Ps2AxisMap[Axis];

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        if (this.Ps2Controller == Ps2Controller.Dualshock2)
        {
            return $"in[{GetAxes(bindings).IndexOf(Axis)}]";
        }
        else
        {
            return _analogMap[Axis];
        }
    }

    private static List<Ps2Axis> GetAxes(IEnumerable<Binding> bindings)
    {
        return bindings
            .Select(binding => binding as Ps2Analog)
            .Where(binding => binding != null)
            .Select(binding => binding!.Axis)
            .OrderBy(axis => dualshock2Order.IndexOf(axis))
            .ToList();
    }
    public static string GeneratePressures(IEnumerable<Binding> bindings)
    {
        var axes = GetAxes(bindings);
        var bits = String.Join("", Ps2Analog.dualshock2Order.Select(axis => axes.Contains(axis) ? 1 : 0));
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

    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Generate(bindings, xbox);
    }
}