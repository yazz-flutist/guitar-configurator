using System;
using GuitarConfiguratorSharp.NetCore.Configuration;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.ViewModels;

public class AddInputWindowViewModel : ReactiveObject
{
    public Output Output { get; }
    public AddInputWindowViewModel(Output output)
    {
        this.Output = output;
    }
}