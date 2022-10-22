using System.Reactive;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.ViewModels;

public class ShowUnoShortWindowViewModel : ReactiveObject
{
    public Arduino Input { get; }
    public ShowUnoShortWindowViewModel(Arduino input)
    {
        Input = input;
    }
}