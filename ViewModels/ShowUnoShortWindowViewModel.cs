using System;
using System.IO;
using System.Reactive;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.ViewModels;

public class ShowUnoShortWindowViewModel : ReactiveObject
{
    public Arduino Input { get; }
    public Bitmap? DFUImage { get; }
    public ShowUnoShortWindowViewModel(Arduino input)
    {
        Input = input;
        DFUImage = GetImage();
    }
    
    public Bitmap? GetImage()
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        string? bitmap = Input.Board.ArdwiinoName switch
        {
            "mega2560" => "ArduinoMegaDFU.png",
            "megaadk" => "ArduinoMegaADKDFU.png",
            "uno" => "ArduinoUnoDFU.png",
            _ => null
        };

        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        try
        {
            return new Bitmap(assets!.Open(new Uri($"avares://{assemblyName}/Assets/Images/{bitmap}")));
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}