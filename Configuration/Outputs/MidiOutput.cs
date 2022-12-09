using System;
using System.Reactive.Linq;
using Avalonia.Media;
using GuitarConfiguratorSharp.NetCore.Configuration.Serialization;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Outputs;

public class MidiOutput : OutputAxis
{
    public MidiOutput(ConfigViewModel model, Input? input, Color ledOn, Color ledOff, byte[] ledIndices, int min, int max, int deadZone, MidiType midiType, int pitch, int command, int cc, int channel) :
        base(model, input, ledOn, ledOff, ledIndices, min, max, deadZone, "midi", _=>true)
    {
        MidiType = midiType;
        Pitch = pitch;
        Command = command;
        CC = cc;
        Channel = channel;
        _value = this
            .WhenAnyValue(x => x.Value).Select(s => s >> 9).ToProperty(this, x => x.ValueMidi);
    }

    private static readonly int VirtualCable = 0;
    public int Channel { get; }
    public int CC { get; }
    public int Command { get; }
    public int Pitch { get; }
    public MidiType MidiType { get; }
    
    private readonly ObservableAsPropertyHelper<int> _value;
    public int ValueMidi => _value.Value;
    protected override string GenerateOutput(bool xbox)
    {
        throw new NotImplementedException();
    }

    public override bool IsCombined => false;

    public override bool IsStrum => false;

    public override SerializedOutput Serialize()
    {
        return new SerializedMidiOutput(Input?.Serialise(), LedOn, LedOff, Min, Max, DeadZone, LedIndices, MidiType, Pitch,
            Command, CC, Channel);
    }

    private int GenerateEvent()
    {
        return ((VirtualCable) << 4) | ((Command) >> 4);
    }

    private int GenerateChannel()
    {
        return Channel - 1;
    }

    protected override string MinCalibrationText()
    {
        return "Move the axis to the minimum position";
    }

    protected override string MaxCalibrationText()
    {
        return "Move the axis to the maximum position";
    }

    protected override bool SupportsCalibration()
    {
        return true;
    }

    public override bool IsKeyboard => false;
    public override bool IsController => false;
    public override bool IsMidi => true;
    public override void UpdateBindings()
    {
    }
}