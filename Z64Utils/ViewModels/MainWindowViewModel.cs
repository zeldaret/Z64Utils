using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Z64;

namespace Z64Utils.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private Z64Game? _game;
    public Z64Game? Game => _game;

    // Provided by the view
    public Func<Task<IStorageFile?>>? GetOpenROM;
    public Func<Task<IStorageFile?>>? GetOpenFile;
    public Func<Task<IStorageFolder?>>? GetOpenFolderForExportFS;
    public Func<Task<IStorageFile?>>? GetOpenROMForSave;
    public Func<Task<IStorageFile?>>? GetOpenFileForSave;
    public Func<RenameFileWindowViewModel, Task<string?>>? GetRenamedFileName;
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

    [RelayCommand]
    private async Task OpenROM()
    {
        try
        {
            Utils.Assert(GetOpenROM != null);
            var file = await GetOpenROM();
            Logger.Debug("file={filePath}", file?.TryGetLocalPath());

            if (file == null)
            {
                // Cancelled file open, do nothing
            }
            else
            {
                // TODO not entirely sure about this IStorageFile -> path
                string path = file.Path.LocalPath;
                OpenROMImpl(path);
            }
        }
        catch (Exception e)
        {
            // FIXME this isn't MVVM
            Logger.Error(e, "An error occured opening the ROM");
            var ewin = new Z64Utils.Views.ErrorWindow();
            ewin.SetMessage("An error occured opening the ROM", e.ToString());
            ewin.Show();
            throw;
        }
    }

    // TODO figure out how to make this method async
    // (currently hidden in Z64Game)
    public void OpenROMImpl(string path)
    {
        ProgressText = $"new Z64Game({path})";
        _game = new Z64Game(path);
        ProgressText = "OpenROM complete";

        UpdateRomFiles();
    }

    [RelayCommand(CanExecute = nameof(CanExportFS))]
    private async Task ExportFS()
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

    private bool CanExportFS()
    {
        return _game != null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveAs))]
    private async Task SaveAs()
    {
        Utils.Assert(GetOpenROMForSave != null);
        Utils.Assert(_game != null);
        var file = await GetOpenROMForSave();
        if (file == null)
            return;

        _game.FixRom();
        File.WriteAllBytes(file.Path.LocalPath, _game.Rom.RawRom);
    }

    private bool CanSaveAs()
    {
        return _game != null;
    }

    [RelayCommand(CanExecute = nameof(CanImportFileNameList))]
    private async Task ImportFileNameList()
    {
        Utils.Assert(GetOpenFile != null);
        Utils.Assert(_game != null);
        var f = await GetOpenFile();
        if (f == null)
            return;
        Z64Version.ImportFileList(_game, f.Path.LocalPath);
        UpdateRomFiles();
    }

    private bool CanImportFileNameList()
    {
        return _game != null;
    }

    [RelayCommand(CanExecute = nameof(CanExportFileNameList))]
    private async Task ExportFileNameList()
    {
        Utils.Assert(GetOpenFileForSave != null);
        Utils.Assert(_game != null);
        var f = await GetOpenFileForSave();
        if (f == null)
            return;
        Z64Version.ExportFileList(_game, f.Path.LocalPath);
    }

    private bool CanExportFileNameList()
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

    [RelayCommand(CanExecute = nameof(CanTextureViewer))]
    private void TextureViewer()
    {
        Utils.Assert(_game != null);
        Utils.Assert(OpenTextureViewer != null);
        OpenTextureViewer(new TextureViewerWindowViewModel(_game));
    }

    private bool CanTextureViewer()
    {
        return _game != null;
    }

    [RelayCommand]
    private async Task ObjectAnalyzer()
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
            (int)segment,
            true
        );
        OpenObjectAnalyzer(vm);
    }

    public ObjectAnalyzerWindowViewModel OpenObjectAnalyzerByZ64File(Z64File file, int segment)
    {
        Utils.Assert(_game != null);
        Utils.Assert(OpenObjectAnalyzer != null);
        var objectAnalyzerVM = new ObjectAnalyzerWindowViewModel();
        OpenObjectAnalyzer(objectAnalyzerVM);
        objectAnalyzerVM.SetFile(_game, _game.GetFileName(file.VRomStart), file, segment, true);
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

public partial class MainWindowViewModelRomFile
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

    [RelayCommand]
    private async Task OpenObjectAnalyzer()
    {
        Utils.Assert(_parentVM.PickSegmentID != null);
        int? segmentID = await _parentVM.PickSegmentID(new());
        Logger.Debug("segmentID={segmentID}", segmentID);
        if (segmentID != null)
            _parentVM.OpenObjectAnalyzerByZ64File(File, (int)segmentID);
    }

    [RelayCommand]
    private async Task InjectFile()
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

    [RelayCommand]
    private async Task SaveFile()
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

    [RelayCommand]
    private async Task RenameFile()
    {
        Utils.Assert(_parentVM.GetRenamedFileName != null);
        Utils.Assert(_parentVM.Game != null);
        var newName = await _parentVM.GetRenamedFileName(new() { Name = Name });
        if (newName == null)
            return;
        _parentVM.Game.Version.RenameFile(File.VRomStart, newName);
        _parentVM.UpdateRomFiles();
    }
}
