using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class DListViewerRenderSettingsWindow : Window
{
    public DListViewerRenderSettingsViewModel ViewModel { get; }

    public DListViewerRenderSettingsWindow(DListViewerRenderSettingsViewModel vm)
    {
        ViewModel = vm;
        DataContext = ViewModel;
        InitializeComponent();
    }
}
