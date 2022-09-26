using System;
using System.Collections.Generic;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public interface IInput
{
    public IReadOnlyList<string> RequiredDefines();
    public string Generate(bool xbox, Microcontroller.Microcontroller controller);

    public bool IsAnalog { get; }

    public bool RequiresSpi();

    public bool RequiresI2C();

    public IInput InnermostInput()
    {
        return this;
    }

    public string GenerateAll(bool xbox, List<Tuple<IInput, string>> bindings,
        Microcontroller.Microcontroller controller);
}