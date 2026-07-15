using RazoWinslim.Catalog;
using RazoWinslim.ViewModels;
using Xunit;

namespace RazoWinslim.Tests.ViewModels;

public class TweakItemViewModelTests
{
    private static TweakCatalogEntry Entry() => new(
        "svc-diagtrack", "Services", "Telemetry service", "desc",
        RiskTier.Safe, TargetType.Service,
        new Dictionary<string, string> { ["serviceName"] = "DiagTrack", ["desiredStartMode"] = "Disabled" });

    [Fact]
    public void StateLabelReflectsEnabledAndDisabled()
    {
        var item = new TweakItemViewModel(Entry(), isEnabled: true);
        Assert.Equal("Enabled now", item.StateLabel);

        item.IsEnabled = false;
        Assert.Equal("Disabled now", item.StateLabel);
    }

    [Fact]
    public void StateLabelShowsApplyingWhileBusyRegardlessOfEnabledState()
    {
        var item = new TweakItemViewModel(Entry(), isEnabled: true);

        item.IsBusy = true;

        Assert.Equal("Applying...", item.StateLabel);
    }
}
