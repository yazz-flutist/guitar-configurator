using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.Configuration.Serialization;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ReactiveUI;
using static GuitarConfiguratorSharp.NetCore.Configuration.Outputs.Combined.WiiCombinedOutput;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Wii;

public class WiiInput : TwiInput
{
    public static readonly string WiiTwiType = "wii";
    public static readonly int WiiTwiFreq = 400000;
    public WiiInputType Input { get; }

    public WiiControllerType WiiControllerType => AxisToType[Input];

    public Bitmap? Image { get; }

    private static readonly Dictionary<WiiInputType, WiiControllerType> AxisToType =
        new()
        {
            {WiiInputType.ClassicLeftStickX, WiiControllerType.ClassicController},
            {WiiInputType.ClassicLeftStickY, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightStickX, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightStickY, WiiControllerType.ClassicController},
            {WiiInputType.ClassicLeftTrigger, WiiControllerType.ClassicController},
            {WiiInputType.ClassicRightTrigger, WiiControllerType.ClassicController},
            {WiiInputType.DjCrossfadeSlider, WiiControllerType.Dj},
            {WiiInputType.DjEffectDial, WiiControllerType.Dj},
            {WiiInputType.DjStickX, WiiControllerType.Dj},
            {WiiInputType.DjStickY, WiiControllerType.Dj},
            {WiiInputType.DjTurntableLeft, WiiControllerType.Dj},
            {WiiInputType.DjTurntableRight, WiiControllerType.Dj},
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
            {WiiInputType.DjHeroRightRed, WiiControllerType.Dj},
            {WiiInputType.DjHeroPlus, WiiControllerType.Dj},
            {WiiInputType.DjHeroMinus, WiiControllerType.Dj},
            {WiiInputType.DjHeroLeftRed, WiiControllerType.Dj},
            {WiiInputType.DjHeroRightBlue, WiiControllerType.Dj},
            {WiiInputType.DjHeroLeftGreen, WiiControllerType.Dj},
            {WiiInputType.DjHeroLeftAny, WiiControllerType.Dj},
            {WiiInputType.DjHeroRightAny, WiiControllerType.Dj},
            {WiiInputType.DjHeroEuphoria, WiiControllerType.Dj},
            {WiiInputType.DjHeroRightGreen, WiiControllerType.Dj},
            {WiiInputType.DjHeroLeftBlue, WiiControllerType.Dj},
            {WiiInputType.DrumPlus, WiiControllerType.Drum},
            {WiiInputType.DrumMinus, WiiControllerType.Drum},
            {WiiInputType.DrumKickPedal, WiiControllerType.Drum},
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
            {WiiInputType.GuitarTapYellow, WiiControllerType.Guitar},
            {WiiInputType.GuitarTapGreen, WiiControllerType.Guitar},
            {WiiInputType.GuitarTapBlue, WiiControllerType.Guitar},
            {WiiInputType.GuitarTapRed, WiiControllerType.Guitar},
            {WiiInputType.GuitarTapOrange, WiiControllerType.Guitar},
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
        {WiiInputType.ClassicLeftStickX, "((wiiData[0] & 0x3f) - 32) << 9"},
        {WiiInputType.ClassicLeftStickY, "((wiiData[1] & 0x3f) - 32) << 9"},
        {
            WiiInputType.ClassicRightStickX,
            "((((wiiData[0] & 0xc0) >> 3) | ((wiiData[1] & 0xc0) >> 5) | (wiiData[2] >> 7)) -16) << 10"
        },
        {WiiInputType.ClassicRightStickY, "((wiiData[2] & 0x1f) - 16) << 10"},
        {WiiInputType.ClassicLeftTrigger, "((wiiData[3] >> 5) | ((wiiData[2] & 0x60) >> 2))"},
        {WiiInputType.ClassicRightTrigger, "(wiiData[3] & 0x1f) << 3"},
        {WiiInputType.DjCrossfadeSlider, "(wiiData[2] & 0x1E) >> 1"},
        {WiiInputType.DjEffectDial, "(wiiData[3] & 0xE0) >> 5 | (wiiData[2] & 0x60) >> 2"},
        {WiiInputType.DjStickX, "((wiiData[0] & 0x3F) - 0x20) << 10"},
        {WiiInputType.DjStickY, "((wiiData[1] & 0x3F) - 0x20) << 10"},
        {WiiInputType.DjTurntableLeft, "(((wiiButtonsLow & 1) ? 32 : 1) + (0x1F - (wiiData[3] & 0x1F))) << 10"},
        {
            WiiInputType.DjTurntableRight,
            "(((wiiData[2] & 1) ? 32 : 1) + (0x1F - ((wiiData[2] & 0x80) >> 7 | (wiiData[1] & 0xC0) >> 5 | (wiiData[0] & 0xC0) >> 3))) << 10"
        },
        {WiiInputType.DrawsomePenPressure, "(wiiButtonsLow | (wiiButtonsHigh & 0x0f) << 8)"},
        {WiiInputType.DrawsomePenX, "wiiData[0] | wiiData[1] << 8"},
        {WiiInputType.DrawsomePenY, "wiiData[2] | wiiData[3] << 8"},
        {WiiInputType.UDrawPenPressure, "wiiData[3]"},
        {WiiInputType.UDrawPenX, "((wiiData[2] & 0x0f) << 8) | wiiData[0]"},
        {WiiInputType.UDrawPenY, "((wiiData[2] & 0xf0) << 4) | wiiData[1]"},
        {WiiInputType.DrumGreenPressure, "drumVelocity[DRUM_GREEN]"},
        {WiiInputType.DrumRedPressure, "drumVelocity[DRUM_RED]"},
        {WiiInputType.DrumYellowPressure, "drumVelocity[DRUM_YELLOW]"},
        {WiiInputType.DrumBluePressure, "drumVelocity[DRUM_BLUE]"},
        {WiiInputType.DrumOrangePressure, "drumVelocity[DRUM_ORANGE]"},
        {WiiInputType.DrumKickPedalPressure, "drumVelocity[DRUM_KICK]"},
        {WiiInputType.GuitarJoystickX, "((wiiData[0] & 0x3f) - 32) << 10"},
        {WiiInputType.GuitarJoystickY, "((wiiData[1] & 0x3f) - 32) << 10"},
        {WiiInputType.GuitarTapBar, "(wiiData[2] & 0x1f) << 11"},
        {WiiInputType.GuitarWhammy, "(wiiData[3] & 0x1f) << 11"},
        {WiiInputType.NunchukAccelerationX, "accX"},
        {WiiInputType.NunchukAccelerationY, "accY"},
        {WiiInputType.NunchukAccelerationZ, "accZ"},
        {WiiInputType.NunchukRotationPitch, "fxpt_atan2(accY,accZ)"},
        {WiiInputType.NunchukRotationRoll, "fxpt_atan2(accX,accZ)"},
        {WiiInputType.NunchukStickX, "(wiiData[0] - 0x80) << 8"},
        {WiiInputType.NunchukStickY, "(wiiData[1] - 0x80) << 8"},
        {WiiInputType.ClassicRt, "((wiiButtonsLow) & (1 << 1))"},
        {WiiInputType.ClassicPlus, "((wiiButtonsLow) & (1 << 2))"},
        {WiiInputType.ClassicHome, "((wiiButtonsLow) & (1 << 3))"},
        {WiiInputType.ClassicMinus, "((wiiButtonsLow) & (1 << 4))"},
        {WiiInputType.ClassicLt, "((wiiButtonsLow) & (1 << 5))"},
        {WiiInputType.ClassicDPadDown, "((wiiButtonsLow) & (1 << 6))"},
        {WiiInputType.ClassicDPadRight, "((wiiButtonsLow) & (1 << 7))"},
        {WiiInputType.ClassicDPadUp, "((wiiButtonsHigh) & (1 << 0))"},
        {WiiInputType.ClassicDPadLeft, "((wiiButtonsHigh) & (1 << 1))"},
        {WiiInputType.ClassicZr, "((wiiButtonsHigh) & (1 << 2))"},
        {WiiInputType.ClassicX, "((wiiButtonsHigh) & (1 << 3))"},
        {WiiInputType.ClassicA, "((wiiButtonsHigh) & (1 << 4))"},
        {WiiInputType.ClassicY, "((wiiButtonsHigh) & (1 << 5))"},
        {WiiInputType.ClassicB, "((wiiButtonsHigh) & (1 << 6))"},
        {WiiInputType.ClassicZl, "((wiiButtonsHigh) & (1 << 7))"},
        {WiiInputType.DjHeroPlus, "((wiiButtonsLow) & (1 << 2))"},
        {WiiInputType.DjHeroMinus, "((wiiButtonsLow) & (1 << 4))"},
        {WiiInputType.DjHeroLeftBlue, "((wiiButtonsHigh) & (1 << 7))"},
        {WiiInputType.DjHeroLeftRed, "((wiiButtonsLow) & (1 << 5))"},
        {WiiInputType.DjHeroLeftGreen, "((wiiButtonsHigh) & (1 << 3))"},
        {WiiInputType.DjHeroLeftAny, "(((wiiButtonsHigh) & ((1 << 3)|1 << 7)) | ((wiiButtonsLow) & (1 << 5)))"},
        {WiiInputType.DjHeroRightGreen, "((wiiButtonsHigh) & (1 << 5))"},
        {WiiInputType.DjHeroRightRed, "((wiiButtonsLow) & (1 << 1))"},
        {WiiInputType.DjHeroRightBlue, "((wiiButtonsHigh) & (1 << 2))"},
        {WiiInputType.DjHeroRightAny, "(((wiiButtonsHigh) & ((1 << 5)|1 << 2)) | ((wiiButtonsLow) & (1 << 1)))"},
        {WiiInputType.DjHeroEuphoria, "((wiiButtonsHigh) & (1 << 4))"},
        {WiiInputType.DrumPlus, "((wiiButtonsLow) & (1 << 2))"},
        {WiiInputType.DrumMinus, "((wiiButtonsLow) & (1 << 4))"},
        {WiiInputType.DrumKickPedal, "((wiiButtonsHigh) & (1 << 2))"},
        {WiiInputType.DrumBlue, "((wiiButtonsHigh) & (1 << 3))"},
        {WiiInputType.DrumGreen, "((wiiButtonsHigh) & (1 << 4))"},
        {WiiInputType.DrumYellow, "((wiiButtonsHigh) & (1 << 5))"},
        {WiiInputType.DrumRed, "((wiiButtonsHigh) & (1 << 6))"},
        {WiiInputType.DrumOrange, "((wiiButtonsHigh) & (1 << 7))"},
        {WiiInputType.GuitarPlus, "((wiiButtonsLow) & (1 << 2))"},
        {WiiInputType.GuitarMinus, "((wiiButtonsLow) & (1 << 4))"},
        {WiiInputType.GuitarStrumDown, "((wiiButtonsLow) & (1 << 6))"},
        {WiiInputType.GuitarStrumUp, "((wiiButtonsHigh) & (1 << 0))"},
        {WiiInputType.GuitarYellow, "((wiiButtonsHigh) & (1 << 3))"},
        {WiiInputType.GuitarGreen, "((wiiButtonsHigh) & (1 << 4))"},
        {WiiInputType.GuitarBlue, "((wiiButtonsHigh) & (1 << 5))"},
        {WiiInputType.GuitarRed, "((wiiButtonsHigh) & (1 << 6))"},
        {WiiInputType.GuitarOrange, "((wiiButtonsHigh) & (1 << 7))"},
        {WiiInputType.NunchukC, "((wiiButtonsHigh) & (1 << 1))"},
        {WiiInputType.NunchukZ, "((wiiButtonsHigh) & (1 << 0))"},
        {WiiInputType.TaTaConRightDrumRim, "((~wiiData[0]) & (1 << 3))"},
        {WiiInputType.TaTaConRightDrumCenter, "((~wiiData[0]) & (1 << 4))"},
        {WiiInputType.TaTaConLeftDrumRim, "((~wiiData[0]) & (1 << 5))"},
        {WiiInputType.TaTaConLeftDrumCenter, "((~wiiData[0]) & (1 << 6))"},
        {WiiInputType.UDrawPenButton1, "((wiiButtonsHigh) & (1 << 0))"},
        {WiiInputType.UDrawPenButton2, "((wiiButtonsHigh) & (1 << 1))"},
        {WiiInputType.UDrawPenClick, "((~wiiButtonsHigh) & (1 << 2))"},
        {WiiInputType.GuitarTapGreen, GetMappingForTapBar(0x04, 0x07)},
        {WiiInputType.GuitarTapRed, GetMappingForTapBar(0x07, 0x0A, 0x0c, 0x0d)},
        {WiiInputType.GuitarTapYellow, GetMappingForTapBar(0x0c, 0x0d, 0x12, 0x13, 0x14, 0x15)},
        {WiiInputType.GuitarTapBlue, GetMappingForTapBar(0x14, 0x15, 0x17, 0x18, 0x1A)},
        {WiiInputType.GuitarTapOrange, GetMappingForTapBar(0x1A, 0x1F)},
    };

    private static readonly List<string> HiResMapOrder = new()
    {
        Mappings[WiiInputType.ClassicLeftStickX],
        Mappings[WiiInputType.ClassicLeftStickY],
        Mappings[WiiInputType.ClassicRightStickX],
        Mappings[WiiInputType.ClassicRightStickY],
        Mappings[WiiInputType.ClassicLeftTrigger],
        Mappings[WiiInputType.ClassicRightTrigger],
    };

    private static readonly Dictionary<string, string> HiResMap = new()
    {
        {Mappings[WiiInputType.ClassicLeftStickX], "(wiiData[0] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicLeftStickY], "(wiiData[2] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicRightStickX], "(wiiData[1] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicRightStickY], "(wiiData[3] - 0x80) << 8"},
        {Mappings[WiiInputType.ClassicLeftTrigger], "wiiData[4] << 8"},
        {Mappings[WiiInputType.ClassicRightTrigger], "wiiData[5] << 8"},
    };

    private static readonly Dictionary<WiiControllerType, string> CType = new()
    {
        {WiiControllerType.ClassicController, "WII_CLASSIC_CONTROLLER"},
        {WiiControllerType.Nunchuk, "WII_NUNCHUK"},
        {WiiControllerType.UDraw, "WII_THQ_UDRAW_TABLET"},
        {WiiControllerType.Drawsome, "WII_UBISOFT_DRAWSOME_TABLET"},
        {WiiControllerType.Guitar, "WII_GUITAR_HERO_GUITAR_CONTROLLER"},
        {WiiControllerType.Drum, "WII_GUITAR_HERO_DRUM_CONTROLLER"},
        {WiiControllerType.Dj, "WII_DJ_HERO_TURNTABLE"},
        {WiiControllerType.Taiko, "WII_TAIKO_NO_TATSUJIN_CONTROLLER"},
        {WiiControllerType.MotionPlus, "WII_MOTION_PLUS"}
    };

    public bool Combined { get; }
    
    public bool BindableTwi { get; }

    public WiiInput(WiiInputType input, ConfigViewModel model, Microcontroller microcontroller, int? sda = null, int? scl = null,
        bool combined = false) : base(
        microcontroller,
        WiiTwiType, WiiTwiFreq, sda, scl, model)
    {
        Input = input;
        Combined = combined;
        BindableTwi = !combined && microcontroller is not AvrController;
        Image = GetImage();
    }

    public override InputType? InputType => Types.InputType.WiiInput;

    public override string Generate()
    {
        return Mappings[Input];
    }

    public override SerializedInput Serialise()
    {
        if (Combined)
        {
            return new SerializedWiiInputCombined(Input);
        }

        return new SerializedWiiInput(Sda, Scl, Input);
    }

    public override bool IsAnalog => Input <= WiiInputType.DrawsomePenPressure;
    public override bool IsUint => !Input.ToString().ToLower().Contains("stick");
    public override List<DevicePin> Pins => new();

    private static string GetMappingForTapBar(params int[] mappings)
    {
        return string.Join(" || ", mappings.Select(s2 => $"(lastTapWii == {s2})"));
    }

    private int[] drumVelocity = new int[8];

    enum DrumType
    {
        DrumGreen,
        DrumRed,
        DrumYellow,
        DrumBlue,
        DrumOrange,
        DrumKick,
        DrumHihat
    };

    public override void Update(List<Output> modelBindings, Dictionary<int, int> analogRaw,
        Dictionary<int, bool> digitalRaw, byte[] ps2Raw,
        byte[] wiiData, byte[] djLeftRaw,
        byte[] djRightRaw, byte[] gh5Raw, byte[] ghWtRaw, byte[] ps2ControllerType, byte[] wiiControllerType)
    {
        if (!wiiControllerType.Any()) return;
        var type = BitConverter.ToUInt16(wiiControllerType);
        var newType = ControllerTypeById.GetValueOrDefault(type);
        var checkedType = newType;
        if (checkedType == WiiControllerType.ClassicControllerPro)
        {
            checkedType = WiiControllerType.ClassicController;
        }

        if (checkedType != WiiControllerType) return;
        var wiiButtonsLow = ~wiiData[4];
        var wiiButtonsHigh = ~wiiData[5];
        var highResolution = checkedType == WiiControllerType.ClassicController && wiiData.Length == 8;
        if (highResolution)
        {
            wiiButtonsLow = ~wiiData[6];
            wiiButtonsHigh = ~wiiData[7];
        }

        switch (checkedType)
        {
            case WiiControllerType.Nunchuk:
                var accX = ((wiiData[2] << 2) | ((wiiData[5] & 0xC0) >> 6)) - 511;
                var accY = ((wiiData[3] << 2) | ((wiiData[5] & 0x30) >> 4)) - 511;
                var accZ = ((wiiData[4] << 2) | ((wiiData[5] & 0xC) >> 2)) - 511;
                RawValue = Input switch
                {
                    WiiInputType.NunchukC => ((wiiButtonsHigh & (1 << 1)) != 0) ? 1 : 0,
                    WiiInputType.NunchukZ => ((wiiButtonsHigh & (1 << 0)) != 0) ? 1 : 0,
                    WiiInputType.NunchukAccelerationX => accX,
                    WiiInputType.NunchukAccelerationY => accY,
                    WiiInputType.NunchukAccelerationZ => accZ,
                    WiiInputType.NunchukRotationPitch => (int) (Math.Atan2(accY, accZ) / (Math.PI / 32767)),
                    WiiInputType.NunchukRotationRoll => (int) (Math.Atan2(accX, accZ) / (Math.PI / 32767)),
                    WiiInputType.NunchukStickX => (wiiData[0] - 0x80) << 8,
                    WiiInputType.NunchukStickY => (wiiData[1] - 0x80) << 8,
                    _ => RawValue
                };
                break;
            case WiiControllerType.ClassicController:
                RawValue = Input switch
                {
                    WiiInputType.ClassicRt => ((wiiButtonsLow) & (1 << 1)),
                    WiiInputType.ClassicPlus => ((wiiButtonsLow) & (1 << 2)),
                    WiiInputType.ClassicHome => ((wiiButtonsLow) & (1 << 3)),
                    WiiInputType.ClassicMinus => ((wiiButtonsLow) & (1 << 4)),
                    WiiInputType.ClassicLt => ((wiiButtonsLow) & (1 << 5)),
                    WiiInputType.ClassicDPadDown => ((wiiButtonsLow) & (1 << 6)),
                    WiiInputType.ClassicDPadRight => ((wiiButtonsLow) & (1 << 7)),
                    WiiInputType.ClassicDPadUp => ((wiiButtonsHigh) & (1 << 0)),
                    WiiInputType.ClassicDPadLeft => ((wiiButtonsHigh) & (1 << 1)),
                    WiiInputType.ClassicZr => ((wiiButtonsHigh) & (1 << 2)),
                    WiiInputType.ClassicX => ((wiiButtonsHigh) & (1 << 3)),
                    WiiInputType.ClassicA => ((wiiButtonsHigh) & (1 << 4)),
                    WiiInputType.ClassicY => ((wiiButtonsHigh) & (1 << 5)),
                    WiiInputType.ClassicB => ((wiiButtonsHigh) & (1 << 6)),
                    WiiInputType.ClassicZl => ((wiiButtonsHigh) & (1 << 7)),
                    _ => RawValue
                };
                if (highResolution)
                {
                    RawValue = Input switch
                    {
                        WiiInputType.ClassicLeftStickX => (wiiData[0] - 0x80) << 8,
                        WiiInputType.ClassicLeftStickY => (wiiData[2] - 0x80) << 8,
                        WiiInputType.ClassicRightStickX => (wiiData[1] - 0x80) << 8,
                        WiiInputType.ClassicRightStickY => (wiiData[3] - 0x80) << 8,
                        WiiInputType.ClassicLeftTrigger => wiiData[4] << 8,
                        WiiInputType.ClassicRightTrigger => wiiData[5] << 8,
                        _ => RawValue
                    };
                }
                else
                {
                    RawValue = Input switch
                    {
                        WiiInputType.ClassicLeftStickX => ((wiiData[0] & 0x3f) - 32) << 9,
                        WiiInputType.ClassicLeftStickY => ((wiiData[1] & 0x3f) - 32) << 9,
                        WiiInputType.ClassicRightStickX => ((((wiiData[0] & 0xc0) >> 3) |
                                                             ((wiiData[1] & 0xc0) >> 5) | (wiiData[2] >> 7)) -
                                                            16) << 10,
                        WiiInputType.ClassicRightStickY => ((wiiData[2] & 0x1f) - 16) << 10,
                        WiiInputType.ClassicLeftTrigger => ((wiiData[3] >> 5) | ((wiiData[2] & 0x60) >> 2)),
                        WiiInputType.ClassicRightTrigger => (wiiData[3] & 0x1f) << 3,
                        _ => RawValue
                    };
                }

                break;
            case WiiControllerType.UDraw:
                RawValue = Input switch
                {
                    WiiInputType.UDrawPenPressure => wiiData[3],
                    WiiInputType.UDrawPenX => ((wiiData[2] & 0x0f) << 8) | wiiData[0],
                    WiiInputType.UDrawPenY => ((wiiData[2] & 0xf0) << 4) | wiiData[1],
                    WiiInputType.UDrawPenButton1 => ((wiiButtonsHigh) & (1 << 0)),
                    WiiInputType.UDrawPenButton2 => ((wiiButtonsHigh) & (1 << 1)),
                    WiiInputType.UDrawPenClick => ((~wiiButtonsHigh) & (1 << 2)),
                    _ => RawValue
                };

                break;
            case WiiControllerType.Drawsome:
                RawValue = Input switch
                {
                    WiiInputType.DrawsomePenPressure => (wiiButtonsLow | (wiiButtonsHigh & 0x0f) << 8),
                    WiiInputType.DrawsomePenX => wiiData[0] | wiiData[1] << 8,
                    WiiInputType.DrawsomePenY => wiiData[2] | wiiData[3] << 8,
                    _ => RawValue
                };
                break;
            case WiiControllerType.Guitar:
                var lastTapWii = (wiiData[2] & 0x1f);
                RawValue = Input switch
                {
                    WiiInputType.GuitarPlus => ((wiiButtonsLow) & (1 << 2)),
                    WiiInputType.GuitarMinus => ((wiiButtonsLow) & (1 << 4)),
                    WiiInputType.GuitarStrumDown => ((wiiButtonsLow) & (1 << 6)),
                    WiiInputType.GuitarStrumUp => ((wiiButtonsHigh) & (1 << 0)),
                    WiiInputType.GuitarYellow => ((wiiButtonsHigh) & (1 << 3)),
                    WiiInputType.GuitarGreen => ((wiiButtonsHigh) & (1 << 4)),
                    WiiInputType.GuitarBlue => ((wiiButtonsHigh) & (1 << 5)),
                    WiiInputType.GuitarRed => ((wiiButtonsHigh) & (1 << 6)),
                    WiiInputType.GuitarOrange => ((wiiButtonsHigh) & (1 << 7)),
                    WiiInputType.GuitarJoystickX => ((wiiData[0] & 0x3f) - 32) << 10,
                    WiiInputType.GuitarJoystickY => ((wiiData[1] & 0x3f) - 32) << 10,
                    WiiInputType.GuitarTapBar => (wiiData[2] & 0x1f) << 11,
                    WiiInputType.GuitarWhammy => (wiiData[3] & 0x1f) << 11,
                    WiiInputType.GuitarTapGreen => lastTapWii is 0x04 or 0x07 ? 1 : 0,
                    WiiInputType.GuitarTapRed => lastTapWii is 0x07 or 0x0A or 0x0c or 0x0d ? 1 : 0,
                    WiiInputType.GuitarTapYellow => lastTapWii is 0x0c or 0x0d or 0x12 or 0x13 or 0x14 or 0x15 ? 1 : 0,
                    WiiInputType.GuitarTapBlue => lastTapWii is 0x14 or 0x15 or 0x17 or 0x18 or 0x1A ? 1 : 0,
                    WiiInputType.GuitarTapOrange => lastTapWii is 0x1A or 0x1F ? 1 : 0,
                    _ => RawValue
                };
                break;
            case WiiControllerType.Drum:
                var vel = (7 - (wiiData[3] >> 5)) << 5;
                var which = (wiiData[2] & 0b01111100) >> 1;
                switch (which)
                {
                    case 0x1B:
                        drumVelocity[(int) DrumType.DrumKick] = vel;
                        break;
                    case 0x12:
                        drumVelocity[(int) DrumType.DrumGreen] = vel;
                        break;
                    case 0x19:
                        drumVelocity[(int) DrumType.DrumRed] = vel;
                        break;
                    case 0x11:
                        drumVelocity[(int) DrumType.DrumYellow] = vel;
                        break;
                    case 0x0F:
                        drumVelocity[(int) DrumType.DrumBlue] = vel;
                        break;
                    case 0x0E:
                        drumVelocity[(int) DrumType.DrumOrange] = vel;
                        break;
                }

                RawValue = Input switch
                {
                    WiiInputType.DrumPlus => ((wiiButtonsLow) & (1 << 2)),
                    WiiInputType.DrumMinus => ((wiiButtonsLow) & (1 << 4)),
                    WiiInputType.DrumKickPedal => ((wiiButtonsHigh) & (1 << 2)),
                    WiiInputType.DrumBlue => ((wiiButtonsHigh) & (1 << 3)),
                    WiiInputType.DrumGreen => ((wiiButtonsHigh) & (1 << 4)),
                    WiiInputType.DrumYellow => ((wiiButtonsHigh) & (1 << 5)),
                    WiiInputType.DrumRed => ((wiiButtonsHigh) & (1 << 6)),
                    WiiInputType.DrumOrange => ((wiiButtonsHigh) & (1 << 7)),
                    WiiInputType.DrumGreenPressure => drumVelocity[(int) DrumType.DrumGreen],
                    WiiInputType.DrumRedPressure => drumVelocity[(int) DrumType.DrumRed],
                    WiiInputType.DrumYellowPressure => drumVelocity[(int) DrumType.DrumYellow],
                    WiiInputType.DrumBluePressure => drumVelocity[(int) DrumType.DrumBlue],
                    WiiInputType.DrumOrangePressure => drumVelocity[(int) DrumType.DrumOrange],
                    WiiInputType.DrumKickPedalPressure => drumVelocity[(int) DrumType.DrumKick],
                    _ => RawValue
                };
                break;
            case WiiControllerType.Dj:
                RawValue = Input switch
                {
                    WiiInputType.DjHeroPlus => ((wiiButtonsLow) & (1 << 2)),
                    WiiInputType.DjHeroMinus => ((wiiButtonsLow) & (1 << 4)),
                    WiiInputType.DjHeroLeftBlue => ((wiiButtonsHigh) & (1 << 7)),
                    WiiInputType.DjHeroLeftRed => ((wiiButtonsLow) & (1 << 5)),
                    WiiInputType.DjHeroLeftGreen => ((wiiButtonsHigh) & (1 << 3)),
                    WiiInputType.DjHeroLeftAny => (((wiiButtonsHigh) & ((1 << 3) | 1 << 7)) |
                                                   ((wiiButtonsLow) & (1 << 5))),
                    WiiInputType.DjHeroRightGreen => ((wiiButtonsHigh) & (1 << 5)),
                    WiiInputType.DjHeroRightRed => ((wiiButtonsLow) & (1 << 1)),
                    WiiInputType.DjHeroRightBlue => ((wiiButtonsHigh) & (1 << 2)),
                    WiiInputType.DjHeroRightAny => (((wiiButtonsHigh) & ((1 << 5) | 1 << 2)) |
                                                    ((wiiButtonsLow) & (1 << 1))),
                    WiiInputType.DjHeroEuphoria => ((wiiButtonsHigh) & (1 << 4)),
                    WiiInputType.DjCrossfadeSlider => (wiiData[2] & 0x1E) >> 1,
                    WiiInputType.DjEffectDial => (wiiData[3] & 0xE0) >> 5 | (wiiData[2] & 0x60) >> 2,
                    WiiInputType.DjStickX => ((wiiData[0] & 0x3F) - 0x20) << 10,
                    WiiInputType.DjStickY => ((wiiData[1] & 0x3F) - 0x20) << 10,
                    WiiInputType.DjTurntableLeft =>
                        (((wiiButtonsLow & 1) != 0 ? 32 : 1) + (0x1F - (wiiData[3] & 0x1F))) << 10,
                    WiiInputType.DjTurntableRight => (((wiiData[2] & 1) != 0 ? 32 : 1) +
                                                      (0x1F - ((wiiData[2] & 0x80) >> 7 | (wiiData[1] & 0xC0) >> 5 |
                                                               (wiiData[0] & 0xC0) >> 3))) << 10,

                    _ => RawValue
                };
                break;
            case WiiControllerType.Taiko:
                RawValue = Input switch
                {
                    WiiInputType.TaTaConRightDrumRim => ((~wiiData[0]) & (1 << 3)),
                    WiiInputType.TaTaConRightDrumCenter => ((~wiiData[0]) & (1 << 4)),
                    WiiInputType.TaTaConLeftDrumRim => ((~wiiData[0]) & (1 << 5)),
                    WiiInputType.TaTaConLeftDrumCenter => ((~wiiData[0]) & (1 << 6)),
                    _ => RawValue
                };
                break;
            case WiiControllerType.MotionPlus:
                break;
        }
    }

    public override string GenerateAll(List<Output> allBindings, List<Tuple<Input, string>> bindings)
    {
        Dictionary<WiiControllerType, List<string>> mappedBindings = new();
        var hasTapBar = false;
        foreach (var binding in bindings)
        {
            if (binding.Item1 is WiiInput input)
            {
                if (!mappedBindings.ContainsKey(input.WiiControllerType))
                {
                    mappedBindings.Add(input.WiiControllerType, new List<string>());
                }

                if (input.Input.ToString().StartsWith("GuitarTap"))
                {
                    hasTapBar = true;
                }

                mappedBindings[input.WiiControllerType].Add(binding.Item2);
            }
        }

        var ret = "if (wiiValid) {";
        if (hasTapBar)
        {
            ret += "uint8_t lastTapWii = (wiiData[2] & 0x1f);";
        }

        ret += " switch(wiiControllerType) {";
        if (mappedBindings.ContainsKey(WiiControllerType.ClassicController))
        {
            var mappings = mappedBindings[WiiControllerType.ClassicController];
            mappedBindings.Remove(WiiControllerType.ClassicController);
            var mappings2 = new List<string>();
            var mappingsDigital = mappings.Where(m => m.Contains("wiiButtons"));
            mappings = mappings.Where(m => !m.Contains("wiiButtons")).ToList();
            foreach (var mapping in mappings)
            {
                var val = mapping;
                foreach (var key in HiResMapOrder)
                {
                    val = val.Replace(key, HiResMap[key]);
                }

                mappings2.Add(val);
            }

            if (mappings.Any())
            {
                ret += @$"
case WII_CLASSIC_CONTROLLER:
case WII_CLASSIC_CONTROLLER_PRO:
if (hiRes) {{
    {string.Join(";\n", mappings2)};
}} else {{
    {string.Join(";\n", mappings)};
}}
break;
";
            }
            else
            {
                ret += @$"
case WII_CLASSIC_CONTROLLER:
case WII_CLASSIC_CONTROLLER_PRO:
{string.Join(";\n", mappingsDigital)};
break;
";
            }
        }

        foreach (var (input, mappings) in mappedBindings)
        {
            ret += @$"case {CType[input]}:
    {string.Join(";\n", mappings)};
    break;";
        }

        return ret + "}}";
    }

    public override IReadOnlyList<string> RequiredDefines()
    {
        return WiiControllerType switch
        {
            WiiControllerType.Drum => base.RequiredDefines().Concat(new[] {"INPUT_WII", "INPUT_WII_DRUM"}).ToList(),
            WiiControllerType.Nunchuk => base.RequiredDefines()
                .Concat(new[] {"INPUT_WII", "INPUT_WII_NUNCHUK"})
                .ToList(),
            _ => base.RequiredDefines().Concat(new[] {"INPUT_WII"}).ToList()
        };
    }

    private Bitmap? GetImage()
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        try
        {
            var asset = assets!.Open(new Uri($"avares://{assemblyName}/Assets/Icons/Wii/{Input}.png"));
            return new Bitmap(asset);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}