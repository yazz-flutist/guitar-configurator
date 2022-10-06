using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using IniParser;
using IniParser.Model;

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
        public delegate void PlatformIoInstalledHandler();

        public event PlatformIoInstalledHandler? PlatformIoInstalled;
        public delegate void CommandCompleteHandler();

        public event CommandCompleteHandler? CommandComplete;

        private string _pioExecutable;
        public string ProjectDir { get; }
        private readonly List<string> _environments;

        public bool Ready { get; }

        public PlatformIo()
        {
            _environments = new List<string>();
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            _pioExecutable = pioExecutablePath;
            ProjectDir = Path.Combine(appdataFolder, "firmware");
            Ready = Directory.Exists(ProjectDir) && File.Exists(pioExecutablePath);
        }

        public async Task RevertFirmware()
        {
            string appdataFolder = AssetUtils.GetAppDataFolder();
            if (Directory.Exists(ProjectDir))
            {
                Directory.Delete(ProjectDir, true);
            }
            ProgressChanged?.Invoke("Extracting Firmware", 0, 0);
            string firmwareZipPath = Path.Combine(appdataFolder, "firmware.zip");
            await AssetUtils.ExtractZip("firmware.zip", firmwareZipPath, ProjectDir);
        }

        public async Task InitialisePlatformIo()
        {
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string installerZipPath = Path.Combine(pioFolder, "installer.zip");
            string installerPath = Path.Combine(pioFolder, "installer");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            _pioExecutable = pioExecutablePath;

            // On startup, reinstall the firmware, this will make sure that an update goes out, and also makes sure that the firmware is clean.
            await RevertFirmware();
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.SkipInvalidLines = true;
            IniData data = parser.ReadFile(Path.Combine(ProjectDir, "platformio.ini"));
            foreach (var key in data.Sections)
            {
                if (key.SectionName.StartsWith("env:"))
                {
                    _environments.Add(key.SectionName.Split(':')[1]);
                }
            }
            File.WriteAllText(Path.Combine(ProjectDir, "platformio.ini"), File.ReadAllText(Path.Combine(ProjectDir, "platformio.ini")).Replace("post:ardwiino_script_post.py", "post:ardwiino_script_post_tool.py"));
            if (!File.Exists(pioExecutablePath))
            {
                ProgressChanged?.Invoke("Extracting Platform.IO Installer", 0, 0);
                Directory.CreateDirectory(pioFolder);
                Directory.CreateDirectory(installerPath);
                await AssetUtils.ExtractZip("pio-installer.zip", installerZipPath, installerPath);
                ProgressChanged?.Invoke("Searching for python", 1, 0);
                var python = await FindPython();
                ProgressChanged?.Invoke("Installing Platform.IO", 2, 10);
                Process installerProcess = new Process();
                installerProcess.StartInfo.FileName = python;
                installerProcess.StartInfo.Arguments = $"-m pioinstaller";
                installerProcess.StartInfo.EnvironmentVariables["PYTHONPATH"] = installerPath;
                installerProcess.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
                installerProcess.StartInfo.UseShellExecute = false;
                installerProcess.StartInfo.RedirectStandardOutput = true;
                installerProcess.StartInfo.RedirectStandardError = true;
                installerProcess.OutputDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);
                installerProcess.ErrorDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);
                installerProcess.Start();
                installerProcess.BeginOutputReadLine();
                installerProcess.BeginErrorReadLine();
                await installerProcess.WaitForExitAsync();
                File.Delete(installerZipPath);
                Directory.Delete(installerPath, true);
                PlatformIoInstalled?.Invoke();
                await RunPlatformIo(null, "run", "Building packages", 2, 20, 90, null).ConfigureAwait(false);
                await RunPlatformIo(null, "system prune -f", "Cleaning up", 2, 90, 90, null).ConfigureAwait(false);
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
            if (_pioExecutable == null)
            {
                return new PlatformIoPort[] { };
            }

            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = _pioExecutable;
            process.StartInfo.WorkingDirectory = ProjectDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;

            process.StartInfo.Arguments = $"device list --json-output";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;


            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();
            if (output != "") {
                return PlatformIoPort.FromJson(output);
            }
            return null;

        }

        public async Task<int> RunPlatformIo(string? environment, string command, string progressMessage, int progressState, double progressStartingPercentage, double progressEndingPercentage, IConfigurableDevice? device)
        {
            PlatformIoError?.Invoke(false);
            PlatformIoWorking?.Invoke(true);
            double percentageStep = (progressEndingPercentage - progressStartingPercentage) / _environments.Count;
            double currentProgress = progressStartingPercentage;
            bool building = environment == null && command == "run";
            bool uploading = environment != null && command.StartsWith("run");
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = _pioExecutable;
            process.StartInfo.WorkingDirectory = ProjectDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            int sections = 5;
            bool isUsb = false;
            if (environment != null)
            {
                if (environment.EndsWith("-usb"))
                {
                    ProgressChanged?.Invoke($"{progressMessage} - Looking for device", progressState, currentProgress);
                    currentProgress += percentageStep / sections;
                    if (device != null)
                    {
                        environment = await device.GetUploadPort();
                        if (environment == null)
                        {
                            throw new NotImplementedException("unable to find port?");
                        }
                        isUsb = true;
                    }
                }
                percentageStep = (progressEndingPercentage - progressStartingPercentage);
                var args = $"{command} --environment {environment}";
                if (uploading && !isUsb)
                {
                    if (environment.Contains("pico"))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Looking for device", progressState, currentProgress);
                        currentProgress += percentageStep / sections;
                        sections = 2;
                    }
                    if (device != null)
                    {
                        var port = await device.GetUploadPort();
                        if (port != null)
                        {
                            args += $" --upload-port {port}";
                        }
                    }
                }
                process.StartInfo.Arguments = args;
            }
            else
            {
                process.StartInfo.Arguments = $"{command}";
            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            TextChanged?.Invoke("", true);
            int state = 0;
            bool done = false;
            process.OutputDataReceived += (sender, e) =>
            {
                TextChanged?.Invoke(e.Data!, false);
                if (building && e.Data != null)
                {
                    var matches = Regex.Matches(e.Data, @"Processing (.+?) \(.+\)");
                    if (matches.Count > 0)
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - {matches[0].Groups[1].Value}", progressState, currentProgress);
                        currentProgress += percentageStep;
                    }
                }
                if (uploading && e.Data != null)
                {
                    var matches = Regex.Matches(e.Data, @"Processing (.+?) \(.+\)");
                    if (matches.Count > 0)
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Building", progressState, currentProgress);
                        currentProgress += percentageStep / sections;
                    }
                    if (e.Data.StartsWith("Looking for upload port..."))
                    {
                        ProgressChanged?.Invoke($"{progressMessage} - Looking for port", progressState, currentProgress);
                        currentProgress += percentageStep / sections;
                        if (environment?.Contains("uno_mega_usb") == true)
                        {
                            device?.BootloaderUsb();
                        }
                        else
                        {
                            device?.Bootloader();
                        }
                    }

                    if (e.Data.Contains("SUCCESS"))
                    {
                        done = true;
                    }
                }
            };

            process.Start();

            process.BeginOutputReadLine();
            // process.BeginErrorReadLine();
            char[] buffer = new char[1];
            bool hasError = false;
            while (true)
            {
                if (state == 0)
                {
                    var line = await process.StandardError.ReadLineAsync();
                    if (line != null)
                    {
                        if (line.Contains("AVR device initialized and ready to accept instructions"))
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Reading Settings", progressState, currentProgress);
                            state = 1;
                        }
                        if (line.Contains("writing flash"))
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Uploading", progressState, currentProgress);
                            state = 2;
                        }
                        if (line.Contains("reading on-chip flash data"))
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Verifying", progressState, currentProgress);
                            state = 3;
                        }
                        if (line.Contains("avrdude done.  Thank you."))
                        {
                            break;
                        }
                        if (line.Contains("FAILED"))
                        {
                            PlatformIoError?.Invoke(true);
                            hasError = true;
                            ProgressChanged?.Invoke($"{progressMessage} - Error", progressState, currentProgress + percentageStep / 5);
                            break;
                        }
                        if (done)
                        {
                            break;
                        }
                        TextChanged?.Invoke(line, false);
                    }
                    else
                    {
                        await Task.Delay(1);
                    }
                }
                else
                {
                    while ((await process.StandardError.ReadAsync(buffer, 0, 1)) > 0)
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
                        if (state == 1)
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Reading Settings", progressState, currentProgress);
                        }
                        if (state == 2)
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Uploading", progressState, currentProgress);
                        }
                        if (state == 3)
                        {
                            ProgressChanged?.Invoke($"{progressMessage} - Verifying", progressState, currentProgress);
                        }
                    }
                }
            }

            await process.WaitForExitAsync();

            CommandComplete?.Invoke();
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
            // TODO:  restart uno when done
            PlatformIoWorking?.Invoke(false);
            return process.ExitCode;
        }
        private async Task<string?> FindPython()
        {
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pythonFolder = Path.Combine(appdataFolder, "python");
            var pythonLoc = Path.Combine(pythonFolder, "python.tar.gz");
            var executables = GetPythonExecutables();
            Directory.CreateDirectory(appdataFolder);

            string? foundExecutable = null;
            foreach (var executable in executables)
            {
                foundExecutable = GetFullPath(executable);
                if (foundExecutable != null)
                {
                    return executable;
                }
            }
            foreach (var executable in executables)
            {
                string pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (File.Exists(pythonAppdataExecutable))
                {
                    return pythonAppdataExecutable;
                }
            }
            Directory.CreateDirectory(pythonFolder);
            if (foundExecutable == null)
            {
                ProgressChanged?.Invoke("Downloading python portable", 1, 10);
                var pythonJsonLoc = Path.Combine(pythonFolder, "python.json");
                string arch = GetSysType();
                using (var download = new HttpClientDownloadWithProgress("https://api.github.com/repos/indygreg/python-build-standalone/releases/62235403", pythonJsonLoc))
                {
                    await download.StartDownload().ConfigureAwait(false);
                };
                var jsonRelease = File.ReadAllText(pythonJsonLoc);
                GithubRelease release = GithubRelease.FromJson(jsonRelease);
                bool found = false;
                foreach (var asset in release.Assets)
                {
                    if (asset.Name!.EndsWith($"{arch}-install_only.tar.gz"))
                    {
                        using (var download = new HttpClientDownloadWithProgress(asset.BrowserDownloadUrl!.ToString(), pythonLoc))
                        {
                            download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => ProgressChanged?.Invoke("Downloading python portable", 1, 20 + (percentage * 0.4) ?? 0);
                            await download.StartDownload().ConfigureAwait(false);
                        };
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

                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                    tarArchive.ExtractContents(appdataFolder);
                    tarArchive.Close();

                    gzipStream.Close();
                    File.Delete(pythonLoc);
                    File.Delete(pythonJsonLoc);
                }
            }
            foreach (var executable in executables)
            {
                string pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (File.Exists(pythonAppdataExecutable))
                {
                    var unixFileInfo = new Mono.Unix.UnixFileInfo(pythonAppdataExecutable);
                    unixFileInfo.FileAccessPermissions |= Mono.Unix.FileAccessPermissions.UserExecute;
                    return pythonAppdataExecutable;
                }
            }
            return null;
        }

        public string[] GetPythonExecutables()
        {
            var executables = new string[] { "python3", "python", Path.Combine("bin", "python3.10") };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executables = new string[] { "python.exe" };
            }
            return executables;
        }
        public string GetSysType()
        {
            string arch = "unknown";
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return $"{arch}-apple-darwin";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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