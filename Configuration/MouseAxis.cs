using System.Collections.Generic;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

[JsonDiscriminator(nameof(MouseAxis))]
public class MouseAxis : IOutputAxis
{
    public StandardAxisType Type { get; }

    public static Dictionary<StandardAxisType, string> mappings = new Dictionary<StandardAxisType, string>() {
        {StandardAxisType.MouseX, "X"},
        {StandardAxisType.MouseY, "Y"},
        {StandardAxisType.ScrollX, "ScrollX"},
        {StandardAxisType.ScrollY, "ScrollY"},
    };
    public OutputType OutputType => OutputType.Keyboard;

    public string Generate(bool xbox)
    {
        return "report->" + mappings[Type];
    }

}