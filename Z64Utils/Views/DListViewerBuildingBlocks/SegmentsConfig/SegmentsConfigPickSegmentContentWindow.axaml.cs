using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Common;
using Z64Utils.ViewModels;

namespace Z64Utils_Avalonia;

public partial class SegmentsConfigPickSegmentContentWindow : Window
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public SegmentsConfigPickSegmentContentWindow()
    {
        InitializeComponent();
        DataContextChanged += (sender, e) =>
        {
            if (DataContext == null)
                return;
            var vm = (SegmentsConfigPickSegmentContentWindowViewModel)DataContext;
            vm.GetOpenFile = ShowDialogOpenFileAsync;
        };
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
        Utils.Assert(DataContext != null);
        var vm = (SegmentsConfigPickSegmentContentWindowViewModel)DataContext;
        Close(vm.MakeSegmentContent());
    }
}
