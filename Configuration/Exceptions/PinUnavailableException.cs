using System;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Exceptions;

public class PinUnavailableException: Exception
{
    public PinUnavailableException(string message): base(message)
    {
    }
}