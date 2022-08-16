using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;
using Avalonia;
using ReactiveUI;
using Avalonia.Platform;
using System.IO.Compression;
using GuitarConfiguratorSharp.Utils.Github;
using IniParser;
using IniParser.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GuitarConfiguratorSharp.Utils
{

    public class PlatformIO
    {
        public delegate void ProgressChangedHandler(string message, int state, double progressPercentage);

        public event ProgressChangedHandler? ProgressChanged;

        public delegate void TextChangedHandler(string message, bool clear);

        public event TextChangedHandler? TextChanged;
        public delegate void PlatformIOWorkingHandler(bool working);

        public event PlatformIOWorkingHandler? PlatformIOWorking;
        public delegate void PlatformIOInstalledHandler();

        public event PlatformIOInstalledHandler? PlatformIOInstalled;
        public delegate void CommandCompleteHandler();

        public event CommandCompleteHandler? CommandComplete;

        private string pioExecutable;
        public string ProjectDir { get; }
        private List<string> environments;

        public bool ready { get; }

        public PlatformIO()
        {
            environments = new List<string>();
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            this.pioExecutable = pioExecutablePath;
            this.ProjectDir = Path.Combine(appdataFolder, "firmware");
            ready = System.IO.Directory.Exists(this.ProjectDir) && System.IO.File.Exists(pioExecutablePath);
        }

        public async Task RevertFirmware()
        {
            string appdataFolder = AssetUtils.GetAppDataFolder();
            if (System.IO.Directory.Exists(this.ProjectDir))
            {
                System.IO.Directory.Delete(this.ProjectDir, true);
            }
            this.ProgressChanged?.Invoke("Extracting Firmware", 0, 0);
            string firmwareZipPath = Path.Combine(appdataFolder, "firmware.zip");
            await AssetUtils.ExtractZip("firmware.zip", firmwareZipPath, ProjectDir);
        }

        public async Task InitialisePlatformIO()
        {
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string installerZipPath = Path.Combine(pioFolder, "installer.zip");
            string installerPath = Path.Combine(pioFolder, "installer");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            this.pioExecutable = pioExecutablePath;

            // On startup, reinstall the firmware, this will make sure that an update goes out, and also makes sure that the firmware is clean.
            await RevertFirmware();
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.SkipInvalidLines = true;
            IniData data = parser.ReadFile(Path.Combine(ProjectDir, "platformio.ini"));
            foreach (var key in data.Sections)
            {
                if (key.SectionName.StartsWith("env:"))
                {
                    environments.Add(key.SectionName.Split(':')[1]);
                }
            }
            File.WriteAllText(Path.Combine(ProjectDir, "platformio.ini"), File.ReadAllText(Path.Combine(ProjectDir, "platformio.ini")).Replace("extra_scripts", "extra_scripts2"));
            if (!System.IO.File.Exists(pioExecutablePath))
            {
                this.ProgressChanged?.Invoke("Extracting Platform.IO Installer", 0, 0);
                Directory.CreateDirectory(pioFolder);
                Directory.CreateDirectory(installerPath);
                await AssetUtils.ExtractZip("pio-installer.zip", installerZipPath, installerPath);
                this.ProgressChanged?.Invoke("Searching for python", 1, 0);
                var python = await FindPython();
                this.ProgressChanged?.Invoke("Installing Platform.IO", 2, 10);
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
                System.IO.File.Delete(installerZipPath);
                System.IO.Directory.Delete(installerPath, true);
                this.PlatformIOInstalled?.Invoke();
                await RunPlatformIO(null, "run", "Building packages", 2, 20, 90, null).ConfigureAwait(false);
                await RunPlatformIO(null, "system prune -f", "Cleaning up", 2, 90, 90, null).ConfigureAwait(false);
            }
            else
            {

                this.PlatformIOInstalled?.Invoke();
            }
            this.ProgressChanged?.Invoke("Ready", 2, 100);
            this.PlatformIOWorking?.Invoke(false);
        }

        public async Task<PlatformIOPort[]> GetPorts()
        {
            if (pioExecutable == null)
            {
                return new PlatformIOPort[] { };
            }

            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = pioExecutable;
            process.StartInfo.WorkingDirectory = ProjectDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;

            process.StartInfo.Arguments = $"device list --json-output";

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;


            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();

            return PlatformIOPort.FromJson(output);

        }

        public async Task<int> RunPlatformIO(string? environment, string command, string progress_message, int progress_state, double progress_starting_percentage, double progress_ending_percentage, ConfigurableDevice? device)
        {
            this.PlatformIOWorking?.Invoke(true);
            double percentageStep = (progress_ending_percentage - progress_starting_percentage) / environments.Count;
            double currentProgress = progress_starting_percentage;
            bool building = environment == null && command == "run";
            bool uploading = environment != null && command.StartsWith("run");
            string appdataFolder = AssetUtils.GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = pioExecutable;
            process.StartInfo.WorkingDirectory = ProjectDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            if (environment != null)
            {
                var args = $"{command} --environment {environment}";
                var arduino = device as Arduino;
                if (arduino != null) {
                    args += $" --upload-port {arduino.GetSerialPort()}";
                }
                process.StartInfo.Arguments = args;
                percentageStep = (progress_ending_percentage - progress_starting_percentage);
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
            process.OutputDataReceived += (sender, e) =>
            {
                TextChanged?.Invoke(e.Data!, false);
                if (building && e.Data != null)
                {
                    var matches = Regex.Matches(e.Data, @"Processing (.+?) \(.+\)");
                    if (matches.Count > 0)
                    {
                        this.ProgressChanged?.Invoke($"{progress_message} - {matches[0].Groups[1].Value}", progress_state, currentProgress);
                        currentProgress += percentageStep;
                    }
                }
                if (uploading && e.Data != null)
                {
                    var matches = Regex.Matches(e.Data, @"Processing (.+?) \(.+\)");
                    if (matches.Count > 0)
                    {
                        this.ProgressChanged?.Invoke($"{progress_message} - Building", progress_state, currentProgress);
                        currentProgress += percentageStep / 5;
                    }
                    if (e.Data.StartsWith("Looking for upload port..."))
                    {
                        this.ProgressChanged?.Invoke($"{progress_message} - Looking for port", progress_state, currentProgress);
                        currentProgress += percentageStep / 5;
                        if (environment?.Contains("uno_mega_usb") == true)
                        {
                            device?.bootloaderUSB();
                        }
                        else
                        {
                            device?.bootloader();
                        }
                    }
                }
            };

            process.ErrorDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);

            process.Start();

            process.BeginOutputReadLine();
            // process.BeginErrorReadLine();
            char[] buffer = new char[1];
            while (true)
            {
                if (state == 0)
                {
                    var line = await process.StandardError.ReadLineAsync();
                    if (line != null)
                    {
                        if (line.Contains("AVR device initialized and ready to accept instructions"))
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Reading Settings", progress_state, currentProgress);
                            state = 1;
                        }
                        if (line.Contains("writing flash"))
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Uploading", progress_state, currentProgress);
                            state = 2;
                        }
                        if (line.Contains("reading on-chip flash data"))
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Verifying", progress_state, currentProgress);
                            state = 3;
                        }
                        if (line.Contains("avrdude done.  Thank you."))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    while ((await process.StandardError.ReadAsync(buffer, 0, 1)) > 0)
                    {

                        // process character...for example:
                        if (buffer[0] == '#')
                        {
                            currentProgress += percentageStep / 50 / 5;
                        }
                        if (buffer[0] == 's')
                        {
                            state = 0;
                            break;
                        }
                        if (state == 1)
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Reading Settings", progress_state, currentProgress);
                        }
                        if (state == 2)
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Uploading", progress_state, currentProgress);
                        }
                        if (state == 3)
                        {
                            this.ProgressChanged?.Invoke($"{progress_message} - Verifying", progress_state, currentProgress);
                        }
                    }
                }
            }

            await process.WaitForExitAsync();
            CommandComplete?.Invoke();
            if (uploading)
            {
                this.ProgressChanged?.Invoke($"{progress_message} - Waiting for Device", progress_state, currentProgress);
            }
            else
            {
                this.ProgressChanged?.Invoke($"{progress_message} - Done", progress_state, currentProgress);
            }
            this.PlatformIOWorking?.Invoke(false);
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
                if (System.IO.File.Exists(pythonAppdataExecutable))
                {
                    return pythonAppdataExecutable;
                }
            }
            Directory.CreateDirectory(pythonFolder);
            if (foundExecutable == null)
            {
                this.ProgressChanged?.Invoke("Downloading python portable", 1, 10);
                var pythonJsonLoc = Path.Combine(pythonFolder, "python.json");
                string arch = GetSysType();
                using (var download = new HttpClientDownloadWithProgress("https://api.github.com/repos/indygreg/python-build-standalone/releases/62235403", pythonJsonLoc))
                {
                    await download.StartDownload().ConfigureAwait(false);
                };
                var jsonRelease = System.IO.File.ReadAllText(pythonJsonLoc);
                GithubRelease release = GithubRelease.FromJson(jsonRelease);
                bool found = false;
                foreach (var asset in release.Assets)
                {
                    if (asset.Name!.EndsWith($"{arch}-install_only.tar.gz"))
                    {
                        using (var download = new HttpClientDownloadWithProgress(asset.BrowserDownloadUrl!.ToString(), pythonLoc))
                        {
                            download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => this.ProgressChanged?.Invoke("Downloading python portable", 1, 20 + (percentage * 0.4) ?? 0);
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
                this.ProgressChanged?.Invoke("Extracting python portable", 1, 60);
                using (var inStream = System.IO.File.OpenRead(pythonLoc))
                {
                    Stream gzipStream = new GZipInputStream(inStream);

                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                    tarArchive.ExtractContents(appdataFolder);
                    tarArchive.Close();

                    gzipStream.Close();
                    System.IO.File.Delete(pythonLoc);
                    System.IO.File.Delete(pythonJsonLoc);
                }
            }
            foreach (var executable in executables)
            {
                string pythonAppdataExecutable = Path.Combine(pythonFolder, executable);
                if (System.IO.File.Exists(pythonAppdataExecutable))
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
            if (System.IO.File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH")!;
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (System.IO.File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
    }
}