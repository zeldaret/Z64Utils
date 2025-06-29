using Avalonia.Controls;
using Avalonia.Interactivity;
using Z64Utils.ViewModels;

namespace Z64Utils.Views;

public partial class ROMFilePickerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public ROMFilePickerWindow()
    {
        InitializeComponent();
    }

    public void OnOKButtonClick(object? sender, RoutedEventArgs args)
    {
        var vm = (ROMFilePickerViewModel?)DataContext;
        Close(vm?.SelectedROMFile);
    }
}
