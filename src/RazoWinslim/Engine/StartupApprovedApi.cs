using Microsoft.Win32;

namespace RazoWinslim.Engine;

public sealed class StartupApprovedApi : IStartupApi
{
    private const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
    private static readonly byte[] EnabledFlag = { 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private static readonly byte[] DisabledFlag = { 0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public bool GetEnabled(string appName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
        var value = key?.GetValue(appName) as byte[];
        if (value is null || value.Length == 0) return true;
        return (value[0] & 1) == 0;
    }

    public void SetEnabled(string appName, bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(KeyPath, writable: true);
        key.SetValue(appName, enabled ? EnabledFlag : DisabledFlag, RegistryValueKind.Binary);
    }
}
