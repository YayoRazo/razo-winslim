using RazoWinslim.Catalog;

namespace RazoWinslim.Engine;

public sealed class TweakEngine
{
    private readonly IServiceApi _serviceApi;
    private readonly ITaskSchedulerApi _taskApi;
    private readonly IRegistryApi _registryApi;
    private readonly IStartupApi _startupApi;
    private readonly IAppxApi _appxApi;
    private readonly StateStore _stateStore;

    public TweakEngine(
        IServiceApi serviceApi,
        ITaskSchedulerApi taskApi,
        IRegistryApi registryApi,
        IStartupApi startupApi,
        IAppxApi appxApi,
        StateStore stateStore)
    {
        _serviceApi = serviceApi;
        _taskApi = taskApi;
        _registryApi = registryApi;
        _startupApi = startupApi;
        _appxApi = appxApi;
        _stateStore = stateStore;
    }

    public bool GetCurrentEnabledState(TweakCatalogEntry entry) => entry.TargetType switch
    {
        TargetType.Service => _serviceApi.GetStartMode(entry.TargetIdentifier["serviceName"])
                              != entry.TargetIdentifier["desiredStartMode"],
        TargetType.ScheduledTask => _taskApi.GetEnabled(entry.TargetIdentifier["taskPath"]),
        TargetType.RegistryValue => !RegistryValueMatchesDesired(entry),
        TargetType.StartupEntry => _startupApi.GetEnabled(entry.TargetIdentifier["appName"]),
        TargetType.AppxPackage => _appxApi.IsInstalled(entry.TargetIdentifier["packageFamilyName"], out _),
        _ => throw new InvalidOperationException($"Unknown target type: {entry.TargetType}")
    };

    public ApplyResult Apply(TweakCatalogEntry entry)
    {
        try
        {
            CaptureOriginalIfAbsent(entry);
            ApplyDesired(entry);
            return ApplyResult.Ok();
        }
        catch (Exception ex)
        {
            return ApplyResult.Fail(ex.Message);
        }
    }

    public ApplyResult Revert(TweakCatalogEntry entry)
    {
        if (!_stateStore.Has(entry.Id))
            return ApplyResult.Fail("No captured original state to revert to.");

        try
        {
            var original = _stateStore.Get(entry.Id);
            RestoreOriginal(entry, original);
            _stateStore.Remove(entry.Id);
            return ApplyResult.Ok();
        }
        catch (Exception ex)
        {
            return ApplyResult.Fail(ex.Message);
        }
    }

    private void CaptureOriginalIfAbsent(TweakCatalogEntry entry)
    {
        if (_stateStore.Has(entry.Id)) return;

        var original = entry.TargetType switch
        {
            TargetType.Service => new Dictionary<string, string>
            {
                ["startMode"] = _serviceApi.GetStartMode(entry.TargetIdentifier["serviceName"])
            },
            TargetType.ScheduledTask => new Dictionary<string, string>
            {
                ["enabled"] = _taskApi.GetEnabled(entry.TargetIdentifier["taskPath"]).ToString()
            },
            TargetType.RegistryValue => CaptureRegistryOriginal(entry),
            TargetType.StartupEntry => new Dictionary<string, string>
            {
                ["enabled"] = _startupApi.GetEnabled(entry.TargetIdentifier["appName"]).ToString()
            },
            TargetType.AppxPackage => CaptureAppxOriginal(entry),
            _ => throw new InvalidOperationException($"Unknown target type: {entry.TargetType}")
        };

        _stateStore.Capture(entry.Id, original);
    }

    private Dictionary<string, string> CaptureRegistryOriginal(TweakCatalogEntry entry)
    {
        var keyPath = entry.TargetIdentifier["keyPath"];
        var valueName = entry.TargetIdentifier["valueName"];
        var existed = _registryApi.TryGetValue(keyPath, valueName, out var kind, out var data);

        var captured = new Dictionary<string, string> { ["existed"] = existed.ToString() };
        if (existed)
        {
            captured["valueKind"] = kind;
            captured["data"] = data;
        }
        return captured;
    }

    private Dictionary<string, string> CaptureAppxOriginal(TweakCatalogEntry entry)
    {
        var installed = _appxApi.IsInstalled(entry.TargetIdentifier["packageFamilyName"], out var fullName);
        return new Dictionary<string, string>
        {
            ["installed"] = installed.ToString(),
            ["packageFullName"] = installed ? fullName : string.Empty
        };
    }

    private bool RegistryValueMatchesDesired(TweakCatalogEntry entry)
    {
        var keyPath = entry.TargetIdentifier["keyPath"];
        var valueName = entry.TargetIdentifier["valueName"];
        var desiredData = entry.TargetIdentifier["desiredData"];
        var exists = _registryApi.TryGetValue(keyPath, valueName, out _, out var currentData);
        return exists && currentData == desiredData;
    }

    private void ApplyDesired(TweakCatalogEntry entry)
    {
        switch (entry.TargetType)
        {
            case TargetType.Service:
                _serviceApi.SetStartMode(entry.TargetIdentifier["serviceName"], entry.TargetIdentifier["desiredStartMode"]);
                break;
            case TargetType.ScheduledTask:
                _taskApi.SetEnabled(entry.TargetIdentifier["taskPath"], false);
                break;
            case TargetType.RegistryValue:
                _registryApi.SetValue(
                    entry.TargetIdentifier["keyPath"],
                    entry.TargetIdentifier["valueName"],
                    entry.TargetIdentifier["desiredValueKind"],
                    entry.TargetIdentifier["desiredData"]);
                break;
            case TargetType.StartupEntry:
                _startupApi.SetEnabled(entry.TargetIdentifier["appName"], false);
                break;
            case TargetType.AppxPackage:
                if (_appxApi.IsInstalled(entry.TargetIdentifier["packageFamilyName"], out var fullName))
                    _appxApi.Remove(fullName);
                break;
            default:
                throw new InvalidOperationException($"Unknown target type: {entry.TargetType}");
        }
    }

    private void RestoreOriginal(TweakCatalogEntry entry, Dictionary<string, string> original)
    {
        switch (entry.TargetType)
        {
            case TargetType.Service:
                _serviceApi.SetStartMode(entry.TargetIdentifier["serviceName"], original["startMode"]);
                break;
            case TargetType.ScheduledTask:
                _taskApi.SetEnabled(entry.TargetIdentifier["taskPath"], bool.Parse(original["enabled"]));
                break;
            case TargetType.RegistryValue:
                var keyPath = entry.TargetIdentifier["keyPath"];
                var valueName = entry.TargetIdentifier["valueName"];
                if (bool.Parse(original["existed"]))
                    _registryApi.SetValue(keyPath, valueName, original["valueKind"], original["data"]);
                else
                    _registryApi.DeleteValue(keyPath, valueName);
                break;
            case TargetType.StartupEntry:
                _startupApi.SetEnabled(entry.TargetIdentifier["appName"], bool.Parse(original["enabled"]));
                break;
            case TargetType.AppxPackage:
                if (bool.Parse(original["installed"]) && !string.IsNullOrEmpty(original["packageFullName"]))
                {
                    if (!_appxApi.TryReinstall(original["packageFullName"]))
                        throw new InvalidOperationException("Package manifest no longer present; not reversible.");
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown target type: {entry.TargetType}");
        }
    }
}
