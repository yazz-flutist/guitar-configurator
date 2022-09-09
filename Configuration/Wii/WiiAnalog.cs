using System;
using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Wii;

[JsonDiscriminator(nameof(WiiAnalog))]
public class WiiAnalog : GroupableAxis, IWiiInput
{
    public WiiAnalog(Microcontroller.Microcontroller controller, WiiAxis axis, WiiController wiiController, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, InputControllerType.Wii, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
        this.Axis = axis;
        this.WiiController = wiiController;
    }
    public WiiAxis Axis { get; }

    public WiiController WiiController { get; }
    public override string Input => Enum.GetName(typeof(WiiAxis), Axis)!;

    public override StandardAxisType StandardAxis => StandardAxisMap.WiiAxisMap[Axis];
    private readonly Dictionary<WiiAxis, string> _mappings = new Dictionary<WiiAxis, string>() {
        {WiiAxis.ClassicLeftStickX,         "((data[0] & 0x3f) - 32) << 9"},
        {WiiAxis.ClassicLeftStickY,         "((data[1] & 0x3f) - 32) << 9"},
        {WiiAxis.ClassicRightStickX,        "((((data[0] & 0xc0) >> 3) | ((data[1] & 0xc0) >> 5) | (data[2] >> 7)) -16) << 10"},
        {WiiAxis.ClassicRightStickY,        "((data[2] & 0x1f) - 16) << 10"},
        {WiiAxis.ClassicLeftTrigger,        "((data[3] >> 5) | ((data[2] & 0x60) >> 2))"},
        {WiiAxis.ClassicRightTrigger,       "(data[3] & 0x1f) << 3"},
        {WiiAxis.ClassicHiResLeftStickX,    "(data[0] - 0x80) << 8"},
        {WiiAxis.ClassicHiResLeftStickY,    "(data[2] - 0x80) << 8"},
        {WiiAxis.ClassicHiResRightStickX,   "(data[1] - 0x80) << 8"},
        {WiiAxis.ClassicHiResRightStickY,   "(data[3] - 0x80) << 8"},
        {WiiAxis.ClassicHiResLeftTrigger,   "data[4]"},
        {WiiAxis.ClassicHiResRightTrigger,  "data[5]"},
        {WiiAxis.DjCrossfadeSlider,         "(data[2] & 0x1E) >> 1"},
        {WiiAxis.DjEffectDial,              "(data[3] & 0xE0) >> 5 | (data[2] & 0x60) >> 2"},
        {WiiAxis.DjStickX,                  "((data[0] & 0x3F) - 0x20) << 10"},
        {WiiAxis.DjStickY,                  "((data[1] & 0x3F) - 0x20) << 10"},
        {WiiAxis.DjTurntableLeft,           "(((buf[4] & 1) ? 32 : 1) + (0x1F - (buf[3] & 0x1F))) << 10"},
        {WiiAxis.DjTurntableRight,          "(((buf[2] & 1) ? 32 : 1) + (0x1F - ((data[2] & 0x80) >> 7 | (data[1] & 0xC0) >> 5 | (data[0] & 0xC0) >> 3))) << 10"},
        {WiiAxis.DrawsomePenPressure,       "(data[4] | (data[5] & 0x0f) << 8)"},
        {WiiAxis.DrawsomePenX,              "data[0] | data[1] << 8"},
        {WiiAxis.DrawsomePenY,              "data[2] | data[3] << 8"},
        {WiiAxis.UDrawPenPressure,          "data[3]"},
        {WiiAxis.UDrawPenX,                 "((data[2] & 0x0f) << 8) | data[0]"},
        {WiiAxis.UDrawPenY,                 "((data[2] & 0xf0) << 4) | data[1]"},
        {WiiAxis.DrumGreen,                 "which == 0x12 ? velocity : {self}"},
        {WiiAxis.DrumRed,                   "which == 0x19 ? velocity : {self}"},
        {WiiAxis.DrumYellow,                "which == 0x11 ? velocity : {self}"},
        {WiiAxis.DrumBlue,                  "which == 0x0E ? velocity : {self}"},
        {WiiAxis.DrumOrange,                "which == 0x0E ? velocity : {self}"},
        {WiiAxis.DrumKickPedal,             "(which == 0x0E && ((data[2] & (1 << 7))) == 0) ? velocity : {self}"},
        {WiiAxis.DrumHiHatPedal,            "(which == 0x0E && ((data[2] & (1 << 7))) != 0) ? velocity : {self}"},
        {WiiAxis.GuitarJoystickX,           "((data[0] & 0x3f) - 32) << 10"},
        {WiiAxis.GuitarJoystickY,           "((data[1] & 0x3f) - 32) << 10"},
        {WiiAxis.GuitarTapBar,              "(data[2] & 0x1f) << 11"},
        {WiiAxis.GuitarWhammy,              "(data[3] & 0x1f) << 11"},
        {WiiAxis.NunchukAccelerationX,      "accX"},
        {WiiAxis.NunchukAccelerationY,      "accY"},
        {WiiAxis.NunchukAccelerationZ,      "accZ"},
        {WiiAxis.NunchukRotationPitch,      $"fxpt_atan2(accY,accZ)"},
        {WiiAxis.NunchukRotationRoll,       $"fxpt_atan2(accX,accZ)"}
    };

    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        return _mappings[this.Axis];
    }

    public static string GenerateDrum() {
        return @"uint8_t velocity = (7 - (data[3] >> 5)) << 5;
                    uint8_t which = (data[2] & 0b01111100) >> 1";
    }

    public static string GenerateNunchuk() {
        return @"uint16_t accX = ((data[2] << 2) | ((data[5] & 0xC0) >> 6)) - 511;
                    uint16_t accY = ((data[3] << 2) | ((data[5] & 0x30) >> 4)) - 511;
                    uint16_t accZ = ((data[4] << 2) | ((data[5] & 0xC) >> 2)) - 511;";
    }
    internal override string GenerateRaw(IEnumerable<Binding> bindings, bool xbox)
    {
        return Generate(bindings, xbox);
    }
}