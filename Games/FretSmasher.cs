using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GuitarConfiguratorSharp.NetCore.Games;

public class FretSmasher
{
    private const uint FretSmasherAppId = 1648350;

    public static void StartLinux()
    {
        StartGame();
        Process? fretSmasherProcess;
        while (true)
        {
            fretSmasherProcess = Process.GetProcesses().FirstOrDefault(s => s.ProcessName.Contains("Fret-Smasher"));
            if (fretSmasherProcess != null) break;
        }
        
        var currentProcess = Process.GetCurrentProcess();
        //TODO: extract this into a temp folder
        var fsdir = Path.Join(Path.GetDirectoryName(currentProcess.MainModule!.FileName), "fs");
        // Steam talks to the game via environment variables, as it injects itself via LD_PRELOAD. So we can just grab the vars that were passed to the game
        // then kill it, add our additional stuff in and start it again, and we get steam integration and stuff is injected
        var vars = File.ReadAllText($"/proc/{fretSmasherProcess.Id}/environ")
            .Trim()
            .Split("\0")
            .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Split("=", 2))
            .ToDictionary(line => line[0], line => line[1]);
        var newProcess = new Process();
        newProcess.StartInfo.FileName = fretSmasherProcess.MainModule!.FileName;
        fretSmasherProcess.Kill();
        vars["DOORSTOP_ENABLED"] = "1";
        vars["DOORSTOP_TARGET_ASSEMBLY"] = Path.Join(fsdir, "FSDoorstop.dll");
        vars["DOORSTOP_IGNORE_DISABLED_ENV"] = "0";
        vars["DOORSTOP_MONO_DLL_SEARCH_PATH_OVERRIDE"] = "";
        vars["DOORSTOP_MONO_DEBUG_ENABLED"] = "0";
        vars["DOORSTOP_MONO_DEBUG_ADDRESS"] = "";
        vars["DOORSTOP_MONO_DEBUG_SUSPEND"] = "";
        vars["DOORSTOP_CLR_RUNTIME_CORECLR_PATH"] = "";
        vars["LD_PRELOAD"] = $"libdoorstop.so:{vars["LD_PRELOAD"]}";
        vars["LD_LIBRARY_PATH"] = $"{fsdir}:{vars["LD_LIBRARY_PATH"]}";
        foreach (var (key, value) in vars)
        {
            newProcess.StartInfo.EnvironmentVariables[key] = value;
        }
        
        newProcess.Start();
    }

    public static void StartGame()
    {
        var process = new Process();
        // This is important as the shell knows how to handle uris
        process.StartInfo.UseShellExecute = true;
        // Ask steam to start fret smasher
        process.StartInfo.FileName = $"steam://run/{FretSmasherAppId}";
        process.Start();
    }
}