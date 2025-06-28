using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Z64;

namespace Z64Utils_Avalonia;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private Z64Game? _game;
    public Z64Game? Game => _game;

    // this may not be needed on Windows, test eventually
    // bug on Linux? idk if this issue is relevant:
    // https://github.com/AvaloniaUI/Avalonia/issues/2958
    private bool FilePickerActive;

    // Provided by the view
    public Func<Task<IStorageFile?>>? GetOpenROM;
    public Func<Task<IStorageFile?>>? GetOpenFile;
    public Func<Task<IStorageFolder?>>? GetOpenFolderForExportFS;
    public Func<Task<IStorageFile?>>? GetOpenROMForSave;
    public Func<Task<IStorageFile?>>? GetOpenFileForSave;
    public Func<PickSegmentIDWindowViewModel, Task<int?>>? PickSegmentID;
    public Action<ObjectAnalyzerWindowViewModel>? OpenObjectAnalyzer;
    public Action<DListViewerWindowViewModel>? OpenDListViewer;
    public Action<F3DZEXDisassemblerViewModel>? OpenF3DZEXDisassembler;
    public Action<ROMRAMConversionsWindowViewModel>? OpenROMRAMConversions;
    public Action<TextureViewerWindowViewModel>? OpenTextureViewer;

    public MainWindowViewModel()
    {
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(FilterText):
                    UpdateRomFiles();
                    break;
            }
        };
    }

    public async Task OpenROMCommand()
    {
        try
        {
            Utils.Assert(!FilePickerActive);
            FilePickerActive = true;
            IStorageFile? file;
            try
            {
                Utils.Assert(GetOpenROM != null);
                file = await GetOpenROM();
            }
            finally
            {
                FilePickerActive = false;
            }
            Logger.Debug("file={filePath}", file?.TryGetLocalPath());

            if (file == null)
            {
                // Cancelled file open, do nothing
            }
            else
            {
                // TODO not entirely sure about this IStorageFile -> path
                string path = file.Path.LocalPath;
                OpenROM(path);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occured opening the ROM");
            var ewin = new ErrorWindow();
            ewin.SetMessage("An error occured opening the ROM", e.ToString());
            ewin.Show();
            throw;
        }
    }

    // avalonia bug?: CanOpenROMCommand is not used if it takes no argument
    public bool CanOpenROMCommand(object arg)
    {
        return !FilePickerActive;
    }

    // TODO figure out how to make this method async
    // (currently hidden in Z64Game)
    public void OpenROM(string path)
    {
        ProgressText = $"new Z64Game({path})";
        _game = new Z64Game(path);
        ProgressText = "OpenROM complete";

        UpdateRomFiles();
    }

    public async void ExportFSCommand()
    {
        Utils.Assert(GetOpenFolderForExportFS != null);
        Utils.Assert(_game != null);
        var folder = await GetOpenFolderForExportFS();
        if (folder == null)
            return;

        for (int i = 0; i < _game.GetFileCount(); i++)
        {
            var file = _game.GetFileFromIndex(i);
            if (!file.Valid())
                continue;
            string name = _game.GetFileName(file.VRomStart);
            if (string.IsNullOrEmpty(name) || !Utils.IsValidFileName(name))
                name = $"{file.VRomStart:X8}-{file.VRomEnd:X8}";
            File.WriteAllBytes($"{folder.Path.LocalPath}/{name}.bin", file.Data);
        }
    }

    public bool CanExportFSCommand(object arg)
    {
        return _game != null;
    }

    public async Task SaveAsCommand()
    {
        Utils.Assert(GetOpenROMForSave != null);
        Utils.Assert(_game != null);
        var file = await GetOpenROMForSave();
        if (file == null)
            return;

        _game.FixRom();
        File.WriteAllBytes(file.Path.LocalPath, _game.Rom.RawRom);
    }

    public bool CanSaveAsCommand(object arg)
    {
        return _game != null;
    }

    public void ImportFileNameListCommand()
    {
        // TODO
    }

    public bool CanImportFileNameListCommand(object arg)
    {
        return _game != null;
    }

    public void ExportFileNameListCommand()
    {
        // TODO
    }

    public bool CanExportFileNameListCommand(object arg)
    {
        return _game != null;
    }

    public void OpenDListViewerCommand()
    {
        Utils.Assert(OpenDListViewer != null);
        OpenDListViewer(new(_game));
    }

    public void F3DZEXDisassemblerCommand()
    {
        Utils.Assert(OpenF3DZEXDisassembler != null);
        OpenF3DZEXDisassembler(new());
    }

    public void ROMRAMConversionsCommand()
    {
        Utils.Assert(_game != null);
        Utils.Assert(OpenROMRAMConversions != null);
        OpenROMRAMConversions(new ROMRAMConversionsWindowViewModel(_game));
    }

    public bool CanROMRAMConversionsCommand(object arg)
    {
        return _game != null;
    }

    public void TextureViewerCommand()
    {
        Utils.Assert(_game != null);
        Utils.Assert(OpenTextureViewer != null);
        OpenTextureViewer(new TextureViewerWindowViewModel(_game));
    }

    public bool CanTextureViewerCommand(object arg)
    {
        return _game != null;
    }

    public async void ObjectAnalyzerCommand()
    {
        Utils.Assert(GetOpenFile != null);
        Utils.Assert(PickSegmentID != null);
        Utils.Assert(OpenObjectAnalyzer != null);

        var file = await GetOpenFile();
        if (file == null)
            return;

        int? segment = await PickSegmentID(new());
        if (segment == null)
            return;

        var vm = new ObjectAnalyzerWindowViewModel();
        vm.SetFile(
            null,
            file.Name,
            new(File.ReadAllBytes(file.Path.LocalPath), 0, 0, 0, false),
            (int)segment
        );
        OpenObjectAnalyzer(vm);
    }

    public void CheckNewReleasesCommand()
    {
        // TODO
    }

    public ObjectAnalyzerWindowViewModel OpenObjectAnalyzerByZ64File(Z64File file, int segment)
    {
        Utils.Assert(_game != null);
        Utils.Assert(OpenObjectAnalyzer != null);
        var objectAnalyzerVM = new ObjectAnalyzerWindowViewModel();
        OpenObjectAnalyzer(objectAnalyzerVM);
        objectAnalyzerVM.SetFile(_game, _game.GetFileName(file.VRomStart), file, segment);
        return objectAnalyzerVM;
    }

    public ObjectAnalyzerWindowViewModel? OpenObjectAnalyzerByFileName(string name, int segment)
    {
        Utils.Assert(_game != null);
        var file = _game.GetFileByName(name);
        if (file == null)
            return null;
        else
            return OpenObjectAnalyzerByZ64File(file, segment);
    }

    //

    [ObservableProperty]
    private string _progressText = "hey";

    [ObservableProperty]
    private string _filterText = "";

    [ObservableProperty]
    public ObservableCollection<MainWindowViewModelRomFile> _romFiles = new();

    public void UpdateRomFiles()
    {
        RomFiles.Clear();

        if (_game == null)
            return;

        string filterText = FilterText.ToLower();

        for (int i = 0; i < _game.GetFileCount(); i++)
        {
            var file = _game.GetFileFromIndex(i);
            if (!file.Valid())
                continue;

            string name = _game.GetFileName(file.VRomStart);
            string vrom = $"{file.VRomStart:X8}-{file.VRomEnd:X8}";
            string rom = $"{file.RomStart:X8}-{file.RomEnd:X8}";
            string type = $"{_game.GetFileType(file.VRomStart)}";

            if (
                filterText == ""
                || name.ToLower().Contains(filterText)
                || type.ToLower().Contains(filterText)
            )
            {
                RomFiles.Add(new MainWindowViewModelRomFile(this, name, vrom, rom, type, file));
            }
        }
    }
}

public class MainWindowViewModelRomFile
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Name { get; }
    public string VROM { get; }
    public string ROM { get; }
    public string Type { get; }

    public Z64File File { get; }

    private MainWindowViewModel _parentVM;

    public MainWindowViewModelRomFile(
        MainWindowViewModel parentVM,
        string name,
        string vrom,
        string rom,
        string type,
        Z64File file
    )
    {
        Name = name;
        VROM = vrom;
        ROM = rom;
        Type = type;
        File = file;
        _parentVM = parentVM;
    }

    public async void OpenObjectAnalyzerCommand()
    {
        Utils.Assert(_parentVM.PickSegmentID != null);
        int? segmentID = await _parentVM.PickSegmentID(new());
        Logger.Debug("segmentID={segmentID}", segmentID);
        if (segmentID != null)
            _parentVM.OpenObjectAnalyzerByZ64File(File, (int)segmentID);
    }

    public async void InjectFileCommand()
    {
        Utils.Assert(_parentVM.GetOpenFile != null);
        Utils.Assert(_parentVM.Game != null);
        var newFile = await _parentVM.GetOpenFile();
        if (newFile == null)
            return;

        _parentVM.Game.InjectFile(
            File.VRomStart,
            System.IO.File.ReadAllBytes(newFile.Path.LocalPath)
        );
        _parentVM.UpdateRomFiles();
    }

    public async void SaveFileCommand()
    {
        Utils.Assert(_parentVM.GetOpenFileForSave != null);
        Utils.Assert(File.Valid());
        Utils.Assert(!File.Deleted);
        var saveToFile = await _parentVM.GetOpenFileForSave();
        if (saveToFile == null)
            return;

        System.IO.File.WriteAllBytes(saveToFile.Path.LocalPath, File.Data);
    }

    public bool CanSaveFileCommand(object arg)
    {
        return File.Valid() && !File.Deleted;
    }
}
