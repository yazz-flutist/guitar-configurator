using System;
using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.Configuration
{
    public interface WiiInput {
        public WiiController wiiController { get; }
        static public Dictionary<WiiController, string> caseStatements = new Dictionary<WiiController, string>() {
            {WiiController.ClassicController, "WII_CLASSIC_CONTROLLER"}
        };
    }
    [JsonDiscriminator(nameof(WiiButton))]
    public class WiiButton : GroupableButton, WiiInput
    {
        public WiiButton(WiiButtonType button, WiiController controller, int debounce, OutputButton type, Color ledOn, Color ledOff) : base(InputControllerType.Wii, debounce, type, ledOn, ledOff)
        {
            this.button = button;
            this.wiiController = controller;
        }
        public WiiButtonType button { get; }

        public WiiController wiiController { get; }

        private Dictionary<WiiButtonType, string> mapping = new Dictionary<WiiButtonType, string>() {
            {WiiButtonType.ClassicRT,               "~(data[4] >> 1) & 1"},
            {WiiButtonType.ClassicPlus,             "~(data[4] >> 2) & 1"},
            {WiiButtonType.ClassicHome,             "~(data[4] >> 3) & 1"},
            {WiiButtonType.ClassicMinus,            "~(data[4] >> 4) & 1"},
            {WiiButtonType.ClassicLT,               "~(data[4] >> 1) & 5"},
            {WiiButtonType.ClassicDPadDown,         "~(data[4] >> 6) & 1"},
            {WiiButtonType.ClassicDPadRight,        "~(data[4] >> 7) & 1"},
            {WiiButtonType.ClassicDPadUp,           "~(data[5] >> 0) & 1"},
            {WiiButtonType.ClassicDPadLeft,         "~(data[5] >> 1) & 1"},
            {WiiButtonType.ClassicZR,               "~(data[5] >> 2) & 1"},
            {WiiButtonType.ClassicX,                "~(data[5] >> 3) & 1"},
            {WiiButtonType.ClassicA,                "~(data[5] >> 4) & 1"},
            {WiiButtonType.ClassicY,                "~(data[5] >> 5) & 1"},
            {WiiButtonType.ClassicB,                "~(data[5] >> 6) & 1"},
            {WiiButtonType.ClassicZL,               "~(data[5] >> 7) & 1"},
            {WiiButtonType.DJHeroRightRed,          "~(data[4] >> 1) & 1"},
            {WiiButtonType.DJHeroPlus,              "~(data[4] >> 2) & 1"},
            {WiiButtonType.DJHeroMinus,             "~(data[4] >> 4) & 1"},
            {WiiButtonType.DJHeroLeftRed,           "~(data[4] >> 5) & 1"},
            {WiiButtonType.DJHeroRightBlue,         "~(data[5] >> 2) & 1"},
            {WiiButtonType.DJHeroLeftGreen,         "~(data[5] >> 3) & 1"},
            {WiiButtonType.DJHeroEuphoria,          "~(data[5] >> 4) & 1"},
            {WiiButtonType.DJHeroRightGreen,        "~(data[5] >> 5) & 1"},
            {WiiButtonType.DJHeroLeftBlue,          "~(data[5] >> 7) & 1"},
            {WiiButtonType.DrumPlus,                "~(data[4] >> 2) & 1"},
            {WiiButtonType.DrumMinus,               "~(data[4] >> 4) & 1"},
            {WiiButtonType.DrumKickPedal,           "~(data[5] >> 2) & 1"},
            {WiiButtonType.DrumBlue,                "~(data[5] >> 3) & 1"},
            {WiiButtonType.DrumGreen,               "~(data[5] >> 4) & 1"},
            {WiiButtonType.DrumYellow,              "~(data[5] >> 5) & 1"},
            {WiiButtonType.DrumRed,                 "~(data[5] >> 6) & 1"},
            {WiiButtonType.DrumOrange,              "~(data[5] >> 7) & 1"},
            {WiiButtonType.GuitarPlus,              "~(data[4] >> 2) & 1"},
            {WiiButtonType.GuitarMinus,             "~(data[4] >> 4) & 1"},
            {WiiButtonType.GuitarStrumDown,         "~(data[4] >> 6) & 1"},
            {WiiButtonType.GuitarStrumUp,           "~(data[5] >> 0) & 1"},
            {WiiButtonType.GuitarYellow,            "~(data[5] >> 3) & 1"},
            {WiiButtonType.GuitarGreen,             "~(data[5] >> 4) & 1"},
            {WiiButtonType.GuitarBlue,              "~(data[5] >> 5) & 1"},
            {WiiButtonType.GuitarRed,               "~(data[5] >> 6) & 1"},
            {WiiButtonType.GuitarOrange,            "~(data[5] >> 7) & 1"},
            {WiiButtonType.NunchukC,                "~(data[5] >> 1) & 1"},
            {WiiButtonType.NunchukZ,                "~(data[5] >> 0) & 1"},
            {WiiButtonType.TaTaConRightDrumRim,     "~(data[0] >> 3) & 1"},
            {WiiButtonType.TaTaConRightDrumCenter,  "~(data[0] >> 4) & 1"},
            {WiiButtonType.TaTaConLeftDrumRim,      "~(data[0] >> 5) & 1"},
            {WiiButtonType.TaTaConLeftDrumCenter,   "~(data[0] >> 6) & 1"},
            {WiiButtonType.UDrawPenButton1,         "~(data[5] >> 0) & 1"},
            {WiiButtonType.UDrawPenButton2,         "~(data[5] >> 1) & 1"},
            {WiiButtonType.UDrawPenClick,           " (data[5] >> 2) & 1"}
        };

        public override StandardButtonType standardButton => StandardButtonMap.wiiButtonMap[button];

        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            // High res uses the same buttons, only they are at different bytes
            if (this.wiiController == WiiController.ClassicControllerHighRes)
            {
                return mapping[button].Replace("data[4]", "data[6]").Replace("data[5]", "data[7]");
            }
            return mapping[button];
        }
    }
    [JsonDiscriminator(nameof(WiiAnalog))]
    public class WiiAnalog : GroupableAxis, WiiInput
    {
        public WiiAnalog(WiiAxis axis, WiiController controller, OutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(InputControllerType.Wii, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
        {
            this.axis = axis;
            this.wiiController = controller;
        }
        public WiiAxis axis { get; }

        public WiiController wiiController { get; }

        public override StandardAxisType standardAxis => StandardAxisMap.wiiAxisMap[axis];
        private Dictionary<WiiAxis, string> mappings = new Dictionary<WiiAxis, string>() {
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
            {WiiAxis.DJCrossfadeSlider,         "(data[2] & 0x1E) >> 1"},
            {WiiAxis.DJEffectDial,              "(data[3] & 0xE0) >> 5 | (data[2] & 0x60) >> 2"},
            {WiiAxis.DJStickX,                  "((data[0] & 0x3F) - 0x20) << 10"},
            {WiiAxis.DJStickY,                  "((data[1] & 0x3F) - 0x20) << 10"},
            {WiiAxis.DJTurntableLeft,           "(((buf[4] & 1) ? 32 : 1) + (0x1F - (buf[3] & 0x1F))) << 10"},
            {WiiAxis.DJTurntableRight,          "(((buf[2] & 1) ? 32 : 1) + (0x1F - ((data[2] & 0x80) >> 7 | (data[1] & 0xC0) >> 5 | (data[0] & 0xC0) >> 3))) << 10"},
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

        public override string generate(Microcontroller controller, IEnumerable<Binding> bindings)
        {
            return mappings[this.axis];
        }

        public static string generateDrum() {
            return @"uint8_t velocity = (7 - (data[3] >> 5)) << 5;
                    uint8_t which = (data[2] & 0b01111100) >> 1";
        }

        public static string generateNunchuk() {
            return @"uint16_t accX = ((data[2] << 2) | ((data[5] & 0xC0) >> 6)) - 511;
                    uint16_t accY = ((data[3] << 2) | ((data[5] & 0x30) >> 4)) - 511;
                    uint16_t accZ = ((data[4] << 2) | ((data[5] & 0xC) >> 2)) - 511;";
        }
    }
}