using System.Text.Json;
using System.Text.Json.Serialization;

namespace RazoWinslim.Catalog;

public static class TweakCatalogLoader
{
    public static List<TweakCatalogEntry> LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var entries = JsonSerializer.Deserialize<List<TweakCatalogEntry>>(json, options)
            ?? throw new InvalidOperationException("Catalog JSON deserialized to null.");

        var seenIds = new HashSet<string>();
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Id))
                throw new InvalidOperationException("Catalog entry has an empty Id.");
            if (!seenIds.Add(entry.Id))
                throw new InvalidOperationException($"Duplicate catalog entry id: {entry.Id}");
        }

        return entries;
    }
}
