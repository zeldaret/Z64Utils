using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Z64;

namespace Z64Utils.ViewModels;

public partial class SegmentsConfigWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public F3DZEX.Memory Memory { get; private set; }

    public SegmentConfigControlViewModel[] Segments { get; private set; }

    public event EventHandler? SegmentsConfigChanged;

    // Provided by the view
    public Func<
        SegmentsConfigPickSegmentContentWindowViewModel,
        Task<F3DZEX.Memory.Segment?>
    >? PickSegmentContent;

    public SegmentsConfigWindowViewModel(F3DZEX.Memory memory, Z64Game? game)
    {
        Memory = memory;
        Segments = new SegmentConfigControlViewModel[16];
        for (var i = 0; i < 16; i++)
            Segments[i] = new(this, game, i);
    }

    public void RaiseSegmentsConfigChanged()
    {
        SegmentsConfigChanged?.Invoke(this, new());
    }
}

public partial class SegmentConfigControlViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    int _segmentNumber;

    [ObservableProperty]
    string _description;

    private SegmentsConfigWindowViewModel _parentVM;
    private Z64Game? _game;

    public SegmentConfigControlViewModel(
        SegmentsConfigWindowViewModel parentVM,
        Z64Game? game,
        int segmentNumber
    )
    {
        _parentVM = parentVM;
        _game = game;
        SegmentNumber = segmentNumber;
        Description = _parentVM.Memory.Segments[segmentNumber].Label;
    }

    [RelayCommand]
    private async Task Edit()
    {
        Utils.Assert(_parentVM.PickSegmentContent != null);
        var newSegmentContent = await _parentVM.PickSegmentContent(new(_game));
        if (newSegmentContent == null)
            return;
        _parentVM.Memory.Segments[SegmentNumber] = newSegmentContent;
        Description = newSegmentContent.Label;
        _parentVM.RaiseSegmentsConfigChanged();
    }
}

public enum SegmentsConfigPickSegmentContentType
{
    EMPTY,
    ADDRESS,
    ROM_FILESYSTEM,
    FILE,
    IDENTITY_MATRICES,
    PRIM_COLOR_DLIST,
    ENV_COLOR_DLIST,
    NULL_BYTES,
    EMPTY_DLIST,
}

public partial class SegmentsConfigPickSegmentContentWindowViewModel : ObservableObject
{
    private Z64Game? _game;

    [ObservableProperty]
    SegmentsConfigPickSegmentContentType _segmentType = SegmentsConfigPickSegmentContentType.EMPTY;

    [ObservableProperty]
    bool _isSegmentPickComplete = true;

    [ObservableProperty]
    ISegmentConfigPickSegmentContentConfigViewModel _segmentConfigPickSegmentContentConfigViewModel;

    // Provided by the view
    public Func<Task<IStorageFile?>>? GetOpenFile;

    public SegmentsConfigPickSegmentContentWindowViewModel(Z64Game? game)
    {
        _game = game;
        SegmentConfigPickSegmentContentConfigViewModel = new EmptySCPSCCViewModel(this);
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(SegmentType):
                    OnSegmentTypeChanged();
                    break;
            }
        };
    }

    private void OnSegmentTypeChanged()
    {
        switch (SegmentType)
        {
            case SegmentsConfigPickSegmentContentType.ADDRESS:
                SegmentConfigPickSegmentContentConfigViewModel = new AddressSCPSCCViewModel(this);
                break;
            case SegmentsConfigPickSegmentContentType.ROM_FILESYSTEM:
                SegmentConfigPickSegmentContentConfigViewModel = new ROMFileSystemSCPSCCViewModel(
                    this,
                    _game
                );
                break;
            case SegmentsConfigPickSegmentContentType.FILE:
                SegmentConfigPickSegmentContentConfigViewModel = new FileSCPSCCViewModel(this);
                break;
            case SegmentsConfigPickSegmentContentType.PRIM_COLOR_DLIST:
                SegmentConfigPickSegmentContentConfigViewModel = new PrimColorDListSCPSCCViewModel(
                    this
                );
                break;
            case SegmentsConfigPickSegmentContentType.ENV_COLOR_DLIST:
                SegmentConfigPickSegmentContentConfigViewModel = new EnvColorDListSCPSCCViewModel(
                    this
                );
                break;
            default:
                SegmentConfigPickSegmentContentConfigViewModel = new EmptySCPSCCViewModel(this);
                break;
        }
    }

    // csharpier-ignore
    private static readonly byte[] IdentityMtxData = new byte[]
    {
        0,1,   0,0,   0,0,   0,0,
        0,0,   0,1,   0,0,   0,0,
        0,0,   0,0,   0,1,   0,0,
        0,0,   0,0,   0,0,   0,1,

        0,0,   0,0,   0,0,   0,0,
        0,0,   0,0,   0,0,   0,0,
        0,0,   0,0,   0,0,   0,0,
        0,0,   0,0,   0,0,   0,0,
    };

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        switch (SegmentType)
        {
            case SegmentsConfigPickSegmentContentType.EMPTY:
                return F3DZEX.Memory.Segment.Empty();
            case SegmentsConfigPickSegmentContentType.IDENTITY_MATRICES:
                return F3DZEX.Memory.Segment.FromFill("Identity Matrices", IdentityMtxData);
            case SegmentsConfigPickSegmentContentType.NULL_BYTES:
                return F3DZEX.Memory.Segment.FromFill("Null Bytes");
            case SegmentsConfigPickSegmentContentType.EMPTY_DLIST:
                return F3DZEX.Memory.Segment.FromFill(
                    "Empty DList",
                    new byte[] { 0xDF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
                );
                ;
            default:
                return SegmentConfigPickSegmentContentConfigViewModel.MakeSegmentContent();
        }
    }
}

public interface ISegmentConfigPickSegmentContentConfigViewModel
{
    F3DZEX.Memory.Segment MakeSegmentContent();
}

public partial class EmptySCPSCCViewModel : ISegmentConfigPickSegmentContentConfigViewModel
{
    public EmptySCPSCCViewModel(SegmentsConfigPickSegmentContentWindowViewModel parentVM)
    {
        parentVM.IsSegmentPickComplete = true;
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        throw new NotImplementedException();
    }
}

public partial class AddressSCPSCCViewModel
    : ObservableObject,
        ISegmentConfigPickSegmentContentConfigViewModel
{
    private SegmentsConfigPickSegmentContentWindowViewModel _parentVM;

    [ObservableProperty]
    uint? _address;

    public AddressSCPSCCViewModel(SegmentsConfigPickSegmentContentWindowViewModel parentVM)
    {
        _parentVM = parentVM;
        _parentVM.IsSegmentPickComplete = false;
        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Address):
                    _parentVM.IsSegmentPickComplete = Address != null;
                    break;
            }
        };
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        Utils.Assert(Address != null);
        var addr = (uint)Address;
        return F3DZEX.Memory.Segment.FromVram($"{addr:X8}", addr);
    }
}

public partial class ROMFileSystemSCPSCCViewModel
    : ObservableObject,
        ISegmentConfigPickSegmentContentConfigViewModel
{
    public partial class FileEntry
    {
        public Z64File File { get; private set; }

        public string FileName { get; private set; }

        public FileEntry(Z64File file, string fileName)
        {
            File = file;
            FileName = fileName;
        }
    }

    private SegmentsConfigPickSegmentContentWindowViewModel _parentVM;

    private Z64Game? _game;

    public IEnumerable<FileEntry> Files { get; private set; }

    [ObservableProperty]
    private Z64File? _file;

    [ObservableProperty]
    public string _statusText = "";

    public ROMFileSystemSCPSCCViewModel(
        SegmentsConfigPickSegmentContentWindowViewModel parentVM,
        Z64Game? game
    )
    {
        _parentVM = parentVM;
        _game = game;
        var files = new List<FileEntry>();
        if (_game != null)
        {
            for (int i = 0; i < _game.GetFileCount(); i++)
            {
                var f = _game.GetFileFromIndex(i);
                files.Add(new(f, _game.GetFileName(f.VRomStart)));
            }
        }
        Files = files;

        PropertyChanged += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(File):
                    Utils.Assert(_game != null);
                    _parentVM.IsSegmentPickComplete = File != null && File.Valid();
                    UpdateStatusText();
                    break;
            }
        };
        _parentVM.IsSegmentPickComplete = false;
        UpdateStatusText();
    }

    void UpdateStatusText()
    {
        if (_game == null)
            StatusText = "No ROM loaded";
        else if (File == null)
            StatusText = "No file chosen";
        else if (!File.Valid())
            StatusText = "Invalid file";
        else
            StatusText = "";
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        Utils.Assert(_game != null);
        Utils.Assert(File != null);
        Utils.Assert(File.Valid());
        return F3DZEX.Memory.Segment.FromBytes(_game.GetFileName(File.VRomStart), File.Data);
    }
}

public partial class FileSCPSCCViewModel
    : ObservableObject,
        ISegmentConfigPickSegmentContentConfigViewModel
{
    private SegmentsConfigPickSegmentContentWindowViewModel _parentVM;

    [ObservableProperty]
    IStorageFile? _file;

    [ObservableProperty]
    public string _fileName = "";

    public FileSCPSCCViewModel(SegmentsConfigPickSegmentContentWindowViewModel parentVM)
    {
        _parentVM = parentVM;
        _parentVM.IsSegmentPickComplete = false;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        Utils.Assert(_parentVM.GetOpenFile != null);
        var file = await _parentVM.GetOpenFile();
        if (file == null)
            return;
        File = file;
        FileName = file.Name;
        _parentVM.IsSegmentPickComplete = true;
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        Utils.Assert(File != null);
        return F3DZEX.Memory.Segment.FromBytes(
            File.Name,
            System.IO.File.ReadAllBytes(File.Path.LocalPath)
        );
    }
}

public partial class PrimColorDListSCPSCCViewModel
    : ObservableObject,
        ISegmentConfigPickSegmentContentConfigViewModel
{
    [ObservableProperty]
    int _mValue = 0;

    [ObservableProperty]
    int _lodFracValue = 0;

    [ObservableProperty]
    Color _color = new(255, 255, 255, 255);

    public PrimColorDListSCPSCCViewModel(SegmentsConfigPickSegmentContentWindowViewModel parentVM)
    {
        parentVM.IsSegmentPickComplete = true;
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        var m = (byte)MValue;
        var lodFrac = (byte)LodFracValue;

        return F3DZEX.Memory.Segment.FromFill(
            "Prim Color",
            new byte[]
            {
                0xFA,
                0x00,
                m,
                lodFrac,
                Color.R,
                Color.G,
                Color.B,
                Color.A,
                0xDF,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            }
        );
    }
}

public partial class EnvColorDListSCPSCCViewModel
    : ObservableObject,
        ISegmentConfigPickSegmentContentConfigViewModel
{
    [ObservableProperty]
    Color _color = new(255, 255, 255, 255);

    public EnvColorDListSCPSCCViewModel(SegmentsConfigPickSegmentContentWindowViewModel parentVM)
    {
        parentVM.IsSegmentPickComplete = true;
    }

    public F3DZEX.Memory.Segment MakeSegmentContent()
    {
        return F3DZEX.Memory.Segment.FromFill(
            "Env Color",
            new byte[]
            {
                0xFB,
                0x00,
                0x00,
                0x00,
                Color.R,
                Color.G,
                Color.B,
                Color.A,
                0xDF,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
                0x00,
            }
        );
    }
}
