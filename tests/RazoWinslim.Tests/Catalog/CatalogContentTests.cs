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
