using System;
using System.Collections.Generic;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Wii
{
    [JsonDiscriminator(nameof(WiiButton))]
    public class WiiButton : GroupableButton, IWiiInput
    {
        public WiiButton(Microcontroller.Microcontroller controller, WiiButtonType button, WiiController wiiController, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Wii, debounce, type, ledOn, ledOff)
        {
            this.Button = button;
            this.WiiController = wiiController;
        }
        public WiiButtonType Button { get; }

        public WiiController WiiController { get; }
        public override string Input => Enum.GetName(typeof(WiiButtonType), Button)!;

        private readonly Dictionary<WiiButtonType, string> _mapping = new Dictionary<WiiButtonType, string>() {
            {WiiButtonType.ClassicRt,               "~(data[4] >> 1) & 1"},
            {WiiButtonType.ClassicPlus,             "~(data[4] >> 2) & 1"},
            {WiiButtonType.ClassicHome,             "~(data[4] >> 3) & 1"},
            {WiiButtonType.ClassicMinus,            "~(data[4] >> 4) & 1"},
            {WiiButtonType.ClassicLt,               "~(data[4] >> 1) & 5"},
            {WiiButtonType.ClassicDPadDown,         "~(data[4] >> 6) & 1"},
            {WiiButtonType.ClassicDPadRight,        "~(data[4] >> 7) & 1"},
            {WiiButtonType.ClassicDPadUp,           "~(data[5] >> 0) & 1"},
            {WiiButtonType.ClassicDPadLeft,         "~(data[5] >> 1) & 1"},
            {WiiButtonType.ClassicZr,               "~(data[5] >> 2) & 1"},
            {WiiButtonType.ClassicX,                "~(data[5] >> 3) & 1"},
            {WiiButtonType.ClassicA,                "~(data[5] >> 4) & 1"},
            {WiiButtonType.ClassicY,                "~(data[5] >> 5) & 1"},
            {WiiButtonType.ClassicB,                "~(data[5] >> 6) & 1"},
            {WiiButtonType.ClassicZl,               "~(data[5] >> 7) & 1"},
            {WiiButtonType.DjHeroRightRed,          "~(data[4] >> 1) & 1"},
            {WiiButtonType.DjHeroPlus,              "~(data[4] >> 2) & 1"},
            {WiiButtonType.DjHeroMinus,             "~(data[4] >> 4) & 1"},
            {WiiButtonType.DjHeroLeftRed,           "~(data[4] >> 5) & 1"},
            {WiiButtonType.DjHeroRightBlue,         "~(data[5] >> 2) & 1"},
            {WiiButtonType.DjHeroLeftGreen,         "~(data[5] >> 3) & 1"},
            {WiiButtonType.DjHeroEuphoria,          "~(data[5] >> 4) & 1"},
            {WiiButtonType.DjHeroRightGreen,        "~(data[5] >> 5) & 1"},
            {WiiButtonType.DjHeroLeftBlue,          "~(data[5] >> 7) & 1"},
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

        public override StandardButtonType StandardButton => StandardButtonMap.WiiButtonMap[Button];

        public override string Generate(IEnumerable<Binding> bindings, bool xbox)
        {
            // High res uses the same buttons, only they are at different bytes
            if (this.WiiController == WiiController.ClassicControllerHighRes)
            {
                return _mapping[Button].Replace("data[4]", "data[6]").Replace("data[5]", "data[7]");
            }
            return _mapping[Button];
        }
    }
}