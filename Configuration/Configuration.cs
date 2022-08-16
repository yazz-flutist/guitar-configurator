using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media;
using GuitarConfiguratorSharp.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using System.Text;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;

namespace GuitarConfiguratorSharp.Configuration
{
    public enum EmulationType
    {
        Universal,
        XInput,
        Keyboard_Mouse,
        Midi
    }
    public enum DeviceType
    {
        Gamepad = 1,
        Guitar = 7,
        Drum = 9,
        TurnTable = 23
    }
    public enum RhythmType
    {
        GuitarHero,
        RockBand,
        Live
    }
    public enum InputControllerType
    {
        None,
        Wii,
        Direct,
        PS2,
        USB_Passthrough
    }
    public enum TiltType
    {
        None,
        MPU_6050,
        Digital_Mercury,
        Analogue,
        ADXL_3xx
    }
    public enum TiltOrientation
    {
        X,
        Y,
        Z
    }
    public enum LedType
    {
        None,
        APA102
    }
    public class DeviceConfiguration
    {

        public LedType LedType { get; set; }
        public bool TiltEnabled { get; set; }
        public InputControllerType InputControllerType { get; set; }
        public DeviceType DeviceType { get; set; }
        public EmulationType EmulationType { get; set; }
        public RhythmType RhythmType { get; set; }
        public Microcontroller MicroController { get; set; }
        public TiltOrientation TiltOrientation { get; set; }


        public List<Binding> Bindings { get; }

        [JsonConstructorAttribute]
        public DeviceConfiguration(Microcontroller microcontroller, List<Binding> bindings, LedType ledType, DeviceType deviceType, EmulationType emulationType, RhythmType rhythmType, bool tiltEnabled)
        {
            this.Bindings = bindings;
            this.MicroController = microcontroller;
            this.LedType = ledType;
            this.DeviceType = deviceType;
            this.EmulationType = emulationType;
            this.RhythmType = rhythmType;
            this.TiltEnabled = tiltEnabled;
        }

        public DeviceConfiguration(Microcontroller microcontroller)
        {
            MicroController = microcontroller;
            this.Bindings = new List<Binding>();
            this.LedType = LedType.None;
            this.DeviceType = DeviceType.Gamepad;
            this.EmulationType = EmulationType.Universal;
            this.RhythmType = RhythmType.GuitarHero;
            this.TiltEnabled = false;
        }

        public static JsonSerializerOptions getJSONOptions(Microcontroller? controller)
        {

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new MicrocontrollerJsonConverter(controller));
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterType<DirectGHFiveTarBarAnalog>();
            registry.RegisterType<DirectGHFiveTarBarButton>();
            registry.RegisterType<DirectGHWTBarAnalog>();
            registry.RegisterType<DirectGHWTBarButton>();
            registry.RegisterType<AnalogToDigital>();
            registry.RegisterType<DigitalToAnalog>();
            registry.RegisterType<XboxControllerButton>();
            registry.RegisterType<GenericControllerButton>();
            registry.RegisterType<GenericControllerHat>();
            registry.RegisterType<GenericAxis>();
            registry.RegisterType<XboxAxis>();
            registry.RegisterType<MouseAxis>();
            registry.RegisterType<DirectDigital>();
            registry.RegisterType<DirectAnalog>();
            registry.RegisterType<PS2Button>();
            registry.RegisterType<PS2Analog>();
            registry.RegisterType<WiiButton>();
            registry.RegisterType<WiiAnalog>();
            return options;
        }

        public void generate(PlatformIO pio)
        {
            var hasWii = Bindings.FilterCast<Binding, WiiInput>().Any();
            var hasPS2 = Bindings.FilterCast<Binding, PS2Input>().Any();
            var hasSPI = hasPS2 || LedType == LedType.APA102;
            var hasI2C = hasWii;
            string configFile = Path.Combine(pio.ProjectDir, "include", "config.h");
            var lines = File.ReadAllLines(configFile);
            var json = JsonSerializer.Serialize(this, getJSONOptions(MicroController));
            var bytes = Encoding.UTF8.GetBytes(json);
            var bytesLine = "";
            var bytesLenLine = "";
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.SmallestSize))
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }
                bytesLine = $"#define CONFIGURATION {{{string.Join(",", outputStream.ToArray().Select(b => b.ToString()))}}}";
                bytesLenLine = $"#define CONFIGURATION_LEN {outputStream.ToArray().Length}";
            }
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("#define TICK "))
                {
                    lines[i] = $"#define TICK {generateTick()}";
                }
                if (line.StartsWith("#define ADC_COUNT "))
                {
                    lines[i] = $"#define ADC_COUNT {Bindings.FilterCast<Binding, DirectAnalog>().Count()}";
                }
                if (line.StartsWith("#define DIGITAL_COUNT "))
                {
                    lines[i] = $"#define DIGITAL_COUNT {Bindings.FilterCast<Binding, Button>().Count()}";
                }
                if (line.StartsWith("#define LED_TYPE "))
                {
                    lines[i] = $"#define LED_TYPE {((byte)LedType)}";
                }
                if (line.StartsWith("#define TILT_ENABLED "))
                {
                    lines[i] = $"#define TILT_ENABLED {TiltEnabled.ToString().ToLower()}";
                }
                if (line.StartsWith("#define CONSOLE_TYPE "))
                {
                    lines[i] = $"#define CONSOLE_TYPE {((byte)EmulationType)}";
                }
                if (line.StartsWith("#define DEVICE_TYPE "))
                {
                    lines[i] = $"#define DEVICE_TYPE {((byte)DeviceType)}";
                }
                if (line.StartsWith("#define ADC_PINS "))
                {
                    // Sort by pin index, and then map to adc number and turn into an array
                    lines[i] = $"#define ADC_PINS {{{String.Join(",", Bindings.FilterCast<Binding, DirectAnalog>().OrderBy(s => s.Pin).Select(s => MicroController.getChannel(s.Pin).ToString()))}}}";
                }
                if (line.StartsWith("#define PIN_INIT "))
                {
                    // Sort by pin index, and then map to adc number and turn into an array
                    lines[i] = $"#define PIN_INIT {MicroController.generateInit(Bindings)}";
                }
                if (MicroController is Pico)
                {
                    if (line.StartsWith("#define SKIP_MASK_PICO "))
                    {
                        lines[i] = $"#define SKIP_MASK_PICO {MicroController.generateSkip(hasSPI, hasI2C)}";
                    }
                }
                else
                {
                    if (line.StartsWith("#define SKIP_MASK_AVR "))
                    {
                        lines[i] = $"#define SKIP_MASK_AVR {MicroController.generateSkip(hasSPI, hasI2C)}";
                    }
                }
                if (line.StartsWith("#define CONFIGURATION_LEN "))
                {
                    lines[i] = bytesLenLine;
                }
                else if (line.StartsWith("#define CONFIGURATION "))
                {
                    lines[i] = bytesLine;
                }
                else if (line.StartsWith("#define ARDWIINO_BOARD "))
                {
                    lines[i] = $"#define ARDWIINO_BOARD \"{MicroController.Board.ardwiinoName}\"";
                }
            }
            // TODO: generate a init define too
            // TODO: theres a bunch of different inits littered around the place
            File.WriteAllLines(configFile, lines);
        }
        public class MicrocontrollerJsonConverter : JsonConverter<Microcontroller>
        {
            private Microcontroller? CurrentController;

            public MicrocontrollerJsonConverter(Microcontroller? currentController)
            {
                CurrentController = currentController;
            }

            public override Microcontroller Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options) => CurrentController!;

            public override void Write(
                Utf8JsonWriter writer,
                Microcontroller dateTimeValue,
                JsonSerializerOptions options) => writer.WriteNullValue();

        }
        public string generateTick()
        {
            // TODO: we should probably add a CLASSIC_CONTROLLER_HIGH_RES for any CLASSIC_CONTROLLER inputs
            var ps2Bindings = Bindings
                .Where(binding => binding.InputType == InputControllerType.PS2)
                .FilterCast<Binding, PS2Input>()
                .GroupBy(binding => binding.ps2Controller)
                .ToList();
            var wiiBindings = Bindings
                .Where(binding => binding.InputType == InputControllerType.Wii)
                .FilterCast<Binding, WiiInput>()
                .GroupBy(binding => binding.WiiController)
                .ToList();
            var directBindings = Bindings.Where(binding => binding.InputType == InputControllerType.Direct);
            var ret = "";
            if (ps2Bindings.Any())
            {
                ret += string.Join(" ", ps2Bindings.Select(grouping =>
                {
                    var ret = "";
                    var buttons = grouping.FilterCast<PS2Input, Button>();
                    var axes = grouping.FilterCast<PS2Input, Axis>();
                    if (buttons.Any())
                    {
                        ret += $"case {PS2Input.caseStatements[grouping.Key]}: {generateButtons(buttons, MicroController)} break;";
                    }
                    if (axes.Any())
                    {
                        ret += $"case {PS2Input.caseStatements[grouping.Key]}: {generateAnalogs(axes, MicroController)} break;";
                    }
                    return ret;
                }));
            }
            if (wiiBindings.Any())
            {
                ret += string.Join(" ", wiiBindings.Select(grouping =>
                {
                    var ret = "";
                    var buttons = grouping.FilterCast<WiiInput, Button>();
                    var axes = grouping.FilterCast<WiiInput, Axis>();
                    if (buttons.Any())
                    {
                        ret += $" case {WiiInput.caseStatements[grouping.Key]}: {generateButtons(buttons, MicroController)} break;";
                    }
                    if (axes.Any())
                    {
                        ret += $" case {WiiInput.caseStatements[grouping.Key]}: {generateAnalogs(axes, MicroController)} break;";
                    }
                    return ret;
                }));
            }
            if (directBindings.Any())
            {
                ret += generateAnalogs(directBindings.FilterCast<Binding, Axis>(), MicroController);
                ret += generateButtons(directBindings.FilterCast<Binding, Button>(), MicroController);
            }
            return ret;
        }

        private string generateAnalogs(IEnumerable<Axis> axes, Microcontroller controller)
        {
            return string.Join(" ", axes.Select(axis => axis.Type.generate() + " = " + axis.generate(axes.FilterCast<Axis, Binding>()).Replace("{self}", axis.Type.generate()) + ";")) + " ";
        }
        private string generateButtons(IEnumerable<Button> buttons, Microcontroller controller)
        {
            return string.Join(
                " ",
                buttons.GroupBy(button => button.Type.OutputType)
                    .Select(buttonsByType =>
                    {
                        var output = buttonsByType.Key;
                        var buttonList = buttonsByType.OrderByDescending(button => button.Type.index()).ToList();
                        string ret = "";
                        int i = 0;
                        // TODO: if debounce is combined, then both strums use the same i for checking (but the first strum does not decrement, only the second.)
                        foreach (var button in buttonList)
                        {
                            int debounce = button.Debounce;
                            var generated = button.generate(Bindings);
                            var outputBit = button.Type.index();
                            var outputVar = button.Type is GenericControllerHat ? "report->hat" : "report->buttons";
                            if (debounce == 0)
                            {
                                ret += $"{outputVar} |= ({generated} << {outputBit});";
                            }
                            else
                            {
                                ret += $"if (({generated})) {{debounce[{i}] = {debounce};{outputVar} |= (1 << {outputBit});}} else if (debounce[{i}]) {{ debounce[{i}]--; {outputVar} |= (1 << {outputBit});}}";
                                i++;
                            }
                        }
                        return ret;
                    }
                )
            );
        }
    }
}