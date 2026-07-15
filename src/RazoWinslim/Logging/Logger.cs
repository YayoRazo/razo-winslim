using System.IO;

namespace RazoWinslim.Logging;

public static class Logger
{
    public static string LogPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "winslim.log");

    public static void Log(string tweakId, bool success, string? message)
    {
        var status = success ? "OK" : $"ERROR: {message}";
        var line = $"{DateTime.UtcNow:O} | {tweakId} | {status}{Environment.NewLine}";
        try
        {
            File.AppendAllText(LogPath, line);
        }
        catch (IOException)
        {
            // Best-effort logging - a write failure here must never crash the caller.
        }
        catch (UnauthorizedAccessException)
        {
            // Same reasoning.
        }
    }
}
