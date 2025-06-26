using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class ROMRAMConversionsWindow : Window
{
    public ROMRAMConversionsWindowViewModel ViewModel;

    public ROMRAMConversionsWindow(ROMRAMConversionsWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}
