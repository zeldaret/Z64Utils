using System;
using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class DListViewerWindow : Window
{
    public DListViewerWindowViewModel ViewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;

    public DListViewerWindow()
    {
        ViewModel = new() { OpenDListViewerRenderSettings = OpenDListViewerRenderSettings };
        DataContext = ViewModel;
        InitializeComponent();
        ViewModel.RenderContextChanged += (sender, e) =>
        {
            DLViewerGL.RequestNextFrameRenderingIfInitialized();
        };
    }

    private DListViewerRenderSettingsViewModel? OpenDListViewerRenderSettings(
        Func<DListViewerRenderSettingsViewModel> vmFactory
    )
    {
        if (_currentRenderSettingsWindow != null)
        {
            _currentRenderSettingsWindow.Activate();
            return null;
        }

        var vm = vmFactory();
        _currentRenderSettingsWindow = new DListViewerRenderSettingsWindow(vm);
        _currentRenderSettingsWindow.Closed += (sender, e) =>
        {
            _currentRenderSettingsWindow = null;
        };
        _currentRenderSettingsWindow.Show();
        return _currentRenderSettingsWindow.ViewModel;
    }
}
