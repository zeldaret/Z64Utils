using Avalonia.Controls;
using Avalonia.Interactivity;
using Z64Utils.ViewModels;

namespace Z64Utils.Views;

public partial class PickSegmentIDWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public PickSegmentIDWindow()
    {
        InitializeComponent();

        // Focus the segment id text box (and the dialog)
        Opened += (sender, e) =>
        {
            // Select the content of the text box on focus
            void OnSegmentIDStrTextBoxGotFocus(object? sender, RoutedEventArgs args)
            {
                Logger.Trace("OnSegmentIDStrTextBoxGotFocus");
                SegmentIDStrTextBox.SelectAll();
            }
            SegmentIDStrTextBox.GotFocus += OnSegmentIDStrTextBoxGotFocus;

            SegmentIDStrTextBox.Focus();
        };
    }

    public void OnOKButtonClick(object? sender, RoutedEventArgs args)
    {
        var vm = (PickSegmentIDWindowViewModel?)DataContext;
        Close(vm?.SegmentID);
    }
}
