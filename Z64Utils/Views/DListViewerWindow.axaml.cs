using System;
using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class DListViewerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public DListViewerWindowViewModel? ViewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;
    private SegmentsConfigWindow? _currentSegmentsConfigWindow;

    public DListViewerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (ViewModel != null)
            {
                ViewModel.RenderContextChanged -= OnRenderContextChanged;
            }

            ViewModel = (DListViewerWindowViewModel?)DataContext;
            if (ViewModel == null)
                return;
            ViewModel.OpenDListViewerRenderSettings = OpenDListViewerRenderSettings;
            ViewModel.OpenSegmentsConfig = OpenSegmentsConfig;
            ViewModel.RenderContextChanged += OnRenderContextChanged;
        };
    }

    private void OnRenderContextChanged(object? sender, EventArgs e)
    {
        Logger.Debug("RenderContextChanged");
        DLViewerGL.Redraw();
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
        _currentRenderSettingsWindow = new DListViewerRenderSettingsWindow() { DataContext = vm };
        _currentRenderSettingsWindow.Closed += (sender, e) =>
        {
            _currentRenderSettingsWindow = null;
        };
        _currentRenderSettingsWindow.Show();
        return vm;
    }

    private SegmentsConfigWindowViewModel? OpenSegmentsConfig(
        Func<SegmentsConfigWindowViewModel> vmFactory
    )
    {
        if (_currentSegmentsConfigWindow != null)
        {
            _currentSegmentsConfigWindow.Activate();
            return null;
        }

        var vm = vmFactory();
        _currentSegmentsConfigWindow = new SegmentsConfigWindow() { DataContext = vm };
        _currentSegmentsConfigWindow.Closed += (sender, e) =>
        {
            _currentSegmentsConfigWindow = null;
        };
        _currentSegmentsConfigWindow.Show();
        return vm;
    }
}
