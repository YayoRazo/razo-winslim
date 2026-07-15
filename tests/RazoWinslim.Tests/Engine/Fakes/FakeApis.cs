using RazoWinslim.Engine;

namespace RazoWinslim.Tests.Engine.Fakes;

public class FakeServiceApi : IServiceApi
{
    public Dictionary<string, string> StartModes { get; } = new();
    public bool ThrowOnSet { get; set; }

    public string GetStartMode(string serviceName) => StartModes.GetValueOrDefault(serviceName, "Automatic");

    public void SetStartMode(string serviceName, string startMode)
    {
        if (ThrowOnSet) throw new InvalidOperationException("Simulated failure setting start mode.");
        StartModes[serviceName] = startMode;
    }
}

public class FakeTaskSchedulerApi : ITaskSchedulerApi
{
    public Dictionary<string, bool> Enabled { get; } = new();

    public bool GetEnabled(string taskPath) => Enabled.GetValueOrDefault(taskPath, true);
    public void SetEnabled(string taskPath, bool enabled) => Enabled[taskPath] = enabled;
}

public class FakeRegistryApi : IRegistryApi
{
    public Dictionary<(string, string), (string Kind, string Data)> Values { get; } = new();

    public bool TryGetValue(string keyPath, string valueName, out string valueKind, out string data)
    {
        if (Values.TryGetValue((keyPath, valueName), out var v))
        {
            valueKind = v.Kind;
            data = v.Data;
            return true;
        }
        valueKind = "";
        data = "";
        return false;
    }

    public void SetValue(string keyPath, string valueName, string valueKind, string data) =>
        Values[(keyPath, valueName)] = (valueKind, data);

    public void DeleteValue(string keyPath, string valueName) => Values.Remove((keyPath, valueName));
}

public class FakeStartupApi : IStartupApi
{
    public Dictionary<string, bool> Enabled { get; } = new();

    public bool GetEnabled(string appName) => Enabled.GetValueOrDefault(appName, true);
    public void SetEnabled(string appName, bool enabled) => Enabled[appName] = enabled;
}

public class FakeAppxApi : IAppxApi
{
    public Dictionary<string, string> InstalledPackages { get; } = new();
    public HashSet<string> Removed { get; } = new();
    public bool SimulateManifestGone { get; set; }

    public bool IsInstalled(string packageFamilyName, out string packageFullName)
    {
        if (InstalledPackages.TryGetValue(packageFamilyName, out var full) && !Removed.Contains(full))
        {
            packageFullName = full;
            return true;
        }
        packageFullName = "";
        return false;
    }

    public void Remove(string packageFullName) => Removed.Add(packageFullName);

    public bool TryReinstall(string packageFullName)
    {
        if (SimulateManifestGone) return false;
        if (!Removed.Contains(packageFullName)) return false;
        Removed.Remove(packageFullName);
        return true;
    }
}
