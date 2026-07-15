using RazoWinslim.Catalog;
using RazoWinslim.Engine;
using RazoWinslim.Tests.Engine.Fakes;
using Xunit;

namespace RazoWinslim.Tests.Engine;

public class TweakEngineTests
{
    private static TweakCatalogEntry ServiceEntry() => new(
        "svc-diagtrack", "Services", "Telemetry service", "desc",
        RiskTier.Safe, TargetType.Service,
        new Dictionary<string, string> { ["serviceName"] = "DiagTrack", ["desiredStartMode"] = "Disabled" });

    private static (TweakEngine engine, FakeServiceApi service, StateStore store, string statePath) BuildEngine()
    {
        var statePath = Path.Combine(Path.GetTempPath(), $"winslim-state-{Guid.NewGuid()}.json");
        var store = new StateStore(statePath);
        var service = new FakeServiceApi();
        var engine = new TweakEngine(service, new FakeTaskSchedulerApi(), new FakeRegistryApi(), new FakeStartupApi(), new FakeAppxApi(), store);
        return (engine, service, store, statePath);
    }

    [Fact]
    public void ApplyCapturesOriginalStateBeforeMutating()
    {
        var (engine, service, store, path) = BuildEngine();
        try
        {
            service.StartModes["DiagTrack"] = "Automatic";
            service.ThrowOnSet = true;

            var result = engine.Apply(ServiceEntry());

            Assert.False(result.Success);
            Assert.True(store.Has("svc-diagtrack"));
            Assert.Equal("Automatic", store.Get("svc-diagtrack")["startMode"]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ApplySucceedsAndSetsDesiredStartMode()
    {
        var (engine, service, _, path) = BuildEngine();
        try
        {
            service.StartModes["DiagTrack"] = "Automatic";

            var result = engine.Apply(ServiceEntry());

            Assert.True(result.Success);
            Assert.Equal("Disabled", service.StartModes["DiagTrack"]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ManualDesiredServiceReadsAsDisabledAfterApply()
    {
        var (engine, service, _, path) = BuildEngine();
        try
        {
            var entry = new TweakCatalogEntry(
                "svc-mapsbroker", "Services", "Downloaded Maps Manager", "desc",
                RiskTier.Safe, TargetType.Service,
                new Dictionary<string, string> { ["serviceName"] = "MapsBroker", ["desiredStartMode"] = "Manual" });
            service.StartModes["MapsBroker"] = "Automatic";

            engine.Apply(entry);

            Assert.False(engine.GetCurrentEnabledState(entry));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RevertRestoresOriginalAndClearsStateStore()
    {
        var (engine, service, store, path) = BuildEngine();
        try
        {
            service.StartModes["DiagTrack"] = "Automatic";
            engine.Apply(ServiceEntry());

            var result = engine.Revert(ServiceEntry());

            Assert.True(result.Success);
            Assert.Equal("Automatic", service.StartModes["DiagTrack"]);
            Assert.False(store.Has("svc-diagtrack"));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RevertFailsCleanlyWhenNothingWasCaptured()
    {
        var (engine, _, _, path) = BuildEngine();
        try
        {
            var result = engine.Revert(ServiceEntry());

            Assert.False(result.Success);
            Assert.Equal("No captured original state to revert to.", result.ErrorMessage);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void AppxRevertReportsNotReversibleWhenManifestGone()
    {
        var statePath = Path.Combine(Path.GetTempPath(), $"winslim-state-{Guid.NewGuid()}.json");
        var store = new StateStore(statePath);
        var appx = new FakeAppxApi();
        appx.InstalledPackages["Microsoft.Xbox_8wekyb3d8bbwe"] = "Microsoft.Xbox_1.0.0.0_x64__8wekyb3d8bbwe";
        var engine = new TweakEngine(new FakeServiceApi(), new FakeTaskSchedulerApi(), new FakeRegistryApi(), new FakeStartupApi(), appx, store);
        var entry = new TweakCatalogEntry(
            "appx-xbox", "Bloatware", "Xbox app", "desc", RiskTier.Safe, TargetType.AppxPackage,
            new Dictionary<string, string> { ["packageFamilyName"] = "Microsoft.Xbox_8wekyb3d8bbwe" });

        try
        {
            engine.Apply(entry);
            appx.SimulateManifestGone = true;

            var reverted = engine.Revert(entry);

            Assert.False(reverted.Success);
            Assert.Equal("Package manifest no longer present; not reversible.", reverted.ErrorMessage);
        }
        finally { File.Delete(statePath); }
    }
}
