using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Outputs;
using GuitarConfigurator.NetCore.Configuration.Serialization;
using GuitarConfigurator.NetCore.Configuration.Types;
using GuitarConfigurator.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfigurator.NetCore.Configuration;

public abstract class Input : ReactiveObject, IDisposable
{
    protected ConfigViewModel Model { get; }

    protected Input(ConfigViewModel model)
    {
        Model = model;
        _imageOpacity = this.WhenAnyValue(x => x.RawValue, x => x.IsAnalog, (i, b) => b ? 1 : i)
            .Select(s => (s == 0 ? 0 : 0.25) + 0.75).ToProperty(this, s => s.ImageOpacity);
    }

    public abstract IReadOnlyList<string> RequiredDefines();
    public abstract string Generate(bool xbox);

    public abstract SerializedInput Serialise();
    private bool _analog;

    public bool IsAnalog
    {
        get => _analog;
        set => this.RaiseAndSetIfChanged(ref _analog, value);
    }

    public abstract bool IsUint { get; }

    private int _rawValue;

    public int RawValue
    {
        get => _rawValue;
        set => this.RaiseAndSetIfChanged(ref _rawValue, value);
    }

    private readonly ObservableAsPropertyHelper<double> _imageOpacity;

    public double ImageOpacity => _imageOpacity.Value;

    public virtual Input InnermostInput()
    {
        return this;
    }

    public virtual IList<Input> Inputs()
    {
        return new List<Input> {this};
    }

    public abstract IList<DevicePin> Pins { get; }
    public abstract IList<PinConfig> PinConfigs { get; }
    public abstract InputType? InputType { get; }

    public abstract void Update(List<Output> modelBindings, Dictionary<int, int> analogRaw,
        Dictionary<int, bool> digitalRaw, byte[] ps2Raw,
        byte[] wiiRaw, byte[] djLeftRaw, byte[] djRightRaw, byte[] gh5Raw, byte[] ghWtRaw, byte[] ps2ControllerType,
        byte[] wiiControllerType);

    public abstract string GenerateAll(List<Output> allBindings, List<Tuple<Input, string>> bindings, bool shared,
        bool xbox);

    public abstract void Dispose();
}