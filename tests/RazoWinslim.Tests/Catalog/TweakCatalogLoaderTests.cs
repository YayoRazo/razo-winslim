using RazoWinslim.Catalog;
using Xunit;

namespace RazoWinslim.Tests.Catalog;

public class TweakCatalogLoaderTests
{
    [Fact]
    public void LoadsValidEntries()
    {
        var json = """
        [
          {
            "id": "svc-diagtrack",
            "category": "Services",
            "displayName": "Connected User Experiences and Telemetry",
            "description": "Collects diagnostic data.",
            "riskTier": "Safe",
            "targetType": "Service",
            "targetIdentifier": { "serviceName": "DiagTrack", "desiredStartMode": "Disabled" }
          }
        ]
        """;

        var entries = TweakCatalogLoader.LoadFromJson(json);

        Assert.Single(entries);
        Assert.Equal("svc-diagtrack", entries[0].Id);
        Assert.Equal(RiskTier.Safe, entries[0].RiskTier);
        Assert.Equal(TargetType.Service, entries[0].TargetType);
        Assert.Equal("DiagTrack", entries[0].TargetIdentifier["serviceName"]);
    }

    [Fact]
    public void ThrowsOnDuplicateIds()
    {
        var json = """
        [
          { "id": "dup", "category": "Services", "displayName": "A", "description": "A", "riskTier": "Safe", "targetType": "Service", "targetIdentifier": {} },
          { "id": "dup", "category": "Services", "displayName": "B", "description": "B", "riskTier": "Safe", "targetType": "Service", "targetIdentifier": {} }
        ]
        """;

        Assert.Throws<InvalidOperationException>(() => TweakCatalogLoader.LoadFromJson(json));
    }
}
