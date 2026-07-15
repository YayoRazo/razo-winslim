using System.Windows;
using System.Windows.Controls.Primitives;
using RazoWinslim.ViewModels;
using RazoWinslim.Views;

namespace RazoWinslim;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void OnToggleClicked(object sender, RoutedEventArgs e)
    {
        var toggle = (ToggleButton)sender;
        var item = (TweakItemViewModel)toggle.Tag;
        var turningOff = toggle.IsChecked == false;

        if (turningOff && item.RequiresConfirmation)
        {
            var dialog = new AdvancedConfirmDialog(item.Entry.DisplayName) { Owner = this };
            if (dialog.ShowDialog() != true)
            {
                toggle.IsChecked = true;
                return;
            }
        }

        var result = turningOff ? _viewModel.ToggleOff(item) : _viewModel.ToggleOn(item);
        if (!result.Success)
            toggle.IsChecked = turningOff;
    }
}
