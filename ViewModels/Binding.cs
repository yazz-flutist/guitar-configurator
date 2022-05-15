using Avalonia.Media;

namespace GuitarConfiguratorSharp.ViewModels;

public class PinBinding
{
    public PinBinding(int pin, string name, string image, string? option, bool optionValue, int debounce, bool isAnalog, Color ledOn, Color ledOff)
    {
        Pin = pin;
        Name = name;
        Image = image;
        Calibration = "test";
        OptionDescription = option;
        Debounce = debounce;
        LedOn = ledOn;
        LedOff = ledOff;
        OptionValue = optionValue;
        IsAnalog = isAnalog;
    }

    public int Pin { get; set; }
    public string Name { get; set; }
    
    public string Image { get; set; }
    
    public string Calibration { get; set; }
    
    public int Debounce { get; set; }
    
    public string? OptionDescription { get; set; }
    public bool OptionValue { get; set; }

    public bool IsAnalog { get; set; }
    
    public bool HasOption => OptionDescription != null;
    
    public Color LedOn {
        get;
        set;
    }
    public Color LedOff {
        get;
        set;
    }
}