using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Json;
public abstract class JsonInput
{
    public abstract Input Generate(Microcontroller microcontroller);
}