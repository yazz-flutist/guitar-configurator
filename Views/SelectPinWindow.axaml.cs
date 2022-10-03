using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Views
{
    public partial class SelectPinWindow : ReactiveWindow<SelectPinWindowViewModel>
    {
        public SelectPinWindow()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}