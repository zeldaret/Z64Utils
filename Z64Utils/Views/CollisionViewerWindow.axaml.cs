using System;
using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class CollisionViewerWindow : Window
{
    public CollisionViewerWindowViewModel ViewModel;

    public CollisionViewerWindow()
    {
        ViewModel = new();
        DataContext = ViewModel;
        InitializeComponent();
    }
}
