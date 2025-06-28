using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;

namespace Z64Utils_Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (DataContext == null)
                return;
            var vm = (MainWindowViewModel)DataContext;
            vm.GetOpenROM = ShowDialogOpenROMAsync;
            vm.GetOpenFile = ShowDialogOpenFileAsync;
            vm.GetOpenFolderForExportFS = ShowDialogOpenFolderForExportFSAsync;
            vm.PickSegmentID = OpenPickSegmentID;
            vm.OpenObjectAnalyzer = OpenObjectAnalyzer;
            vm.OpenDListViewer = OpenDListViewer;
            vm.OpenF3DZEXDisassembler = OpenF3DZEXDisassembler;
            vm.OpenROMRAMConversions = OpenROMRAMConversions;
            vm.OpenTextureViewer = OpenTextureViewer;
        };
    }

    private async Task<IStorageFile?> ShowDialogOpenROMAsync()
    {
        Utils.Assert(StorageProvider.CanOpen);
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                Title = "Open ROM",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>()
                {
                    new FilePickerFileType("N64 ROM image")
                    {
                        Patterns = new[] { "*.z64" },
                        MimeTypes = new[] { "application/x-n64-rom" },
                    },
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

    private async Task<IStorageFolder?> ShowDialogOpenFolderForExportFSAsync()
    {
        Utils.Assert(StorageProvider.CanPickFolder);
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions()
            {
                Title = "Choose folder to export into",
                AllowMultiple = false,
            }
        );

        if (folders.Count == 0)
        {
            return null;
        }
        else
        {
            Utils.Assert(folders.Count == 1);
            return folders[0];
        }
    }

    private async Task<int?> OpenPickSegmentID(PickSegmentIDWindowViewModel vm)
    {
        var pickSegmentIDWin = new PickSegmentIDWindow() { DataContext = vm };
        var dialogResultTask = pickSegmentIDWin.ShowDialog<int?>(this);
        int? segmentID = await dialogResultTask;
        return segmentID;
    }

    private void OpenObjectAnalyzer(ObjectAnalyzerWindowViewModel vm)
    {
        var win = new ObjectAnalyzerWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenDListViewer(DListViewerWindowViewModel vm)
    {
        var win = new DListViewerWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenF3DZEXDisassembler(F3DZEXDisassemblerViewModel vm)
    {
        var win = new F3DZEXDisassemblerWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenROMRAMConversions(ROMRAMConversionsWindowViewModel vm)
    {
        var win = new ROMRAMConversionsWindow() { DataContext = vm };
        win.Show();
    }

    private void OpenTextureViewer(TextureViewerWindowViewModel vm)
    {
        var win = new TextureViewerWindow() { DataContext = vm };
        win.Show();
    }

    public async void OnCheckNewReleasesMenuItemClick(object? sender, RoutedEventArgs args)
    {
        Common.GithubRelease release;
        try
        {
            release = await Common.UpdateChecker.GetLatestRelease();
        }
        catch (Exception e)
        {
            var errWin = new ErrorWindow();
            errWin.SetMessage("An error occurred while looking for updates.", e.ToString());
            errWin.Show();
            return;
        }
        string currentTag = Common.UpdateChecker.CurrentTag;
        Utils.Assert(release.TagName != null);
        string latestTag = release.TagName;
        var w = new UpdateWindow();
        w.SetNewRelease(currentTag, latestTag, currentTag == latestTag);
        w.Show();
    }

    public void OnAboutMenuItemClick(object? sender, RoutedEventArgs args)
    {
        new AboutWindow().Show();
    }
}
