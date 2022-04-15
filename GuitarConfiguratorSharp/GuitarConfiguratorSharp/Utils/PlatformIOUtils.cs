using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Net;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Avalonia;
using Avalonia.Platform;
using System.IO.Compression;
using GuitarConfiguratorSharp.Utils.Github;

namespace GuitarConfiguratorSharp.Utils
{
    public class PlatformIOUtils
    {
        // TODO: instead of all these console.writelines, expose a percentage and message as a event that can be listened to, similar to what HttpClientDownloadWithProgress is doing.

        private async static Task<string?> FindPlatformIO()
        {
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string installerZipPath = Path.Combine(pioFolder, "installer.zip");
            string installerPath = Path.Combine(pioFolder, "installer");
            string pioExecutable = Path.Combine(pioFolder, "penv", "bin", "platformio");
            if (!System.IO.File.Exists(pioExecutable))
            {
                Directory.CreateDirectory(pioFolder);
                Directory.CreateDirectory(installerPath);
                using (var f = System.IO.File.OpenWrite(installerZipPath))
                {
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    using (var target = assets.Open(new Uri("avares://GuitarConfiguratorSharp/Assets/pio-installer.zip")))
                    {
                        await target.CopyToAsync(f).ConfigureAwait(false);
                    }
                }
                ZipFile.ExtractToDirectory(installerZipPath, installerPath);
                Process installerProcess = new Process();
                installerProcess.StartInfo.FileName = await FindPython();
                installerProcess.StartInfo.Arguments = $"-m pioinstaller";
                installerProcess.StartInfo.EnvironmentVariables["PYTHONPATH"] = installerPath;
                installerProcess.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
                installerProcess.StartInfo.UseShellExecute = false;
                installerProcess.Start();
                installerProcess.WaitForExit();
            }
            return pioExecutable;
        }

        public async static Task<string> RunPlatformIO(string environment, string command)
        {
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            Process process = new Process();
            process.StartInfo.FileName = await FindPlatformIO().ConfigureAwait(false);
            process.StartInfo.WorkingDirectory = "/home/sanjay/Code/ArdwiinoV3/src";
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            process.StartInfo.Arguments = $"--environment {environment} {command}";
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            return "";
        }

        private static string GetAppDataFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(folder, "SantrollerConfigurator");
        }
        private async static Task<string?> FindPython()
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
                    Console.WriteLine($"Found python at: {foundExecutable}");
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

                Console.WriteLine("Downloading python portable");
                var pythonJsonLoc = Path.Combine(pythonFolder, "python.json");
                string arch = GetSysType();
                using (var download = new HttpClientDownloadWithProgress("https://api.github.com/repos/indygreg/python-build-standalone/releases/62235403", pythonJsonLoc))
                {
                    download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => Console.WriteLine($"json: {percentage}");
                    await download.StartDownload().ConfigureAwait(false);
                    Console.WriteLine("downloaded json");
                };
                var jsonRelease = System.IO.File.ReadAllText(pythonJsonLoc);
                GithubRelease release = GithubRelease.FromJson(jsonRelease);
                bool found = false;
                foreach (var asset in release.Assets)
                {
                    if (asset.Name.EndsWith($"{arch}-install_only.tar.gz"))
                    {
                        using (var download = new HttpClientDownloadWithProgress(asset.BrowserDownloadUrl.ToString(), pythonLoc))
                        {
                            download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => Console.WriteLine($"python: {percentage}");
                            await download.StartDownload().ConfigureAwait(false);
                            Console.WriteLine("downloaded python");
                        };
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return null;
                }
                using (var inStream = System.IO.File.OpenRead(pythonLoc))
                {
                    Console.WriteLine("Extracting python");
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

        public static string[] GetPythonExecutables()
        {
            var executables = new string[] { "python3", "python", Path.Combine("bin", "python3.10") };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executables = new string[] { "python.exe" };
            }
            return executables;
        }
        public static string GetSysType()
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
        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string? GetFullPath(string fileName)
        {
            if (System.IO.File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, fileName);
                if (System.IO.File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
        public class HttpClientDownloadWithProgress : IDisposable
        {
            private readonly string _downloadUrl;
            private readonly string _destinationFilePath;

            private HttpClient _httpClient;

            public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

            public event ProgressChangedHandler? ProgressChanged;

            public HttpClientDownloadWithProgress(string downloadUrl, string destinationFilePath)
            {
                _downloadUrl = downloadUrl;
                _destinationFilePath = destinationFilePath;
                _httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
            }

            public async Task StartDownload()
            {
                using (var response = await _httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    await DownloadFileFromHttpResponseMessage(response).ConfigureAwait(false);
            }

            private async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response)
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    await ProcessContentStream(totalBytes, contentStream).ConfigureAwait(false);
            }

            private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
            {
                var totalBytesRead = 0L;
                var readCount = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;
                using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    do
                    {
                        var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            isMoreToRead = false;
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                            continue;
                        }

                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        totalBytesRead += bytesRead;
                        readCount += 1;

                        if (readCount % 100 == 0)
                            TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                    }
                    while (isMoreToRead);
                }
            }

            private void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
            {
                if (ProgressChanged == null)
                    return;

                double? progressPercentage = null;
                if (totalDownloadSize.HasValue)
                    progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

                ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
            }

            public void Dispose()
            {
                _httpClient?.Dispose();
            }
        }
    }
}