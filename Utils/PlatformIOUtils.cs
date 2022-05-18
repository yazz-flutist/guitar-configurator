using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;
using Avalonia;
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
        public delegate void PlatformIOReadyHandler();

        public event PlatformIOReadyHandler? PlatformIOReady;
        public delegate void CommandCompleteHandler();

        public event CommandCompleteHandler? CommandComplete;

        private string pioExecutable;
        private string projectDir;
        private List<string> environments;

        public bool ready { get; }

        public PlatformIO()
        {
            environments = new List<string>();
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            this.pioExecutable = pioExecutablePath;
            this.projectDir = "/home/sanjay/Code/ArdwiinoV3/src";
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.SkipInvalidLines = true;
            IniData data = parser.ReadFile(Path.Combine(projectDir, "platformio.ini"));
            foreach (var key in data.Sections)
            {
                if (key.SectionName.StartsWith("env:"))
                {
                    environments.Add(key.SectionName.Split(':')[1]);
                }
            }
            ready = System.IO.File.Exists(pioExecutablePath);
        }

        public async Task InitialisePlatformIO()
        {
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string installerZipPath = Path.Combine(pioFolder, "installer.zip");
            string installerPath = Path.Combine(pioFolder, "installer");
            string pioExecutablePath = Path.Combine(pioFolder, "penv", "bin", "platformio");
            this.pioExecutable = pioExecutablePath;
            if (!System.IO.File.Exists(pioExecutablePath))
            {
                this.ProgressChanged?.Invoke("Extracting Platform.IO Installer", 0, 0);
                Directory.CreateDirectory(pioFolder);
                Directory.CreateDirectory(installerPath);
                using (var f = System.IO.File.OpenWrite(installerZipPath))
                {
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    using (var target = assets!.Open(new Uri("/Assets/pio-installer.zip")))
                    {
                        await target.CopyToAsync(f).ConfigureAwait(false);
                    }
                }
                ZipFile.ExtractToDirectory(installerZipPath, installerPath);
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
                installerProcess.WaitForExit();
                System.IO.File.Delete(installerZipPath);
                System.IO.Directory.Delete(installerPath, true);
                await RunPlatformIO(null, "run", "Building packages", 2, 20, 90).ConfigureAwait(false);
                await RunPlatformIO(null, "system prune -f", "Cleaning up", 2, 90, 90).ConfigureAwait(false);
            }
            this.ProgressChanged?.Invoke("Ready", 2, 100);
            this.PlatformIOReady?.Invoke();
        }

        public async Task<PlatformIOPort[]> GetPorts()
        {
            if (pioExecutable == null)
            {
                return new PlatformIOPort[]{};
            }
            
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = pioExecutable;
            process.StartInfo.WorkingDirectory = projectDir;
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

        public Task<int> RunPlatformIO(string? environment, string command, string progress_message, int progress_state, double progress_starting_percentage, double progress_ending_percentage)
        {
            // When environment isn't set, we should be able to get a list of all targets, and then create percentages based on what target we are on
            var tcs = new TaskCompletionSource<int>();
            if (pioExecutable == null)
            {
                tcs.SetResult(0);
            }
            // TODO: can we get some sort of progress out of the text? I suspect not for compiling, but maybe for programming?
            // If we can, then divide it into percentageStep
            double percentageStep = (progress_ending_percentage - progress_starting_percentage) / environments.Count;
            double currentProgress = progress_starting_percentage;
            bool building = environment == null && command == "run";
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.StartInfo.FileName = pioExecutable;
            process.StartInfo.WorkingDirectory = projectDir;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            if (environment != null)
            {
                process.StartInfo.Arguments = $"--environment {environment} {command}";
                percentageStep = (progress_ending_percentage - progress_starting_percentage);
            }
            else
            {
                process.StartInfo.Arguments = $"{command}";

            }
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
                CommandComplete?.Invoke();
                this.ProgressChanged?.Invoke($"{progress_message} - Done", progress_state, currentProgress);
            };
            TextChanged?.Invoke("", true);
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
                    Console.WriteLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) => TextChanged?.Invoke(e.Data!, false);

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }

        private string GetAppDataFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(folder, "SantrollerConfigurator");
        }
        private async Task<string?> FindPython()
        {
            string appdataFolder = GetAppDataFolder();
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
                foreach (var asset in release.Assets!)
                {
                    if (asset.Name!.EndsWith($"{arch}-install_only.tar.gz"))
                    {
                        using (var download = new HttpClientDownloadWithProgress(asset.BrowserDownloadUrl!.ToString(), pythonLoc))
                        {
                            download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => this.ProgressChanged?.Invoke("Downloading python portable", 1, 20 + (percentage * 0.4) ?? 0); ;
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