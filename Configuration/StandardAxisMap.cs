using System.Collections.Generic;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;

namespace GuitarConfiguratorSharp.NetCore.Configuration
{
    public static class StandardAxisMap
    {

        public static readonly Dictionary<WiiAxis, StandardAxisType> WiiAxisMap = new Dictionary<WiiAxis, StandardAxisType>() {
            {WiiAxis.ClassicLeftStickX, StandardAxisType.LeftStickX}
        };
        public static readonly Dictionary<Ps2Axis, StandardAxisType> Ps2AxisMap = new Dictionary<Ps2Axis, StandardAxisType>() {
            {Ps2Axis.DualshockLeftX, StandardAxisType.LeftStickX}
        };
    }

    // Have two modes, one where people just pick a standard axis when mapping ps2 stuff
    // And an advanced mode which exposes all of the below for mapping
    // Note that if someone is using one of these, then we also need to enable the pressures for it.
}