using GuitarConfiguratorSharp.NetCore.Configuration;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.ViewModels;

public class SelectPinWindowViewModel : ReactiveObject
{
    public InputWithPin Input { get; }
    public SelectPinWindowViewModel(InputWithPin input)
    {
        Input = input;
    }
}