using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontroller;
using GuitarConfiguratorSharp.NetCore.Configuration.Neck;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Wii;
using GuitarConfiguratorSharp.NetCore.Utils;

namespace GuitarConfiguratorSharp.NetCore.Configuration
{
    public class DeviceConfiguration
    {

        public LedType LedType { get; set; }
        public bool TiltEnabled { get; set; }
        public bool XInputOnWindows {get; set;}
        public InputControllerType InputControllerType { get; set; }
        public DeviceControllerType DeviceType { get; set; }
        public EmulationType EmulationType { get; set; }
        public RhythmType RhythmType { get; set; }
        public Microcontroller.Microcontroller MicroController { get; set; }
        public TiltOrientation TiltOrientation { get; set; }


        public IEnumerable<Binding> Bindings { get; }

        [JsonConstructor]
        public DeviceConfiguration(Microcontroller.Microcontroller microcontroller, IEnumerable<Binding> bindings, LedType ledType, DeviceControllerType deviceType, EmulationType emulationType, RhythmType rhythmType, bool tiltEnabled, bool xInputOnWindows)
        {
            this.Bindings = bindings;
            this.MicroController = microcontroller;
            this.LedType = ledType;
            this.DeviceType = deviceType;
            this.EmulationType = emulationType;
            this.RhythmType = rhythmType;
            this.TiltEnabled = tiltEnabled;
            this.XInputOnWindows = xInputOnWindows;
        }

        public DeviceConfiguration(Microcontroller.Microcontroller microcontroller)
        {
            MicroController = microcontroller;
            this.Bindings = new List<Binding>();
            this.LedType = LedType.None;
            this.DeviceType = DeviceControllerType.Gamepad;
            this.EmulationType = EmulationType.Universal;
            this.RhythmType = RhythmType.GuitarHero;
            this.TiltEnabled = false;
            this.XInputOnWindows = false;
        }

        public static JsonSerializerOptions GetJsonOptions(Microcontroller.Microcontroller? controller)
        {

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new MicrocontrollerJsonConverter(controller));
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterType<DirectGhFiveTarBarAnalog>();
            registry.RegisterType<DirectGhFiveTarBarButton>();
            registry.RegisterType<DirectGhwtBarAnalog>();
            registry.RegisterType<DirectGhwtBarButton>();
            registry.RegisterType<AnalogToDigital>();
            registry.RegisterType<DigitalToAnalog>();
            registry.RegisterType<GenericControllerButton>();
            registry.RegisterType<GenericAxis>();
            registry.RegisterType<MouseAxis>();
            registry.RegisterType<DirectDigital>();
            registry.RegisterType<DirectAnalog>();
            registry.RegisterType<Ps2Button>();
            registry.RegisterType<Ps2Analog>();
            registry.RegisterType<WiiButton>();
            registry.RegisterType<WiiAnalog>();
            return options;
        }

        public void Generate(PlatformIo pio)
        {
            var hasWii = Bindings.FilterCast<Binding, IWiiInput>().Any();
            var hasPs2 = Bindings.FilterCast<Binding, IPs2Input>().Any();
            var hasSpi = hasPs2 || LedType == LedType.APA102;
            var hasI2C = hasWii;
            string configFile = Path.Combine(pio.ProjectDir, "include", "config.h");
            var lines = File.ReadAllLines(configFile);
            var json = JsonSerializer.Serialize(this, GetJsonOptions(MicroController));
            var bytes = Encoding.UTF8.GetBytes(json);
            var bytesLine = "";
            var bytesLenLine = "";
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.SmallestSize))
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }
                bytesLine = $"#define CONFIGURATION {{{string.Join(",", outputStream.ToArray().Select(b => "0x"+b.ToString("X")))}}}";
                bytesLenLine = $"#define CONFIGURATION_LEN {outputStream.ToArray().Length}";
            }
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("#define WINDOWS_USES_XINPUT "))
                {
                    lines[i] = $"#define WINDOWS_USES_XINPUT {XInputOnWindows.ToString().ToLower()}";
                }
                if (line.StartsWith("#define TICK_PS3 "))
                {
                    lines[i] = $"#define TICK_PS3 {GenerateTick(false)}";
                }
                if (line.StartsWith("#define TICK_XINPUT "))
                {
                    lines[i] = $"#define TICK_XINPUT {GenerateTick(true)}";
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
                    lines[i] = $"#define ADC_PINS {{{String.Join(",", Bindings.FilterCast<Binding, DirectAnalog>().OrderBy(s => s.Pin).Select(s => MicroController.GetChannel(s.Pin).ToString()))}}}";
                }
                if (line.StartsWith("#define PIN_INIT "))
                {
                    lines[i] = $"#define PIN_INIT {MicroController.GenerateInit(Bindings)}";
                }
                if (MicroController is Pico)
                {
                    if (line.StartsWith("#define SKIP_MASK_PICO "))
                    {
                        lines[i] = $"#define SKIP_MASK_PICO {MicroController.GenerateSkip(hasSpi, hasI2C)}";
                    }
                }
                else
                {
                    if (line.StartsWith("#define SKIP_MASK_AVR "))
                    {
                        lines[i] = $"#define SKIP_MASK_AVR {MicroController.GenerateSkip(hasSpi, hasI2C)}";
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
                    lines[i] = $"#define ARDWIINO_BOARD \"{MicroController.Board.ArdwiinoName}\"";
                }
            }
            // TODO: generate a init define too
            // TODO: theres a bunch of different inits littered around the place
            File.WriteAllLines(configFile, lines);
        }
        public class MicrocontrollerJsonConverter : JsonConverter<Microcontroller.Microcontroller>
        {
            private readonly Microcontroller.Microcontroller? _currentController;

            public MicrocontrollerJsonConverter(Microcontroller.Microcontroller? currentController)
            {
                _currentController = currentController;
            }

            public override Microcontroller.Microcontroller Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options) => _currentController!;

            public override void Write(
                Utf8JsonWriter writer,
                Microcontroller.Microcontroller dateTimeValue,
                JsonSerializerOptions options) => writer.WriteNullValue();

        }
        public string GenerateTick(bool xbox)
        {
            // TODO: we should probably add a CLASSIC_CONTROLLER_HIGH_RES for any CLASSIC_CONTROLLER inputs
            var ps2Bindings = Bindings
                .Where(binding => binding.InputType == InputControllerType.PS2)
                .FilterCast<Binding, IPs2Input>()
                .GroupBy(binding => binding.Ps2Controller)
                .ToList();
            var wiiBindings = Bindings
                .Where(binding => binding.InputType == InputControllerType.Wii)
                .FilterCast<Binding, IWiiInput>()
                .GroupBy(binding => binding.WiiController)
                .ToList();
            var directBindings = Bindings.Where(binding => binding.InputType == InputControllerType.Direct);
            var ret = "";
            if (ps2Bindings.Any())
            {
                ret += string.Join(" ", ps2Bindings.Select(grouping =>
                {
                    var retPs2 = "";
                    var buttons = grouping.FilterCast<IPs2Input, Button>().ToList();
                    var axes = grouping.FilterCast<IPs2Input, Axis>().ToList();
                    if (buttons.Any())
                    {
                        retPs2 += $"case {IPs2Input.caseStatements[grouping.Key]}: {GenerateButtons(buttons, MicroController, xbox)} break;";
                    }
                    if (axes.Any())
                    {
                        retPs2 += $"case {IPs2Input.caseStatements[grouping.Key]}: {GenerateAnalogs(axes, MicroController, xbox)} break;";
                    }
                    return retPs2;
                }));
            }
            if (wiiBindings.Any())
            {
                ret += string.Join(" ", wiiBindings.Select(grouping =>
                {
                    var retWii = "";
                    var buttons = grouping.FilterCast<IWiiInput, Button>();
                    var axes = grouping.FilterCast<IWiiInput, Axis>();
                    var enumerable1 = buttons.ToList();
                    if (enumerable1.Any())
                    {
                        retWii += $" case {IWiiInput.caseStatements[grouping.Key]}: {GenerateButtons(enumerable1, MicroController, xbox)} break;";
                    }

                    var axisEnumerable = axes.ToList();
                    if (axisEnumerable.Any())
                    {
                        retWii += $" case {IWiiInput.caseStatements[grouping.Key]}: {GenerateAnalogs(axisEnumerable, MicroController, xbox)} break;";
                    }
                    return retWii;
                }));
            }

            var enumerable = directBindings.ToList();
            if (enumerable.Any())
            {
                ret += GenerateAnalogs(enumerable.FilterCast<Binding, Axis>(), MicroController, xbox);
                ret += GenerateButtons(enumerable.FilterCast<Binding, Button>(), MicroController, xbox);
            }
            return ret;
        }

        private string GenerateAnalogs(IEnumerable<Axis> axes, Microcontroller.Microcontroller controller, bool xbox)
        {
            var axisEnumerable = axes.ToList();
            return string.Join(" ", axisEnumerable.Select(axis => axis.Type.Generate(xbox) + " = " + axis.Generate(axisEnumerable.FilterCast<Axis, Binding>(), xbox).Replace("{self}", axis.Type.Generate(xbox)) + ";")) + " ";
        }
        private string GenerateButtons(IEnumerable<Button> buttons, Microcontroller.Microcontroller controller, bool xbox)
        {
            return string.Join(
                " ",
                buttons.GroupBy(button => button.Type.OutputType)
                    .Select(buttonsByType =>
                    {
                        var output = buttonsByType.Key;
                        var buttonList = buttonsByType.OrderByDescending(button => button.Type.Index(xbox)).ToList();
                        string ret = "";
                        int i = 0;
                        // TODO: if debounce is combined, then both strums use the same i for checking (but the first strum does not decrement, only the second.)
                        foreach (var button in buttonList)
                        {
                            int debounce = button.Debounce;
                            var generated = button.Generate(Bindings, xbox);
                            var outputVar = button.Type.Generate(xbox);
                            var outputBit = button.Type.Index(xbox);
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