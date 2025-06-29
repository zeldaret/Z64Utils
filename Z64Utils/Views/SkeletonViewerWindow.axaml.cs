using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Common;
using Z64Utils.ViewModels;
using Z64Utils.Views.DListViewerBuildingBlocks.SegmentsConfig;

namespace Z64Utils.Views;

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
            _viewModel.PickROMFile = OpenPickROMFile;
            _viewModel.PickSegmentID = OpenPickSegmentID;
            _viewModel.GetOpenFile = ShowDialogOpenFileAsync;
            _viewModel.RenderContextChanged += OnRenderContextChanged;
        };

        SizeChanged += (sender, e) => WorkAroundDataGridSizeBug();
        WorkAroundDataGridSizeBug();
    }

    private void WorkAroundDataGridSizeBug()
    {
        if (double.IsNaN(Height))
            return;
        // Work around a bug where the DataGrid extends over the UI below and beyond the window.
        // (not sure which control's fault it is)
        var maxHeight = Height * 0.8;
        LimbsAndAnimationsGrid.RowDefinitions[2].MaxHeight = maxHeight;
        LimbsAndAnimationsGrid.Children[2].MaxHeight = maxHeight;
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

    private async Task<ROMFilePickerViewModel.ROMFile?> OpenPickROMFile(ROMFilePickerViewModel vm)
    {
        var pickSegmentIDWin = new ROMFilePickerWindow() { DataContext = vm };
        return await pickSegmentIDWin.ShowDialog<ROMFilePickerViewModel.ROMFile?>(this);
    }

    private async Task<int?> OpenPickSegmentID(PickSegmentIDWindowViewModel vm)
    {
        var pickSegmentIDWin = new PickSegmentIDWindow() { DataContext = vm };
        var dialogResultTask = pickSegmentIDWin.ShowDialog<int?>(this);
        int? segmentID = await dialogResultTask;
        return segmentID;
    }

    private async Task<IStorageFile?> ShowDialogOpenFileAsync()
    {
        Utils.Assert(StorageProvider.CanOpen);
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                Title = "Open File",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new FilePickerFileType("Any file") { Patterns = new[] { "*" } },
                },
            }
        );

        if (files.Count == 0)
        {
            return null;
        }
        else
        {
            Utils.Assert(files.Count == 1);
            return files[0];
        }
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
