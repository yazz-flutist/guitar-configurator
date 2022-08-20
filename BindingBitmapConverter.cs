using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GuitarConfiguratorSharp.Configuration;

namespace GuitarConfiguratorSharp;

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
    public static BindingBitmapConverter Instance = new BindingBitmapConverter();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[1] is Avalonia.UnsetValueType || values[0] is Avalonia.UnsetValueType)
            return null;
        string? imageValue = null;
        object value = values[1]!;
        DeviceControllerType type = (DeviceControllerType)values[0]!;
        // we need to do something to make it observable or some shit
        // https://stackoverflow.com/questions/58743/databinding-an-enum-property-to-a-combobox-in-wpf
        // Look at the second answer here.
        var hat = value as GenericControllerHat;
        if (hat != null)
        {
            var name = Enum.GetName(typeof(StandardButtonType), hat.Type);
            if (type == DeviceControllerType.Gamepad)
            {
                imageValue = $"Others/Xbox360/360_Dpad_{name}.png";
            }
            else if (type == DeviceControllerType.Guitar)
            {
                imageValue = $"GH/{name}.png";
            }
        }
        var button = value as GenericControllerButton;
        if (button != null)
        {
            var name = Enum.GetName(typeof(StandardButtonType), button.Type);
            if (type == DeviceControllerType.Gamepad)
            {
                // imageValue = $"GH/{name}.png";
            }
            else if (type == DeviceControllerType.Guitar)
            {
                imageValue = $"GH/{name}.png";
            }
        }
        var axis = value as GenericAxis;
        if (axis != null)
        {
            var name = Enum.GetName(typeof(StandardAxisType), axis.Type);
            if (type == DeviceControllerType.Gamepad)
            {
                // imageValue = $"GH/{name}.png";
            }
            else if (type == DeviceControllerType.Guitar)
            {
                imageValue = $"GH/{name}.png";
            }
        }


        var xboxButton = value as XboxControllerButton;
        if (xboxButton != null)
        {

        }
        var xboxAxis = value as XboxAxis;
        if (xboxAxis != null)
        {

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