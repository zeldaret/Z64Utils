using System;
using Avalonia.Controls;
using Common;

namespace Z64Utils_Avalonia;

public partial class SkeletonViewerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private SkeletonViewerWindowViewModel? _viewModel;

    private DListViewerRenderSettingsWindow? _currentRenderSettingsWindow;

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

    public void OnAnimationEntriesDataGridSelectionChanged(
        object? sender,
        SelectionChangedEventArgs ev
    )
    {
        var selectedItem = AnimationEntriesDataGrid.SelectedItem;
        if (selectedItem == null)
            return;
        Utils.Assert(selectedItem is SkeletonViewerWindowViewModel.AnimationEntry);
        var animationEntry = (SkeletonViewerWindowViewModel.AnimationEntry)selectedItem;
        animationEntry.OnSelected();
    }
}
