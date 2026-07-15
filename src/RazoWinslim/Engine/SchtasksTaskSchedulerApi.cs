using System.Diagnostics;
using System.Xml.Linq;

namespace RazoWinslim.Engine;

public sealed class SchtasksTaskSchedulerApi : ITaskSchedulerApi
{
    public bool GetEnabled(string taskPath)
    {
        var xml = RunSchtasks($"/Query /TN \"{taskPath}\" /XML");
        var doc = XDocument.Parse(xml);
        var ns = doc.Root!.Name.Namespace;
        var enabledElement = doc.Root!.Element(ns + "Settings")?.Element(ns + "Enabled");
        return enabledElement is null || bool.Parse(enabledElement.Value);
    }

    public void SetEnabled(string taskPath, bool enabled)
    {
        var flag = enabled ? "/Enable" : "/Disable";
        RunSchtasks($"/Change /TN \"{taskPath}\" {flag}");
    }

    private static string RunSchtasks(string arguments)
    {
        var psi = new ProcessStartInfo("schtasks.exe", arguments)
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
            throw new InvalidOperationException($"schtasks.exe {arguments} failed: {stderr}");

        return stdout;
    }
}
