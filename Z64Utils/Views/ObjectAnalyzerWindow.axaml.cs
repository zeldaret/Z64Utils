using System;
using Avalonia.Controls;
using Common;

namespace Z64Utils_Avalonia;

public partial class ObjectAnalyzerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public ObjectAnalyzerWindowViewModel? ViewModel;

    private F3DZEXDisassemblerSettingsWindow? _currentF3DZEXDisassemblerSettingsWindow;

    public ObjectAnalyzerWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            ViewModel = (ObjectAnalyzerWindowViewModel?)DataContext;
            if (ViewModel == null)
                return;
            ViewModel.OpenDListViewer = OpenDListViewer;
            ViewModel.OpenSkeletonViewer = OpenSkeletonViewer;
            ViewModel.OpenCollisionViewer = OpenCollisionViewer;
            ViewModel.OpenF3DZEXDisassemblerSettings = OpenF3DZEXDisassemblerSettings;
        };
    }

    private void OpenDListViewer(DListViewerWindowViewModel vm)
    {
        var win = new DListViewerWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenSkeletonViewer(SkeletonViewerWindowViewModel vm)
    {
        var win = new SkeletonViewerWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenCollisionViewer(CollisionViewerWindowViewModel vm)
    {
        var win = new CollisionViewerWindow() { DataContext = vm };
        win.Show();
    }

    private F3DZEXDisassemblerSettingsViewModel? OpenF3DZEXDisassemblerSettings(
        Func<F3DZEXDisassemblerSettingsViewModel> vmFactory
    )
    {
        if (_currentF3DZEXDisassemblerSettingsWindow != null)
        {
            _currentF3DZEXDisassemblerSettingsWindow.Activate();
            return null;
        }

        var vm = vmFactory();
        _currentF3DZEXDisassemblerSettingsWindow = new F3DZEXDisassemblerSettingsWindow()
        {
            DataContext = vm,
        };
        _currentF3DZEXDisassemblerSettingsWindow.Closed += (sender, e) =>
        {
            _currentF3DZEXDisassemblerSettingsWindow = null;
        };
        _currentF3DZEXDisassemblerSettingsWindow.Show();
        return vm;
    }

    public void OnObjectHolderEntriesDataGridSelectionChanged(
        object? sender,
        SelectionChangedEventArgs ev
    )
    {
        var selectedItem = ObjectHolderEntriesDataGrid.SelectedItem;
        if (selectedItem == null)
            return;
        Utils.Assert(selectedItem is ObjectAnalyzerWindowViewModel.ObjectHolderEntry);
        var ohe = (ObjectAnalyzerWindowViewModel.ObjectHolderEntry)selectedItem;
        var t1 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        ViewModel?.OnObjectHolderEntrySelected(ohe);
        var t2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Logger.Trace("ViewModel.OnObjectHolderEntrySelected(ohe); t2-t1={0}ms", t2 - t1);
    }

    public void OnObjectHolderEntriesDataGridLoadingRow(object? sender, DataGridRowEventArgs ev)
    {
        Utils.Assert(ev.Row.DataContext != null);
        Utils.Assert(ev.Row.DataContext is ObjectAnalyzerWindowViewModel.ObjectHolderEntry);
        var rowObjectHolderEntry = (ObjectAnalyzerWindowViewModel.ObjectHolderEntry)
            ev.Row.DataContext;

        var cm = new ContextMenu();
        foreach (var action in rowObjectHolderEntry.GetAvailableActions())
        {
            cm.Items.Add(
                new MenuItem()
                {
                    Header = action.label,
                    Command = action.command,
                    CommandParameter = action.commandParameter,
                }
            );
        }
        ev.Row.ContextMenu = cm;
    }
}
