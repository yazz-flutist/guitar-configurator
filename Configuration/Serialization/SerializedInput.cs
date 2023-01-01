using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Serialization;

[ProtoContract(SkipConstructor = true)]
[ProtoInclude(101, typeof(SerializedAnalogToDigital))]
[ProtoInclude(102, typeof(SerializedDigitalToAnalog))]
[ProtoInclude(103, typeof(SerializedDirectInput))]
[ProtoInclude(104, typeof(SerializedDjInput))]
[ProtoInclude(105, typeof(SerializedGh5NeckInput))]
[ProtoInclude(106, typeof(SerializedGhWtInput))]
[ProtoInclude(107, typeof(SerializedPs2Input))]
[ProtoInclude(108, typeof(SerializedWiiInput))]
[ProtoInclude(109, typeof(SerializedWiiInputCombined))]
[ProtoInclude(110, typeof(SerializedPs2InputCombined))]
[ProtoInclude(111, typeof(SerializedGhWtInputCombined))]
[ProtoInclude(112, typeof(SerializedGh5NeckInputCombined))]
[ProtoInclude(113, typeof(SerializedDjInputCombined))]
[ProtoInclude(114, typeof(SerializedMacroInput))]

public abstract class SerializedInput
{
    public abstract Input Generate(Microcontroller microcontroller1, ConfigViewModel model);
}