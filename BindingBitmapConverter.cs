using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GuitarConfiguratorSharp.NetCore.Configuration;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.ViewModels;

namespace GuitarConfiguratorSharp.NetCore;

/// <summary>
/// <para>
/// Converts a string path to a bitmap asset.
/// </para>
/// <para>
/// The asset must be in the same assembly as the program. If it isn't,
/// specify "avares://<assemblynamehere>/" in front of the path to the asset.
/// </para>
/// </summary>
public class BindingBitmapConverter : IMultiValueConverter
{
    public static BindingBitmapConverter instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[1] is UnsetValueType || values[0] is UnsetValueType)
            return null;
        string? imageValue = null;
        DeviceControllerType type = (DeviceControllerType) values[0];
        String name = (string) values[1]!;
        if (type == DeviceControllerType.Gamepad)
        {
            imageValue = $"Others/Xbox360/360_{name}.png";
        }
        else if (type == DeviceControllerType.Guitar)
        {
            imageValue = $"GH/{name}.png";
        }

        if (imageValue == null)
            return null;

        Uri uri;

        string assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        uri = new Uri($"avares://{assemblyName}/Assets/Icons/{imageValue}");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        var asset = assets!.Open(uri);

        return new Bitmap(asset);
    }

    public object[] ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}