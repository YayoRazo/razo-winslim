using System.IO;
using System.Windows;
using RazoWinslim.Catalog;
using RazoWinslim.Engine;
using RazoWinslim.ViewModels;

namespace RazoWinslim;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var catalogPath = Path.Combine(AppContext.BaseDirectory, "Catalog", "tweaks.json");
            var catalog = TweakCatalogLoader.LoadFromJson(File.ReadAllText(catalogPath));

            var statePath = Path.Combine(AppContext.BaseDirectory, "state.json");
            var stateStore = new StateStore(statePath);

            var engine = new TweakEngine(
                new RegistryServiceApi(),
                new SchtasksTaskSchedulerApi(),
                new Win32RegistryApi(),
                new StartupApprovedApi(),
                new PowerShellAppxApi(),
                stateStore);

            var viewModel = new MainViewModel(catalog, engine);
            new MainWindow(viewModel).Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"razo-winslim failed to start: {ex.Message}",
                "razo-winslim - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"An unexpected error occurred: {e.Exception.Message}",
            "razo-winslim - Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
