using System.Windows;

namespace RazoWinslim.Views;

public partial class AdvancedConfirmDialog : Window
{
    public AdvancedConfirmDialog(string tweakDisplayName)
    {
        InitializeComponent();
        MessageText.Text = $"You are about to change \"{tweakDisplayName}\", which affects Windows security protection. Continue?";
    }

    private void OnCheckedChanged(object sender, RoutedEventArgs e) =>
        ApplyButton.IsEnabled = ConfirmCheckBox.IsChecked == true;

    private void OnApply(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
