using System.Collections.Generic;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;

namespace GuitarConfiguratorSharp.NetCore.Configuration
{
    public class StandardButtonMap
    {

        public static readonly Dictionary<Ps2ButtonType, StandardButtonType> Ps2ButtonMap = new Dictionary<Ps2ButtonType, StandardButtonType>() {
            {Ps2ButtonType.GuitarGreen, StandardButtonType.A}
        };
        public static readonly Dictionary<WiiButtonType, StandardButtonType> WiiButtonMap = new Dictionary<WiiButtonType, StandardButtonType>() {
            {WiiButtonType.GuitarGreen, StandardButtonType.A}
        };
    }
}