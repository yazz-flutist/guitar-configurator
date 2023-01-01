using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Neck;
using GuitarConfigurator.NetCore.Configuration.Outputs;
using GuitarConfigurator.NetCore.Configuration.Outputs.Combined;
using GuitarConfigurator.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfigurator.NetCore.Configuration.Serialization;

[ProtoContract(SkipConstructor = true)]
public class SerializedGhwtCombinedOutput : SerializedOutput
{
    [ProtoMember(1)] public override SerializedInput? Input => null;
    [ProtoMember(4)] public int Pin { get; }

    [ProtoMember(5)] public List<SerializedOutput> Outputs { get; }
    public override uint LedOn => Colors.Transparent.ToUint32();
    public override uint LedOff => Colors.Transparent.ToUint32();
    public override byte[] LedIndex => Array.Empty<byte>();

    public SerializedGhwtCombinedOutput(int pin, List<Output> outputs)
    {
        Pin = pin;
        Outputs = outputs.Select(s => s.Serialize()).ToList();
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        microcontroller.AssignPin(new DirectPinConfig(model, GhWtTapInput.GhWtTapPinType, Pin, DevicePinMode.Floating));
        return new GhwtCombinedOutput(model, microcontroller, Pin, Outputs.Select(s => s.Generate(model, microcontroller)).ToList());
    }
}