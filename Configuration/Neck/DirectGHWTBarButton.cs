using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Dahomey.Json.Attributes;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Neck;

[JsonDiscriminator(nameof(DirectGhwtBarButton))]
public class DirectGhwtBarButton : Button
{
    public DirectGhwtBarButton(Microcontroller.Microcontroller controller, GhwtTarButton button, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, InputControllerType.Direct, debounce, type, ledOn, ledOff)
    {
        this.Button = button;
    }

    public override string Input => Enum.GetName(typeof(GhwtTarButton), Button)!;
    public GhwtTarButton Button { get; }
    private static readonly List<GhwtTarButton> _tap = new List<GhwtTarButton>() {
        GhwtTarButton.TapGreen,
        GhwtTarButton.TapRed,
        GhwtTarButton.TapYellow,
        GhwtTarButton.TapBlue,
        GhwtTarButton.TapOrange
    };
    public override string Generate(IEnumerable<Binding> bindings, bool xbox)
    {
        return $"(wttapbindings[fivetar_buttons[1]] >> {_tap.IndexOf(this.Button)}) & 1";
    }
    static readonly Dictionary<int, BarButton> _mappings = new Dictionary<int, BarButton>() {
        {0x17, BarButton.Green},
        {0x16, BarButton.Green},
        {0x14, BarButton.Green | BarButton.Red},
        {0x11, BarButton.Red},
        {0x12, BarButton.Red},
        {0xf, BarButton.Red | BarButton.Yellow},
        {0xa, BarButton.Yellow},
        {0xb, BarButton.Yellow},
        {0x9, BarButton.Yellow | BarButton.Blue},
        {0x7, BarButton.Blue},
        {0x5, BarButton.Blue | BarButton.Orange},
        {0x4, BarButton.Blue | BarButton.Orange},
        {0x3, BarButton.Blue | BarButton.Orange},
        {0x0, BarButton.Orange},
    };
    public static string InitWt()
    {
        return "uint8_t wttapbindings = {" + String.Join(" ", _mappings.Keys.Select(key => $"[{key}] = {(int)_mappings[key]},")) + "};";
    }
}