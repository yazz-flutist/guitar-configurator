using System.Collections.Generic;
using System.Linq;

namespace GuitarConfiguratorSharp.NetCore.Configuration.Microcontroller;

public abstract class AvrController : Microcontroller
{
    protected abstract int PinA0 { get; }

    public enum AvrPinMode
    {
        INPUT,
        INPUT_PULLDOWN,
        OUTPUT
    }

    public override string GenerateDigitalRead(int pin, bool pullUp)
    {
        // Invert on pullup
        if (pullUp)
        {
            return $"(PIN{GetPort(pin)} & (1 << {GetIndex(pin)})) == 0";
        }

        return $"PIN{GetPort(pin)} & (1 << {GetIndex(pin)})";
    }

    public abstract int GetIndex(int pin);
    public abstract char GetPort(int pin);

    public abstract AvrPinMode? ForcedMode(int pin);

    public abstract int PinCount { get; }

    public override string GenerateSkip(bool spiEnabled, bool i2CEnabled)
    {
        List<int> skippedPins = new List<int>();
        if (spiEnabled)
        {
            skippedPins.Add(SpiCSn);
            skippedPins.Add(SpiRx);
            skippedPins.Add(SpiTx);
            skippedPins.Add(SpiSck);
        }

        if (i2CEnabled)
        {
            skippedPins.Add(I2CScl);
            skippedPins.Add(I2CSda);
        }

        Dictionary<char, int> skippedByPort = new Dictionary<char, int>();
        for (var i = 0; i < PinCount; i++)
        {
            if (ForcedMode(i) is not null)
            {
                skippedPins.Add(i);
            }

            skippedByPort[GetPort(i)] = 0;
        }

        foreach (var pin in skippedPins)
        {
            skippedByPort[GetPort(pin)] |= 1 << GetIndex(pin);
        }

        return "{" + string.Join(", ", skippedByPort.Keys.OrderBy(x => x).Select(x => skippedByPort[x].ToString())) +
               "}";
    }

    public override string GenerateInit(List<Output> bindings)
    {
        // DDRx 1 = output, 0 = input
        // PORTx input 1= pullup, 0 = floating
        // TODO: outputs (Start power led?)
        Dictionary<char, int> ddrByPort = new Dictionary<char, int>();
        Dictionary<char, int> portByPort = new Dictionary<char, int>();
        foreach (var output in bindings)
        {
            if (output.Input?.InnermostInput() is DirectInput direct)
            {
                if (!direct.IsAnalog)
                {
                    var port = GetPort(direct.Pin);
                    var idx = GetIndex(direct.Pin);
                    var currentPort = portByPort.GetValueOrDefault(port, 0);
                    var currentDdr = ddrByPort.GetValueOrDefault(port, 0);
                    if (direct.PinMode == DevicePinMode.PullUp)
                    {
                        currentPort += 1 << idx;
                    }

                    if (currentPort != 0)
                    {
                        portByPort[port] = currentPort;
                    }

                    ddrByPort[port] = currentDdr;
                }
            }
        }

        for (var i = 0; i < PinCount; i++)
        {
            var force = ForcedMode(i);
            var port = GetPort(i);
            var idx = GetIndex(i);
            if (ForcedMode(i) is not null)
            {
                var currentPort = portByPort.GetValueOrDefault(port, 0);
                var currentDdr = ddrByPort.GetValueOrDefault(port, 0);
                switch (force)
                {
                    case AvrPinMode.INPUT_PULLDOWN:
                        currentPort |= 1 << idx;
                        break;
                    case AvrPinMode.OUTPUT:
                        currentDdr |= 1 << idx;
                        break;
                }

                portByPort[port] = currentPort;
                ddrByPort[port] = currentDdr;
            }
        }

        string ret = "uint8_t oldSREG = SREG;cli();";
        foreach (var port in portByPort)
        {
            ret += $"PORT{port.Key} = {port.Value};";
        }

        foreach (var port in ddrByPort)
        {
            ret += $"DDR{port.Key} = {port.Value};";
        }

        ret += "SREG = oldSREG;";
        return ret;
    }

    public override string GetPin(int pin)
    {
        string ret = $"{pin}";
        if (pin >= PinA0)
        {
            ret += $" / A{pin - PinA0}";
        }

        if (pin == SpiCSn)
        {
            ret += $" / SPI CS";
        }

        if (pin == SpiRx)
        {
            ret += $" / SPI MISO";
        }

        if (pin == SpiTx)
        {
            ret += $" / SPI MOSI";
        }

        if (pin == SpiSck)
        {
            ret += $" / SPI CLK";
        }

        if (pin == I2CScl)
        {
            ret += $" / I2C SCL";
        }

        if (pin == I2CSda)
        {
            ret += $" / I2C SDA";
        }

        return ret;
    }
}