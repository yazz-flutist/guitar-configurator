using System;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using GuitarConfigurator.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfigurator.NetCore.Views
{
    public class UnoShortWindow : ReactiveWindow<ShowUnoShortWindowViewModel>
    {
        public UnoShortWindow()
        {
            this.WhenActivated(disposables => disposables(ViewModel!.Input.DfuDetected.ObserveOn(RxApp.MainThreadScheduler).Subscribe(s => Close())));
            AvaloniaXamlLoader.Load(this);
        }
    }
}