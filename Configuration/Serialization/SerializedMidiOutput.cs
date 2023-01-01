using Avalonia.Media;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Outputs;
using GuitarConfigurator.NetCore.Configuration.Types;
using GuitarConfigurator.NetCore.ViewModels;
using ProtoBuf;

namespace GuitarConfigurator.NetCore.Configuration.Serialization;

[ProtoContract(SkipConstructor = true)]
public class SerializedMidiOutput : SerializedOutput
{
    [ProtoMember(1)] public override SerializedInput? Input { get; }
    [ProtoMember(2)] public override uint LedOn { get; }
    [ProtoMember(3)] public override uint LedOff { get; }
    [ProtoMember(4)] public int Min { get; }
    [ProtoMember(5)] public int Max { get; }
    [ProtoMember(6)] public int Deadzone { get; }
    [ProtoMember(8)] public override byte[] LedIndex { get; }
    [ProtoMember(9)] public MidiType MidiType { get; }
    [ProtoMember(10)] public int Pitch { get; }
    [ProtoMember(11)] public int Command { get; }
    [ProtoMember(12)] public int CC { get; }
    [ProtoMember(13)] public int Channel { get; }

    public SerializedMidiOutput(SerializedInput? input, Color ledOn, Color ledOff, int min, int max, int deadzone,byte[] ledIndex, MidiType midiType, int pitch, int command, int cc, int channel)
    {
        Input = input;
        LedOn = ledOn.ToUint32();
        LedOff = ledOff.ToUint32();
        Min = min;
        Max = max;
        Deadzone = deadzone;
        LedIndex = ledIndex;
        MidiType = midiType;
        Pitch = pitch;
        Command = command;
        CC = cc;
        Channel = channel;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new MidiOutput(model, Input?.Generate(microcontroller, model), Color.FromUInt32(LedOn), Color.FromUInt32(LedOff), LedIndex, Min, Max, Deadzone, MidiType, Pitch, Command, CC, Channel);
    }
}