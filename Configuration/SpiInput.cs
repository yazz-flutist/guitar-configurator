using System;
using System.Collections.Generic;
using System.Linq;
using GuitarConfigurator.NetCore.Configuration.Microcontrollers;
using GuitarConfigurator.NetCore.ViewModels;
using ReactiveUI;

namespace GuitarConfigurator.NetCore.Configuration;

public abstract class SpiInput : Input, ISpi
{
    protected readonly Microcontroller Microcontroller;
    protected SpiInput(Microcontroller microcontroller, string spiType, uint spiFreq, bool cpol,
        bool cpha, bool msbFirst, ConfigViewModel model, int? miso = null, int? mosi = null, int? sck = null) : base(model)
    {
        Microcontroller = microcontroller;
        _spiType = spiType;
        var config = microcontroller.GetSpiForType(_spiType);
        if (config != null)
        {
            _spiConfig = config;
        }
        else
        {

            if (miso == null || mosi == null || sck == null)
            {
                var pins = microcontroller.SpiPins(_spiType);
                miso = pins.First(pair => pair.Value is SpiPinType.Miso).Key;
                mosi = pins.First(pair => pair.Value is SpiPinType.Mosi).Key;
                sck = pins.First(pair => pair.Value is SpiPinType.Sck).Key;
            }

            _spiConfig = microcontroller.AssignSpiPins(model, _spiType, mosi.Value, miso.Value, sck.Value, cpol, cpha, msbFirst, spiFreq)!;
        }

        this.WhenAnyValue(x => x._spiConfig.Miso).Subscribe(_ => this.RaisePropertyChanged(nameof(Miso)));
        this.WhenAnyValue(x => x._spiConfig.Mosi).Subscribe(_ => this.RaisePropertyChanged(nameof(Mosi)));
        this.WhenAnyValue(x => x._spiConfig.Sck).Subscribe(_ => this.RaisePropertyChanged(nameof(Sck)));
    }

    private string _spiType;

    private readonly SpiConfig _spiConfig;
    
    public override IReadOnlyList<string> RequiredDefines()
    {
        return new[] {$"{_spiType.ToUpper()}_SPI_PORT {_spiConfig.Definition}"};
    }
    
    public int Mosi
    {
        get => _spiConfig.Mosi;
        set => _spiConfig.Mosi = value;
    }

    public int Miso
    {
        get => _spiConfig.Miso;
        set => _spiConfig.Miso = value;
    }

    public int Sck
    {
        get => _spiConfig.Sck;
        set => _spiConfig.Sck = value;
    }

    public List<int> AvailableMosiPins => GetMosiPins();
    public List<int> AvailableMisoPins => GetMisoPins();
    public List<int> AvailableSckPins => GetSckPins();

    private List<int> GetMosiPins()
    {
        return Microcontroller.SpiPins(_spiType)
            .Where(s => s.Value is SpiPinType.Mosi)
            .Select(s => s.Key).ToList();
    }

    private List<int> GetMisoPins()
    {
        return Microcontroller.SpiPins(_spiType)
            .Where(s => s.Value is SpiPinType.Miso)
            .Select(s => s.Key).ToList();
    }

    private List<int> GetSckPins()
    {
        return Microcontroller.SpiPins(_spiType)
            .Where(s => s.Value is SpiPinType.Sck)
            .Select(s => s.Key).ToList();
    }

    public override void Dispose()
    {
        Microcontroller.UnAssignPins(_spiType);
    }

    public override IList<PinConfig> PinConfigs => new List<PinConfig>() {_spiConfig};
    public List<int> SpiPins() => new() {Mosi, Miso, Sck};
}