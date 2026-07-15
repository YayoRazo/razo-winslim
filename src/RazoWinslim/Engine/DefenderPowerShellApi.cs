using System.Diagnostics;

namespace RazoWinslim.Engine;

public sealed class DefenderPowerShellApi : IDefenderApi
{
    public bool GetRealTimeProtectionEnabled()
    {
        var output = RunPowerShell("(Get-MpComputerStatus).RealTimeProtectionEnabled");
        return bool.Parse(output.Trim());
    }

    public void SetRealTimeProtectionEnabled(bool enabled)
    {
        var disableFlag = enabled ? "$false" : "$true";
        RunPowerShell($"Set-MpPreference -DisableRealtimeMonitoring {disableFlag} -ErrorAction Stop");
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
