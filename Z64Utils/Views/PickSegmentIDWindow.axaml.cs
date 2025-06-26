using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Z64Utils_Avalonia;

public partial class PickSegmentIDWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public PickSegmentIDWindowViewModel ViewModel;

    public PickSegmentIDWindow()
    {
        ViewModel = new PickSegmentIDWindowViewModel();
        DataContext = ViewModel;
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
        Close(ViewModel.SegmentID);
    }
}
