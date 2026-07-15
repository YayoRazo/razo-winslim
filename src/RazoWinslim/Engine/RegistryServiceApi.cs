using Microsoft.Win32;

namespace RazoWinslim.Engine;

public sealed class RegistryServiceApi : IServiceApi
{
    private const string ServicesKeyPath = @"SYSTEM\CurrentControlSet\Services";

    public string GetStartMode(string serviceName)
    {
        using var key = Registry.LocalMachine.OpenSubKey($@"{ServicesKeyPath}\{serviceName}")
            ?? throw new InvalidOperationException($"Service not found: {serviceName}");
        var value = (int)(key.GetValue("Start") ?? throw new InvalidOperationException($"Service {serviceName} has no Start value."));
        return DwordToStartMode(value);
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
