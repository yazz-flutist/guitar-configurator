using System.Reactive.Linq;
using System.Reflection;
using Avalonia.Media;
using DynamicData.Binding;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public abstract class Output : ReactiveObject
{
    protected Output(ConfigViewModel model, IInput? input, Color ledOn, Color ledOff)
    {
        Input = input;
        LedOn = ledOn;
        LedOff = ledOff;
        this.model = model;
        _image = this.WhenAnyValue(x => x.model.DeviceType).Select(GetImage).ToProperty(this, x => x.Image);
    }

    private readonly ObservableAsPropertyHelper<string> _image;
    public string Image => _image.Value;

    public abstract string Name { get; }

    public void ClearInput()
    {
        Input = null;
    }

    private string GetImage(DeviceControllerType type)
    {
        string assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        switch (type)
        {
            case DeviceControllerType.Guitar:
                return $"avares://{assemblyName}/Assets/Icons/GH/{this.Name}.png";
            case DeviceControllerType.Gamepad:
                return $"avares://{assemblyName}/Assets/Icons/Others/Xbox360/360_{this.Name}.png";
        }

        return "";
    }

    protected ConfigViewModel model;

    private IInput? _input;

    public IInput? Input
    {
        get => _input;
        set => this.RaiseAndSetIfChanged(ref _input, value);
    }

    private Color _ledOn;
    private Color _ledOff;

    public Color LedOn
    {
        get => _ledOn;
        set => this.RaiseAndSetIfChanged(ref _ledOn, value);
    }

    public Color LedOff
    {
        get => _ledOff;
        set => this.RaiseAndSetIfChanged(ref _ledOff, value);
    }

    public abstract string Generate(bool xbox, Microcontroller.Microcontroller microcontroller);
}