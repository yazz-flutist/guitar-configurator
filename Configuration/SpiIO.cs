using System.Collections.Generic;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public interface ISpi
{
    public List<int> SpiPins();
}