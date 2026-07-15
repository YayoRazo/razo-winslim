using RazoWinslim.Logging;
using Xunit;

namespace RazoWinslim.Tests.Logging;

public class LoggerTests
{
    [Fact]
    public void LogWritesExpectedLineFormat()
    {
        var path = Path.Combine(Path.GetTempPath(), $"winslim-log-{Guid.NewGuid()}.log");
        var original = Logger.LogPath;
        Logger.LogPath = path;
        try
        {
            Logger.Log("svc-diagtrack", success: true, message: null);
            Logger.Log("svc-defender", success: false, message: "access denied");

            var lines = File.ReadAllLines(path);
            Assert.Equal(2, lines.Length);
            Assert.Contains("svc-diagtrack | OK", lines[0]);
            Assert.Contains("svc-defender | ERROR: access denied", lines[1]);
        }
        finally
        {
            Logger.LogPath = original;
            File.Delete(path);
        }
    }
}
