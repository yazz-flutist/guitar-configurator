using System.ComponentModel;

namespace GuitarConfigurator.NetCore.Configuration.DJ;

public enum DjInputType
{
    [Description("Left Turntable Spin")]
    LeftTurntable,
    [Description("Right Turntable Spin")]
    RightTurntable,
    [Description("Right Turntable Green Fret")]
    LeftGreen,
    [Description("Right Turntable Red Fret")]
    LeftRed,
    [Description("Right Turntable Blue Fret")]
    LeftBlue,
    [Description("Left Turntable Green Fret")]
    RightGreen,
    [Description("Left Turntable Red Fret")]
    RightRed,
    [Description("Left Turntable Blue Fret")]
    RightBlue,
}