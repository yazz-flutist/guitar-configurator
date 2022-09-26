using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GuitarConfiguratorSharp.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Views
{
    public partial class AddInputWindow : ReactiveWindow<AddInputWindowViewModel>
    {
        public AddInputWindow()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}