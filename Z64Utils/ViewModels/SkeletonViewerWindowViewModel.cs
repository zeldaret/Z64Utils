using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Mathematics;
using RDP;
using Syroot.BinaryData;
using Z64;

namespace Z64Utils_Avalonia;

// TODO this is 100% copypaste from DListViewerWindowVM for now, adapt, refactor.
// also cleanup asserts
public partial class SkeletonViewerWindowViewModel : ObservableObject
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    public F3DZEX.Render.Renderer? _renderer;

    [ObservableProperty]
    private ObservableCollection<IDLViewerControlDisplayElement> _displayElements = new();

    [ObservableProperty]
    private string? _decodeError;

    [ObservableProperty]
    private string? _renderError;

    [ObservableProperty]
    private int _maxFrame = 0;

    [ObservableProperty]
    private int _curFrame = 0;

    [ObservableProperty]
    private ObservableCollection<SkeletonViewerLimbNode> _skeletonRootLimbNode = new();

    public class AnimationEntry
    {
        private SkeletonViewerWindowViewModel _parentVM;
        public string Name { get; }
        public Z64Object.AnimationHolder AnimationHolder { get; }

        public AnimationEntry(
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

    [ObservableProperty]
    private ObservableCollection<AnimationEntry> _animationEntries = new();

    [ObservableProperty]
    private double _playAnimTickPeriodMs;
    private DispatcherTimer _playAnimTimer = new();
    private bool _playAnimForwards;

    [ObservableProperty]
    Z64Skeleton? _skel;

    [ObservableProperty]
    Z64Animation? _curAnim;

    [ObservableProperty]
    F3DZEX.Command.Dlist?[]? _limbsDLists;

    [ObservableProperty]
    Matrix4[]? _curPose;

    public SkeletonViewerWindowViewModel()
    {
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
            switch (e.PropertyName)
            {
                case nameof(Renderer):
                    if (Renderer != null)
                        Renderer.PropertyChanged += OnRendererPropertyChanged;
                    break;
                case nameof(Skel):
                    CurAnim = null;
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
                    if (CurAnim != null)
                        MaxFrame = CurAnim.FrameCount - 1;
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
        // TODO
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
            var node = new SkeletonViewerLimbNode(limbHolder.Name);
            parent.ChildrenLimbs.Add(node);

            if (treeLimb.Sibling != null)
                VisitLimb(parent, treeLimb.Sibling);
            if (treeLimb.Child != null)
                VisitLimb(node, treeLimb.Child);
        }

        var skeletonRootLimbNode = new SkeletonViewerLimbNode(Skel.Limbs[0].Name);
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
        ObservableCollection<AnimationEntry> newAnimations = new(
            animationHolders.Select(animationHolder => new AnimationEntry(
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
        if (CurAnim == null)
        {
            SetIdentityPose();
        }
        else
        {
            Utils.Assert(Skel != null);
            CurPose = Z64SkeletonPose.Get(Skel, CurAnim, CurFrame).LimbsPose;
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
                    new DLViewerControlDlistWithMatrixDisplayElement(dl, CurPose[index])
                );
        });
    }

    public void PlayAnimBackwardsCommand()
    {
        if (!_playAnimForwards)
        {
            _playAnimTimer.IsEnabled = !_playAnimTimer.IsEnabled;
        }
        else
        {
            _playAnimForwards = false;
            _playAnimTimer.IsEnabled = true;
        }
    }

    public void PlayAnimForwardsCommand()
    {
        if (_playAnimForwards)
        {
            _playAnimTimer.IsEnabled = !_playAnimTimer.IsEnabled;
        }
        else
        {
            _playAnimForwards = true;
            _playAnimTimer.IsEnabled = true;
        }
    }

    private void OnPlayAnimTimerTick(object? sender, EventArgs e)
    {
        CurFrame = (CurFrame + (_playAnimForwards ? 1 : ((MaxFrame + 1) - 1))) % (MaxFrame + 1);
    }

    public void OnAnimationEntrySelected(AnimationEntry animationEntry)
    {
        Utils.Assert(Renderer != null);
        Utils.Assert(Skel != null);

        CurAnim = Z64Animation.Get(
            Renderer.Memory,
            animationEntry.AnimationHolder,
            Skel.Limbs.Count
        );
    }
}

public class SkeletonViewerLimbNode
{
    public string Name { get; }
    public ObservableCollection<SkeletonViewerLimbNode> ChildrenLimbs { get; }

    public SkeletonViewerLimbNode(
        string name,
        IEnumerable<SkeletonViewerLimbNode>? childrenLimbs = null
    )
    {
        Name = name;
        ChildrenLimbs = childrenLimbs == null ? new() : new(childrenLimbs);
    }
}
