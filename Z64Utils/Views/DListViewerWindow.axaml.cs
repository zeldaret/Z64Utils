using System;
using Avalonia.Controls;
using Z64Utils.ViewModels;

namespace Z64Utils_Avalonia;

public partial class DListViewerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private DListViewerWindowViewModel? _viewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;
    private SegmentsConfigWindow? _currentSegmentsConfigWindow;

    public DListViewerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (_viewModel != null)
            {
                _viewModel.RenderContextChanged -= OnRenderContextChanged;
            }

            _viewModel = (DListViewerWindowViewModel?)DataContext;
            if (_viewModel == null)
                return;
            _viewModel.OpenDListViewerRenderSettings = OpenDListViewerRenderSettings;
            _viewModel.OpenSegmentsConfig = OpenSegmentsConfig;
            _viewModel.RenderContextChanged += OnRenderContextChanged;
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
