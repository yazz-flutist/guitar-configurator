
using System;
using System.Collections.Generic;
using System.Linq;
using GuitarConfiguratorSharp.Utils;

namespace GuitarConfiguratorSharp.Configuration
{
    public abstract class Microcontroller
    {
        public abstract string generateDigitalRead(int pin, bool pull_up);
        public abstract string generateAnalogRead(int pin, int index, int offset, float multiplier, int deadzone);
        public abstract string generateAnalogTriggerRead(int pin, int index, int offset, float multiplier, int deadzone);
        public abstract string generateSkip(bool SPIEnabled, bool I2CEnabled);

        public abstract int getChannel(int pin);

        public abstract string generateInit(IEnumerable<Binding> bindings);
        public abstract int SPI_RX { get; }

        public abstract string GetPin(int pin);

        public abstract int SPI_TX { get; }
        public abstract int SPI_SCK { get; }
        public abstract int SPI_CSn { get; }
        public abstract int I2C_SDA { get; }
        public abstract int I2C_SCL { get; }

        public abstract string getBoard();
        public string generateAnalogReadRaw(IEnumerable<Binding> bindings, int pin) {
            return $"adc_raw({pin})";
        }
    }

    public class Pico : Microcontroller
    {
        private const int GPIO_COUNT = 30;
        private const int PIN_A0 = 26;

        public int SPI_RX_Pico { get; set; } = 0xff;
        public int SPI_TX_Pico { get; set; } = 0xff;
        public int SPI_CSN_Pico { get; set; } = 0xff;
        public int SPI_SCK_Pico { get; set; } = 0xff;
        public int I2C_SDA_Pico { get; set; } = 0xff;
        public int I2C_SCL_Pico { get; set; } = 0xff;

        public override int SPI_RX => SPI_RX_Pico;

        public override int SPI_TX => SPI_TX_Pico;

        public override int SPI_CSn => SPI_CSN_Pico;
        public override int SPI_SCK => SPI_SCK_Pico;

        public override int I2C_SDA => I2C_SDA_Pico;

        public override int I2C_SCL => I2C_SCL_Pico;

        public override string generateDigitalRead(int pin, bool pull_up)
        {
            // Invert on pullup
            if (pull_up)
            {
                return $"(sio_hw->gpio_in & (1 << {pin})) == 0";
            }

            return $"sio_hw->gpio_in & (1 << {pin})";
        }

        public override string generateAnalogRead(int pin, int index, int offset, float multiplier, int deadzone)
        {
            return $"adc({pin - PIN_A0}, {offset}, {(int)(multiplier * 64)}, {deadzone})";
        }

        public override string generateAnalogTriggerRead(int pin, int index, int offset, float multiplier, int deadzone)
        {
            return $"adc_trigger({pin - PIN_A0}, {offset}, {(int)(multiplier * 64)}, {deadzone})";
        }

        public override string generateSkip(bool SPIEnabled, bool I2CEnabled)
        {
            List<int> skippedPins = new List<int>();
            if (SPIEnabled)
            {
                skippedPins.Add(SPI_CSn);
                skippedPins.Add(SPI_RX);
                skippedPins.Add(SPI_TX);
                skippedPins.Add(SPI_SCK);
            }
            if (I2CEnabled)
            {
                skippedPins.Add(I2C_SCL);
                skippedPins.Add(I2C_SDA);
            }
            int skip = 0;
            foreach (var pin in skippedPins)
            {
                if (pin != 0xFF)
                {
                    skip |= 1 << pin;
                }
            }
            return skip.ToString();
        }

        public override string generateInit(IEnumerable<Binding> bindings)
        {
            IEnumerable<DirectDigital> buttons = bindings.FilterCast<Binding, DirectDigital>();
            IEnumerable<DirectAnalog> axes = bindings.FilterCast<Binding, DirectAnalog>();
            string ret = "";
            foreach (var button in buttons)
            {
                bool up = button.PinMode == DevicePinMode.BusKeep || button.PinMode == DevicePinMode.Ground;
                bool down = button.PinMode == DevicePinMode.BusKeep || button.PinMode == DevicePinMode.VCC;
                ret += $"gpio_init({button.Pin});";
                ret += $"gpio_set_dir({button.Pin},false);";
                ret += $"gpio_set_pulls({button.Pin},{up.ToString().ToLower()},{down.ToString().ToLower()}, false);";
            }
            foreach (var axis in axes)
            {
                ret += $"adc_gpio_init({axis.Pin});";
            }
            return ret;
        }

        public override int getChannel(int pin)
        {
            return pin;
        }

        public override string getBoard()
        {
            // TODO: handle other boards
            return "pico";
        }

        public override string GetPin(int pin)
        {
            string ret = $"GP{pin}";
            if (pin >= 26)
            {
                ret += $" / ADC{pin - 26}";
            }
            return ret;
        }
    }

    public abstract class AVRController : Microcontroller
    {
        protected abstract int PIN_A0 { get; }

        public enum AVRPinMode
        {
            INPUT,
            INPUT_PULLDOWN,
            OUTPUT
        }
        public override string generateDigitalRead(int pin, bool pull_up)
        {
            // Invert on pullup
            if (pull_up)
            {
                return $"(PIN{getPort(pin)} & (1 << {getIndex(pin)})) == 0";
            }
            return $"PIN{getPort(pin)} & (1 << {getIndex(pin)})";
        }

        public override string generateAnalogRead(int pin, int index, int offset, float multiplier, int deadzone)
        {
            return $"adc({index}, {offset}, {(int)(multiplier * 64)}, {deadzone})";
        }

        public override string generateAnalogTriggerRead(int pin, int index, int offset, float multiplier, int deadzone)
        {
            return $"adc_trigger({index}, {offset}, {(int)(multiplier * 64)}, {deadzone})";
        }

        public abstract int getIndex(int pin);
        public abstract char getPort(int pin);

        public abstract AVRPinMode? forcedMode(int pin);

        public abstract int PinCount { get; }

        public override string generateSkip(bool SPIEnabled, bool I2CEnabled)
        {
            List<int> skippedPins = new List<int>();
            if (SPIEnabled)
            {
                skippedPins.Add(SPI_CSn);
                skippedPins.Add(SPI_RX);
                skippedPins.Add(SPI_TX);
                skippedPins.Add(SPI_SCK);
            }
            if (I2CEnabled)
            {
                skippedPins.Add(I2C_SCL);
                skippedPins.Add(I2C_SDA);
            }
            Dictionary<char, int> skippedByPort = new Dictionary<char, int>();
            for (var i = 0; i < PinCount; i++)
            {
                if (forcedMode(i) is not null)
                {
                    skippedPins.Add(i);
                }
                skippedByPort[getPort(i)] = 0;
            }

            foreach (var pin in skippedPins)
            {
                skippedByPort[getPort(pin)] |= 1 << getIndex(pin);
            }
            return "{" + string.Join(", ", skippedByPort.Keys.OrderBy(x => x).Select(x => skippedByPort[x].ToString())) + "}";
        }

        public override string generateInit(IEnumerable<Binding> bindings)
        {
            IEnumerable<DirectDigital> buttons = bindings.FilterCast<Binding, DirectDigital>();
            IEnumerable<DirectAnalog> axes = bindings.FilterCast<Binding, DirectAnalog>();
            // DDRx 1 = output, 0 = input
            // PORTx input 1= pullup, 0 = floating
            // TODO: outputs (Start power led?)
            Dictionary<char, int> ddrByPort = new Dictionary<char, int>();
            Dictionary<char, int> portByPort = new Dictionary<char, int>();
            foreach (var button in buttons)
            {
                var port = getPort(button.Pin);
                var idx = getIndex(button.Pin);
                var currentPort = portByPort.GetValueOrDefault(port, 0);
                var currentDDR = ddrByPort.GetValueOrDefault(port, 0);
                if (button.PinMode == DevicePinMode.VCC)
                {
                    currentPort += 1 << idx;
                }
                if (currentPort != 0)
                {
                    portByPort[port] = currentPort;
                }
                ddrByPort[port] = currentDDR;
            }
            for (var i = 0; i < PinCount; i++)
            {
                var force = forcedMode(i);
                var port = getPort(i);
                var idx = getIndex(i);
                if (forcedMode(i) is not null)
                {
                    var currentPort = portByPort.GetValueOrDefault(port, 0);
                    var currentDDR = ddrByPort.GetValueOrDefault(port, 0);
                    switch (force)
                    {
                        case AVRPinMode.INPUT_PULLDOWN:
                            currentPort |= 1 << idx;
                            break;
                        case AVRPinMode.OUTPUT:
                            currentDDR |= 1 << idx;
                            break;
                    }
                    portByPort[port] = currentPort;
                    ddrByPort[port] = currentDDR;
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
            if (pin >= PIN_A0)
            {
                ret += $" / A{pin - PIN_A0}";
            }
            if (pin == SPI_CSn)
            {
                ret += $" / SPI CS";
            }
            if (pin == SPI_RX)
            {
                ret += $" / SPI MISO";
            }
            if (pin == SPI_TX)
            {
                ret += $" / SPI MOSI";
            }
            if (pin == SPI_SCK)
            {
                ret += $" / SPI CLK";
            }
            if (pin == I2C_SCL)
            {
                ret += $" / I2C SCL";
            }
            if (pin == I2C_SDA)
            {
                ret += $" / I2C SDA";
            }
            return ret;
        }
    }

    public class Mega : AVRController
    {
        public override int SPI_RX => 50;

        public override int SPI_TX => 51;

        public override int SPI_CSn => 53;
        public override int SPI_SCK => 52;

        public override int I2C_SDA => 20;

        public override int I2C_SCL => 21;
        private readonly int[] pin_inputs = {
             0     , // PE 0 ** 0 ** USART0_RX	
             1     , // PE 1 ** 1 ** USART0_TX	
             4     , // PE 4 ** 2 ** PWM2	
             5     , // PE 5 ** 3 ** PWM3	
             5     , // PG 5 ** 4 ** PWM4	
             3     , // PE 3 ** 5 ** PWM5	
             3     , // PH 3 ** 6 ** PWM6	
             4     , // PH 4 ** 7 ** PWM7	
             5     , // PH 5 ** 8 ** PWM8	
             6     , // PH 6 ** 9 ** PWM9	
             4     , // PB 4 ** 10 ** PWM10	
             5     , // PB 5 ** 11 ** PWM11	
             6     , // PB 6 ** 12 ** PWM12	
             7     , // PB 7 ** 13 ** PWM13	
             1     , // PJ 1 ** 14 ** USART3_TX	
             0     , // PJ 0 ** 15 ** USART3_RX	
             1     , // PH 1 ** 16 ** USART2_TX	
             0     , // PH 0 ** 17 ** USART2_RX	
             3     , // PD 3 ** 18 ** USART1_TX	
             2     , // PD 2 ** 19 ** USART1_RX	
             1     , // PD 1 ** 20 ** I2C_SDA	
             0     , // PD 0 ** 21 ** I2C_SCL	
             0     , // PA 0 ** 22 ** D22	
             1     , // PA 1 ** 23 ** D23	
             2     , // PA 2 ** 24 ** D24	
             3     , // PA 3 ** 25 ** D25	
             4     , // PA 4 ** 26 ** D26	
             5     , // PA 5 ** 27 ** D27	
             6     , // PA 6 ** 28 ** D28	
             7     , // PA 7 ** 29 ** D29	
             7     , // PC 7 ** 30 ** D30	
             6     , // PC 6 ** 31 ** D31	
             5     , // PC 5 ** 32 ** D32	
             4     , // PC 4 ** 33 ** D33	
             3     , // PC 3 ** 34 ** D34	
             2     , // PC 2 ** 35 ** D35	
             1     , // PC 1 ** 36 ** D36	
             0     , // PC 0 ** 37 ** D37	
             7     , // PD 7 ** 38 ** D38	
             2     , // PG 2 ** 39 ** D39	
             1     , // PG 1 ** 40 ** D40	
             0     , // PG 0 ** 41 ** D41	
             7     , // PL 7 ** 42 ** D42	
             6     , // PL 6 ** 43 ** D43	
             5     , // PL 5 ** 44 ** D44	
             4     , // PL 4 ** 45 ** D45	
             3     , // PL 3 ** 46 ** D46	
             2     , // PL 2 ** 47 ** D47	
             1     , // PL 1 ** 48 ** D48	
             0     , // PL 0 ** 49 ** D49	
             3     , // PB 3 ** 50 ** SPI_MISO	
             2     , // PB 2 ** 51 ** SPI_MOSI	
             1     , // PB 1 ** 52 ** SPI_SCK	
             0     , // PB 0 ** 53 ** SPI_SS	
             0     , // PF 0 ** 54 ** A0	
             1     , // PF 1 ** 55 ** A1	
             2     , // PF 2 ** 56 ** A2	
             3     , // PF 3 ** 57 ** A3	
             4     , // PF 4 ** 58 ** A4	
             5     , // PF 5 ** 59 ** A5	
             6     , // PF 6 ** 60 ** A6	
             7     , // PF 7 ** 61 ** A7	
             0     , // PK 0 ** 62 ** A8	
             1     , // PK 1 ** 63 ** A9	
             2     , // PK 2 ** 64 ** A10	
             3     , // PK 3 ** 65 ** A11	
             4     , // PK 4 ** 66 ** A12	
             5     , // PK 5 ** 67 ** A13	
             6     , // PK 6 ** 68 ** A14	
             7     , // PK 7 ** 69 ** A15	
        };

        public override int PinCount => pin_inputs.Length;

        private static readonly char[] ports = {
            'E'    , // 'E' 0 ** 0 ** USART0_RX	
            'E'    , // 'E' 1 ** 1 ** USART0_TX	
            'E'    , // 'E' 4 ** 2 ** 'W'M2	
            'E'    , // 'E' 5 ** 3 ** 'W'M3	
            'G'    , // 'G' 5 ** 4 ** 'W'M4	
            'E'    , // 'E' 3 ** 5 ** 'W'M5	
            'H'    , // 'H' 3 ** 6 ** 'W'M6	
            'H'    , // 'H' 4 ** 7 ** 'W'M7	
            'H'    , // 'H' 5 ** 8 ** 'W'M8	
            'H'    , // 'H' 6 ** 9 ** 'W'M9	
            'B'    , // 'B' 4 ** 10 ** 'W'M10	
            'B'    , // 'B' 5 ** 11 ** 'W'M11	
            'B'    , // 'B' 6 ** 12 ** 'W'M12	
            'B'    , // 'B' 7 ** 13 ** 'W'M13	
            'J'    , // 'J' 1 ** 14 ** USART3_TX	
            'J'    , // 'J' 0 ** 15 ** USART3_RX	
            'H'    , // 'H' 1 ** 16 ** USART2_TX	
            'H'    , // 'H' 0 ** 17 ** USART2_RX	
            'D'    , // 'D' 3 ** 18 ** USART1_TX	
            'D'    , // 'D' 2 ** 19 ** USART1_RX	
            'D'    , // 'D' 1 ** 20 ** I2C_SDA	
            'D'    , // 'D' 0 ** 21 ** I2C_SCL	
            'A'    , // 'A' 0 ** 22 ** D22	
            'A'    , // 'A' 1 ** 23 ** D23	
            'A'    , // 'A' 2 ** 24 ** D24	
            'A'    , // 'A' 3 ** 25 ** D25	
            'A'    , // 'A' 4 ** 26 ** D26	
            'A'    , // 'A' 5 ** 27 ** D27	
            'A'    , // 'A' 6 ** 28 ** D28	
            'A'    , // 'A' 7 ** 29 ** D29	
            'C'    , // 'C' 7 ** 30 ** D30	
            'C'    , // 'C' 6 ** 31 ** D31	
            'C'    , // 'C' 5 ** 32 ** D32	
            'C'    , // 'C' 4 ** 33 ** D33	
            'C'    , // 'C' 3 ** 34 ** D34	
            'C'    , // 'C' 2 ** 35 ** D35	
            'C'    , // 'C' 1 ** 36 ** D36	
            'C'    , // 'C' 0 ** 37 ** D37	
            'D'    , // 'D' 7 ** 38 ** D38	
            'G'    , // 'G' 2 ** 39 ** D39	
            'G'    , // 'G' 1 ** 40 ** D40	
            'G'    , // 'G' 0 ** 41 ** D41	
            'L'    , // 'L' 7 ** 42 ** D42	
            'L'    , // 'L' 6 ** 43 ** D43	
            'L'    , // 'L' 5 ** 44 ** D44	
            'L'    , // 'L' 4 ** 45 ** D45	
            'L'    , // 'L' 3 ** 46 ** D46	
            'L'    , // 'L' 2 ** 47 ** D47	
            'L'    , // 'L' 1 ** 48 ** D48	
            'L'    , // 'L' 0 ** 49 ** D49	
            'B'    , // 'B' 3 ** 50 ** SPI_MISO	
            'B'    , // 'B' 2 ** 51 ** SPI_MOSI	
            'B'    , // 'B' 1 ** 52 ** SPI_SCK	
            'B'    , // 'B' 0 ** 53 ** SPI_SS	
            'F'    , // 'F' 0 ** 54 ** A0	
            'F'    , // 'F' 1 ** 55 ** A1	
            'F'    , // 'F' 2 ** 56 ** A2	
            'F'    , // 'F' 3 ** 57 ** A3	
            'F'    , // 'F' 4 ** 58 ** A4	
            'F'    , // 'F' 5 ** 59 ** A5	
            'F'    , // 'F' 6 ** 60 ** A6	
            'F'    , // 'F' 7 ** 61 ** A7	
            'K'    , // 'K' 0 ** 62 ** A8	
            'K'    , // 'K' 1 ** 63 ** A9	
            'K'    , // 'K' 2 ** 64 ** A10	
            'K'    , // 'K' 3 ** 65 ** A11	
            'K'    , // 'K' 4 ** 66 ** A12	
            'K'    , // 'K' 5 ** 67 ** A13	
            'K'    , // 'K' 6 ** 68 ** A14	
            'K'    , // 'K' 7 ** 69 ** A15            
        };

        private static readonly char[] port_names = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'L' };

        protected override int PIN_A0 => 54;

        public override int getIndex(int pin)
        {
            return pin_inputs[pin];
        }

        public override char getPort(int pin)
        {
            return ports[pin];
        }

        public override int getChannel(int pin)
        {
            return PIN_A0 - pin;
        }

        public override AVRPinMode? forcedMode(int pin)
        {
            switch (pin)
            {
                case 0:
                    return AVRPinMode.INPUT;
                case 1:
                    return AVRPinMode.OUTPUT;
                case 13:
                    return AVRPinMode.INPUT;
                default:
                    return null;
            }
        }

        public override string getBoard()
        {
            return "mega";
        }
    }



    public class Uno : AVRController
    {
        public override int SPI_RX => 12;

        public override int SPI_TX => 11;

        public override int SPI_CSn => 10;
        public override int SPI_SCK => 13;

        public override int I2C_SDA => 18;

        public override int I2C_SCL => 19;
        private readonly int[] pin_inputs = {
            0, /* 0, port D */
            1, 2, 3, 4, 5, 6,
            7, 0,                                 /* 8, port B */
            1, 2, 3, 4, 5, 0, /* 14, port C */
            1, 2, 3, 4, 5
        };

        private static readonly char[] ports = {
            'D',                             /* 0 */
            'D', 'D', 'D', 'D', 'D', 'D', 'D', 'B', /* 8 */
            'B', 'B', 'B', 'B', 'B', 'C',         /* 14 */
            'C', 'C', 'C', 'C', 'C'
        };
        private static readonly char[] port_names = { 'B', 'C', 'D' };
        protected override int PIN_A0 => 14;

        public override int PinCount => pin_inputs.Length;

        public override int getIndex(int pin)
        {
            return pin_inputs[pin];
        }

        public override char getPort(int pin)
        {
            return ports[pin];
        }

        public override int getChannel(int pin)
        {
            return PIN_A0 - pin;
        }

        public override AVRPinMode? forcedMode(int pin)
        {
            switch (pin)
            {
                case 0:
                    return AVRPinMode.INPUT;
                case 1:
                    return AVRPinMode.OUTPUT;
                case 13:
                    return AVRPinMode.INPUT;
                default:
                    return null;
            }
        }

        public override string getBoard()
        {
            return "uno";
        }
    }

    public class Micro : AVRController
    {
        public override int SPI_RX => 14;

        public override int SPI_TX => 16;

        public override int SPI_CSn => 17;
        public override int SPI_SCK => 15;

        public override int I2C_SDA => 2;

        public override int I2C_SCL => 3;
        private readonly int[] pin_index = {
            2, // D0 - PD2
            3,	// D1 - PD3
            1, // D2 - PD1
            0,	// D3 - PD0
            4,	// D4 - PD4
            6, // D5 - PC6
            7, // D6 - PD7
            6, // D7 - PE6
            
            4, // D8 - PB4
            5,	// D9 - PB5
            6, // D10 - PB6
            7,	// D11 - PB7
            6, // D12 - PD6
            7, // D13 - PC7
            
            3,	// D14 - MISO - PB3
            1,	// D15 - SCK - PB1
            2,	// D16 - MOSI - PB2
            0,	// D17 - SS - PB0
            
            7,	// D18 - A0 - PF7
            6, // D19 - A1 - PF6
            5, // D20 - A2 - PF5
            4, // D21 - A3 - PF4
            1, // D22 - A4 - PF1
            0, // D23 - A5 - PF0
            
            4, // D24 / D4 - A6 - PD4
            7, // D25 / D6 - A7 - PD7
            4, // D26 / D8 - A8 - PB4
            5, // D27 / D9 - A9 - PB5
            6, // D28 / D10 - A10 - PB6
            6, // D29 / D12 - A11 - PD6
            5, // D30 / TX Led - PD5	
        };

        public override int PinCount => pin_index.Length;

        private static readonly char[] ports = {
            'D', // D0 - 'D'2
            'D',	// D1 - 'D'3
            'D', // D2 - 'D'1
            'D',	// D3 - 'D'0
            'D',	// D4 - 'D'4
            'C', // D5 - 'C'6
            'D', // D6 - 'D'7
            'E', // D7 - 'E'6

            'B', // D8 - 'B'4
            'B',	// D9 - 'B'5
            'B', // D10 - 'B'6
            'B',	// D11 - 'B'7
            'D', // D12 - 'D'6
            'C', // D13 - 'C'7

            'B',	// D14 - MISO - 'B'3
            'B',	// D15 - SCK - 'B'1
            'B',	// D16 - MOSI - 'B'2
            'B',	// D17 - SS - 'B'0

            'F',	// D18 - A0 - 'F'7
            'F', // D19 - A1 - 'F'6
            'F', // D20 - A2 - 'F'5
            'F', // D21 - A3 - 'F'4
            'F', // D22 - A4 - 'F'1
            'F', // D23 - A5 - 'F'0

            'D', // D24 / D4 - A6 - 'D'4
            'D', // D25 / D6 - A7 - 'D'7
            'B', // D26 / D8 - A8 - 'B'4
            'B', // D27 / D9 - A9 - 'B'5
            'B', // D28 / D10 - A10 - 'B'6
            'D', // D29 / D12 - A11 - 'D'6
            'D' // D30 / TX Led - 'D'5            
        };
        private static readonly char[] port_names = { 'B', 'C', 'D', 'E', 'F' };

        private static readonly int[] channels = {
            7,	// A0				PF7					ADC7
            6,	// A1				PF6					ADC6	
            5,	// A2				PF5					ADC5	
            4,	// A3				PF4					ADC4
            1,	// A4				PF1					ADC1	
            0,	// A5				PF0					ADC0	
            8,	// A6		D4		PD4					ADC8
            10,	// A7		D6		PD7					ADC10
            11,	// A8		D8		PB4					ADC11
            12,	// A9		D9		PB5					ADC12
            13,	// A10		D10		PB6					ADC13
            9	// A11		D12		PD6					ADC9
        };

        protected override int PIN_A0 => 18;

        public override int getIndex(int pin)
        {
            return pin_index[pin];
        }

        public override char getPort(int pin)
        {
            return ports[pin];
        }
        public override int getChannel(int pin)
        {
            return channels[pin - PIN_A0];
        }

        public override AVRPinMode? forcedMode(int pin)
        {
            return null;
        }

        public override string getBoard()
        {
            // TODO: handle other boards
            return "micro";
        }
    }
}