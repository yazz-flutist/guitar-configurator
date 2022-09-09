using Avalonia.Media;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public abstract class GroupableAxis : Axis
{
    protected GroupableAxis(Microcontroller.Microcontroller controller, InputControllerType inputType, IOutputAxis type, Color ledOn, Color ledOff, float multiplier, int offset, int deadzone, bool trigger) : base(controller, inputType, type, ledOn, ledOff, multiplier, offset, deadzone, trigger)
    {
    }

    public abstract StandardAxisType StandardAxis { get; }
}