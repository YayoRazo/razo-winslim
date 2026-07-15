using System.ComponentModel;
using System.Runtime.CompilerServices;
using RazoWinslim.Catalog;

namespace RazoWinslim.ViewModels;

public sealed class TweakItemViewModel : INotifyPropertyChanged
{
    public TweakCatalogEntry Entry { get; }

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); OnPropertyChanged(nameof(StateLabel)); }
    }

    public string StateLabel => IsEnabled ? "Enabled now" : "Disabled now";

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool RequiresConfirmation => Entry.RiskTier == RiskTier.Advanced;

    public TweakItemViewModel(TweakCatalogEntry entry, bool isEnabled)
    {
        Entry = entry;
        _isEnabled = isEnabled;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
