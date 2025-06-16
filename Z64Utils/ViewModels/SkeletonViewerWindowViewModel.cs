using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Threading;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using F3DZEX.Command;
using F3DZEX.Render;
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
        public string Name { get; }
        public Z64.Z64Object.AnimationHolder AnimationHolder { get; }

        public AnimationEntry(string name, Z64.Z64Object.AnimationHolder animationHolder)
        {
            Name = name;
            AnimationHolder = animationHolder;
        }
    }

    [ObservableProperty]
    private ObservableCollection<AnimationEntry> _animationEntries = new();

    [ObservableProperty]
    private double _playAnimTickPeriodMs;
    private DispatcherTimer _playAnimTimer = new();
    private bool _playAnimForwards;

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
                case nameof(CurFrame):
                    Utils.Assert(CurFrame >= 0 && CurFrame <= MaxFrame);
                    UpdateDisplayElements();
                    break;
                case nameof(PlayAnimTickPeriodMs):
                    Debug.WriteLine("PlayAnimTickPeriodMs changed");
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

            // TODO redecode dlist, rerender
        }
    }

    public void OpenRenderSettingsCommand()
    {
        // TODO
    }

    Z64Object.SkeletonHolder? _bruhwip_skel;
    Z64Object.SkeletonLimbHolder[]? _bruhwip_limbs;
    Z64Object.AnimationHolder? _bruhwip_anim;
    Z64Object.AnimationFrameDataHolder? _bruhwip_animFrameData;
    Z64Object.AnimationJointIndicesHolder? _bruhwip_animJointIndices;

    public void SetSkeleton(Z64.Z64Object.SkeletonHolder skeletonHolder)
    {
        // TODO most of this should absolutely be in Model

        Utils.Assert(Renderer != null);

        Z64Object.SkeletonLimbHolder[] limbHolders;

        {
            byte[] limbsData = Renderer.Memory.ReadBytes(
                skeletonHolder.LimbsSeg,
                skeletonHolder.LimbCount * 4
            );
            var limbsHolder = new Z64.Z64Object.SkeletonLimbsHolder("limbs", limbsData);

            limbHolders = new Z64Object.SkeletonLimbHolder[limbsHolder.LimbSegments.Length];

            for (int i = 0; i < limbsHolder.LimbSegments.Length; i++)
            {
                byte[] limbData = Renderer.Memory.ReadBytes(
                    limbsHolder.LimbSegments[i],
                    Z64.Z64Object.SkeletonLimbHolder.STANDARD_LIMB_SIZE
                );
                var limbHolder = new Z64.Z64Object.SkeletonLimbHolder(
                    $"limb_{i}",
                    limbData,
                    Z64.Z64Object.EntryType.StandardLimb
                ); // TODO support other limb types

                limbHolders[i] = limbHolder;
            }
        }

        void AddLimbAndSiblingsNodes(int i, List<SkeletonViewerLimbNode> list)
        {
            const byte LIMB_NONE = 0xFF;

            var limbHolder = limbHolders[i];

            List<SkeletonViewerLimbNode> children = new();
            if (limbHolder.Child != LIMB_NONE)
            {
                AddLimbAndSiblingsNodes(limbHolder.Child, children);
            }
            list.Add(new SkeletonViewerLimbNode(limbHolder.Name, children));
            if (limbHolder.Sibling != LIMB_NONE)
            {
                AddLimbAndSiblingsNodes(limbHolder.Sibling, list);
            }
        }

        List<SkeletonViewerLimbNode> root = new();
        AddLimbAndSiblingsNodes(0, root);
        SkeletonRootLimbNode = new() { root.Single() };

        _bruhwip_skel = skeletonHolder;
        _bruhwip_limbs = limbHolders;
    }

    public void SetAnimations(IEnumerable<Z64.Z64Object.AnimationHolder> animationHolders)
    {
        ObservableCollection<AnimationEntry> newAnimations = new(
            animationHolders.Select(animationHolder => new AnimationEntry(
                animationHolder.Name,
                animationHolder
            ))
        );
        AnimationEntries = newAnimations;
    }

    // TODO cleanup
    public void UpdateDisplayElements()
    {
        DisplayElements.Clear();

        Utils.Assert(_bruhwip_skel != null);
        Utils.Assert(_bruhwip_limbs != null);
        Utils.Assert(_bruhwip_anim != null);
        Utils.Assert(_bruhwip_animFrameData != null);
        Utils.Assert(_bruhwip_animJointIndices != null);
        Utils.Assert(Renderer != null);

        var jointTable = ComputeJointTable(
            _bruhwip_skel,
            _bruhwip_anim,
            _bruhwip_animFrameData,
            _bruhwip_animJointIndices,
            CurFrame
        );
        var matrices = ComputeMatricesFromJointTable(_bruhwip_skel, _bruhwip_limbs, jointTable);

        foreach (var i in Enumerable.Range(0, _bruhwip_skel.LimbCount))
        {
            var vaddr = _bruhwip_limbs[i].DListSeg;
            if (vaddr == null || vaddr == 0)
            {
                continue;
            }

            Dlist dList;
            try
            {
                dList = Renderer.GetDlist(vaddr);
            }
            catch (Exception e)
            {
                DecodeError = $"Could not decode DL 0x{vaddr:X8} (limb {i}): {e.Message}";
                break;
            }
            DisplayElements.Add(
                new DLViewerControlDlistWithMatrixDisplayElement(dList, matrices[i])
            );
        }
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
        MaxFrame = animationEntry.AnimationHolder.FrameCount - 1;

        Utils.Assert(Renderer != null);
        Utils.Assert(_bruhwip_skel != null);

        // adapted from SkeletonViewerForm.UpdateAnim
        _bruhwip_anim = animationEntry.AnimationHolder;
        _bruhwip_animJointIndices = new Z64Object.AnimationJointIndicesHolder(
            "jointIndices",
            Renderer.Memory.ReadBytes(
                _bruhwip_anim.JointIndices,
                (1 + _bruhwip_skel.LimbCount) * Z64Object.AnimationJointIndicesHolder.ENTRY_SIZE
            )
        );
        int max = 0;
        foreach (var joint in _bruhwip_animJointIndices.JointIndices)
        {
            max = Math.Max(max, joint.X);
            max = Math.Max(max, joint.Y);
            max = Math.Max(max, joint.Z);
        }
        int bytesToRead =
            (max < _bruhwip_anim.StaticIndexMax ? max + 1 : _bruhwip_anim.FrameCount + max) * 2;
        _bruhwip_animFrameData = new Z64Object.AnimationFrameDataHolder(
            "frameData",
            Renderer.Memory.ReadBytes(_bruhwip_anim.FrameData, bytesToRead)
        );

        UpdateDisplayElements();
    }

    // should go in model
    public Vector3i[] ComputeJointTable(
        Z64Object.SkeletonHolder skel,
        Z64Object.AnimationHolder anim,
        Z64Object.AnimationFrameDataHolder animFrameData,
        Z64Object.AnimationJointIndicesHolder animJointIndices,
        int frameIndex
    )
    {
        short GetFrameData(int frameDataIdx)
        {
            return animFrameData.FrameData[
                frameDataIdx < anim.StaticIndexMax ? frameDataIdx : frameDataIdx + frameIndex
            ];
        }
        Vector3i[] jointTable = new Vector3i[1 + skel.LimbCount];
        foreach (var i in Enumerable.Range(0, jointTable.Length))
        {
            jointTable[i].X = GetFrameData(animJointIndices.JointIndices[i].X);
            jointTable[i].Y = GetFrameData(animJointIndices.JointIndices[i].Y);
            jointTable[i].Z = GetFrameData(animJointIndices.JointIndices[i].Z);
        }

        return jointTable;
    }

    public Matrix4[] ComputeMatricesFromJointTable(
        Z64Object.SkeletonHolder skel,
        Z64Object.SkeletonLimbHolder[] skelLimbsArray,
        Vector3i[] jointTable
    )
    {
        float S16ToRad(int x) => x * (float)Math.PI / 0x8000;

        Stack<Matrix4> matrixStack = new();
        Matrix4[] matricesByLimb = new Matrix4[skel.LimbCount];
        List<Matrix4> matrixBufferForFlex = new();
        void ProcessLimb(int limbIdx)
        {
            var pos = new Vector3(
                skelLimbsArray[limbIdx].JointX,
                skelLimbsArray[limbIdx].JointY,
                skelLimbsArray[limbIdx].JointZ
            );
            Vector3i rotS = jointTable[1 + limbIdx];
            // idk where this is documented but OpenTK matrices transform row vectors multiplied to the left of the matrix
            Matrix4 limbMtx =
                Matrix4.CreateRotationX(S16ToRad(rotS.X))
                * Matrix4.CreateRotationY(S16ToRad(rotS.Y))
                * Matrix4.CreateRotationZ(S16ToRad(rotS.Z))
                * Matrix4.CreateTranslation(pos)
                * matrixStack.Peek();

            matricesByLimb[limbIdx] = limbMtx;
            var seg = skelLimbsArray[limbIdx].DListSeg;
            Utils.Assert(seg != null);
            if (seg.VAddr != 0)
                matrixBufferForFlex.Add(limbMtx);

            matrixStack.Push(limbMtx);

            if (skelLimbsArray[limbIdx].Child != 0xFF)
                ProcessLimb(skelLimbsArray[limbIdx].Child);

            matrixStack.Pop();

            if (skelLimbsArray[limbIdx].Sibling != 0xFF)
                ProcessLimb(skelLimbsArray[limbIdx].Sibling);
        }

        Vector3i rootPos = jointTable[0];
        Vector3i rootRotS = jointTable[1];
        Matrix4 rootLimbMtx =
            Matrix4.CreateRotationX(S16ToRad(rootRotS.X))
            * Matrix4.CreateRotationY(S16ToRad(rootRotS.Y))
            * Matrix4.CreateRotationZ(S16ToRad(rootRotS.Z))
            * Matrix4.CreateTranslation(rootPos);

        matricesByLimb[0] = rootLimbMtx;
        var seg = skelLimbsArray[0].DListSeg;
        Utils.Assert(seg != null);
        if (seg.VAddr != 0)
            matrixBufferForFlex.Add(rootLimbMtx);

        matrixStack.Push(rootLimbMtx);

        if (skelLimbsArray[0].Child != 0xFF)
            ProcessLimb(skelLimbsArray[0].Child);

        matrixStack.Pop();

        var mtxBufForFlexBytes = new byte[matrixBufferForFlex.Count * Mtx.SIZE];
        using (
            BinaryStream bw = new(
                new MemoryStream(mtxBufForFlexBytes),
                Syroot.BinaryData.ByteConverter.Big
            )
        )
        {
            foreach (var mtx in matrixBufferForFlex)
            {
                Mtx.FromMatrix4(mtx).Write(bw);
            }
        }

        Utils.Assert(Renderer != null);
        Renderer.Memory.Segments[0xD] = F3DZEX.Memory.Segment.FromBytes("flex", mtxBufForFlexBytes);

        return matricesByLimb;
    }
    //
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
