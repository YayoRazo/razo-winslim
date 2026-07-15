using RazoWinslim.Catalog;
using RazoWinslim.Engine;
using RazoWinslim.Logging;
using RazoWinslim.Tests.Engine.Fakes;
using RazoWinslim.ViewModels;
using Xunit;

namespace RazoWinslim.Tests.ViewModels;

public class MainViewModelTests
{
    private static TweakCatalogEntry SafeEntry() => new(
        "svc-diagtrack", "Services", "Telemetry service", "desc",
        RiskTier.Safe, TargetType.Service,
        new Dictionary<string, string> { ["serviceName"] = "DiagTrack", ["desiredStartMode"] = "Disabled" });

    private static TweakCatalogEntry AdvancedEntry() => new(
        "svc-defender", "Services", "Defender real-time protection", "desc",
        RiskTier.Advanced, TargetType.Service,
        new Dictionary<string, string> { ["serviceName"] = "WinDefend", ["desiredStartMode"] = "Disabled" });

    private static TweakEngine BuildEngine(FakeServiceApi service, StateStore store) =>
        new(service, new FakeTaskSchedulerApi(), new FakeRegistryApi(), new FakeStartupApi(), new FakeAppxApi(), store);

    [Fact]
    public void GroupsItemsByCategory()
    {
        var service = new FakeServiceApi();
        var store = new StateStore(Path.Combine(Path.GetTempPath(), $"winslim-{Guid.NewGuid()}.json"));
        var vm = new MainViewModel(new List<TweakCatalogEntry> { SafeEntry() }, BuildEngine(service, store));

        Assert.Single(vm.GroupedTweaks);
        Assert.Equal("Services", vm.GroupedTweaks.First().Key);
    }

    [Fact]
    public void AdvancedTierItemFlagsRequiresConfirmation()
    {
        var item = new TweakItemViewModel(AdvancedEntry(), isEnabled: true);
        Assert.True(item.RequiresConfirmation);

        var safeItem = new TweakItemViewModel(SafeEntry(), isEnabled: true);
        Assert.False(safeItem.RequiresConfirmation);
    }

    [Fact]
    public void ToggleOffAppliesAndUpdatesViewModel()
    {
        var logPath = Path.Combine(Path.GetTempPath(), $"winslim-log-{Guid.NewGuid()}.log");
        var originalLogPath = Logger.LogPath;
        Logger.LogPath = logPath;
        try
        {
            var service = new FakeServiceApi();
            service.StartModes["DiagTrack"] = "Automatic";
            var store = new StateStore(Path.Combine(Path.GetTempPath(), $"winslim-{Guid.NewGuid()}.json"));
            var vm = new MainViewModel(new List<TweakCatalogEntry> { SafeEntry() }, BuildEngine(service, store));
            var item = vm.GroupedTweaks.First().First();

            var result = vm.ToggleOff(item);

            Assert.True(result.Success);
            Assert.False(item.IsEnabled);
            Assert.Equal("Disabled", service.StartModes["DiagTrack"]);
        }
        finally
        {
            Logger.LogPath = originalLogPath;
            File.Delete(logPath);
        }
    }
}
