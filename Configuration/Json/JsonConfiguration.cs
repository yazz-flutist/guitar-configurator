using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;
public class JsonConfiguration
{
    public LedType LedType { get; }
    public bool XInputOnWindows { get; }
    public bool CombinedDebounce { get; }
    public DeviceControllerType DeviceType { get; }
    public EmulationType EmulationType { get; }
    public RhythmType RhythmType { get; }
    public List<JsonOutput> Bindings { get; }

    [JsonConstructor]
    public JsonConfiguration(List<JsonOutput> bindings, LedType ledType, DeviceControllerType deviceType,
        EmulationType emulationType,
        RhythmType rhythmType, bool xInputOnWindows, bool combinedDebounce)
    {
        Bindings = bindings;
        LedType = ledType;
        DeviceType = deviceType;
        EmulationType = emulationType;
        RhythmType = rhythmType;
        XInputOnWindows = xInputOnWindows;
        CombinedDebounce = combinedDebounce;
    }

    public JsonConfiguration(ConfigViewModel model)
    {
        Bindings = model.Bindings.Select(s => s.GetJson()).ToList();
        LedType = model.LedType;
        DeviceType = model.DeviceType;
        EmulationType = model.EmulationType;
        RhythmType = model.RhythmType;
        XInputOnWindows = model.XInputOnWindows;
        CombinedDebounce = model.CombinedDebounce;
    }

    public static JsonSerializerOptions GetJsonOptions(ConfigViewModel model)
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.IncludeFields = false;
        options.IgnoreReadOnlyFields = false;
        options.IgnoreReadOnlyProperties = false;
        options.SetupExtensions();
        DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
        registry.RegisterType<JsonAnalogToDigital>();
        registry.RegisterType<JsonControllerAxis>();
        registry.RegisterType<JsonControllerButton>();
        registry.RegisterType<JsonDigitalToAnalog>();
        registry.RegisterType<JsonDirectInput>();
        registry.RegisterType<JsonDjCombinedOutput>();
        registry.RegisterType<JsonDjInput>();
        registry.RegisterType<JsonGh5CombinedOutput>();
        registry.RegisterType<JsonGh5NeckInput>();
        registry.RegisterType<JsonGhwtCombinedOutput>();
        registry.RegisterType<JsonGhWtInput>();
        registry.RegisterType<JsonKeyboardButton>();
        registry.RegisterType<JsonMouseAxis>();
        registry.RegisterType<JsonMouseButton>();
        registry.RegisterType<JsonPs2CombinedOutput>();
        registry.RegisterType<JsonPs2Input>();
        registry.RegisterType<JsonWiiCombinedOutput>();
        registry.RegisterType<JsonWiiInput>();
        return options;
    }

    public void LoadConfiguration(ConfigViewModel model)
    {
        model.CombinedDebounce = CombinedDebounce;
        model.DeviceType = DeviceType;
        model.EmulationType = EmulationType;
        model.RhythmType = RhythmType;
        model.LedType = LedType;
        model.XInputOnWindows = XInputOnWindows;
        model.Bindings.Clear();
        model.Bindings.AddRange(Bindings.Select(s => s.Generate(model, model.MicroController!)));
    }
}