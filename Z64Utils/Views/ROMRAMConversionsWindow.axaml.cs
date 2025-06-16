using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

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
