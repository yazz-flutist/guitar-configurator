using System;
using System.Linq;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Microcontroller;

public class PicoTwiConfig : TwiConfig
{
    public PicoTwiConfig(string type, int sda, int scl, int clock) : base(type, sda, scl, clock
    )
    {
    }

    public int Index => Pico.TwiIndexByPin[_sda];
    public override string Definition => $"TWI_{Index}";
    // ReSharper disable ExplicitCallerInfoArgument
    protected override void UpdatePins(string field)
    {
        var indexSda = Pico.TwiIndexByPin[_sda];
        var indexScl = Pico.TwiIndexByPin[_scl];
        if (indexSda != indexScl)
        {
            switch (field)
            {
                case "sda":
                    this.RaiseAndSetIfChanged(ref _scl,
                        Pico.TwiIndexByPin.OrderBy(x => Math.Abs(x.Key - _sda)).First(x => x.Value == indexSda && Pico.TwiTypeByPin[x.Key] == TwiPinType.SCL).Key, "Scl");
                    break;
                case "scl":
                    this.RaiseAndSetIfChanged(ref _sda,
                        Pico.TwiIndexByPin.OrderBy(x => Math.Abs(x.Key - _scl)).First(x => x.Value == indexScl && Pico.TwiTypeByPin[x.Key] == TwiPinType.SDA).Key, "Sda");
                    break;
            } 
        }
    }
}