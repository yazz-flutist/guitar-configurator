using Avalonia.Media;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;

public abstract class JsonOutput
{
    public abstract JsonInput? Input { get; }
    public abstract Color LedOn { get; }
    public abstract Color LedOff { get; }
    public abstract Output Generate(ConfigViewModel model, Microcontroller microcontroller);
}