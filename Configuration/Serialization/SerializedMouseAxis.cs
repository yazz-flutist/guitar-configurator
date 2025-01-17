using Avalonia.Media;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Outputs;
using GuitarConfigurator.NetCore.Configuration.Types;
using GuitarConfigurator.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfigurator.NetCore.Configuration.Serialization;

[ProtoContract(SkipConstructor = true)]
public class SerializedMouseAxis : SerializedOutput
{
    [ProtoMember(1)] public override SerializedInput? Input { get; }
    [ProtoMember(2)] public override uint LedOn { get; }
    [ProtoMember(3)] public override uint LedOff { get; }
    [ProtoMember(7)] public override byte[] LedIndex { get; }
    [ProtoMember(4)] public int Min { get; }
    [ProtoMember(5)] public int Max { get; }
    [ProtoMember(6)] public int Deadzone { get; }

    public MouseAxisType Type { get; }

    public SerializedMouseAxis(SerializedInput? input, MouseAxisType type, Color ledOn, Color ledOff, byte[] ledIndex, int min, int max,
        int deadzone)
    {
        Input = input;
        LedOn = ledOn.ToUint32();
        LedOff = ledOff.ToUint32();
        Min = min;
        Max = max;
        Deadzone = deadzone;
        Type = type;
        LedIndex = ledIndex;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new MouseAxis(model, Input?.Generate(microcontroller, model), Color.FromUInt32(LedOn), Color.FromUInt32(LedOff), LedIndex, Min, Max, Deadzone,
            Type);
    }
}