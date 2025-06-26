using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;

namespace Z64Utils_Avalonia;

public partial class SegmentsConfigPickSegmentContentWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public SegmentsConfigPickSegmentContentWindowViewModel ViewModel;

    public SegmentsConfigPickSegmentContentWindow(
        SegmentsConfigPickSegmentContentWindowViewModel vm
    )
    {
        ViewModel = vm;
        ViewModel.GetOpenFile = ShowDialogOpenFileAsync;
        DataContext = ViewModel;
        InitializeComponent();
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

    public void OnOKButtonClick(object? sender, RoutedEventArgs args)
    {
        Close(ViewModel.MakeSegmentContent());
    }
}
