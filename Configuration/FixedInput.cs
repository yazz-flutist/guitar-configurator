using System;
using System.Collections.Generic;
using System.Linq;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.Configuration.Outputs;
using GuitarConfigurator.NetCore.Configuration.Serialization;
using GuitarConfigurator.NetCore.Configuration.Types;
using GuitarConfigurator.NetCore.ViewModels;

namespace GuitarConfigurator.NetCore.Configuration;

public class FixedInput : Input
{
    private int Value { get; }

    public FixedInput(ConfigViewModel model, int value) : base(model)
    {
        Value = value;
        IsAnalog = true;
    }

    public override IReadOnlyList<string> RequiredDefines()
    {
        return Array.Empty<string>();
    }

    public override string Generate(bool xbox)
    {
        return Value.ToString();
    }

    public override SerializedInput Serialise()
    {
        throw new NotImplementedException();
    }
    public override bool IsUint => true;
    public override IList<DevicePin> Pins => Array.Empty<DevicePin>();
    public override IList<PinConfig> PinConfigs => Array.Empty<PinConfig>();
    public override InputType? InputType => null;

    public override void Update(List<Output> modelBindings, Dictionary<int, int> analogRaw, Dictionary<int, bool> digitalRaw, byte[] ps2Raw, byte[] wiiRaw,
        byte[] djLeftRaw, byte[] djRightRaw, byte[] gh5Raw, byte[] ghWtRaw, byte[] ps2ControllerType,
        byte[] wiiControllerType)
    {
    }

    public override string GenerateAll(List<Output> allBindings, List<Tuple<Input, string>> bindings, bool shared,
        bool xbox)
    {
        return string.Join(";\n", bindings.Select(binding => binding.Item2));
    }

    public override void Dispose()
    {
    }
}