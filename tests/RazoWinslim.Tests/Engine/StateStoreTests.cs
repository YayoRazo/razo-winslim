using RazoWinslim.Engine;
using Xunit;

namespace RazoWinslim.Tests.Engine;

public class StateStoreTests
{
    [Fact]
    public void CaptureThenGetRoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), $"winslim-state-{Guid.NewGuid()}.json");
        try
        {
            var store = new StateStore(path);
            store.Capture("svc-diagtrack", new Dictionary<string, string> { ["startMode"] = "Automatic" });

            var reloaded = new StateStore(path);
            Assert.True(reloaded.Has("svc-diagtrack"));
            Assert.Equal("Automatic", reloaded.Get("svc-diagtrack")["startMode"]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CaptureIsIdempotent_NeverOverwritesExistingCapture()
    {
        var path = Path.Combine(Path.GetTempPath(), $"winslim-state-{Guid.NewGuid()}.json");
        try
        {
            var store = new StateStore(path);
            store.Capture("svc-diagtrack", new Dictionary<string, string> { ["startMode"] = "Automatic" });
            store.Capture("svc-diagtrack", new Dictionary<string, string> { ["startMode"] = "Manual" });

            Assert.Equal("Automatic", store.Get("svc-diagtrack")["startMode"]);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RemoveClearsEntry()
    {
        var path = Path.Combine(Path.GetTempPath(), $"winslim-state-{Guid.NewGuid()}.json");
        try
        {
            var store = new StateStore(path);
            store.Capture("svc-diagtrack", new Dictionary<string, string> { ["startMode"] = "Automatic" });
            store.Remove("svc-diagtrack");

            Assert.False(store.Has("svc-diagtrack"));
        }
        finally { File.Delete(path); }
    }
}
