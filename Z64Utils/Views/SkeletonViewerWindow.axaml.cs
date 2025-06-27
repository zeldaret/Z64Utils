using System;
using Avalonia.Controls;
using Common;

namespace Z64Utils_Avalonia;

public partial class SkeletonViewerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private SkeletonViewerWindowViewModel? _viewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;
    private SegmentsConfigWindow? _currentSegmentsConfigWindow;

    public SkeletonViewerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (_viewModel != null)
            {
                _viewModel.RenderContextChanged -= OnRenderContextChanged;
            }

            _viewModel = (SkeletonViewerWindowViewModel?)DataContext;
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
        SkeletonViewerGL.Redraw();
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

    public void OnAnimationEntriesDataGridSelectionChanged(
        object? sender,
        SelectionChangedEventArgs ev
    )
    {
        var selectedItem = AnimationEntriesDataGrid.SelectedItem;
        if (selectedItem == null)
            return;
        Utils.Assert(selectedItem is SkeletonViewerWindowViewModel.IAnimationEntry);
        var animationEntry = (SkeletonViewerWindowViewModel.IAnimationEntry)selectedItem;
        animationEntry.OnSelected();
    }
}
