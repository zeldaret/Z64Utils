using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using RDP;
using Syroot.BinaryData;
using Z64;

namespace Z64Utils_Avalonia;

public partial class SkeletonViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // Used by the view to redraw when needed
    public event EventHandler? RenderContextChanged;

    private Z64Game? _game;

    [ObservableProperty]
    public F3DZEX.Render.Renderer? _renderer;

    [ObservableProperty]
    private ObservableCollection<IDLViewerControlDisplayElement> _displayElements = new();

    [ObservableProperty]
    private string? _decodeError;

    [ObservableProperty]
    private string? _renderError;

    [ObservableProperty]
    private string? _animationError;

    [ObservableProperty]
    private int _maxFrame = 0;

    [ObservableProperty]
    private int _curFrame = 0;

    [ObservableProperty]
    private ObservableCollection<SkeletonViewerLimbNode> _skeletonRootLimbNode = new();

    [ObservableProperty]
    private ObservableCollection<SkeletonViewerLimbNode> _selectedLimbNodes = new();

    public interface IAnimationEntry
    {
        string Name { get; }
        void OnSelected();
    }

    public interface IExternalAnimationEntry : IAnimationEntry
    {
        Dictionary<int, F3DZEX.Memory.Segment> ExternalData { get; }
    }

    public class RegularAnimationEntry : IAnimationEntry
    {
        private SkeletonViewerWindowViewModel _parentVM;
        public string Name { get; }
        public Z64Object.AnimationHolder AnimationHolder { get; }

        public RegularAnimationEntry(
            SkeletonViewerWindowViewModel parentVM,
            string name,
            Z64Object.AnimationHolder animationHolder
        )
        {
            _parentVM = parentVM;
            Name = name;
            AnimationHolder = animationHolder;
        }

        public void OnSelected()
        {
            _parentVM.OnAnimationEntrySelected(this);
        }
    }

    public class ExternalRegularAnimationEntry : RegularAnimationEntry, IExternalAnimationEntry
    {
        public Dictionary<int, F3DZEX.Memory.Segment> ExternalData { get; }

        public ExternalRegularAnimationEntry(
            SkeletonViewerWindowViewModel parentVM,
            string name,
            Z64Object.AnimationHolder animationHolder,
            Dictionary<int, F3DZEX.Memory.Segment> externalData
        )
            : base(parentVM, name, animationHolder)
        {
            ExternalData = externalData;
        }
    }

    public class PlayerAnimationEntry : IAnimationEntry
    {
        private SkeletonViewerWindowViewModel _parentVM;
        public string Name { get; }
        public Z64Object.PlayerAnimationHolder PlayerAnimationHolder { get; }

        public PlayerAnimationEntry(
            SkeletonViewerWindowViewModel parentVM,
            string name,
            Z64Object.PlayerAnimationHolder playerAnimationHolder
        )
        {
            _parentVM = parentVM;
            Name = name;
            PlayerAnimationHolder = playerAnimationHolder;
        }

        public void OnSelected()
        {
            _parentVM.OnAnimationEntrySelected(this);
        }
    }

    public class ExternalPlayerAnimationEntry : PlayerAnimationEntry, IExternalAnimationEntry
    {
        public Dictionary<int, F3DZEX.Memory.Segment> ExternalData { get; }

        public ExternalPlayerAnimationEntry(
            SkeletonViewerWindowViewModel parentVM,
            string name,
            Z64Object.PlayerAnimationHolder playerAnimationHolder,
            Dictionary<int, F3DZEX.Memory.Segment> externalData
        )
            : base(parentVM, name, playerAnimationHolder)
        {
            ExternalData = externalData;
        }
    }

    [ObservableProperty]
    private ObservableCollection<IAnimationEntry> _animationEntries = new();

    [ObservableProperty]
    private double _playAnimTickPeriodMs;
    private DispatcherTimer _playAnimTimer = new();

    [ObservableProperty]
    private bool _isPlayingBackwards;

    [ObservableProperty]
    private bool _isPlayingForwards;

    [ObservableProperty]
    Z64Skeleton? _skel;

    [ObservableProperty]
    Z64Animation? _curAnim;

    [ObservableProperty]
    Z64PlayerAnimation? _curPlayerAnim;

    [ObservableProperty]
    F3DZEX.Command.Dlist?[]? _limbsDLists;

    [ObservableProperty]
    Matrix4[]? _curPose;

    // Provided by the view
    public Func<
        Func<DListViewerRenderSettingsViewModel>,
        DListViewerRenderSettingsViewModel?
    >? OpenDListViewerRenderSettings;
    public Func<
        Func<SegmentsConfigWindowViewModel>,
        SegmentsConfigWindowViewModel?
    >? OpenSegmentsConfig;
    public Func<ROMFilePickerViewModel, Task<ROMFilePickerViewModel.ROMFile?>>? PickROMFile;
    public Func<PickSegmentIDWindowViewModel, Task<int?>>? PickSegmentID;
    internal Func<Task<IStorageFile?>>? GetOpenFile;

    public SkeletonViewerWindowViewModel(Z64Game? game)
    {
        _game = game;
        PropertyChanging += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(Renderer):
                    if (Renderer != null)
                        Renderer.PropertyChanged -= OnRendererPropertyChanged;
                    DisplayElements.Clear();
                    DecodeError = null;
                    RenderError = null;
                    break;
            }
        };
        PropertyChanged += (sender, e) =>
        {
            Logger.Debug("PropertyChanged {PropertyName}", e.PropertyName);
            switch (e.PropertyName)
            {
                case nameof(Renderer):
                    if (Renderer != null)
                        Renderer.PropertyChanged += OnRendererPropertyChanged;
                    break;
                case nameof(Skel):
                    CurAnim = null;
                    CurPlayerAnim = null;
                    if (Skel != null)
                    {
                        UpdateCurPose();
                        UpdateLimbNodes();
                        UpdateLimbsDLists();
                    }
                    else
                    {
                        LimbsDLists = null;
                        SkeletonRootLimbNode.Clear();
                        DisplayElements.Clear();
                    }
                    break;
                case nameof(LimbsDLists):
                    if (LimbsDLists != null)
                        UpdateDisplayElements();
                    break;
                case nameof(CurAnim):
                case nameof(CurPlayerAnim):
                    if (CurAnim != null)
                        MaxFrame = CurAnim.FrameCount - 1;
                    else if (CurPlayerAnim != null)
                        MaxFrame = CurPlayerAnim.FrameCount - 1;
                    else
                        MaxFrame = 0;
                    CurFrame = 0;
                    UpdateCurPose();
                    break;
                case nameof(CurFrame):
                    Utils.Assert(CurFrame >= 0 && CurFrame <= MaxFrame);
                    UpdateCurPose();
                    break;
                case nameof(CurPose):
                    if (LimbsDLists != null)
                        UpdateDisplayElements();
                    break;
                case nameof(PlayAnimTickPeriodMs):
                    if (PlayAnimTickPeriodMs < 1)
                        PlayAnimTickPeriodMs = 1;
                    _playAnimTimer.Interval = TimeSpan.FromMilliseconds(PlayAnimTickPeriodMs);
                    break;
            }
        };
        SelectedLimbNodes.CollectionChanged += (sender, e) =>
        {
            Logger.Debug("SelectedLimbNodes.CollectionChanged");
            UpdateDisplayElements();
        };
        _playAnimTimer.Tick += OnPlayAnimTimerTick;
        PlayAnimTickPeriodMs = 1000 / 20;
    }

    private void OnRendererPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Utils.Assert(Renderer != null);
        switch (e.PropertyName)
        {
            case nameof(Renderer.HasError):
                if (Renderer.HasError)
                {
                    RenderError =
                        $"RENDER ERROR AT 0x{Renderer.RenderErrorAddr:X8}! ({Renderer.ErrorMsg})";
                }
                else
                {
                    RenderError = null;
                }
                break;
        }
    }

    public void SetSegment(int index, F3DZEX.Memory.Segment segment)
    {
        if (Renderer == null)
            throw new Exception("Renderer is null");

        if (index >= 0 && index < F3DZEX.Memory.Segment.COUNT)
        {
            Renderer.Memory.Segments[index] = segment;

            if (Skel != null)
            {
                UpdateLimbsDLists();
            }
        }
    }

    public void OpenRenderSettingsCommand()
    {
        Utils.Assert(OpenDListViewerRenderSettings != null);
        Utils.Assert(Renderer != null);
        var vm = OpenDListViewerRenderSettings(
            () => new DListViewerRenderSettingsViewModel(Renderer.CurrentConfig)
        );
        if (vm == null)
        {
            // Was already open
            return;
        }

        vm.RendererConfigChanged += (sender, e) =>
        {
            RenderContextChanged?.Invoke(this, new());
        };
    }

    [DependsOn(nameof(Renderer))]
    public bool CanOpenRenderSettingsCommand(object arg)
    {
        return Renderer != null;
    }

    public void OpenSegmentsConfigCommand()
    {
        Utils.Assert(OpenSegmentsConfig != null);
        Utils.Assert(Renderer != null);
        var vm = OpenSegmentsConfig(
            () => new SegmentsConfigWindowViewModel(Renderer.Memory, _game)
        );
        if (vm == null)
        {
            // Was already open
            return;
        }
        vm.SegmentsConfigChanged += (sender, e) =>
        {
            Logger.Debug("SegmentsConfigChanged");
            for (int i = 0; i < F3DZEX.Memory.Segment.COUNT; i++)
                Logger.Debug("{segmentIndex} {segmentLabel}", i, Renderer.Memory.Segments[i].Label);
            UpdateLimbsDLists();
            RenderContextChanged?.Invoke(this, new());
        };
    }

    public async void LoadROMFileAnimationsCommand()
    {
        Utils.Assert(_game != null);
        Utils.Assert(PickROMFile != null);

        List<ROMFilePickerViewModel.ROMFile> files = new();
        for (int i = 0; i < _game.GetFileCount(); i++)
        {
            var f = _game.GetFileFromIndex(i);
            if (f.Valid())
                files.Add(new(f, _game.GetFileName(f.VRomStart)));
        }

        var extROMFile = await PickROMFile(new(files));
        if (extROMFile == null)
            return;
        var extFile = extROMFile.File;
        Utils.Assert(extFile.Valid());

        await LoadExternalFileAnimationsImpl(extFile.Data);
    }

    public bool CanLoadROMFileAnimationsCommand(object arg)
    {
        return _game != null;
    }

    public async Task LoadExternalFileAnimationsCommand()
    {
        Utils.Assert(GetOpenFile != null);
        var file = await GetOpenFile();
        if (file == null)
            return;
        await LoadExternalFileAnimationsImpl(File.ReadAllBytes(file.Path.LocalPath));
    }

    public async Task LoadExternalFileAnimationsImpl(byte[] data)
    {
        Utils.Assert(PickSegmentID != null);
        var segmentPick = await PickSegmentID(new());
        if (segmentPick == null)
            return;
        var segment = (int)segmentPick;

        var extObj = new Z64Object(_game, data, "gameplay_keep");
        Z64ObjectAnalyzer.FindDlists(extObj, data, segment, new());
        Z64ObjectAnalyzer.AnalyzeDlists(extObj, data, segment);

        var externalData = new Dictionary<int, F3DZEX.Memory.Segment>()
        {
            [segment] = F3DZEX.Memory.Segment.FromBytes("", data),
        };

        extObj.Entries.ForEach(e =>
        {
            if (e is Z64Object.AnimationHolder eAnim)
            {
                AnimationEntries.Add(
                    new ExternalRegularAnimationEntry(
                        this,
                        "ext_" + eAnim.Name,
                        eAnim,
                        externalData
                    )
                );
            }
        });
    }

    public void LoadPlayerAnimationsCommand()
    {
        Utils.Assert(_game != null);

        Z64File? gKeepFile = _game.GetFileByName("gameplay_keep");
        if (gKeepFile == null || !gKeepFile.Valid())
        {
            Logger.Error("gameplay_keep not found/invalid");
            // TODO show error
            return;
        }

        Z64File? link_animetionFile = _game.GetFileByName("link_animetion");
        if (link_animetionFile == null || !link_animetionFile.Valid())
        {
            Logger.Error("link_animetion not found/invalid");
            // TODO show error
            return;
        }

        var gKeepObj = new Z64Object(_game, gKeepFile.Data, "gameplay_keep");
        Z64ObjectAnalyzer.FindDlists(gKeepObj, gKeepFile.Data, 4, new());
        Z64ObjectAnalyzer.AnalyzeDlists(gKeepObj, gKeepFile.Data, 4);

        var externalData = new Dictionary<int, F3DZEX.Memory.Segment>()
        {
            [4] = F3DZEX.Memory.Segment.FromBytes("", gKeepFile.Data),
            [7] = F3DZEX.Memory.Segment.FromBytes("", link_animetionFile.Data),
        };

        gKeepObj.Entries.ForEach(e =>
        {
            if (e is Z64Object.PlayerAnimationHolder ePlayerAnim)
            {
                AnimationEntries.Add(
                    new ExternalPlayerAnimationEntry(
                        this,
                        "ext_" + ePlayerAnim.Name,
                        ePlayerAnim,
                        externalData
                    )
                );
            }
        });
    }

    public bool CanLoadPlayerAnimationsCommand(object arg)
    {
        return _game != null;
    }

    public void SetSkeleton(Z64Object.SkeletonHolder skeletonHolder)
    {
        Utils.Assert(Renderer != null);

        if (skeletonHolder is Z64Object.FlexSkeletonHolder flexSkeletonHolder)
            Skel = Z64FlexSkeleton.Get(Renderer.Memory, flexSkeletonHolder);
        else
            Skel = Z64Skeleton.Get(Renderer.Memory, skeletonHolder);
    }

    void UpdateLimbNodes()
    {
        Utils.Assert(Skel != null);

        void VisitLimb(SkeletonViewerLimbNode parent, Z64SkeletonTreeLimb treeLimb)
        {
            Utils.Assert(Skel != null);
            var limbHolder = Skel.Limbs[treeLimb.Index];
            var node = new SkeletonViewerLimbNode(treeLimb.Index, limbHolder.Name);
            parent.ChildrenLimbs.Add(node);

            if (treeLimb.Sibling != null)
                VisitLimb(parent, treeLimb.Sibling);
            if (treeLimb.Child != null)
                VisitLimb(node, treeLimb.Child);
        }

        var skeletonRootLimbNode = new SkeletonViewerLimbNode(0, Skel.Limbs[0].Name);
        if (Skel.Root.Child != null)
            VisitLimb(skeletonRootLimbNode, Skel.Root.Child);
        Utils.Assert(Skel.Root.Sibling == null);
        SkeletonRootLimbNode = new() { skeletonRootLimbNode };
    }

    void UpdateLimbsDLists()
    {
        Utils.Assert(Renderer != null);
        Utils.Assert(Skel != null);

        DecodeError = null;
        var limbsDLists = new F3DZEX.Command.Dlist?[Skel.Limbs.Count];

        for (int i = 0; i < Skel.Limbs.Count; i++)
        {
            var limb = Skel.Limbs[i];
            if (
                limb.Type != Z64Object.EntryType.StandardLimb
                && limb.Type != Z64Object.EntryType.LODLimb
            )
                throw new Exception($"Unimplemented limb type in skeleton viewer {limb.Type}");
            Utils.Assert(limb.DListSeg != null); // always set for Standard and LOD limbs
            F3DZEX.Command.Dlist? dlist = null;
            try
            {
                if (limb.DListSeg.VAddr != 0)
                    dlist = Renderer.GetDlist(limb.DListSeg);
            }
            catch (Exception ex)
            {
                if (DecodeError == null)
                    DecodeError =
                        $"Error while decoding dlist 0x{limb.DListSeg.VAddr:X8} : {ex.Message}";
            }
            if (dlist != null)
                limbsDLists[i] = dlist;
        }

        if (DecodeError == null)
            LimbsDLists = limbsDLists;
        else
            LimbsDLists = null;
    }

    public void SetAnimations(IEnumerable<Z64Object.AnimationHolder> animationHolders)
    {
        ObservableCollection<IAnimationEntry> newAnimations = new(
            animationHolders.Select(animationHolder => new RegularAnimationEntry(
                this,
                animationHolder.Name,
                animationHolder
            ))
        );
        AnimationEntries = newAnimations;
    }

    [MemberNotNull(nameof(CurPose))]
    void SetIdentityPose()
    {
        Utils.Assert(Skel != null);
        CurPose = new Matrix4[Skel.Limbs.Count];
        for (int i = 0; i < CurPose.Length; i++)
        {
            CurPose[i] = Matrix4.Identity;
        }
    }

    void UpdateCurPose()
    {
        if (CurAnim == null && CurPlayerAnim == null)
        {
            SetIdentityPose();
        }
        else
        {
            Utils.Assert(Skel != null);
            if (CurAnim != null)
            {
                CurPose = Z64SkeletonPose.Get(Skel, CurAnim, CurFrame).LimbsPose;
            }
            else
            {
                Utils.Assert(CurPlayerAnim != null);
                CurPose = Z64SkeletonPose.Get(Skel, CurPlayerAnim, CurFrame).LimbsPose;
            }
        }

        if (Skel is Z64FlexSkeleton flexSkeleton)
        {
            Utils.Assert(Renderer != null);

            byte[] mtxBuff = new byte[flexSkeleton.DListCount * Mtx.SIZE];

            using (MemoryStream ms = new MemoryStream(mtxBuff))
            {
                BinaryStream bw = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);

                flexSkeleton.Root.Visit(index =>
                {
                    var seg = flexSkeleton.Limbs[index].DListSeg;
                    Utils.Assert(seg != null);
                    if (seg.VAddr != 0)
                        Mtx.FromMatrix4(CurPose[index]).Write(bw);
                });
            }

            Renderer.Memory.Segments[0xD] = F3DZEX.Memory.Segment.FromBytes(
                "[RESERVED] Anim Matrices",
                mtxBuff
            );
        }
    }

    public void UpdateDisplayElements()
    {
        DisplayElements.Clear();

        Utils.Assert(Skel != null);
        Utils.Assert(LimbsDLists != null);
        Utils.Assert(CurPose != null);

        Skel.Root.Visit(index =>
        {
            var dl = LimbsDLists[index];
            if (dl != null)
                DisplayElements.Add(
                    new DLViewerControlDlistWithMatrixDisplayElement(
                        dl,
                        SelectedLimbNodes.Any(n => n.LimbIndex == index),
                        CurPose[index]
                    )
                );
        });
    }

    public void PlayAnimBackwardsCommand()
    {
        IsPlayingBackwards = !IsPlayingBackwards;
        IsPlayingForwards = false;
        _playAnimTimer.IsEnabled = IsPlayingBackwards;
    }

    public void PlayAnimForwardsCommand()
    {
        IsPlayingBackwards = false;
        IsPlayingForwards = !IsPlayingForwards;
        _playAnimTimer.IsEnabled = IsPlayingForwards;
    }

    private void OnPlayAnimTimerTick(object? sender, EventArgs e)
    {
        CurFrame = (CurFrame + (IsPlayingForwards ? 1 : ((MaxFrame + 1) - 1))) % (MaxFrame + 1);
    }

    public void OnAnimationEntrySelected(IAnimationEntry animationEntry)
    {
        Utils.Assert(Renderer != null);
        Utils.Assert(Skel != null);

        AnimationError = null;
        CurAnim = null;
        CurPlayerAnim = null;

        var savedSegmentData = new Dictionary<int, F3DZEX.Memory.Segment>();
        if (animationEntry is IExternalAnimationEntry externalAnimationEntry)
        {
            foreach (var item in externalAnimationEntry.ExternalData)
            {
                savedSegmentData[item.Key] = Renderer.Memory.Segments[item.Key];
                Renderer.Memory.Segments[item.Key] = item.Value;
            }
        }

        if (animationEntry is RegularAnimationEntry regularAnimationEntry)
        {
            try
            {
                CurAnim = Z64Animation.Get(
                    Renderer.Memory,
                    regularAnimationEntry.AnimationHolder,
                    Skel.Limbs.Count
                );
            }
            catch (Exception e)
            {
                Logger.Error(e);
                AnimationError =
                    "Animation is glitchy; displaying folded pose. To view this animation, load it in-game.";
            }
        }
        else
        {
            var playerAnimationEntry = animationEntry as PlayerAnimationEntry;
            Utils.Assert(playerAnimationEntry != null);
            CurPlayerAnim = Z64PlayerAnimation.Get(
                Renderer.Memory,
                playerAnimationEntry.PlayerAnimationHolder
            );
        }

        foreach (var item in savedSegmentData)
        {
            Renderer.Memory.Segments[item.Key] = item.Value;
        }
    }
}

public class SkeletonViewerLimbNode
{
    public int LimbIndex { get; }
    public string Name { get; }
    public ObservableCollection<SkeletonViewerLimbNode> ChildrenLimbs { get; }

    public SkeletonViewerLimbNode(
        int limbIndex,
        string name,
        IEnumerable<SkeletonViewerLimbNode>? childrenLimbs = null
    )
    {
        LimbIndex = limbIndex;
        Name = name;
        ChildrenLimbs = childrenLimbs == null ? new() : new(childrenLimbs);
    }
}
