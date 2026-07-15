using Microsoft.Win32;

namespace RazoWinslim.Engine;

public sealed class Win32RegistryApi : IRegistryApi
{
    public bool TryGetValue(string keyPath, string valueName, out string valueKind, out string data)
    {
        using var key = Registry.LocalMachine.OpenSubKey(keyPath);
        var value = key?.GetValue(valueName);
        if (key is null || value is null)
        {
            valueKind = "";
            data = "";
            return false;
        }

        var kind = key.GetValueKind(valueName);
        if (kind is RegistryValueKind.Binary or RegistryValueKind.MultiString)
            throw new NotSupportedException($"Registry value kind {kind} is not supported for value '{valueName}'.");

        valueKind = kind.ToString();
        data = value.ToString() ?? "";
        return true;
    }

    public void SetValue(string keyPath, string valueName, string valueKind, string data)
    {
        using var key = Registry.LocalMachine.CreateSubKey(keyPath, writable: true);
        var kind = Enum.Parse<RegistryValueKind>(valueKind);
        object typedData = kind == RegistryValueKind.DWord ? int.Parse(data) : data;
        key.SetValue(valueName, typedData, kind);
    }

    public void DeleteValue(string keyPath, string valueName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(keyPath, writable: true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
