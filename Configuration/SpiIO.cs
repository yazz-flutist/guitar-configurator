using System.Collections.Generic;

namespace GuitarConfigurator.NetCore.Configuration;

public interface ISpi
{
    public List<int> SpiPins();
}