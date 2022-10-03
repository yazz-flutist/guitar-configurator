using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Wii;

public class WiiInput : Input
{
    public WiiInputType Input { get; }

    public WiiControllerType WiiControllerType => AxisToType[Input];

    private static readonly Dictionary<WiiInputType, WiiControllerType> AxisToType =
        new()
        {
            {WiiInputType.ClassicLeftStickX, WiiControllerType.ClassicController},
            {WiiInputType.ClassicLeftStickY, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightStickX, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightStickY, WiiControllerType.ClassicController},
            {WiiInputType.ClassicLeftTrigger, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightTrigger, WiiControllerType.ClassicController},
            {WiiInputType.DjCrossfadeSlider, WiiControllerType.DJ},
            {WiiInputType.DjEffectDial, WiiControllerType.DJ},
            {WiiInputType.DjStickX, WiiControllerType.DJ},
            {WiiInputType.DjStickY, WiiControllerType.DJ},
            {WiiInputType.DjTurntableLeft, WiiControllerType.DJ},
            {WiiInputType.DjTurntableRight, WiiControllerType.DJ},
            {WiiInputType.DrawsomePenPressure, WiiControllerType.Drawsome},
            {WiiInputType.DrawsomePenX, WiiControllerType.Drawsome},
            {WiiInputType.DrawsomePenY, WiiControllerType.Drawsome},
            {WiiInputType.UDrawPenPressure, WiiControllerType.UDraw},
            {WiiInputType.UDrawPenX, WiiControllerType.UDraw},
            {WiiInputType.UDrawPenY, WiiControllerType.UDraw},
            {WiiInputType.DrumGreenPressure, WiiControllerType.Drum},
            {WiiInputType.DrumRedPressure, WiiControllerType.Drum},
            {WiiInputType.DrumYellowPressure, WiiControllerType.Drum},
            {WiiInputType.DrumBluePressure, WiiControllerType.Drum},
            {WiiInputType.DrumOrangePressure, WiiControllerType.Drum},
            {WiiInputType.DrumKickPedalPressure, WiiControllerType.Drum},
            {WiiInputType.DrumHiHatPedalPressure, WiiControllerType.Drum},
            {WiiInputType.GuitarJoystickX, WiiControllerType.Guitar},
            {WiiInputType.GuitarJoystickY, WiiControllerType.Guitar},
            {WiiInputType.GuitarTapBar, WiiControllerType.Guitar},
            {WiiInputType.GuitarWhammy, WiiControllerType.Guitar},
            {WiiInputType.NunchukAccelerationX, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukAccelerationY, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukAccelerationZ, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukRotationPitch, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukRotationRoll, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukStickX, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukStickY, WiiControllerType.Nunchuk},
            {WiiInputType.ClassicRt, WiiControllerType.ClassicController},
            {WiiInputType.ClassicPlus, WiiControllerType.ClassicController},
            {WiiInputType.ClassicHome, WiiControllerType.ClassicController},
            {WiiInputType.ClassicMinus, WiiControllerType.ClassicController},
            {WiiInputType.ClassicLt, WiiControllerType.ClassicController},
            {WiiInputType.ClassicDPadDown, WiiControllerType.ClassicController},
            {WiiInputType.ClassicDPadRight, WiiControllerType.ClassicController},
            {WiiInputType.ClassicDPadUp, WiiControllerType.ClassicController},
            {WiiInputType.ClassicDPadLeft, WiiControllerType.ClassicController},
            {WiiInputType.ClassicZr, WiiControllerType.ClassicController},
            {WiiInputType.ClassicX, WiiControllerType.ClassicController},
            {WiiInputType.ClassicA, WiiControllerType.ClassicController},
            {WiiInputType.ClassicY, WiiControllerType.ClassicController},
            {WiiInputType.ClassicB, WiiControllerType.ClassicController},
            {WiiInputType.ClassicZl, WiiControllerType.ClassicController},
            {WiiInputType.DjHeroRightRed, WiiControllerType.DJ},
            {WiiInputType.DjHeroPlus, WiiControllerType.DJ},
            {WiiInputType.DjHeroMinus, WiiControllerType.DJ},
            {WiiInputType.DjHeroLeftRed, WiiControllerType.DJ},
            {WiiInputType.DjHeroRightBlue, WiiControllerType.DJ},
            {WiiInputType.DjHeroLeftGreen, WiiControllerType.DJ},
            {WiiInputType.DjHeroLeftAny, WiiControllerType.DJ},
            {WiiInputType.DjHeroRightAny, WiiControllerType.DJ},
            {WiiInputType.DjHeroEuphoria, WiiControllerType.DJ},
            {WiiInputType.DjHeroRightGreen, WiiControllerType.DJ},
            {WiiInputType.DjHeroLeftBlue, WiiControllerType.DJ},
            {WiiInputType.DrumPlus, WiiControllerType.Drum},
            {WiiInputType.DrumMinus, WiiControllerType.Drum},
            {WiiInputType.DrumKickPedal, WiiControllerType.Drum},
            {WiiInputType.DrumHiHatPedal, WiiControllerType.Drum},
            {WiiInputType.DrumBlue, WiiControllerType.Drum},
            {WiiInputType.DrumGreen, WiiControllerType.Drum},
            {WiiInputType.DrumYellow, WiiControllerType.Drum},
            {WiiInputType.DrumRed, WiiControllerType.Drum},
            {WiiInputType.DrumOrange, WiiControllerType.Drum},
            {WiiInputType.GuitarPlus, WiiControllerType.Guitar},
            {WiiInputType.GuitarMinus, WiiControllerType.Guitar},
            {WiiInputType.GuitarStrumDown, WiiControllerType.Guitar},
            {WiiInputType.GuitarStrumUp, WiiControllerType.Guitar},
            {WiiInputType.GuitarYellow, WiiControllerType.Guitar},
            {WiiInputType.GuitarGreen, WiiControllerType.Guitar},
            {WiiInputType.GuitarBlue, WiiControllerType.Guitar},
            {WiiInputType.GuitarRed, WiiControllerType.Guitar},
            {WiiInputType.GuitarOrange, WiiControllerType.Guitar},
            {WiiInputType.NunchukC, WiiControllerType.Nunchuk},
            {WiiInputType.NunchukZ, WiiControllerType.Nunchuk},
            {WiiInputType.TaTaConRightDrumRim, WiiControllerType.Taiko},
            {WiiInputType.TaTaConRightDrumCenter, WiiControllerType.Taiko},
            {WiiInputType.TaTaConLeftDrumRim, WiiControllerType.Taiko},
            {WiiInputType.TaTaConLeftDrumCenter, WiiControllerType.Taiko},
            {WiiInputType.UDrawPenButton1, WiiControllerType.UDraw},
            {WiiInputType.UDrawPenButton2, WiiControllerType.UDraw},
            {WiiInputType.UDrawPenClick, WiiControllerType.UDraw}
        };

    private static readonly Dictionary<WiiInputType, string> Mappings = new()
    {
        {WiiInputType.ClassicLeftStickX, "((data[0] & 0x3f) - 32) << 9"},
        {WiiInputType.ClassicLeftStickY, "((data[1] & 0x3f) - 32) << 9"},
        {
            WiiInputType.ClassicRightStickX,
            "((((data[0] & 0xc0) >> 3) | ((data[1] & 0xc0) >> 5) | (data[2] >> 7)) -16) << 10"
        },
        {WiiInputType.ClassicRightStickY, "((data[2] & 0x1f) - 16) << 10"},
        {WiiInputType.ClassicLeftTrigger, "((data[3] >> 5) | ((data[2] & 0x60) >> 2))"},
        {WiiInputType.ClassicRightTrigger, "(data[3] & 0x1f) << 3"},
        {WiiInputType.DjCrossfadeSlider, "(data[2] & 0x1E) >> 1"},
        {WiiInputType.DjEffectDial, "(data[3] & 0xE0) >> 5 | (data[2] & 0x60) >> 2"},
        {WiiInputType.DjStickX, "((data[0] & 0x3F) - 0x20) << 10"},
        {WiiInputType.DjStickY, "((data[1] & 0x3F) - 0x20) << 10"},
        {WiiInputType.DjTurntableLeft, "(((data[4] & 1) ? 32 : 1) + (0x1F - (data[3] & 0x1F))) << 10"},
        {
            WiiInputType.DjTurntableRight,
            "(((data[2] & 1) ? 32 : 1) + (0x1F - ((data[2] & 0x80) >> 7 | (data[1] & 0xC0) >> 5 | (data[0] & 0xC0) >> 3))) << 10"
        },
        {WiiInputType.DrawsomePenPressure, "(data[4] | (data[5] & 0x0f) << 8)"},
        {WiiInputType.DrawsomePenX, "data[0] | data[1] << 8"},
        {WiiInputType.DrawsomePenY, "data[2] | data[3] << 8"},
        {WiiInputType.UDrawPenPressure, "data[3]"},
        {WiiInputType.UDrawPenX, "((data[2] & 0x0f) << 8) | data[0]"},
        {WiiInputType.UDrawPenY, "((data[2] & 0xf0) << 4) | data[1]"},
        {WiiInputType.DrumGreenPressure, "drumVelocity[DRUM_GREEN]"},
        {WiiInputType.DrumRedPressure, "drumVelocity[DRUM_RED]"},
        {WiiInputType.DrumYellowPressure, "drumVelocity[DRUM_YELLOW]"},
        {WiiInputType.DrumBluePressure, "drumVelocity[DRUM_BLUE]"},
        {WiiInputType.DrumOrangePressure, "drumVelocity[DRUM_ORANGE]"},
        {WiiInputType.DrumKickPedalPressure, "drumVelocity[DRUM_KICK]"},
        {WiiInputType.DrumHiHatPedalPressure, "drumVelocity[DRUM_HIHAT]"},
        {WiiInputType.GuitarJoystickX, "((data[0] & 0x3f) - 32) << 10"},
        {WiiInputType.GuitarJoystickY, "((data[1] & 0x3f) - 32) << 10"},
        {WiiInputType.GuitarTapBar, "(data[2] & 0x1f) << 11"},
        {WiiInputType.GuitarWhammy, "(data[3] & 0x1f) << 11"},
        {WiiInputType.NunchukAccelerationX, "accX"},
        {WiiInputType.NunchukAccelerationY, "accY"},
        {WiiInputType.NunchukAccelerationZ, "accZ"},
        {WiiInputType.NunchukRotationPitch, $"fxpt_atan2(accY,accZ)"},
        {WiiInputType.NunchukRotationRoll, $"fxpt_atan2(accX,accZ)"},
        {WiiInputType.NunchukStickX, "(data[0] - 0x80) << 8"},
        {WiiInputType.NunchukStickY, "(data[1] - 0x80) << 8"},
        {WiiInputType.ClassicRt, "((~data[4]) & (1 << 1))"},
        {WiiInputType.ClassicPlus, "((~data[4]) & (1 << 2))"},
        {WiiInputType.ClassicHome, "((~data[4]) & (1 << 3))"},
        {WiiInputType.ClassicMinus, "((~data[4]) & (1 << 4))"},
        {WiiInputType.ClassicLt, "((~data[4]) & (1 << 1))"},
        {WiiInputType.ClassicDPadDown, "((~data[4]) & (1 << 6))"},
        {WiiInputType.ClassicDPadRight, "((~data[4]) & (1 << 7))"},
        {WiiInputType.ClassicDPadUp, "((~data[5]) & (1 << 0))"},
        {WiiInputType.ClassicDPadLeft, "((~data[5]) & (1 << 1))"},
        {WiiInputType.ClassicZr, "((~data[5]) & (1 << 2))"},
        {WiiInputType.ClassicX, "((~data[5]) & (1 << 3))"},
        {WiiInputType.ClassicA, "((~data[5]) & (1 << 4))"},
        {WiiInputType.ClassicY, "((~data[5]) & (1 << 5))"},
        {WiiInputType.ClassicB, "((~data[5]) & (1 << 6))"},
        {WiiInputType.ClassicZl, "((~data[5]) & (1 << 7))"},
        {WiiInputType.DjHeroPlus, "((~data[4]) & (1 << 2))"},
        {WiiInputType.DjHeroMinus, "((~data[4]) & (1 << 4))"},
        {WiiInputType.DjHeroLeftBlue, "((~data[5]) & (1 << 7))"},
        {WiiInputType.DjHeroLeftRed, "((~data[4]) & (1 << 5))"},
        {WiiInputType.DjHeroLeftGreen, "((~data[5]) & (1 << 3))"},
        {WiiInputType.DjHeroLeftAny, "(((~data[5]) & ((1 << 3)|1 << 7)) | ((~data[4]) & (1 << 5)))"},
        {WiiInputType.DjHeroRightGreen, "((~data[5]) & (1 << 5))"},
        {WiiInputType.DjHeroRightRed, "((~data[4]) & (1 << 1))"},
        {WiiInputType.DjHeroRightBlue, "((~data[5]) & (1 << 2))"},
        {WiiInputType.DjHeroRightAny, "(((~data[5]) & ((1 << 5)|1 << 2)) | ((~data[4]) & (1 << 1)))"},
        {WiiInputType.DjHeroEuphoria, "((~data[5]) & (1 << 4))"},
        {WiiInputType.DrumPlus, "((~data[4]) & (1 << 2))"},
        {WiiInputType.DrumMinus, "((~data[4]) & (1 << 4))"},
        {WiiInputType.DrumKickPedal, "((~data[5]) & (1 << 2))"},
        // TODO
        {WiiInputType.DrumHiHatPedal, "drumVelocity[DRUM_HIHAT]"},
        {WiiInputType.DrumBlue, "((~data[5]) & (1 << 3))"},
        {WiiInputType.DrumGreen, "((~data[5]) & (1 << 4))"},
        {WiiInputType.DrumYellow, "((~data[5]) & (1 << 5))"},
        {WiiInputType.DrumRed, "((~data[5]) & (1 << 6))"},
        {WiiInputType.DrumOrange, "((~data[5]) & (1 << 7))"},
        {WiiInputType.GuitarPlus, "((~data[4]) & (1 << 2))"},
        {WiiInputType.GuitarMinus, "((~data[4]) & (1 << 4))"},
        {WiiInputType.GuitarStrumDown, "((~data[4]) & (1 << 6))"},
        {WiiInputType.GuitarStrumUp, "((~data[5]) & (1 << 0))"},
        {WiiInputType.GuitarYellow, "((~data[5]) & (1 << 3))"},
        {WiiInputType.GuitarGreen, "((~data[5]) & (1 << 4))"},
        {WiiInputType.GuitarBlue, "((~data[5]) & (1 << 5))"},
        {WiiInputType.GuitarRed, "((~data[5]) & (1 << 6))"},
        {WiiInputType.GuitarOrange, "((~data[5]) & (1 << 7))"},
        {WiiInputType.NunchukC, "((~data[5]) & (1 << 1))"},
        {WiiInputType.NunchukZ, "((~data[5]) & (1 << 0))"},
        {WiiInputType.TaTaConRightDrumRim, "((~data[0]) & (1 << 3))"},
        {WiiInputType.TaTaConRightDrumCenter, "((~data[0]) & (1 << 4))"},
        {WiiInputType.TaTaConLeftDrumRim, "((~data[0]) & (1 << 5))"},
        {WiiInputType.TaTaConLeftDrumCenter, "((~data[0]) & (1 << 6))"},
        {WiiInputType.UDrawPenButton1, "((~data[5]) & (1 << 0))"},
        {WiiInputType.UDrawPenButton2, "((~data[5]) & (1 << 1))"},
        {WiiInputType.UDrawPenClick, "(( data[5]) & (1 << 2))"}
    };

    private static readonly List<string> HiResMapOrder = new()
    {
        "data[4]",
        "data[5]",
        Mappings[WiiInputType.ClassicLeftStickX],
        Mappings[WiiInputType.ClassicLeftStickY], 
        Mappings[WiiInputType.ClassicRightStickX],
        Mappings[WiiInputType.ClassicRightStickY], 
        Mappings[WiiInputType.ClassicLeftTrigger],
        Mappings[WiiInputType.ClassicRightTrigger],
    };
    private static readonly Dictionary<string,string> HiResMap = new()
    {
        {"data[4]", "data[6]"},
        {"data[5]", "data[7]"},
        {Mappings[WiiInputType.ClassicLeftStickX], "(data[0] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicLeftStickY], "(data[2] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicRightStickX], "(data[1] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicRightStickY], "(data[3] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicLeftTrigger], "data[4]"},
        {Mappings[WiiInputType.ClassicRightTrigger], "data[5]"},
    };

    private static readonly Dictionary<WiiControllerType, string> CType = new() {
        {WiiControllerType.ClassicController, "WII_CLASSIC_CONTROLLER"},
        {WiiControllerType.Nunchuk, "WII_NUNCHUK"},
        {WiiControllerType.UDraw, "WII_THQ_UDRAW_TABLET"},
        {WiiControllerType.Drawsome, "WII_UBISOFT_DRAWSOME_TABLET"},
        {WiiControllerType.Guitar, "WII_GUITAR_HERO_GUITAR_CONTROLLER"},
        {WiiControllerType.Drum, "WII_GUITAR_HERO_DRUM_CONTROLLER"},
        {WiiControllerType.DJ, "WII_DJ_HERO_TURNTABLE"},
        {WiiControllerType.Taiko, "WII_TAIKO_NO_TATSUJIN_CONTROLLER"},
        {WiiControllerType.MotionPlus, "WII_MOTION_PLUS"}
    };

    public WiiInput(WiiInputType input)
    {
        Input = input;
    }

    public override string Generate()
    {
        return Mappings[Input];
    }

    public override bool IsAnalog => Input <= WiiInputType.DrawsomePenPressure;

    public override string GenerateAll(bool xbox, List<Tuple<Input, string>> bindings,
        Microcontroller.Microcontroller controller)
    {
        Dictionary<WiiControllerType, List<string>> mappedBindings = new();
        foreach (var binding in bindings)
        {
            if (binding.Item1 is WiiInput input)
            {
                if (!mappedBindings.ContainsKey(input.WiiControllerType))
                {
                    mappedBindings.Add(input.WiiControllerType, new List<string>());
                }
                mappedBindings[input.WiiControllerType].Add(binding.Item2);
            }
        }

        var ret = "";
        if (mappedBindings.ContainsKey(WiiControllerType.ClassicController))
        {
            var mappings = mappedBindings[WiiControllerType.ClassicController];
            mappedBindings.Remove(WiiControllerType.ClassicController);
            var mappings2 = new List<string>();
            foreach (var mapping in mappings)
            {
                var val = mapping;
                foreach (var key in HiResMapOrder)
                {
                    val = mapping.Replace(key, HiResMap[key]);
                }
                mappings2.Add(val);
            }
            ret += @$"case WII_CLASSIC_CONTROLLER:
case WII_CLASSIC_CONTROLLER_PRO:
if (hiRes) {{
    {String.Join(";\n",mappings2)};
}} else {{
    {String.Join(";\n",mappings)};
}}
";
        }
        foreach (var binding in mappedBindings)
        {
            var input = binding.Key;
            var mappings = binding.Value;
            ret += @$"case {CType[input]}:
    {String.Join(";\n",mappings)};
    break;";
        }
        return ret;
    }

    public override IReadOnlyList<string> RequiredDefines()
    {
        if (WiiControllerType == WiiControllerType.Drum)
        {
            return new[] {"INPUT_WII", "INPUT_WII_DRUM"};
        }
        if (WiiControllerType == WiiControllerType.Nunchuk)
        {
            return new[] {"INPUT_WII", "INPUT_WII_NUNCHUK"};
        }
        if (WiiControllerType == WiiControllerType.ClassicController)
        {
            return new[] {"INPUT_WII", "INPUT_WII_CLASSIC"};
        }
        return new[] {"INPUT_WII"};
    }
}