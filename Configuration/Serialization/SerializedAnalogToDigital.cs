using GuitarConfigurator.NetCore.Configuration.Conversions;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfigurator.NetCore.Configuration.Serialization;
[ProtoContract(SkipConstructor = true)]
public class SerializedAnalogToDigital : SerializedInput
{
    [ProtoMember( 1)] public SerializedInput Child { get; }
    [ProtoMember( 2)] public AnalogToDigitalType Type { get; }
    [ProtoMember( 3)] public int Threshold { get; }

    public SerializedAnalogToDigital(SerializedInput child, AnalogToDigitalType type, int threshold)
    {
        Child = child;
        Type = type;
        Threshold = threshold;
    }

    public override Input Generate(Microcontroller microcontroller, ConfigViewModel model)
    {
        return new AnalogToDigital(Child.Generate(microcontroller, model), Type, Threshold, model);
    }
}