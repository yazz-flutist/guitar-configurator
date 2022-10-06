using Avalonia.Media;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Serialization;

[ProtoContract(SkipConstructor = true)]
public class SerializedControllerAxis : SerializedOutput
{
    [ProtoMember(1)] public override SerializedInput? Input { get; }
    [ProtoMember(2)] public override Color LedOn { get; }
    [ProtoMember(3)] public override Color LedOff { get; }
    [ProtoMember(4)] public float Multiplier { get; }
    [ProtoMember(5)] public int Offset { get; }
    [ProtoMember(6)] public int Deadzone { get; }

    [ProtoMember(7)] public StandardAxisType Type { get; }

    public SerializedControllerAxis(SerializedInput? input, StandardAxisType type, Color ledOn, Color ledOff, float multiplier,
        int offset, int deadzone)
    {
        Input = input;
        LedOn = ledOn;
        LedOff = ledOff;
        Multiplier = multiplier;
        Offset = offset;
        Deadzone = deadzone;
        Type = type;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new ControllerAxis(model, Input?.Generate(microcontroller), LedOn, LedOff, Multiplier, Offset, Deadzone,
            Type);
    }
}