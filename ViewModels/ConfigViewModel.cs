﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Media;
using DynamicData;
using GuitarConfiguratorSharp.NetCore.Configuration;
using GuitarConfiguratorSharp.NetCore.Configuration.Conversions;
using GuitarConfiguratorSharp.NetCore.Configuration.DJ;
using GuitarConfiguratorSharp.NetCore.Configuration.Exceptions;
using GuitarConfiguratorSharp.NetCore.Configuration.Microcontrollers;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs;
using GuitarConfiguratorSharp.NetCore.Configuration.Outputs.Combined;
using GuitarConfiguratorSharp.NetCore.Configuration.PS2;
using GuitarConfiguratorSharp.NetCore.Configuration.Serialization;
using GuitarConfiguratorSharp.NetCore.Configuration.Types;
using GuitarConfiguratorSharp.NetCore.Utils;
using ProtoBuf;
using ReactiveUI;

namespace GuitarConfiguratorSharp.NetCore.ViewModels
{
    public class ConfigViewModel : ReactiveObject, IRoutableViewModel
    {
        public static readonly string Apa102SpiType = "APA102";

        public Interaction<(string _platformIOText, ConfigViewModel), RaiseIssueWindowViewModel?> ShowIssueDialog
        {
            get;
        }

        public Interaction<Arduino, ShowUnoShortWindowViewModel?> ShowUnoShortDialog { get; }

        public Interaction<(string yesText, string noText, string text), AreYouSureWindowViewModel> ShowYesNoDialog
        {
            get;
        }

        public Interaction<(ConfigViewModel model, Microcontroller microcontroller, Output output, DirectInput input),
                BindAllWindowViewModel>
            ShowBindAllDialog { get; }

        public ICommand BindAllCommand { get; }

        public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];

        public IScreen HostScreen { get; }

        public MainWindowViewModel Main { get; }

        public IEnumerable<DeviceControllerType> DeviceControllerTypes => Enum.GetValues<DeviceControllerType>();

        public IEnumerable<RhythmType> RhythmTypes => Enum.GetValues<RhythmType>();

        public IEnumerable<EmulationType> EmulationTypes => Enum.GetValues<EmulationType>();

        public IEnumerable<LedType> LedTypes => Enum.GetValues<LedType>();

        //TODO: actually read and write this as part of the config
        public bool KvEnabled { get; set; } = false;
        public int[] KvKey1 { get; set; } = Enumerable.Repeat(0x00, 16).ToArray();
        public int[] KvKey2 { get; set; } = Enumerable.Repeat(0x00, 16).ToArray();

        public ICommand WriteConfig { get; }

        public ICommand GoBack { get; }

        private SpiConfig? _apa102SpiConfig;

        public int Apa102Mosi
        {
            get => _apa102SpiConfig?.Mosi ?? 0;
            set => _apa102SpiConfig!.Mosi = value;
        }

        public int Apa102Sck
        {
            get => _apa102SpiConfig?.Sck ?? 0;
            set => _apa102SpiConfig!.Sck = value;
        }

        private LedType _ledType;

        private bool _hasError;

        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        private bool _fininalised;

        public bool Finalised
        {
            get => _fininalised;
            set => this.RaiseAndSetIfChanged(ref _fininalised, value);
        }

        public LedType LedType
        {
            get => _ledType;
            set
            {
                if (value == LedType.None)
                {
                    MicroController!.UnAssignPins(Apa102SpiType);
                }
                else if (_ledType == LedType.None)
                {
                    var pins = MicroController!.SpiPins(Apa102SpiType);
                    var mosi = pins.First(pair => pair.Value is SpiPinType.Mosi).Key;
                    var sck = pins.First(pair => pair.Value is SpiPinType.Sck).Key;
                    _apa102SpiConfig = MicroController.AssignSpiPins(this, Apa102SpiType, mosi, -1, sck, true, true,
                        true,
                        Math.Min(MicroController.Board.CpuFreq / 2, 12000000))!;
                    this.RaisePropertyChanged(nameof(Apa102Mosi));
                    this.RaisePropertyChanged(nameof(Apa102Sck));
                }

                this.RaiseAndSetIfChanged(ref _ledType, value);
            }
        }

        private bool _xinputOnWindows;

        public bool XInputOnWindows
        {
            get => _xinputOnWindows;
            set => this.RaiseAndSetIfChanged(ref _xinputOnWindows, value);
        }

        private bool _combinedDebounce;

        public bool CombinedDebounce
        {
            get => _combinedDebounce;
            set => this.RaiseAndSetIfChanged(ref _combinedDebounce, value);
        }

        private DeviceControllerType _deviceControllerType;

        public DeviceControllerType DeviceType
        {
            get => _deviceControllerType;
            set
            {
                this.RaiseAndSetIfChanged(ref _deviceControllerType, value);
                UpdateBindings();
            }
        }

        private EmulationType _emulationType;

        public EmulationType EmulationType
        {
            get => _emulationType;
            set => SetDefaultBindings(value);
        }

        private RhythmType _rhythmType;

        public RhythmType RhythmType
        {
            get => _rhythmType;
            set
            {
                this.RaiseAndSetIfChanged(ref _rhythmType, value);
                UpdateBindings();
            }
        }

        public void SetDeviceTypeAndRhythmTypeWithoutUpdating(DeviceControllerType type, RhythmType rhythmType,
            EmulationType emulationType)
        {
            this.RaiseAndSetIfChanged(ref _deviceControllerType, type, nameof(DeviceType));
            this.RaiseAndSetIfChanged(ref _rhythmType, rhythmType, nameof(RhythmType));
            this.RaiseAndSetIfChanged(ref _emulationType, emulationType, nameof(EmulationType));
        }

        private void UpdateBindings()
        {
            foreach (var binding in Bindings)
            {
                binding.UpdateBindings();
            }

            // If the user has a ps2 or wii combined output mapped, they don't need the default bindings
            if (Bindings.Any(s => s is WiiCombinedOutput or Ps2CombinedOutput))
            {
                return;
            }

            var types = ControllerEnumConverter.GetTypes((_deviceControllerType, _rhythmType))
                .Where(s => s is not SimpleType).ToList();
            foreach (var binding in Bindings)
            {
                switch (binding)
                {
                    case ControllerButton button:
                        types.Remove(button.Type);
                        break;
                    case ControllerAxis axis:
                        types.Remove(axis.Type);
                        break;
                    case DrumAxis axis:
                        types.Remove(axis.Type);
                        break;
                }
            }

            if (_deviceControllerType == DeviceControllerType.Drum)
            {
                IEnumerable<DrumAxisType> difference = DrumAxisTypeMethods.GetDifferenceFor(_rhythmType).ToHashSet();
                Bindings.RemoveAll(Bindings.Where(s => s is DrumAxis axis && difference.Contains(axis.Type)));
            }
            else
            {
                Bindings.RemoveAll(Bindings.Where(s => s is DrumAxis));
            }

            if (_deviceControllerType == DeviceControllerType.Turntable)
            {
                if (!Bindings.Any(s => s is DjCombinedOutput))
                {
                    Bindings.Add(new DjCombinedOutput(this, MicroController!));
                }
            }

            foreach (var type in types)
            {
                switch (type)
                {
                    case StandardButtonType buttonType:
                        Bindings.Add(new ControllerButton(this,
                            new DirectInput(0, DevicePinMode.PullUp, this, MicroController!),
                            Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 1, buttonType));
                        break;
                    case StandardAxisType axisType:
                        Bindings.Add(new ControllerAxis(this,
                            new DirectInput(MicroController!.GetFirstAnalogPin(), DevicePinMode.Analog, this,
                                MicroController!),
                            Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), short.MinValue, short.MaxValue,
                            0, axisType));
                        break;
                    case DrumAxisType axisType:
                        Bindings.Add(new DrumAxis(this,
                            new DirectInput(MicroController!.GetFirstAnalogPin(), DevicePinMode.Analog, this,
                                MicroController!),
                            Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), short.MinValue, short.MaxValue,
                            0, 64, 10, axisType));
                        break;
                }
            }
        }

        private Microcontroller? _microController;

        public Microcontroller? MicroController
        {
            get => _microController;
            set => this.RaiseAndSetIfChanged(ref _microController, value);
        }


        public AvaloniaList<Output> Bindings { get; }
        private readonly ObservableAsPropertyHelper<bool> _isRhythm;
        public bool IsRhythm => _isRhythm.Value;
        private readonly ObservableAsPropertyHelper<bool> _isController;
        public bool IsController => _isController.Value;
        private readonly ObservableAsPropertyHelper<bool> _isKeyboard;
        public bool IsKeyboard => _isKeyboard.Value;
        private readonly ObservableAsPropertyHelper<bool> _isMidi;
        public bool IsMidi => _isMidi.Value;

        private readonly ObservableAsPropertyHelper<bool> _isApa102;
        public bool IsApa102 => _isApa102.Value;


        private readonly ObservableAsPropertyHelper<bool> _bindableSpi;
        public bool BindableSpi => _bindableSpi.Value;

        private string _platformIoText = "";

        public ConfigViewModel(MainWindowViewModel screen)
        {
            ShowIssueDialog = new Interaction<(string _platformIOText, ConfigViewModel), RaiseIssueWindowViewModel?>();
            ShowUnoShortDialog = new Interaction<Arduino, ShowUnoShortWindowViewModel?>();
            ShowYesNoDialog =
                new Interaction<(string yesText, string noText, string text), AreYouSureWindowViewModel>();
            ShowBindAllDialog =
                new Interaction<(ConfigViewModel model, Microcontroller microcontroller, Output output, DirectInput
                    input), BindAllWindowViewModel>();
            BindAllCommand = ReactiveCommand.CreateFromTask(BindAll);
            Main = screen;
            HostScreen = screen;

            WriteConfig = ReactiveCommand.CreateFromTask(Write,
                this.WhenAnyValue(x => x.Main.Working, x => x.Main.Connected, x => x.HasError)
                    .ObserveOn(RxApp.MainThreadScheduler).Select(x => !x.Item1 && x.Item2 && !x.Item3));
            GoBack = ReactiveCommand.CreateFromObservable<Unit, IRoutableViewModel?>(Main.GoBack.Execute);
            Bindings = new AvaloniaList<Output>();

            _writeToolTip = this.WhenAnyValue(x => x.HasError)
                .Select(s => s ? "There are errors in your configuration" : null).ToProperty(this, s => s.WriteToolTip);

            _isRhythm = this.WhenAnyValue(x => x.DeviceType)
                .Select(x => x is DeviceControllerType.Drum or DeviceControllerType.Guitar)
                .ToProperty(this, x => x.IsRhythm);
            _isController = this.WhenAnyValue(x => x.EmulationType)
                .Select(x => x is EmulationType.Controller)
                .ToProperty(this, x => x.IsController);
            _isKeyboard = this.WhenAnyValue(x => x.EmulationType)
                .Select(x => x is EmulationType.KeyboardMouse)
                .ToProperty(this, x => x.IsKeyboard);
            _isMidi = this.WhenAnyValue(x => x.EmulationType)
                .Select(x => x is EmulationType.Midi)
                .ToProperty(this, x => x.IsMidi);
            _isApa102 = this.WhenAnyValue(x => x.LedType)
                .Select(x => x is LedType.APA102_BGR or LedType.APA102_BRG or LedType.APA102_GBR or LedType.APA102_GRB
                    or LedType.APA102_RBG or LedType.APA102_RGB)
                .ToProperty(this, x => x.IsApa102);
            _bindableSpi = this.WhenAnyValue(x => x.MicroController, x => x.IsApa102)
                .Select(x => x.Item1 is not AvrController && x.Item2)
                .ToProperty(this, x => x.BindableSpi);
            _availableMosiPins = this.WhenAnyValue(x => x.MicroController)
                .Select(GetMosiPins)
                .ToProperty(this, x => x.AvailableMosiPins);
            _availableSckPins = this.WhenAnyValue(x => x.MicroController)
                .Select(GetSckPins)
                .ToProperty(this, x => x.AvailableSckPins);
            Main.Pio.PlatformIoWorking += working =>
            {
                if (working)
                {
                    _platformIoText = "";
                }
            };
            Main.Pio.TextChanged += (message, clear) =>
            {
                _platformIoText += message;
                _platformIoText += "\n";
            };

            Main.Pio.PlatformIoError += val =>
            {
                if (val)
                {
                    ShowIssueDialog.Handle((_platformIoText, this)).ToTask();
                }
            };
        }

        private readonly ObservableAsPropertyHelper<string?> _writeToolTip;
        public string? WriteToolTip => _writeToolTip.Value;
        private readonly ObservableAsPropertyHelper<List<int>> _availableMosiPins;
        private readonly ObservableAsPropertyHelper<List<int>> _availableSckPins;
        public List<int> AvailableMosiPins => _availableMosiPins.Value;
        public List<int> AvailableSckPins => _availableSckPins.Value;
        public IEnumerable<PinConfig> PinConfigs => new[] {_apa102SpiConfig!};

        private List<int> GetMosiPins(Microcontroller? microcontroller)
        {
            if (MicroController == null) return new List<int>();
            return microcontroller!.SpiPins(Apa102SpiType)
                .Where(s => s.Value is SpiPinType.Mosi)
                .Select(s => s.Key).ToList();
        }

        private List<int> GetSckPins(Microcontroller? microcontroller)
        {
            if (MicroController == null) return new List<int>();
            return microcontroller!.SpiPins(Apa102SpiType)
                .Where(s => s.Value is SpiPinType.Sck)
                .Select(s => s.Key).ToList();
        }

        public async Task Write()
        {
            await Main.Write(this);
        }

        public async Task SetDefaults(Microcontroller microcontroller)
        {
            ClearOutputs();
            MicroController = microcontroller;
            LedType = LedType.None;
            _deviceControllerType = DeviceControllerType.Gamepad;
            _emulationType = EmulationType.Controller;
            _rhythmType = RhythmType.GuitarHero;
            this.RaisePropertyChanged(nameof(DeviceType));
            this.RaisePropertyChanged(nameof(EmulationType));
            this.RaisePropertyChanged(nameof(RhythmType));
            XInputOnWindows = false;

            switch (Main.DeviceInputType)
            {
                case DeviceInputType.Direct:
                    SetDefaultBindings(EmulationType);
                    break;
                case DeviceInputType.Wii:
                    Bindings.Add(new WiiCombinedOutput(this, microcontroller));
                    break;
                case DeviceInputType.Ps2:
                    Bindings.Add(new Ps2CombinedOutput(this, microcontroller));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Main.IsUno || Main.IsMega)
            {
                await Task.WhenAll(Write(), ShowUnoShortDialog.Handle((Arduino) Main.SelectedDevice!).ToTask());
                return;
            }

            UpdateErrors();

            await Write();
        }

        private async void SetDefaultBindings(EmulationType emulationType)
        {
            if (Bindings.Any())
            {
                var yesNo = await ShowYesNoDialog.Handle(("Clear", "Cancel",
                    "The following action will clear all your bindings, are you sure you want to do this?")).ToTask();
                if (!yesNo.Response)
                {
                    return;
                }
            }

            _emulationType = emulationType;
            this.RaisePropertyChanged(nameof(EmulationType));
            ClearOutputs();
            if (EmulationType != EmulationType.Controller) return;
            foreach (var type in Enum.GetValues<StandardAxisType>())
            {
                if (ControllerEnumConverter.GetAxisText(_deviceControllerType, _rhythmType, type) == null) continue;
                if (DeviceType == DeviceControllerType.Turntable &&
                    type is StandardAxisType.LeftStickX or StandardAxisType.LeftStickY) continue;
                Bindings.Add(new ControllerAxis(this,
                    new DirectInput(MicroController!.GetFirstAnalogPin(), DevicePinMode.Analog, this, MicroController!),
                    Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), short.MinValue, short.MaxValue, 0,
                    type));
            }

            foreach (var type in Enum.GetValues<StandardButtonType>())
            {
                if (ControllerEnumConverter.GetButtonText(_deviceControllerType, _rhythmType, type) ==
                    null) continue;
                Bindings.Add(new ControllerButton(this,
                    new DirectInput(0, DevicePinMode.PullUp, this, MicroController!),
                    Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 1, type));
            }

            UpdateErrors();
        }

        public void Finalise()
        {
            Finalised = true;
        }

        public void Generate(PlatformIo pio)
        {
            if (_microController == null) return;
            var outputs = Bindings.SelectMany(binding => binding.Outputs.Items).ToList();
            var inputs = outputs.Select(binding => binding.Input?.InnermostInput()).OfType<Input>().ToList();
            var directInputs = inputs.OfType<DirectInput>().ToList();
            var configFile = Path.Combine(pio.FirmwareDir, "include", "config_data.h");
            var lines = new List<string>();
            var leds = outputs.SelectMany(s => s.Outputs.Items).SelectMany(s => s.LedIndices).ToList();
            var ledCount = 0;
            if (leds.Any())
            {
                ledCount = leds.Max() + 1;
            }

            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.SmallestSize))
                {
                    Serializer.Serialize(compressStream, new SerializedConfiguration(this));
                }

                lines.Add(
                    $"#define CONFIGURATION {{{string.Join(",", outputStream.ToArray().Select(b => "0x" + b.ToString("X")))}}}");
                lines.Add($"#define CONFIGURATION_LEN {outputStream.ToArray().Length}");
            }


            lines.Add($"#define WINDOWS_USES_XINPUT {XInputOnWindows.ToString().ToLower()}");

            lines.Add($"#define TICK_SHARED {GenerateTick(false, true)}");

            lines.Add($"#define TICK_PS3 {GenerateTick(false, false)}");

            lines.Add($"#define TICK_XINPUT {GenerateTick(true, false)}");

            lines.Add(
                $"#define ADC_COUNT {directInputs.DistinctBy(s => s.PinConfig.Pin).Count(input => input.IsAnalog)}");

            lines.Add($"#define DIGITAL_COUNT {CalculateDebounceTicks()}");
            lines.Add($"#define LED_COUNT {ledCount}");

            lines.Add($"#define LED_TYPE {GetLedType()}");

            if (IsApa102)
            {
                lines.Add($"#define {Apa102SpiType.ToUpper()}_SPI_PORT {_apa102SpiConfig!.Definition}");

                lines.Add($"#define TICK_LED {GenerateLedTick()}");
            }

            lines.Add($"#define CONSOLE_TYPE {((byte) EmulationType)}");

            lines.Add($"#define DEVICE_TYPE {((byte) DeviceType)}");

            lines.Add($"#define RHYTHM_TYPE {((byte) RhythmType)}");
            if (KvEnabled)
            {
                lines.Add(
                    $"#define KV_KEY_1 {{{string.Join(",", KvKey1.ToArray().Select(b => "0x" + b.ToString("X")))}}}");
                lines.Add(
                    $"#define KV_KEY_2 {{{string.Join(",", KvKey2.ToArray().Select(b => "0x" + b.ToString("X")))}}}");
            }

            lines.Add(Ps2Input.GeneratePs2Pressures(inputs));

            // Sort by pin index, and then map to adc number and turn into an array
            lines.Add(
                $"#define ADC_PINS {{{string.Join(",", directInputs.Where(s => s.IsAnalog).OrderBy(s => s.PinConfig.Pin).Select(s => _microController.GetChannel(s.PinConfig.Pin, false).ToString()).Distinct())}}}");

            lines.Add($"#define PIN_INIT {_microController.GenerateInit()}");

            lines.Add(_microController.GenerateDefinitions());

            lines.Add($"#define ARDWIINO_BOARD \"{_microController.Board.ArdwiinoName}\"");
            lines.Add(string.Join("\n",
                inputs.SelectMany(input => input.RequiredDefines()).Distinct().Select(define => $"#define {define}")));

            File.WriteAllLines(configFile, lines);
        }

        private int GetLedType()
        {
            switch (LedType)
            {
                case LedType.APA102_RGB:
                case LedType.APA102_RBG:
                case LedType.APA102_GRB:
                case LedType.APA102_GBR:
                case LedType.APA102_BRG:
                case LedType.APA102_BGR:
                    return 1;
                case LedType.None:
                default:
                    return 0;
            }
        }

        private async Task BindAll()
        {
            foreach (var binding in Bindings)
            {
                if (binding.Input?.InnermostInput() is not DirectInput direct) continue;
                var response = await ShowBindAllDialog.Handle((this, MicroController!, binding, direct)).ToTask();
                if (!response.Response)
                {
                    return;
                }
            }
        }

        public void RemoveOutput(Output output)
        {
            output.Dispose();
            if (Bindings.Remove(output))
            {
                UpdateErrors();
                return;
            }

            foreach (var binding in Bindings)
            {
                binding.Outputs.Remove(output);
            }

            UpdateErrors();
        }

        public void ClearOutputs()
        {
            foreach (var binding in Bindings)
            {
                binding.Dispose();
            }

            Bindings.Clear();
            UpdateErrors();
        }

        public async void ClearOutputsWithConfirmation()
        {
            var yesNo = await ShowYesNoDialog.Handle(("Clear", "Cancel",
                "The following action will clear all your inputs, are you sure you want to do this?")).ToTask();
            if (!yesNo.Response)
            {
                return;
            }

            foreach (var binding in Bindings)
            {
                binding.Dispose();
            }

            Bindings.Clear();
            UpdateErrors();
        }

        public async void Reset()
        {
            var yesNo = await ShowYesNoDialog.Handle(("Reset", "Cancel",
                    "The following action will revert your device back to an Arduino, are you sure you want to do this?"))
                .ToTask();
            if (!yesNo.Response)
            {
                return;
            }
            //TODO: actually revert the device to an arduino
            //TODO: probably don't offer this for the pico since you can just use bootsel on those
        }

        public void AddOutput()
        {
            if (IsController)
            {
                Bindings.Add(new EmptyOutput(this));
            }
            else if (IsKeyboard)
            {
                Bindings.Add(new KeyboardButton(this, new DirectInput(0, DevicePinMode.PullUp, this, _microController!),
                    Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 1, Key.Space));
            }
            else if (IsMidi)
            {
                Bindings.Add(new MidiOutput(this, new DirectInput(0, DevicePinMode.PullUp, this, _microController!),
                    Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), 0, 128, 0, MidiType.Note, 64, 0, 0,
                    1));
            }

            UpdateErrors();
        }

        private string GenerateLedTick()
        {
            var outputs = Bindings.SelectMany(binding => binding.Outputs.Items).ToList();
            if (_microController == null || _ledType == LedType.None ||
                !outputs.Any(s => s.LedIndices.Any())) return "";
            var ledMax = outputs.SelectMany(output => output.LedIndices).Max();
            var ret =
                "spi_transfer(APA102_SPI_PORT, 0x00);spi_transfer(APA102_SPI_PORT, 0x00);spi_transfer(APA102_SPI_PORT, 0x00);spi_transfer(APA102_SPI_PORT, 0x00);";
            for (var i = 0; i <= ledMax; i++)
            {
                ret +=
                    $"spi_transfer(APA102_SPI_PORT, 0xff);spi_transfer(APA102_SPI_PORT, ledState[{i}].r);spi_transfer(APA102_SPI_PORT, ledState[{i}].g);spi_transfer(APA102_SPI_PORT, ledState[{i}].b);";
            }

            for (var i = 0; i <= ledMax; i += 16)
            {
                ret += "spi_transfer(APA102_SPI_PORT, 0xff);";
            }

            return ret.Replace('\n', ' ');
        }

        private string GenerateTick(bool xbox, bool shared)
        {
            if (_microController == null) return "";
            var outputs = Bindings.SelectMany(binding => binding.Outputs.Items).ToList();
            // If whammy isn't bound, then default to -32767 instead of 0.
            if (xbox && DeviceType == DeviceControllerType.Guitar && !outputs.Any(output =>
                    output is ControllerAxis {Type: StandardAxisType.RightStickX}))
            {
                outputs.Add(new ControllerAxis(this, new FixedInput(this, -32767), Colors.Transparent,
                    Colors.Transparent, Array.Empty<byte>(), 0, 0, 0, StandardAxisType.RightStickX));
            }

            if (DeviceType == DeviceControllerType.Turntable)
            {
                var outputsToAdd = new List<Output>();
                foreach (var output in outputs.SelectMany(s => s.Outputs.Items))
                {
                    switch (output)
                    {
                        case DjButton {Type: DjInputType.LeftGreen}:
                        case DjButton {Type: DjInputType.RightGreen}:
                            outputsToAdd.Add(new ControllerButton(this, output.Input!, Colors.Transparent,
                                Colors.Transparent, Array.Empty<byte>(), 10, StandardButtonType.A));
                            break;
                        case DjButton {Type: DjInputType.LeftRed}:
                        case DjButton {Type: DjInputType.RightRed}:
                            outputsToAdd.Add(new ControllerButton(this, output.Input!, Colors.Transparent,
                                Colors.Transparent, Array.Empty<byte>(), 10, StandardButtonType.B));
                            break;
                        case DjButton {Type: DjInputType.LeftBlue}:
                        case DjButton {Type: DjInputType.RightBlue}:
                            outputsToAdd.Add(new ControllerButton(this, output.Input!, Colors.Transparent,
                                Colors.Transparent, Array.Empty<byte>(), 10, StandardButtonType.X));
                            break;
                    }
                }

                outputs.AddRange(outputsToAdd);
            }

            // GHL guitars require mapping the strum to left joy Y
            if (DeviceType == DeviceControllerType.LiveGuitar)
            {
                var outputsToAdd = new List<Output>();
                foreach (var output in outputs)
                {
                    switch (output)
                    {
                        case ControllerButton {Type: StandardButtonType.Down}:
                            outputsToAdd.Add(new ControllerAxis(this,
                                new DigitalToAnalog(output.Input!, short.MaxValue, 0, this), Colors.Transparent,
                                Colors.Transparent, Array.Empty<byte>(), short.MinValue, short.MaxValue, 0,
                                StandardAxisType.LeftStickY));
                            break;
                        case ControllerButton {Type: StandardButtonType.Up}:
                            outputsToAdd.Add(new ControllerAxis(this,
                                new DigitalToAnalog(output.Input!, short.MinValue, 0, this), Colors.Transparent,
                                Colors.Transparent, Array.Empty<byte>(), short.MinValue, short.MaxValue, 0,
                                StandardAxisType.LeftStickY));
                            break;
                    }
                }

                outputs.AddRange(outputsToAdd);
            }

            if (!xbox)
            {
                var toTest = new List<StandardAxisType>()
                {
                    StandardAxisType.LeftStickX, StandardAxisType.LeftStickY, StandardAxisType.RightStickX,
                    StandardAxisType.RightStickY, StandardAxisType.AccelerationX, StandardAxisType.AccelerationY,
                    StandardAxisType.AccelerationZ, StandardAxisType.Gyro
                };
                
                foreach (var output in outputs.SelectMany(s => s.Outputs.Items))
                {
                    switch (output)
                    {
                        case ControllerAxis axis:
                            toTest.Remove(axis.GetRealAxis(xbox));
                            break;
                        case DjButton:
                            toTest.Remove(StandardAxisType.AccelerationY);
                            break;
                    }
                }

                foreach (var standardAxisType in toTest)
                {
                    switch (standardAxisType)
                    {
                        case StandardAxisType.Gyro:
                        case StandardAxisType.AccelerationX:
                        case StandardAxisType.AccelerationY:
                        case StandardAxisType.AccelerationZ:
                            outputs.Add(new ControllerAxis(this, new FixedInput(this, 0x0200),
                                Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), short.MinValue,
                                short.MaxValue, 0, standardAxisType));
                            break;
                        case StandardAxisType.LeftStickX:
                        case StandardAxisType.LeftStickY:
                        case StandardAxisType.RightStickX:
                        case StandardAxisType.RightStickY:
                            outputs.Add(new ControllerAxis(this, new FixedInput(this, sbyte.MaxValue),
                                Colors.Transparent, Colors.Transparent, Array.Empty<byte>(), byte.MinValue,
                                byte.MaxValue, 0, standardAxisType));
                            break;
                    }
                }
            }

            var groupedOutputs = outputs
                .SelectMany(s => s.Input?.Inputs().Zip(Enumerable.Repeat(s, s.Input?.Inputs().Count ?? 0))!)
                .GroupBy(s => s.First.InnermostInput().GetType()).ToList();
            var combined = DeviceType == DeviceControllerType.Guitar && CombinedDebounce;

            Dictionary<string, int> debounces = new();
            if (combined)
            {
                foreach (var output in outputs.Where(output => output.IsStrum))
                {
                    debounces[output.Name] = debounces.Count;
                }
            }

            // Pass 1: work out debounces and map inputs to debounces
            var inputs = new Dictionary<string, List<int>>();
            var macros = new List<Output>();
            foreach (var groupedOutput in groupedOutputs)
            {
                foreach (var (input, output) in groupedOutput)
                {
                    var generatedInput = input.Generate(xbox);
                    if (input == null) throw new IncompleteConfigurationException("Missing input!");
                    if (output is not OutputButton and not DrumAxis) continue;

                    if (output.Input is MacroInput)
                    {
                        if (!debounces.ContainsKey(output.Name + generatedInput))
                        {
                            debounces[output.Name + generatedInput] = debounces.Count;
                        }

                        macros.Add(output);
                    }
                    else
                    {
                        if (!debounces.ContainsKey(output.Name))
                        {
                            debounces[output.Name] = debounces.Count;
                        }
                    }

                    if (!inputs.ContainsKey(generatedInput))
                    {
                        inputs[generatedInput] = new List<int>();
                    }

                    inputs[generatedInput].Add(debounces[output.Name]);
                }
            }

            var seen = new HashSet<Output>();
            var seenDebounce = new HashSet<int>();
            var seenAnalog = new HashSet<string>();
            // Handle most mappings
            // Sort in a way that any digital to analog based groups are last. This is so that seenAnalog will be filled in when necessary.
            var ret = groupedOutputs.OrderByDescending(s => s.Count(s2 => s2.Second.Input is DigitalToAnalog))
                .Aggregate("", (current, group) =>
                {
                    // we need to ensure that DigitalToAnalog is last
                    return current + (group
                        .First().First.InnermostInput()
                        .GenerateAll(Bindings.ToList(), group.OrderByDescending(s => s.First is DigitalToAnalog ? 0 : 1)
                            .Select((s) =>
                            {
                                var input = s.First;
                                var output = s.Second;
                                var generatedInput = input.Generate(xbox);
                                var index = new List<int> {0};
                                var extra = "";
                                if (output is OutputButton or DrumAxis)
                                {
                                    index = new List<int> {debounces[output.Name]};
                                    if (output.Input is MacroInput)
                                    {
                                        if (shared)
                                        {
                                            output = output.Serialize().Generate(this, _microController);
                                            output.Input = input;
                                            index = new List<int> {debounces[output.Name + generatedInput]};
                                        }
                                        else
                                        {
                                            if (seen.Contains(output)) return new Tuple<Input, string>(input, "");
                                            seen.Add(output);
                                            index = output.Input!.Inputs()
                                                .Select(input1 => debounces[output.Name + input1.Generate(xbox)])
                                                .ToList();
                                        }
                                    }
                                }

                                var generated = output.Generate(xbox, shared, index, combined, extra);

                                if (output is OutputAxis axis && !shared)
                                {
                                    generated = generated.Replace("{output}", axis.GenerateOutput(xbox, false));
                                    if (!seenAnalog.Contains(output.Name) && input is DigitalToAnalog dta)
                                    {
                                        generated =
                                            $"{axis.GenerateOutput(xbox, false)} = {dta.GenerateOff(xbox)}; {generated}";
                                    }

                                    seenAnalog.Add(output.Name);
                                }

                                return new Tuple<Input, string>(input, generated);
                            })
                            .Where(s => !string.IsNullOrEmpty(s.Item2))
                            .ToList(), shared, xbox) + ";");
                });
            // Flick off intersecting outputs when multiple buttons are pressed
            if (shared)
            {
                foreach (var output in macros)
                {
                    var ifStatement = string.Join(" && ",
                        output.Input!.Inputs().Select(input =>
                            $"debounce[{debounces[output.Name + input.Generate(xbox)]}]"));
                    var sharedReset = output.Input!.Inputs().Aggregate("",
                        (current, input) => current + string.Join("",
                            inputs[input.Generate(xbox)].Select(s => $"debounce[{s}]=0;").Distinct()));
                    ret += @$"if ({ifStatement}) {{{sharedReset}}}";
                }
            }

            return ret.Replace('\n', ' ');
        }

        private int CalculateDebounceTicks()
        {
            var combined = DeviceType == DeviceControllerType.Guitar && CombinedDebounce;
            var count = Bindings.SelectMany(binding => binding.Outputs.Items)
                .Where(s => s is OutputButton button && (!combined || !button.IsStrum)).Select(s => s.Name).Distinct()
                .Count();
            if (combined)
            {
                count++;
            }

            return count;
        }

        public bool IsCombinedChild(Output output)
        {
            return !Bindings.Contains(output);
        }

        public Dictionary<string, List<int>> GetPins(string type)
        {
            var pins = new Dictionary<string, List<int>>();
            foreach (var binding in Bindings)
            {
                var configs = binding.GetPinConfigs();
                //Exclude digital or analog pins (which use a guid containing a -
                if (configs.Any(s => s.Type == type || (type.Contains("-") && s.Type.Contains("-")))) continue;
                if (!pins.ContainsKey(binding.Name))
                {
                    pins[binding.Name] = new();
                }

                foreach (var pinConfig in configs)
                {
                    pins[binding.Name].AddRange(pinConfig.Pins);
                }
            }

            if (IsApa102 && _apa102SpiConfig != null)
            {
                pins["APA102"] = _apa102SpiConfig.Pins.ToList();
            }

            return pins;
        }

        public void UpdateErrors()
        {
            bool foundError = false;
            foreach (var output in Bindings)
            {
                output.UpdateErrors();
                if (!string.IsNullOrEmpty(output.ErrorText))
                {
                    foundError = true;
                }
            }

            HasError = foundError;
        }
    }
}