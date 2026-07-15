using System.Diagnostics;
using System.IO;

namespace RazoWinslim.Engine;

public sealed class PowerShellAppxApi : IAppxApi
{
    public bool IsInstalled(string packageFamilyName, out string packageFullName)
    {
        var output = RunPowerShell(
            $"(Get-AppxPackage | Where-Object {{ $_.PackageFamilyName -eq '{packageFamilyName}' }}).PackageFullName");
        var fullName = output.Trim();
        packageFullName = fullName;
        return !string.IsNullOrEmpty(fullName);
    }

    public void Remove(string packageFullName) =>
        RunPowerShell($"Remove-AppxPackage -Package '{packageFullName}' -ErrorAction Stop");

    public bool TryReinstall(string packageFullName)
    {
        var manifestPath = $@"C:\Program Files\WindowsApps\{packageFullName}\AppxManifest.xml";
        if (!File.Exists(manifestPath)) return false;

        RunPowerShell($"Add-AppxPackage -DisableDevelopmentMode -Register '{manifestPath}' -ErrorAction Stop");
        return true;
    }

    private static string RunPowerShell(string command)
    {
        var psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -NonInteractive -Command \"{command}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"powershell command failed: {stderr}");

        return stdout;
    }
}
