using Avalonia.Input;
using Avalonia.Media;
using Dahomey.Json.Attributes;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

[JsonDiscriminator("kb")]
public class JsonKeyboardButton : JsonOutput
{
    public override JsonInput? Input { get; }
    public override Color LedOn { get; }
    public override Color LedOff { get; }

    public int Debounce { get; }
    public Key Type { get; }

    public JsonKeyboardButton(JsonInput? input, Color ledOn, Color ledOff, int debounce, Key type)
    {
        Input = input;
        LedOn = ledOn;
        LedOff = ledOff;
        Debounce = debounce;
        Type = type;
    }

    public override Output Generate(ConfigViewModel model, Microcontroller microcontroller)
    {
        return new KeyboardButton(model, Input?.Generate(microcontroller), LedOn, LedOff, Debounce, Type);
    }
}