using System;
using Avalonia.Media;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Types;

public enum LedOrderType
{
    RGB,
    RBG,
    GRB,
    GBR,
    BRG,
    BGR
}

public static class LedOrderTypeMethods
{
    public static byte[] GetColors(this LedOrderType type, Color color)
    {
        switch (type)
        {
            case LedOrderType.RGB:
                return new[] {color.R, color.G, color.B};
            case LedOrderType.RBG:
                return new[] {color.R, color.B, color.G};
            case LedOrderType.GRB:
                return new[] {color.G, color.R, color.B};
            case LedOrderType.GBR:
                return new[] {color.G, color.B, color.R};
            case LedOrderType.BRG:
                return new[] {color.B, color.R, color.G};
            case LedOrderType.BGR:
                return new[] {color.B, color.G, color.R};
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}