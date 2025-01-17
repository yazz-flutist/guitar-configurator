using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GuitarConfigurator.NetCore.Configuration.DJ;
using GuitarConfigurator.NetCore.Configuration.Exceptions;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Outputs.Combined;
using GuitarConfigurator.NetCore.Configuration.Serialization;
using GuitarConfigurator.NetCore.Configuration.Types;
using GuitarConfigurator.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfigurator.NetCore.Configuration.Outputs;

public class EmptyOutput : Output
{
    public EmptyOutput(ConfigViewModel model) : base(model, null, Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), "Empty")
    {
        _isController = this.WhenAnyValue(x => x.Model.EmulationType)
            .Select(x => x is EmulationType.Controller)
            .ToProperty(this, x => x.IsController);
        _isKeyboard = this.WhenAnyValue(x => x.Model.EmulationType)
            .Select(x => x is EmulationType.KeyboardMouse)
            .ToProperty(this, x => x.IsKeyboard);
        _isMidi = this.WhenAnyValue(x => x.Model.EmulationType)
            .Select(x => x is EmulationType.Midi)
            .ToProperty(this, x => x.IsMidi);

        _combinedTypes = this.WhenAnyValue(vm => vm.Model.DeviceType, vm => vm.Model.RhythmType)
            .Select(ControllerEnumConverter.GetTypes).ToProperty(this, x => x.CombinedTypes);
    }

    private readonly ObservableAsPropertyHelper<bool> _isController;
    public override bool IsController => _isController.Value;
    private readonly ObservableAsPropertyHelper<bool> _isKeyboard;
    public override bool IsKeyboard => _isKeyboard.Value;
    private readonly ObservableAsPropertyHelper<bool> _isMidi;
    public override bool IsMidi => _isMidi.Value;

    public override bool Valid => true;

    private readonly ObservableAsPropertyHelper<IEnumerable<object>> _combinedTypes;

    public IEnumerable<object> CombinedTypes => _combinedTypes.Value;

    public object? CombinedType
    {
        get => null;
        set => Generate(value);
    }

    private Key? _key;

    public Key? Key
    {
        get => _key;
        set
        {
            this.RaiseAndSetIfChanged(ref _key, value);
            this.RaiseAndSetIfChanged(ref _mouseAxisType, null, nameof(MouseAxisType));
            this.RaiseAndSetIfChanged(ref _mouseButtonType, null, nameof(MouseButtonType));
        }
    }

    public IEnumerable<Key> Keys => Enum.GetValues<Key>();

    private MouseAxisType? _mouseAxisType;

    public MouseAxisType? MouseAxisType
    {
        get => _mouseAxisType;
        set
        {
            this.RaiseAndSetIfChanged(ref _mouseAxisType, value);
            this.RaiseAndSetIfChanged(ref _mouseButtonType, null, nameof(MouseButtonType));
            this.RaiseAndSetIfChanged(ref _key, null, nameof(Key));
        }
    }

    public IEnumerable<MouseAxisType> MouseAxisTypes => Enum.GetValues<MouseAxisType>();

    private MouseButtonType? _mouseButtonType;

    public MouseButtonType? MouseButtonType
    {
        get => _mouseButtonType;
        set
        {
            this.RaiseAndSetIfChanged(ref _mouseButtonType, value);
            this.RaiseAndSetIfChanged(ref _mouseAxisType, null, nameof(MouseAxisType));
            this.RaiseAndSetIfChanged(ref _key, null, nameof(Key));
        }
    }

    public IEnumerable<MouseButtonType> MouseButtonTypes => Enum.GetValues<MouseButtonType>();

    private void Generate(object? value)
    {
        if (Model.MicroController == null) return;
        switch (Model.EmulationType)
        {
            case EmulationType.Controller:
                switch (value)
                {
                    case SimpleType simpleType:
                        switch (simpleType)
                        {
                            case SimpleType.WiiInputSimple:
                                Model.Bindings.Add(new WiiCombinedOutput(Model, Model.MicroController!));
                                break;
                            case SimpleType.Gh5NeckSimple:
                                Model.Bindings.Add(new Gh5CombinedOutput(Model, Model.MicroController!));
                                break;
                            case SimpleType.Ps2InputSimple:
                                Model.Bindings.Add(new Ps2CombinedOutput(Model, Model.MicroController!));
                                break;
                            case SimpleType.WtNeckSimple:
                                Model.Bindings.Add(new GhwtCombinedOutput(Model, Model.MicroController!));
                                break;
                            case SimpleType.DjTurntableSimple:
                                Model.Bindings.Add(new DjCombinedOutput(Model, Model.MicroController!));
                                break;
                        }

                        break;
                    case StandardAxisType standardAxisType:
                        Model.Bindings.Add(new ControllerAxis(Model, new DirectInput(Model.MicroController.GetFirstAnalogPin(), DevicePinMode.Analog, Model, Model.MicroController), Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),short.MinValue, short.MaxValue, 0,
                            standardAxisType));
                        break;
                    case StandardButtonType standardButtonType:
                        Model.Bindings.Add(new ControllerButton(Model, new DirectInput(0, DevicePinMode.PullUp, Model, Model.MicroController), Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 5,
                            standardButtonType));
                        break;
                    case DrumAxisType drumAxisType:
                        Model.Bindings.Add(new DrumAxis(Model, new DirectInput(Model.MicroController.GetFirstAnalogPin(), DevicePinMode.Analog, Model, Model.MicroController), Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),short.MinValue, short.MaxValue, 0,
                            1000, 10, drumAxisType));
                        break;
                    case Ps3AxisType ps3AxisType:
                        Model.Bindings.Add(new PS3Axis(Model, new DirectInput(Model.MicroController.GetFirstAnalogPin(), DevicePinMode.Analog, Model, Model.MicroController), Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),short.MinValue, short.MaxValue, 0,
                            ps3AxisType));
                        break;
                    case DjInputType djInputType:
                        Model.Bindings.Add(new DjButton(Model, new DirectInput(Model.MicroController.GetFirstAnalogPin(), DevicePinMode.Analog, Model, Model.MicroController), Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),10,
                            djInputType));
                        break;
                }

                break;
            case EmulationType.KeyboardMouse:
                if (MouseAxisType.HasValue)
                {
                    Model.Bindings.Add(new MouseAxis(Model, null, Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 1, 0, 0,
                        MouseAxisType.Value));
                }
                else if (MouseButtonType.HasValue)
                {
                    Model.Bindings.Add(new MouseButton(Model, null, Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),5,
                        MouseButtonType.Value));
                }
                else if (Key.HasValue)
                {
                    Model.Bindings.Add(new KeyboardButton(Model, null, Colors.Transparent, Colors.Transparent, Array.Empty<byte>(),5,
                        Key.Value));
                }

                break;
        }

        Dispatcher.UIThread.InvokeAsync(() => Model.Bindings.Remove(this));
    }
    public override string ErrorText => "Input is not bound!";
    public override void UpdateBindings()
    {
    }

    public override bool IsCombined => false;
    public override bool IsStrum => false;

    public override SerializedOutput Serialize()
    {
        throw new IncompleteConfigurationException(ErrorText);
    }

    public override string Generate(bool xbox, bool shared, List<int> debounceIndex, bool combined, string extra)
    {
        throw new IncompleteConfigurationException("Unconfigured output");
    }
}