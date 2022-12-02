using System.Collections.Generic;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.Serialization;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Leds;

public class LedOutput : CombinedOutput
{
    public LedOutput(ConfigViewModel model) : base(model, null, "Leds")
    {
    }

    public override bool IsCombined => true;
    public override string? GetLocalisedName()
    {
        return Name;
    }

    public override bool IsStrum => false;
    public override SerializedOutput Serialize()
    {
        throw new System.NotImplementedException();
    }

    public override string Generate(bool xbox, bool shared, List<int> debounceIndex, bool combined, string extra)
    {
        return "";
    }
}