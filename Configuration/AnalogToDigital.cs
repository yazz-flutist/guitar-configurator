using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

[JsonDiscriminator(nameof(AnalogToDigital))]
public class AnalogToDigital : Button, IWiiInput, IPs2Input
{
    public AnalogToDigital(Microcontroller.Microcontroller controller, int threshold, Axis analog, AnalogToDigitalType analogToDigitalType, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, analog.InputType, debounce, type, ledOn, ledOff)
    {
        this.Analog = analog;
        this.AnalogToDigitalType = analogToDigitalType;
        this.Threshold = threshold;
    }
    public AnalogToDigitalType AnalogToDigitalType { get; set; }
    public Axis Analog { get; set; }
    public override string Input => Analog.Input;

    public int Threshold { get; }

    [JsonIgnore]
    public int Pin => Analog is DirectAnalog analog ? analog.Pin : throw new InvalidOperationException();
    [JsonIgnore]
    public Ps2Controller Ps2Controller => Analog is Ps2Analog ? (Analog as Ps2Analog)!.Ps2Controller : throw new InvalidOperationException();
    [JsonIgnore]
    public WiiController WiiController => Analog is WiiAnalog ? (Analog as WiiAnalog)!.WiiController : throw new InvalidOperationException();

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        switch (AnalogToDigitalType)
        {
            case AnalogToDigitalType.Trigger:
                return $"{Analog.GenerateRaw(bindings, xbox)} > {(int)((Threshold + (Analog.Offset * Analog.Multiplier)) * 2)}";
            case AnalogToDigitalType.JoyHigh:
                return $"{Analog.GenerateRaw(bindings, xbox)} > {(int)(Threshold + 128 + (Analog.Offset * Analog.Multiplier))}";
            case AnalogToDigitalType.JoyLow:
                return $"{Analog.GenerateRaw(bindings, xbox)} < {(int)((-Threshold) + 128 + (Analog.Offset * Analog.Multiplier))}";
        }
        return "";
    }
}