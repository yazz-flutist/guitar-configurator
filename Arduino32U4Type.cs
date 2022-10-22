using System.ComponentModel;

namespace GuitarConfiguratorSharp.NetCore;

public enum Arduino32U4Type
{
    [Description("Sparkfun Pro Micro")]
    ProMicro,
    [Description("Arduino Leonardo")]
    Leonardo,
    [Description("Arduino Micro")]
    Micro
}


public enum MegaType
{
    [Description("Arduino Mega ADK")]
    MegaAdk,
    [Description("Arduino Mega")]
    Mega
}

public enum DeviceInputType
{
    [Description("Directly Wired")]
    Direct,
    [Description("Wii Adapter")]
    Wii,
    [Description("PS2 Adapter")]
    Ps2
}