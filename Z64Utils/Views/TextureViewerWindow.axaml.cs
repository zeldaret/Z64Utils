using Avalonia.Controls;

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
