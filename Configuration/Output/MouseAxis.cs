using System.Collections.Generic;
using Avalonia.Media;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Output;

public class MouseAxis : OutputAxis
{
    
    public static Dictionary<MouseAxisType, string> mappings = new() {
        {MouseAxisType.X, "X"},
        {MouseAxisType.Y, "Y"},
        {MouseAxisType.ScrollX, "ScrollX"},
        {MouseAxisType.ScrollY, "ScrollY"},
    };
    public MouseAxis(IInput? input, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(input, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
    }
    public MouseAxisType Type { get; }
    public override string Name => Type.ToString();
    // TODO this
    public override string Image => Name;
    public override string GenerateOutput(bool xbox)
    {
        return "report->" + mappings[Type];
    }
}