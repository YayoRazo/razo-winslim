using System.Collections.ObjectModel;
using RazoWinslim.Catalog;
using RazoWinslim.Engine;
using RazoWinslim.Logging;

namespace RazoWinslim.ViewModels;

public sealed class MainViewModel
{
    private readonly TweakEngine _engine;

    public ObservableCollection<IGrouping<string, TweakItemViewModel>> GroupedTweaks { get; }

    public MainViewModel(List<TweakCatalogEntry> catalog, TweakEngine engine)
    {
        _engine = engine;

        var items = catalog
            .Select(entry => new TweakItemViewModel(entry, engine.GetCurrentEnabledState(entry)))
            .ToList();

        GroupedTweaks = new ObservableCollection<IGrouping<string, TweakItemViewModel>>(
            items.GroupBy(i => i.Entry.Category));
    }

    public ApplyResult ToggleOff(TweakItemViewModel item)
    {
        var result = _engine.Apply(item.Entry);
        Logger.Log(item.Entry.Id, result.Success, result.ErrorMessage);
        item.ErrorMessage = result.Success ? null : result.ErrorMessage;
        if (result.Success) item.IsEnabled = false;
        return result;
    }

    public ApplyResult ToggleOn(TweakItemViewModel item)
    {
        var result = _engine.Revert(item.Entry);
        Logger.Log(item.Entry.Id, result.Success, result.ErrorMessage);
        item.ErrorMessage = result.Success ? null : result.ErrorMessage;
        if (result.Success) item.IsEnabled = true;
        return result;
    }
}
