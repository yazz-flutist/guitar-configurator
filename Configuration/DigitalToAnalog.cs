using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

[JsonDiscriminator(nameof(DigitalToAnalog))]
public class DigitalToAnalog : Axis, IWiiInput, IPs2Input
{
    public DigitalToAnalog(Microcontroller.Microcontroller controller, int value, Button button, AnalogToDigitalType analogToDigitalType, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, button.InputType, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
        this.Button = button;
        this.AnalogToDigitalType = analogToDigitalType;
        this.Value = value;
    }
    public AnalogToDigitalType AnalogToDigitalType { get; set; }

    public Button Button { get; set; }
    public int Value { get; set; }
    [JsonIgnore]
    public int Pin => Button is DirectDigital ? (Button as DirectDigital)!.Pin : throw new InvalidOperationException();
    [JsonIgnore]
    public Ps2Controller Ps2Controller => Button is Ps2Button ? (Button as Ps2Button)!.Ps2Controller : throw new InvalidOperationException();
    [JsonIgnore]
    public WiiController WiiController => Button is WiiButton ? (Button as WiiButton)!.WiiController : throw new InvalidOperationException();

    public override string Input => Button.Input;

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        switch (AnalogToDigitalType)
        {
            case AnalogToDigitalType.Trigger:
            case AnalogToDigitalType.JoyHigh:
                return $"({Button.Generate(bindings, xbox)}) * {Value}";
            case AnalogToDigitalType.JoyLow:
                return $"({Button.Generate(bindings, xbox)}) * {-Value}";
        }
        return "";
    }

    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Generate(bindings, xbox);
    }
}