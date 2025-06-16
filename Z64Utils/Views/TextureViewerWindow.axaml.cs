using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Z64Utils_Avalonia;

public partial class TextureViewerWindow : Window
{
    public TextureViewerWindowViewModel ViewModel;

    public TextureViewerWindow(TextureViewerWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }
}
