using System;
using Avalonia.Controls;

namespace Z64Utils_Avalonia;

public partial class DListViewerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public DListViewerWindowViewModel ViewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;
    private SegmentsConfigWindow? _currentSegmentsConfigWindow;

    public DListViewerWindow(DListViewerWindowViewModel vm)
    {
        ViewModel = vm;
        ViewModel.OpenDListViewerRenderSettings = OpenDListViewerRenderSettings;
        ViewModel.OpenSegmentsConfig = OpenSegmentsConfig;
        DataContext = ViewModel;
        InitializeComponent();
        ViewModel.RenderContextChanged += (sender, e) =>
        {
            Logger.Debug("RenderContextChanged");
            DLViewerGL.Redraw();
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
        _currentSegmentsConfigWindow = new SegmentsConfigWindow(vm);
        _currentSegmentsConfigWindow.Closed += (sender, e) =>
        {
            _currentSegmentsConfigWindow = null;
        };
        _currentSegmentsConfigWindow.Show();
        return _currentSegmentsConfigWindow.ViewModel;
    }
}
