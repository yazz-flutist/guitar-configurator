using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Collections;
using DynamicData;
using DynamicData.Kernel;
using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.DeviceNotify.Info;
using LibUsbDotNet.DeviceNotify.Linux;
using LibUsbDotNet.Main;
using ReactiveUI;
using Timer = System.Timers.Timer;
#if Windows
using GuitarConfigurator.NetCore.Notify;
using LibUsbDotNet.WinUsb;
#endif

namespace GuitarConfigurator.NetCore.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen, IDisposable
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new();
        public ReactiveCommand<Unit, IRoutableViewModel> Configure { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, IRoutableViewModel?> GoBack => Router.NavigateBack;

        public AvaloniaList<IConfigurableDevice> Devices { get; } = new();

        private IConfigurableDevice? _selectedDevice;
        private IConfigurableDevice? _disconnectedDevice;

        private readonly ObservableAsPropertyHelper<bool> _migrationSupported;
        public bool MigrationSupported => _migrationSupported.Value;

        private readonly ObservableAsPropertyHelper<bool> _isPico;
        public bool IsPico => _isPico.Value;

        private readonly ObservableAsPropertyHelper<bool> _is32U4;
        public bool Is32U4 => _is32U4.Value;

        private readonly ObservableAsPropertyHelper<bool> _isUno;
        public bool IsUno => _isUno.Value;

        private readonly ObservableAsPropertyHelper<bool> _isMega;
        public bool IsMega => _isMega.Value;

        private readonly ObservableAsPropertyHelper<bool> _isDfu;
        public bool IsDfu => _isDfu.Value;

        private readonly ObservableAsPropertyHelper<bool> _newDevice;
        public bool NewDevice => _newDevice.Value;

        private static readonly string UdevFile = "99-ardwiino.rules";
        private static readonly string UdevPath = $"/etc/udev/rules.d/{UdevFile}";

        public IEnumerable<Arduino32U4Type> Arduino32U4Types => Enum.GetValues<Arduino32U4Type>();
        public IEnumerable<MegaType> MegaTypes => Enum.GetValues<MegaType>();
        public IEnumerable<UnoMegaType> UnoMegaTypes => Enum.GetValues<UnoMegaType>();
        public IEnumerable<Board> PicoTypes => Board.Rp2040Boards;
        public IEnumerable<DeviceInputType> DeviceInputTypes => Enum.GetValues<DeviceInputType>();

        private UnoMegaType _unoMegaType;

        public UnoMegaType UnoMegaType
        {
            get => _unoMegaType;
            set => this.RaiseAndSetIfChanged(ref _unoMegaType, value);
        }

        private MegaType _megaType;

        public MegaType MegaType
        {
            get => _megaType;
            set => this.RaiseAndSetIfChanged(ref _megaType, value);
        }

        private DeviceInputType _deviceInputType;

        public DeviceInputType DeviceInputType
        {
            get => _deviceInputType;
            set => this.RaiseAndSetIfChanged(ref _deviceInputType, value);
        }


        private Arduino32U4Type _arduino32U4Type;

        public Arduino32U4Type Arduino32U4Type
        {
            get => _arduino32U4Type;
            set => this.RaiseAndSetIfChanged(ref _arduino32U4Type, value);
        }

        private Board _picoType = Board.Rp2040Boards[0];

        public Board PicoType
        {
            get => _picoType;
            set => this.RaiseAndSetIfChanged(ref _picoType, value);
        }

        public IConfigurableDevice? SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }


        private bool _working = true;

        public bool Working
        {
            get => _working;
            set => this.RaiseAndSetIfChanged(ref _working, value);
        }


        private bool _programming;

        private bool _installed;

        public bool Installed
        {
            get => _installed;
            set => this.RaiseAndSetIfChanged(ref _installed, value);
        }

        private string _progressbarcolor = "#FF0078D7";

        public string ProgressbarColor
        {
            get => _progressbarcolor;
            set => this.RaiseAndSetIfChanged(ref _progressbarcolor, value);
        }

        private readonly ObservableAsPropertyHelper<bool> _connected;
        public bool Connected => _connected.Value;

        private bool _readyToConfigure;

        public bool ReadyToConfigure
        {
            get => _readyToConfigure;
            set => this.RaiseAndSetIfChanged(ref _readyToConfigure, value);
        }

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _message = "Connected";

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        internal IObservable<PlatformIo.PlatformIoState> Write(ConfigViewModel config)
        {
            StartWorking();
            config.Generate(Pio);
            var env = config.MicroController!.Board.Environment;
            if (config.MicroController.Board.HasUsbmcu)
            {
                env += "_usb";
            }

            if (NewDevice)
            {
                env = env.Replace("_8", "");
                env = env.Replace("_16", "");
            }

            var output = new StringBuilder();
            _programming = true;
            var command = Pio.RunPlatformIo(env, new[] {"run", "--target", "upload"},
                "Writing",
                0, 90, SelectedDevice);
            command.ObserveOn(RxApp.MainThreadScheduler).Subscribe(s =>
                {
                    UpdateProgress(s);
                    if (s.Log != null)
                    {
                        output.Append(s.Log + "\n");
                    }
                }, (_) =>
                {
                    ProgressbarColor = "red";
                    config.ShowIssueDialog.Handle((output.ToString(), config)).Subscribe(s => _programming = false);
                },
                () => { _programming = false; });
            return command.OnErrorResumeNext(Observable.Return(command.Value));
        }

        private readonly IDeviceNotifier _deviceListener;
        public PlatformIo Pio { get; } = new();

        private readonly Timer _timer = new();

        private class RegDeviceNotifyInfo : IUsbDeviceNotifyInfo
        {
            private readonly UsbRegistry _dev;

            public RegDeviceNotifyInfo(UsbRegistry dev)
            {
                _dev = dev;
            }

            public UsbSymbolicName SymbolicName => UsbSymbolicName.Parse(_dev.SymbolicName);

            public string Name => _dev.DevicePath;

            public Guid ClassGuid => _dev.DeviceInterfaceGuids[0];

            public int IdVendor => _dev.Vid;

            public int IdProduct => _dev.Pid;

            public string SerialNumber => _dev.Device.Info.SerialString;

            public bool Open(out UsbDevice usbDevice)
            {
                usbDevice = _dev.Device;
                return usbDevice != null && usbDevice.Open();
            }
        }

        private class DeviceNotifyArgsRegistry : DeviceNotifyEventArgs
        {
            public DeviceNotifyArgsRegistry(UsbRegistry dev)
            {
                Device = new RegDeviceNotifyInfo(dev);
                DeviceType = DeviceType.DeviceInterface;
                EventType = EventType.DeviceArrival;
            }
        }

        public MainWindowViewModel()
        {
            Configure = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new ConfigViewModel(this))
            );
            Router.Navigate.Execute(new MainViewModel(this));
            _migrationSupported = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s?.MigrationSupported != false)
                .ToProperty(this, s => s.MigrationSupported);
            _connected = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s != null)
                .ToProperty(this, s => s.Connected);
            _isPico = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s is PicoDevice)
                .ToProperty(this, s => s.IsPico);
            _is32U4 = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s is Arduino arduino && arduino.Is32U4())
                .ToProperty(this, s => s.Is32U4);
            _isUno = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s is Arduino arduino && arduino.IsUno())
                .ToProperty(this, s => s.IsUno);
            _isMega = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s is Arduino arduino && arduino.IsMega())
                .ToProperty(this, s => s.IsMega);
            _isDfu = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s is Dfu)
                .ToProperty(this, s => s.IsDfu);
            _newDevice = this.WhenAnyValue(x => x.SelectedDevice)
                .Select(s => s != null && s is not Ardwiino && s is not Santroller)
                .ToProperty(this, s => s.NewDevice);

            Devices.CollectionChanged += (_, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        SelectedDevice ??= Devices.First();
                        break;
                    case NotifyCollectionChangedAction.Remove:
                    {
                        if (Devices.Any())
                        {
                            if (e.OldItems!.Contains(SelectedDevice))
                            {
                                _ = Task.Delay(1)
                                    .ContinueWith(_ => SelectedDevice = Devices.FirstOrDefault(_ => true, null));
                            }
                        }

                        break;
                    }
                }
            };
#if Windows
            _deviceListener = new WindowsDeviceNotifierAvalonia();
#else
            _deviceListener = new LinuxDeviceNotifier();
#endif
            _deviceListener.OnDeviceNotify += OnDeviceNotify;
            _timer.Elapsed += DevicePoller_Tick;
            _timer.AutoReset = false;
            StartWorking();
            Pio.InitialisePlatformIo().Subscribe(UpdateProgress, (_) => ProgressbarColor = "red", () =>
            {
                Complete(100);
                Working = false;
                Installed = true;
#if Windows
                List<UsbRegistry> deviceListAll = new List<UsbRegistry>();
                List<WinUsbRegistry> deviceList = new List<WinUsbRegistry>();
                WinUsbRegistry.GetWinUsbRegistryList(WindowsDeviceNotifierAvalonia.UsbGUID, out deviceList);
                deviceListAll.AddRange(deviceList);
                WinUsbRegistry.GetWinUsbRegistryList(WindowsDeviceNotifierAvalonia.ArdwiinoGUID, out deviceList);
                deviceListAll.AddRange(deviceList);
                WinUsbRegistry.GetWinUsbRegistryList(WindowsDeviceNotifierAvalonia.SantrollerGUID, out deviceList);
                deviceListAll.AddRange(deviceList);
#else
                List<UsbRegistry> deviceListAll = UsbDevice.AllDevices.AsList();
#endif
                foreach (var dev in deviceListAll)
                {
                    OnDeviceNotify(null, new DeviceNotifyArgsRegistry(dev));
                }

                _timer.Start();
            });

            Task.Run(InstallDependencies);
        }

        private void Complete(int total)
        {
            Working = false;
            Message = "Done";
            Progress = total;
        }

        private void StartWorking()
        {
            Working = true;
            ProgressbarColor = "#FF0078D7";
        }

        private void UpdateProgress(PlatformIo.PlatformIoState state)
        {
            Progress = state.Percentage;
            Message = state.Message;
        }

        private readonly List<string> _currentDrives = new();
        private readonly HashSet<string> _currentDrivesTemp = new();
        private readonly List<string> _currentPorts = new();

        private void AddDevice(IConfigurableDevice device)
        {
            if (device is Arduino)
            {
                _ = Task.Delay(500).ContinueWith(_ => Devices.Add(device));
            }
            else
            {
                Devices.Add(device);
            }

            if (_disconnectedDevice == null) return;
            if (!_disconnectedDevice.DeviceAdded(device)) return;
            if (device is not ConfigurableUsbDevice) return;
            SelectedDevice = device;
            _disconnectedDevice = null;
            Complete(100);
        }

        private void DevicePoller_Tick(object? sender, ElapsedEventArgs e)
        {
            var drives = DriveInfo.GetDrives();
            _currentDrivesTemp.UnionWith(_currentDrives);
            foreach (var drive in drives)
            {
                if (_currentDrivesTemp.Remove(drive.RootDirectory.FullName))
                {
                    continue;
                }

                var uf2 = Path.Combine(drive.RootDirectory.FullName, "INFO_UF2.txt");
                if (drive.IsReady)
                {
                    if (File.Exists(uf2) && File.ReadAllText(uf2).Contains("RPI-RP2"))
                    {
                        AddDevice(new PicoDevice(Pio, drive.RootDirectory.FullName));
                    }
                }

                _currentDrives.Add(drive.RootDirectory.FullName);
            }

            // We removed all valid devices above, so anything left in currentDrivesSet is no longer valid
            Devices.RemoveMany(Devices.Where(x => x is PicoDevice pico && _currentDrivesTemp.Contains(pico.GetPath())));
            _currentDrives.RemoveMany(_currentDrivesTemp);

            var existingPorts = _currentPorts.ToHashSet();
            var ports = Pio.GetPorts().Result;
            if (ports != null)
            {
                foreach (var port in ports)
                {
                    if (existingPorts.Contains(port.Port))
                    {
                        continue;
                    }


                    var arduino = new Arduino(Pio, port);
                    AddDevice(arduino);
                    _currentPorts.Add(arduino.GetSerialPort());
                }

                var currentSerialPorts = ports.Select(port => port.Port).ToHashSet();
                _currentPorts.RemoveMany(_currentPorts.Where(port => !currentSerialPorts.Contains(port)));
                Devices.RemoveMany(Devices.Where(device =>
                    device is Arduino arduino && !currentSerialPorts.Contains(arduino.GetSerialPort())));
                if (_selectedDevice is Arduino arduinoDevice &&
                    !currentSerialPorts.Contains(arduinoDevice.GetSerialPort()))
                {
                    _disconnectedDevice = _selectedDevice;
                    _selectedDevice = null;
                }
            }

            ReadyToConfigure = null != SelectedDevice && Installed;
            _timer.Start();
        }


        private void OnDeviceNotify(object? sender, DeviceNotifyEventArgs e)
        {
            if (e.DeviceType != DeviceType.DeviceInterface) return;
            if (e.EventType == EventType.DeviceArrival)
            {
                var vid = e.Device.IdVendor;
                var pid = e.Device.IdProduct;
                if (vid == Dfu.DfuVid && (pid == Dfu.DfuPid16U2 || pid == Dfu.DfuPid8U2))
                {
                    AddDevice(new Dfu(e));
                }
                else if (e.Device.Open(out var dev))
                {
                    var revision = (ushort) dev.Info.Descriptor.BcdDevice;
                    var product = dev.Info.ProductString?.Split(new[] {'\0'}, 2)[0];
                    var serial = dev.Info.SerialString?.Split(new[] {'\0'}, 2)[0];
                    switch (product)
                    {
                        case "Santroller" when _programming && !IsPico:
                            return;
                        case "Santroller":
                            AddDevice(new Santroller(Pio, e.Device.Name, dev, product, serial, revision));
                            break;
                        case "Ardwiino" when _programming:
                        case "Ardwiino" when revision == Ardwiino.SerialArdwiinoRevision:
                            return;
                        case "Ardwiino":
                            AddDevice(new Ardwiino(Pio, e.Device.Name, dev, product, serial, revision));
                            break;
                        default:
                            dev.Close();
                            break;
                    }
                }
            }
            else
            {
                Devices.RemoveMany(Devices.Where(device => device.IsSameDevice(e.Device.Name)));
                if (_disconnectedDevice != null || _selectedDevice?.IsSameDevice(e.Device.Name) != true) return;
                _disconnectedDevice = _selectedDevice;
                _selectedDevice = null;
            }
        }

        public void Dispose()
        {
            _deviceListener.OnDeviceNotify -= OnDeviceNotify;
        }

        private static bool CheckDependencies()
        {
            // Call check dependencies on startup, and pop up a dialog saying drivers are missing would you like to install if they are missing
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var info = new ProcessStartInfo(Path.Combine(windowsDir, "pnputil.exe"));
                info.ArgumentList.Add("-e");
                info.RedirectStandardOutput = true;
                var process = Process.Start(info);
                if (process == null) return false;
                var output = process.StandardOutput.ReadToEnd();
                // Check if the driver exists (we install this specific version of the driver so its easiest to check for it.)
                return output.Contains("Atmel USB Devices") && output.Contains("Atmel Corporation") &&
                       output.Contains("10/02/2010 1.2.2.0");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return File.Exists(UdevPath);
            }

            return true;
        }

        private static async void InstallDependencies()
        {
            if (CheckDependencies()) return;
            //TODO: pop open a dialog before doing this
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                var appdataFolder = AssetUtils.GetAppDataFolder();
                var driverZip = Path.Combine(appdataFolder, "drivers.zip");
                var driverFolder = Path.Combine(appdataFolder, "drivers");
                await AssetUtils.ExtractZip("dfu.zip", driverZip, driverFolder);

                var info = new ProcessStartInfo(Path.Combine(windowsDir, "pnputil.exe"));
                info.ArgumentList.AddRange(new[] {"-i", "-a", Path.Combine(driverFolder, "atmel_usb_dfu.inf")});
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Just copy the file to install it, using pkexec for admin
                var appdataFolder = AssetUtils.GetAppDataFolder();
                var rules = Path.Combine(appdataFolder, UdevFile);
                await AssetUtils.ExtractFile(UdevFile, rules);
                var info = new ProcessStartInfo("pkexec");
                info.ArgumentList.AddRange(new[] {"cp", rules, UdevPath});
                info.UseShellExecute = true;
                Process.Start(info);
            }

            if (!CheckDependencies())
            {
                // Pop open a dialog that it failed and to try again
            }
        }
    }
}