using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;

namespace Z64Utils_Avalonia;

public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel;

    public MainWindow()
    {
        ViewModel = new MainWindowViewModel()
        {
            GetOpenROM = ShowDialogOpenROMAsync,
            PickSegmentID = OpenPickSegmentID,
            OpenObjectAnalyzer = OpenObjectAnalyzer,
            OpenDListViewer = OpenDListViewer,
            OpenF3DZEXDisassembler = OpenF3DZEXDisassembler,
            OpenROMRAMConversions = OpenROMRAMConversions,
            OpenTextureViewer = OpenTextureViewer,
        };
        DataContext = ViewModel;
        InitializeComponent();
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

    private async Task<int?> OpenPickSegmentID()
    {
        var pickSegmentIDWin = new PickSegmentIDWindow();
        var dialogResultTask = pickSegmentIDWin.ShowDialog<int?>(this);
        int? segmentID = await dialogResultTask;
        return segmentID;
    }

    private ObjectAnalyzerWindowViewModel OpenObjectAnalyzer()
    {
        var win = new ObjectAnalyzerWindow();
        win.Show();
        return win.ViewModel;
    }

    private DListViewerWindowViewModel OpenDListViewer()
    {
        var win = new DListViewerWindow();
        win.Show();
        return win.ViewModel;
    }

    private F3DZEXDisassemblerViewModel OpenF3DZEXDisassembler()
    {
        var win = new F3DZEXDisassemblerWindow();
        win.Show();
        return win.ViewModel;
    }

    private void OpenROMRAMConversions(ROMRAMConversionsWindowViewModel vm)
    {
        var win = new ROMRAMConversionsWindow(vm);
        win.Show();
    }

    private void OpenTextureViewer(TextureViewerWindowViewModel vm)
    {
        var win = new TextureViewerWindow(vm);
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
