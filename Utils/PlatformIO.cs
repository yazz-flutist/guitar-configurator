using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using IniParser;
using IniParser.Model;
using Mono.Unix;

namespace GuitarConfiguratorSharp.NetCore.Utils
{
    public class PlatformIo
    {
        public delegate void ProgressChangedHandler(string message, int state, double progressPercentage);

        public event ProgressChangedHandler? ProgressChanged;

        public delegate void TextChangedHandler(string message, bool clear);

        public event TextChangedHandler? TextChanged;

        public delegate void PlatformIoErrorHandler(bool error);

        public event PlatformIoErrorHandler? PlatformIoError;

        public delegate void PlatformIoWorkingHandler(bool working);

        public event PlatformIoWorkingHandler? PlatformIoWorking;

        public delegate void PlatformIoProgrammingHandler(bool working);

        public event PlatformIoProgrammingHandler? PlatformIoProgramming;

        public delegate void PlatformIoInstalledHandler();

        public event PlatformIoInstalledHandler? PlatformIoInstalled;

        private readonly string _pioExecutable;

        public string FirmwareDir { get; }

        //TODO: probab
        //TODO: probably have a nice script to update this, but for now: ` pio pkg list | grep "@"|cut -f1 -d"(" |cut -c 11- | sort -u | wc -l`
        private const int PackageCount = 17;

        private readonly Process _portProcess;

        public PlatformIo()
        {
            var appdataFolder = AssetUtils.GetAppDataFolder();
            if (!File.Exists(appdataFolder))
            {
                Directory.CreateDirectory(appdataFolder);
            }

            var pioFolder = Path.Combine(appdataFolder, "platformio");
            //TODO: is this correct for windows?
            var pioExecutablePath = Path.Combine(appdataFolder, "python", "bin", "platformio");
            _pioExecutable = pioExecutablePath;
            FirmwareDir = Path.Combine(appdataFolder, "firmware");

            _portProcess = new Process();
            _portProcess.EnableRaisingEvents = true;
            _portProcess.StartInfo.FileName = _pioExecutable;
            _portProcess.StartInfo.WorkingDirectory = FirmwareDir;
            _portProcess.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;

            _portProcess.StartInfo.Arguments = "device list --json-output";

            _portProcess.StartInfo.UseShellExecute = false;
            _portProcess.StartInfo.RedirectStandardOutput = true;
            _portProcess.StartInfo.RedirectStandardError = true;
        }

        public async Task RevertFirmware()
        {
            var appdataFolder = AssetUtils.GetAppDataFolder();
            if (Directory.Exists(FirmwareDir))
            {
                Directory.Delete(FirmwareDir, true);
            }

            ProgressChanged?.Invoke("Extracting Firmware", 0, 0);
            var firmwareZipPath = Path.Combine(appdataFolder, "firmware.zip");
            await AssetUtils.ExtractZip("firmware.zip", firmwareZipPath, FirmwareDir);
        }

        public async Task InitialisePlatformIo()
        {
            // On startup, reinstall the firmware, this will make sure that an update goes out, and also makes sure that the firmware is clean.
            await RevertFirmware();
            await File.WriteAllTextAsync(Path.Combine(FirmwareDir, "platformio.ini"),
                (await File.ReadAllTextAsync(Path.Combine(FirmwareDir, "platformio.ini"))).Replace(
                    "post:ardwiino_script_post.py",
                    "post:ardwiino_script_post_tool.py"));
            if (!File.Exists(_pioExecutable))
            {
                ProgressChanged?.Invoke("Searching for python", 1, 0);
                var python = await FindPython();
                ProgressChanged?.Invoke("Installing Platform.IO", 2, 60);
                var installerProcess = new Process();
                installerProcess.StartInfo.FileName = python;
                installerProcess.StartInfo.Arguments = $"-m pip install platformio==6.1.5";
                installerProcess.StartInfo.UseShellExecute = false;
                installerProcess.StartInfo.RedirectStandardOutput = true;
                installerProcess.StartInfo.RedirectStandardError = true;
                installerProcess.OutputDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);
                installerProcess.ErrorDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);
                installerProcess.Start();
                installerProcess.BeginOutputReadLine();
                installerProcess.BeginErrorReadLine();
                await installerProcess.WaitForExitAsync().ConfigureAwait(false);
                PlatformIoInstalled?.Invoke();
                await RunPlatformIo(null, new[] { "pkg", "install" }, "Installing packages (This may take a while)",
                    2,
                    60, 90, null).ConfigureAwait(false);
                await RunPlatformIo(null, new[] { "system", "prune", "-f" }, "Cleaning up", 2, 90, 90, null)
                    .ConfigureAwait(false);
            }
            else
            {
                PlatformIoInstalled?.Invoke();
            }

            ProgressChanged?.Invoke("Ready", 2, 100);
            PlatformIoWorking?.Invoke(false);
        }

        public async Task<PlatformIoPort[]?> GetPorts()
        {
            _portProcess.Start();
            var output = await _portProcess.StandardOutput.ReadToEndAsync();
            await _portProcess.WaitForExitAsync();
            return output != "" ? PlatformIoPort.FromJson(output) : null;
        }

        public async Task<int> RunPlatformIo(string? environment, string[] command, string progressMessage,
            int progressState, double progressStartingPercentage, double progressEndingPercentage,
            IConfigurableDevice? device)
        {
            PlatformIoError?.Invoke(false);
            PlatformIoWorking?.Invoke(true);
            var percentageStep = (progressEndingPercentage - progressStartingPercentage) / PackageCount;
            var currentProgress = progressStartingPercentage;
            var updating = environment == null && command.Length == 2 && command[1] == "install";
            var uploading = environment != null && command.Length > 1;
            var appdataFolder = AssetUtils.GetAppDataFolder();
            var pioFolder = Path.Combine(appdataFolder, "platformio");
            var python = await FindPython();
            var process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = python;
            process.StartInfo.WorkingDirectory = FirmwareDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            process.StartInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
            var args = new List<string>(command);
            args.Insert(0, _pioExecutable);
            var sections = 5;
            var isUsb = false;
            if (environment != null)
            {
                PlatformIoProgramming?.Invoke(true);
                if (device is Arduino)
                {
                    sections = 10;
                }

                if (environment.EndsWith("_usb"))
                {
                    ProgressChanged?.Invoke($"{progressMessage} - Looking for device", progressState, currentProgress);
                    currentProgress += percentageStep / sections;
                    if (device != null)
                    {
                        isUsb = true;
                    }

                    sections = 10;
                }

                percentageStep = (progressEndingPercentage - progressStartingPercentage);
                args.Add("--environment");
                args.Add(environment);
                if (uploading && !isUsb)
                {
                    if (environment.Contains("pico"))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Looking for device", progressState,
                            currentProgress);
                        currentProgress += percentageStep / sections;
                        sections = 2;
                    }

                    if (device != null)
                    {
                        var port = await device.GetUploadPort();
                        if (port != null)
                        {
                            args.Add("--upload-port");
                            args.Add(port);
                        }
                    }
                }
            }

            //Some pio stuff uses Standard Output, some uses Standard Error, its easier to just flatten both of those to a single stream
            process.StartInfo.Arguments =
                $"-c \"import subprocess;subprocess.run([{string.Join(",", args.Select(s => $"'{s}'"))}],stderr=subprocess.STDOUT)\"";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            TextChanged?.Invoke("", true);
            var state = 0;
            process.Start();

            // process.BeginOutputReadLine();
            // process.BeginErrorReadLine();
            var buffer = new char[1];
            var hasError = false;
            var main = sections == 5;
            var uploadPackage = "";
            var uploadCount = 11;
            var seen = new List<string>();
            while (!process.HasExited)
            {
                if (state == 0)
                {
                    var line = await process.StandardOutput.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    TextChanged?.Invoke(line, false);
                    if (updating)
                    {
                        var matches = Regex.Matches(line, @".+: Installing (.+)");
                        if (matches.Count > 0)
                        {
                            uploadPackage = matches[0].Groups[1].Value;
                            if (seen.Contains(uploadPackage)) continue;
                            seen.Add(uploadPackage);
                            uploadCount = 10;
                            state = 5;
                        }
                    }

                    if (uploading)
                    {
                        var matches = Regex.Matches(line, @"Processing (.+?) \(.+\)");
                        if (matches.Count > 0)
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Building", progressState,
                                currentProgress);
                            currentProgress += percentageStep / sections;
                        }

                        if (line.StartsWith("Detecting microcontroller type"))
                        {
                            if (device is Santroller)
                            {
                                device.Bootloader();
                            }
                        }

                        if (line.StartsWith("Looking for upload port..."))
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Looking for port", progressState,
                                currentProgress);
                            currentProgress += percentageStep / sections;


                            if (device is Santroller or Ardwiino && !isUsb)
                            {
                                device.Bootloader();
                            }
                        }

                        if (line.Contains("SUCCESS"))
                        {
                            if (device is PicoDevice || sections == 5)
                            {
                                break;
                            }
                        }
                    }

                    if (line.Contains("AVR device initialized and ready to accept instructions"))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Reading Settings", progressState,
                            currentProgress);
                        state = 1;
                    }

                    if (line.Contains("writing flash"))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Uploading", progressState,
                            currentProgress);
                        state = 2;
                    }

                    if (line.Contains("reading on-chip flash data"))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Verifying", progressState,
                            currentProgress);
                        state = 3;
                    }

                    if (line.Contains("avrdude done.  Thank you."))
                    {
                        if (!main)
                        {
                            main = true;
                            continue;
                        }

                        break;
                    }

                    if (line.Contains("FAILED"))
                    {
                        PlatformIoError?.Invoke(true);
                        hasError = true;
                        ProgressChanged?.Invoke($"{progressMessage} - Error", progressState,
                            currentProgress + percentageStep / 5);
                        break;
                    }
                }
                else
                {
                    while ((await process.StandardOutput.ReadAsync(buffer, 0, 1)) > 0)
                    {
                        // process character...for example:
                        if (buffer[0] == '#')
                        {
                            currentProgress += percentageStep / 50 / sections;
                        }

                        if (buffer[0] == 's')
                        {
                            state = 0;
                            break;
                        }

                        switch (state)
                        {
                            case 1:
                                ProgressChanged?.Invoke($"{progressMessage} - Reading Settings", progressState,
                                    currentProgress);
                                break;
                            case 2:
                                ProgressChanged?.Invoke($"{progressMessage} - Uploading", progressState,
                                    currentProgress);
                                break;
                            case 3:
                                ProgressChanged?.Invoke($"{progressMessage} - Verifying", progressState,
                                    currentProgress);
                                break;
                            case 5:
                                if (buffer[0] == '%')
                                {
                                    uploadCount--;
                                    currentProgress += percentageStep / 11;
                                }

                                if (buffer[0] == '\n')
                                {
                                    // If a file is downloaded fast, it doesn't hit 100
                                    if (uploadCount > 0)
                                    {
                                        currentProgress += percentageStep / 11 * uploadCount;
                                    }

                                    state = 0;
                                }

                                ProgressChanged?.Invoke($"{progressMessage} - {uploadPackage}",
                                    progressState, currentProgress);
                                break;
                        }

                        if (state == 0)
                        {
                            break;
                        }
                    }
                }
            }

            await process.WaitForExitAsync();

            if (!hasError)
            {
                if (uploading)
                {
                    currentProgress = progressEndingPercentage;
                    ProgressChanged?.Invoke($"{progressMessage} - Waiting for Device", progressState, currentProgress);
                }
                else
                {
                    currentProgress = progressEndingPercentage;
                    ProgressChanged?.Invoke($"{progressMessage} - Done", progressState, currentProgress);
                }
            }

            PlatformIoWorking?.Invoke(false);
            PlatformIoProgramming?.Invoke(false);
            return process.ExitCode;
        }

        private async Task<string?> SetupVenv(string pythonApp, string pythonFolder)
        {
            var error = false;
            var penvProcess = new Process();
            penvProcess.StartInfo.FileName = pythonApp;
            penvProcess.StartInfo.Arguments = $"-m venv {pythonFolder}";
            penvProcess.StartInfo.UseShellExecute = false;
            penvProcess.StartInfo.RedirectStandardOutput = true;
            penvProcess.StartInfo.RedirectStandardError = true;
            penvProcess.OutputDataReceived += (sender, e) =>
            {
                TextChanged?.Invoke(e.Data!, false);
                // No support for venv here, so just fall back to downloading python in that case.
                if (e.Data != null && e.Data!.Contains("ensurepip is not"))
                {
                    error = true;
                    Directory.Delete(pythonFolder, true);
                    Console.WriteLine("Stdout");
                }
            };
            penvProcess.ErrorDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);
            penvProcess.Start();
            penvProcess.BeginOutputReadLine();
            penvProcess.BeginErrorReadLine();
            await penvProcess.WaitForExitAsync();
            if (error)
            {
                return null;
            }

            var executables = GetPythonExecutables();
            foreach (var executable in executables)
            {
                var pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (File.Exists(pythonAppdataExecutable))
                {
                    return pythonAppdataExecutable;
                }
            }

            return null;
        }

        private async Task<string?> FindPython()
        {
            var appdataFolder = AssetUtils.GetAppDataFolder();
            var pythonFolder = Path.Combine(appdataFolder, "python");
            var pythonLoc = Path.Combine(pythonFolder, "python.tar.gz");
            var executables = GetPythonExecutables();
            Directory.CreateDirectory(appdataFolder);

            foreach (var executable in executables)
            {
                var pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (File.Exists(pythonAppdataExecutable))
                {
                    return pythonAppdataExecutable;
                }
            }

            string? foundExecutable = null;
            foreach (var executable in executables)
            {
                foundExecutable = GetFullPath(executable);
                if (foundExecutable == null) continue;
                var ret = await SetupVenv(executable, pythonFolder);
                if (ret != null)
                {
                    return ret;
                }
            }

            Directory.CreateDirectory(pythonFolder);
            if (foundExecutable == null)
            {
                ProgressChanged?.Invoke("Downloading python portable", 1, 10);
                var pythonJsonLoc = Path.Combine(pythonFolder, "python.json");
                var arch = GetSysType();
                using (var download = new HttpClientDownloadWithProgress(
                           "https://api.github.com/repos/indygreg/python-build-standalone/releases/62235403",
                           pythonJsonLoc))
                {
                    await download.StartDownload().ConfigureAwait(false);
                }

                ;
                var jsonRelease = File.ReadAllText(pythonJsonLoc);
                var release = GithubRelease.FromJson(jsonRelease);
                var found = false;
                foreach (var asset in release.Assets)
                {
                    if (asset.Name!.EndsWith($"{arch}-install_only.tar.gz"))
                    {
                        using (var download =
                               new HttpClientDownloadWithProgress(asset.BrowserDownloadUrl!.ToString(), pythonLoc))
                        {
                            // This isnt right, the percentage bar goes too far
                            download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) =>
                                ProgressChanged?.Invoke("Downloading python portable", 1, 20 + (percentage * 0.4) ?? 0);
                            await download.StartDownload().ConfigureAwait(false);
                        }

                        ;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return null;
                }

                ProgressChanged?.Invoke("Extracting python portable", 1, 60);
                using (var inStream = File.OpenRead(pythonLoc))
                {
                    Stream gzipStream = new GZipInputStream(inStream);

                    var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                    tarArchive.ExtractContents(appdataFolder);
                    tarArchive.Close();

                    gzipStream.Close();
                    File.Delete(pythonLoc);
                    File.Delete(pythonJsonLoc);
                }
            }

            foreach (var executable in executables)
            {
                var pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (File.Exists(pythonAppdataExecutable))
                {
                    var unixFileInfo = new UnixFileInfo(pythonAppdataExecutable);
                    unixFileInfo.FileAccessPermissions |= FileAccessPermissions.UserExecute;
                    return pythonAppdataExecutable;
                }
            }

            return null;
        }

        public string[] GetPythonExecutables()
        {
            var executables = new[] { "python3", "python", Path.Combine("bin", "python3.10") };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executables = new[] { "python.exe" };
            }

            return executables;
        }

        public string GetSysType()
        {
            var arch = "unknown";
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                    arch = "i686";
                    break;
                case Architecture.X64:
                    arch = "x86_64";
                    break;
                case Architecture.Arm:
                    arch = "armv6l";
                    break;
                case Architecture.Arm64:
                    arch = "aarch64";
                    break;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"{arch}-pc-windows-msvc-shared";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"{arch}-apple-darwin";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return $"{arch}-unknown-linux-gnu";
            }

            return "unsupported";
        }

        public bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public string? GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH")!;
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return null;
        }
    }
}