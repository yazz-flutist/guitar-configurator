using System;
using System.Collections.Generic;
using System.Linq;
using GuitarConfiguratorSharp.NetCore.Configuration.Exceptions;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Configuration;

public abstract class SpiInput : Input
{
    protected readonly Microcontroller Microcontroller;
    protected SpiInput(Microcontroller microcontroller, string spiType, int spiFreq, bool cpol,
        bool cpha, bool msbFirst, int? miso = null, int? mosi = null, int? sck = null)
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
                if (!pins.Any())
                {
                    throw new PinUnavailableException("No SPI Pins Available!");
                }
                miso = pins.First(pair => pair.Value is SpiPinType.Miso).Key;
                mosi = pins.First(pair => pair.Value is SpiPinType.Mosi).Key;
                sck = pins.First(pair => pair.Value is SpiPinType.Sck).Key;
            }

            _spiConfig = microcontroller.AssignSpiPins(_spiType, mosi.Value, miso.Value, sck.Value, cpol, cpha, msbFirst, spiFreq)!;
        }

        this.WhenAnyValue(x => x._spiConfig.Miso).Subscribe(_ => this.RaisePropertyChanged(nameof(Miso)));
        this.WhenAnyValue(x => x._spiConfig.Mosi).Subscribe(_ => this.RaisePropertyChanged(nameof(Mosi)));
        this.WhenAnyValue(x => x._spiConfig.Sck).Subscribe(_ => this.RaisePropertyChanged(nameof(Sck)));
        microcontroller.PinConfigs.CollectionChanged +=
            (_, _) =>
            {
                var mosi2 = Mosi;
                var miso2 = Miso;
                var sck2 = Sck;
                this.RaisePropertyChanged(nameof(AvailableMosiPins));
                this.RaisePropertyChanged(nameof(AvailableMisoPins));
                this.RaisePropertyChanged(nameof(AvailableSckPins));
                Mosi = mosi2;
                Miso = miso2;
                Sck = sck2;
            };
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

    public override List<PinConfig> PinConfigs => new() {_spiConfig};
}