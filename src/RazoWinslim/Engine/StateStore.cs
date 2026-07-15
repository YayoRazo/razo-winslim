using System.IO;
using System.Text.Json;

namespace RazoWinslim.Engine;

public sealed class StateStore
{
    private readonly string _path;
    private readonly Dictionary<string, Dictionary<string, string>> _state;

    public StateStore(string path)
    {
        _path = path;
        _state = File.Exists(path)
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(path)) ?? new()
            : new();
    }

    public bool Has(string tweakId) => _state.ContainsKey(tweakId);

    public Dictionary<string, string> Get(string tweakId) => _state[tweakId];

    public void Capture(string tweakId, Dictionary<string, string> originalState)
    {
        if (_state.ContainsKey(tweakId)) return;
        _state[tweakId] = originalState;
        Save();
    }

    public void Remove(string tweakId)
    {
        _state.Remove(tweakId);
        Save();
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
