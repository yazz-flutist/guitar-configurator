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
using Python.Included;
using Python.Runtime;

namespace GuitarConfiguratorSharp.Utils
{
    public class PlatformIOFinder
    {
        public async static Task<string?> FindPlatformIO()
        {
            string appdataFolder = GetAppDataFolder();
            string pioFolder = Path.Combine(appdataFolder, "platformio");
            string installerZipPath = Path.Combine(pioFolder, "installer.zip");
            string installerPath = Path.Combine(pioFolder, "installer");
            string statePath = Path.Combine(pioFolder, "state.json");
            Directory.CreateDirectory(pioFolder);
            using (var f = System.IO.File.OpenWrite(installerZipPath))
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using (var target = assets.Open(new Uri("avares://GuitarConfiguratorSharp/Assets/pio-installer.zip")))
                {
                    await target.CopyToAsync(f).ConfigureAwait(false);
                }
            }
            ZipFile.ExtractToDirectory(installerZipPath, installerPath);
            Debug.WriteLine("Running installer");
            Process process = new Process();
            process.StartInfo.FileName = await FindPython();
            process.StartInfo.Arguments = $"-m pioinstaller";
            process.StartInfo.EnvironmentVariables["PYTHONPATH"] = installerPath;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            process = new Process();
            process.StartInfo.FileName = await FindPython();
            process.StartInfo.Arguments = $"-m pioinstaller";
            process.StartInfo.EnvironmentVariables["PYTHONPATH"] = installerPath;
            process.StartInfo.EnvironmentVariables["PLATFORMIO_CORE_DIR"] = pioFolder;
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
        public async static Task<string?> FindPython()
        {
            // TODO: instead of all these console.writelines, expose a percentage and message as a event that can be listened to, similar to what HttpClientDownloadWithProgress is doing.
            string appdataFolder = GetAppDataFolder();
            string pythonFolder = Path.Combine(appdataFolder, "python");
            var executables = GetPythonExecutables();
            string pythonAppdataExecutable = Path.Combine(pythonFolder, executables[0]);
            Directory.CreateDirectory(appdataFolder);
            if (System.IO.File.Exists(pythonAppdataExecutable))
            {
                return pythonAppdataExecutable;
            }
            Directory.CreateDirectory(pythonFolder);

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
            if (foundExecutable == null)
            {
                Console.WriteLine("Downloading python portable");
                var sysType = GetSysType();
                Console.WriteLine("downloading json");
                var pythonJsonLoc = Path.Combine(pythonFolder, "python.json");
                using (var download = new HttpClientDownloadWithProgress("https://api.registry.platformio.org/v3/packages/platformio/tool/python-portable", pythonJsonLoc))
                {
                    download.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => Console.WriteLine($"json: {percentage}");
                    await download.StartDownload().ConfigureAwait(false);
                    Console.WriteLine("downloaded json");
                };
                var json = System.IO.File.ReadAllText(pythonJsonLoc);
                PlatformIOPackage python = PlatformIOPackage.FromJson(json);
                using (var handler = new HttpClientHandler())
                {
                    handler.AllowAutoRedirect = false;
                    using (HttpClient wc = new HttpClient(handler))
                    {
                        var compatibleVersions = python.Versions.Where(version => version.Files.Any(file => file.System.Contains(sysType)));
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            var os = System.Environment.OSVersion.Version;
                            if (os <= new System.Version(6, 1))
                            {
                                compatibleVersions = compatibleVersions.Where(version => int.Parse(version.Name.Split('.')[1]) < 30900);
                            }
                        }
                        compatibleVersions = compatibleVersions.OrderBy(version => new SemanticVersioning.Version(version.Name));
                        if (!compatibleVersions.Any())
                        {
                            Console.WriteLine($"Unable to find python executable, and no portable version available");
                            return null;
                        }
                        Console.WriteLine("Found package");
                        var package = compatibleVersions.First().Files.First(file => file.System.Contains(sysType));
                        var visitedMirrors = new List<string>();
                        Console.WriteLine("Finding mirror");
                        while (true)
                        {
                            var searchParams = "";
                            if (visitedMirrors.Any())
                            {
                                searchParams = "?bypass=" + HttpUtility.UrlEncode(String.Join(",", visitedMirrors));
                            }
                            Console.WriteLine(package.DownloadUrl + searchParams);
                            var hrm = new HttpRequestMessage(HttpMethod.Head, new Uri(package.DownloadUrl + searchParams));
                            var response = await wc.SendAsync(hrm);
                            if (response.StatusCode != HttpStatusCode.Redirect && response.StatusCode != HttpStatusCode.TemporaryRedirect)
                            {
                                break;
                            }
                            if (!response.Headers.Contains("location"))
                            {
                                break;
                            }
                            if (!response.Headers.Contains("x-pio-mirror"))
                            {
                                break;
                            }
                            var mirror = response.Headers.GetValues("x-pio-mirror").First();
                            if (visitedMirrors.Contains(mirror))
                            {
                                break;
                            }
                            visitedMirrors.Add(mirror);
                            var checksum = response.Headers.GetValues("x-pio-content-sha256").First();
                            Console.WriteLine("Downloading");
                            var pythonLoc = Path.Combine(pythonFolder, "python.tar.gz");
                            try
                            {
                                using (var progress = new HttpClientDownloadWithProgress(response.Headers.Location.ToString(), pythonLoc))
                                {
                                    progress.ProgressChanged += (totalFileSize, totalBytesDownloaded, percentage) => Console.WriteLine(percentage);
                                    await progress.StartDownload();
                                }
                                using (var inStream = System.IO.File.OpenRead(pythonLoc))
                                {
                                    using (var sha256 = SHA256.Create())
                                    {
                                        var fileChecksum = sha256.ComputeHash(inStream);
                                        // convert hash to hex
                                        var checksumBuilder = new StringBuilder();
                                        for (int i = 0; i < fileChecksum.Length; i++)
                                        {
                                            checksumBuilder.Append(fileChecksum[i].ToString("x2"));
                                        }
                                        // And compare to the expected hash, irrespective of case
                                        StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                                        if (comparer.Compare(checksumBuilder.ToString(), checksum) != 0)
                                        {
                                            Console.WriteLine("Checksum mismatch, trying another mirror");
                                            continue;
                                        }
                                    }
                                    inStream.Position = 0;
                                    Console.WriteLine("Extracting python");
                                    Stream gzipStream = new GZipInputStream(inStream);

                                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                                    tarArchive.ProgressMessageEvent += (TarArchive archive, TarEntry entry, string message) => Console.WriteLine(message);
                                    tarArchive.ExtractContents(pythonFolder);
                                    tarArchive.Close();

                                    gzipStream.Close();
                                    System.IO.File.Delete(pythonLoc);
                                    System.IO.File.Delete(pythonJsonLoc);
                                }
                                break;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Exception occurred, trying another mirror");
                            }
                        }
                        return pythonAppdataExecutable;
                    }
                }
            }
            return null;
        }

        public static string[] GetPythonExecutables()
        {
            var executables = new string[] { "python3", "python" };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                executables = new string[] { "python.exe" };
            }
            return executables;
        }
        public static string GetSysType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        return "windows_x86";
                    case Architecture.X64:
                        return "windows_amd64";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        return "darwin_i686";
                    case Architecture.X64:
                        return "darwin_x86_64";
                    case Architecture.Arm64:
                        return "darwin_arm64";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        return "linux_i686";
                    case Architecture.X64:
                        return "linux_x86_64";
                    case Architecture.Arm:
                        return "linux_armv6l";
                    case Architecture.Arm64:
                        return "linux_aarch64";
                }
            }
            return "unsupported";
        }
        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
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