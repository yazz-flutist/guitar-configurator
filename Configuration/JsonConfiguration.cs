using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using GuitarConfiguratorSharp.NetCore.Configuration.Conversions;
using GuitarConfiguratorSharp.NetCore.Configuration.DJ;
using GuitarConfiguratorSharp.NetCore.Configuration.Neck;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public class JsonConfiguration
{
    public LedType LedType;
    public bool TiltEnabled;
    public bool XInputOnWindows;
    public bool CombinedDebounce;
    public DeviceControllerType DeviceType;
    public EmulationType EmulationType;
    public RhythmType RhythmType;
    public List<Output> Bindings;
    public int WtPin;

    [JsonConstructor]
    public JsonConfiguration(List<Output> bindings, LedType ledType, DeviceControllerType deviceType,
        EmulationType emulationType,
        RhythmType rhythmType, bool tiltEnabled, bool xInputOnWindows)
    {
        this.Bindings = bindings;
        this.LedType = ledType;
        this.DeviceType = deviceType;
        this.EmulationType = emulationType;
        this.RhythmType = rhythmType;
        this.TiltEnabled = tiltEnabled;
        this.XInputOnWindows = xInputOnWindows;
    }


    public static JsonSerializerOptions GetJsonOptions(Microcontroller.Microcontroller? controller)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.SetupExtensions();
        DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
        registry.RegisterType<DjInput>();
        registry.RegisterType<Gh5NeckInput>();
        registry.RegisterType<GhWtTapInput>();
        registry.RegisterType<AnalogToDigital>();
        registry.RegisterType<DigitalToAnalog>();
        registry.RegisterType<ControllerAxis>();
        registry.RegisterType<ControllerButton>();
        registry.RegisterType<KeyboardButton>();
        registry.RegisterType<Ps2Input>();
        registry.RegisterType<WiiInput>();
        registry.RegisterType<DirectInput>();
        return options;
    }

    public void LoadConfiguration(ConfigViewModel model)
    {
        model.CombinedDebounce = CombinedDebounce;
        model.DeviceType = DeviceType;
        model.EmulationType = EmulationType;
        model.RhythmType = RhythmType;
        model.LedType = LedType;
        model.Bindings.Clear();
        model.Bindings.AddRange(Bindings);
    }
}