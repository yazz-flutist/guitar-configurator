using Avalonia.Media;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public abstract class GroupableButton : Button
{
    protected GroupableButton(Microcontroller.Microcontroller controller, InputControllerType inputType, int debounce, IOutputButton type, Color ledOn, Color ledOff) : base(controller, inputType, debounce, type, ledOn, ledOff)
    {
    }

    public abstract StandardButtonType StandardButton { get; }
}