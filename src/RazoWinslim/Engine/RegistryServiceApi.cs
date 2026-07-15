using Microsoft.Win32;

namespace RazoWinslim.Engine;

public sealed class RegistryServiceApi : IServiceApi
{
    private const string ServicesKeyPath = @"SYSTEM\CurrentControlSet\Services";

    public string GetStartMode(string serviceName)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"{ServicesKeyPath}\{serviceName}");
        if (key is null) return "Disabled"; // service not present - nothing running, already at the safe end state
        var raw = key.GetValue("Start");
        if (raw is null) return "Disabled"; // no Start value - SCM cannot start it, already inert
        return DwordToStartMode((int)raw);
    }

    public void SetStartMode(string serviceName, string startMode)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"{ServicesKeyPath}\{serviceName}", writable: true)
            ?? throw new InvalidOperationException($"Service not found: {serviceName}");
        key.SetValue("Start", StartModeToDword(startMode), RegistryValueKind.DWord);
    }

    private static string DwordToStartMode(int value) => value switch
    {
        0 => "Boot",
        1 => "System",
        2 => "Automatic",
        3 => "Manual",
        4 => "Disabled",
        _ => throw new InvalidOperationException($"Unknown service Start value: {value}")
    };

    private static int StartModeToDword(string mode) => mode switch
    {
        "Boot" => 0,
        "System" => 1,
        "Automatic" => 2,
        "Manual" => 3,
        "Disabled" => 4,
        _ => throw new InvalidOperationException($"Unknown start mode: {mode}")
    };
}
