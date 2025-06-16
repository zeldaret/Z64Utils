using System;
using System.Diagnostics;
using Avalonia.Controls;
using Common;

namespace Z64Utils_Avalonia;

public partial class ObjectAnalyzerWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public ObjectAnalyzerWindowViewModel ViewModel;

    private F3DZEXDisassemblerSettingsWindow? _currentF3DZEXDisassemblerSettingsWindow;

    public ObjectAnalyzerWindow()
    {
        ViewModel = new ObjectAnalyzerWindowViewModel()
        {
            OpenDListViewer = OpenDListViewer,
            OpenSkeletonViewer = OpenSkeletonViewer,
            OpenCollisionViewer = OpenCollisionViewer,
            OpenF3DZEXDisassemblerSettings = OpenF3DZEXDisassemblerSettings,
        };
        DataContext = ViewModel;
        InitializeComponent();
    }

    private DListViewerWindowViewModel OpenDListViewer()
    {
        var win = new DListViewerWindow();
        win.Show();
        return win.ViewModel;
    }

    private SkeletonViewerWindowViewModel OpenSkeletonViewer()
    {
        var win = new SkeletonViewerWindow();
        win.Show();
        return win.ViewModel;
    }

    private CollisionViewerWindowViewModel OpenCollisionViewer()
    {
        var win = new CollisionViewerWindow();
        win.Show();
        return win.ViewModel;
    }

    private F3DZEXDisassemblerSettingsViewModel? OpenF3DZEXDisassemblerSettings()
    {
        if (_currentF3DZEXDisassemblerSettingsWindow != null)
        {
            _currentF3DZEXDisassemblerSettingsWindow.Activate();
            return null;
        }

        _currentF3DZEXDisassemblerSettingsWindow = new F3DZEXDisassemblerSettingsWindow();
        _currentF3DZEXDisassemblerSettingsWindow.Closed += (sender, e) =>
        {
            _currentF3DZEXDisassemblerSettingsWindow = null;
        };
        _currentF3DZEXDisassemblerSettingsWindow.Show();
        return _currentF3DZEXDisassemblerSettingsWindow.ViewModel;
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
        ViewModel.OnObjectHolderEntrySelected(ohe);
        var t2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        Logger.Trace("ViewModel.OnObjectHolderEntrySelected(ohe); t2-t1={0}ms", t2 - t1);
    }

    public void OnObjectHolderEntriesDataGridLoadingRow(object? sender, DataGridRowEventArgs ev)
    {
        Utils.Assert(ev.Row.DataContext != null);
        Utils.Assert(ev.Row.DataContext is ObjectAnalyzerWindowViewModel.ObjectHolderEntry);
        var rowObjectHolderEntry = (ObjectAnalyzerWindowViewModel.ObjectHolderEntry)
            ev.Row.DataContext;

        // TODO should use some kind of "template" xaml thing for this, not code?
        var cm = new ContextMenu();
        // TODO this is very Model stuff, may not be correct to put in View code
        switch (rowObjectHolderEntry.ObjectHolder.GetEntryType())
        {
            case Z64.Z64Object.EntryType.DList:
                Utils.Assert(rowObjectHolderEntry != null);
                cm.Items.Add(
                    new MenuItem()
                    {
                        Header = "Open in DList Viewer",
                        Command = ViewModel.OpenDListViewerObjectHolderEntryCommand,
                        CommandParameter = rowObjectHolderEntry,
                    }
                );
                break;

            case Z64.Z64Object.EntryType.SkeletonHeader:
            case Z64.Z64Object.EntryType.FlexSkeletonHeader:
                Utils.Assert(rowObjectHolderEntry != null);
                cm.Items.Add(
                    new MenuItem()
                    {
                        Header = "Open in Skeleton Viewer",
                        Command = ViewModel.OpenSkeletonViewerObjectHolderEntryCommand,
                        CommandParameter = rowObjectHolderEntry,
                    }
                );
                break;

            case Z64.Z64Object.EntryType.CollisionHeader:
                Utils.Assert(rowObjectHolderEntry != null);
                cm.Items.Add(
                    new MenuItem()
                    {
                        Header = "Open in Collision Viewer",
                        Command = ViewModel.OpenCollisionViewerObjectHolderEntryCommand,
                        CommandParameter = rowObjectHolderEntry,
                    }
                );
                break;
        }
        cm.Items.Add(new MenuItem() { Header = "(TODO)" });
        ev.Row.ContextMenu = cm;
    }
}
