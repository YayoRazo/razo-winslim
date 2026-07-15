using System.Linq;
using RazoWinslim.Catalog;
using Xunit;

namespace RazoWinslim.Tests.Catalog;

public class CatalogContentTests
{
    private static readonly string CatalogPath = Path.Combine(AppContext.BaseDirectory, "Catalog", "tweaks.json");

    [Fact]
    public void ShippedCatalogParsesWithoutError()
    {
        var entries = TweakCatalogLoader.LoadFromJson(File.ReadAllText(CatalogPath));
        Assert.NotEmpty(entries);
    }

    [Fact]
    public void NeverContainsWindowsUpdateService()
    {
        var entries = TweakCatalogLoader.LoadFromJson(File.ReadAllText(CatalogPath));
        var forbidden = new[] { "wuauserv", "UsoSvc", "WaaSMedicSvc" };

        foreach (var entry in entries.Where(e => e.TargetType == TargetType.Service))
        {
            Assert.DoesNotContain(entry.TargetIdentifier["serviceName"], forbidden);
        }
    }

    [Fact]
    public void DefenderEntryUsesMpPreferenceNotRawServiceStart()
    {
        var entries = TweakCatalogLoader.LoadFromJson(File.ReadAllText(CatalogPath));
        var defender = entries.Single(e => e.Id == "svc-windefend");

        Assert.Equal(TargetType.DefenderProtection, defender.TargetType);
    }

    [Fact]
    public void CompatAppraiserTaskPathMatchesCurrentWindowsTaskName()
    {
        var entries = TweakCatalogLoader.LoadFromJson(File.ReadAllText(CatalogPath));
        var appraiser = entries.Single(e => e.Id == "task-compat-appraiser");

        Assert.Equal(
            "\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser Exp",
            appraiser.TargetIdentifier["taskPath"]);
    }

    [Fact]
    public void EveryCategoryHasAtLeastOneEntry()
    {
        var entries = TweakCatalogLoader.LoadFromJson(File.ReadAllText(CatalogPath));
        var categories = new[] { "Services", "Scheduled tasks", "Telemetry & diagnostics", "Startup apps", "Bloatware uninstall" };

        foreach (var category in categories)
        {
            Assert.Contains(entries, e => e.Category == category);
        }
    }
}
