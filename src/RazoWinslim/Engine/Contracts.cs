namespace RazoWinslim.Engine;

public interface IServiceApi
{
    string GetStartMode(string serviceName);
    void SetStartMode(string serviceName, string startMode);
}

public interface ITaskSchedulerApi
{
    bool GetEnabled(string taskPath);
    void SetEnabled(string taskPath, bool enabled);
}

public interface IRegistryApi
{
    bool TryGetValue(string keyPath, string valueName, out string valueKind, out string data);
    void SetValue(string keyPath, string valueName, string valueKind, string data);
    void DeleteValue(string keyPath, string valueName);
}

public interface IStartupApi
{
    bool GetEnabled(string appName);
    void SetEnabled(string appName, bool enabled);
}

public interface IAppxApi
{
    bool IsInstalled(string packageFamilyName, out string packageFullName);
    void Remove(string packageFullName);
    bool TryReinstall(string packageFullName);
}

public sealed record ApplyResult(bool Success, string? ErrorMessage)
{
    public static ApplyResult Ok() => new(true, null);
    public static ApplyResult Fail(string message) => new(false, message);
}
