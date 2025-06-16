using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RDP;
using Syroot.BinaryData;
using static Z64.Z64Object;

namespace Z64.Forms
{
    public partial class SkeletonViewerForm : MicrosoftFontForm
    {
        enum PlayState
        {
            Pause,
            Forward,
            Backward,
        }

        interface ISkeletonViewerAnimationEntry
        {
            string GetName();
        }

        interface ISkeletonViewerRegularAnimationEntry : ISkeletonViewerAnimationEntry
        {
            AnimationHolder GetAnim();
        }

        interface ISkeletonViewerPlayerAnimationEntry : ISkeletonViewerAnimationEntry
        {
            PlayerAnimationHolder GetPlayerAnim();
        }

        interface ISkeletonViewerExternalAnimationEntry : ISkeletonViewerAnimationEntry
        {
            int GetExternalSegment();
            F3DZEX.Memory.Segment GetExternalData();
        }

        class SkeletonViewerRegularAnimationEntry : ISkeletonViewerRegularAnimationEntry
        {
            private AnimationHolder _anim;

            public SkeletonViewerRegularAnimationEntry(AnimationHolder anim)
            {
                _anim = anim;
            }

            public string GetName()
            {
                return _anim.Name;
            }

            public AnimationHolder GetAnim()
            {
                return _anim;
            }
        }

        class SkeletonViewerPlayerAnimationEntry : ISkeletonViewerPlayerAnimationEntry
        {
            private PlayerAnimationHolder _playerAnim;

            public SkeletonViewerPlayerAnimationEntry(PlayerAnimationHolder playerAnim)
            {
                _playerAnim = playerAnim;
            }

            public string GetName()
            {
                return _playerAnim.Name;
            }

            public PlayerAnimationHolder GetPlayerAnim()
            {
                return _playerAnim;
            }
        }

        class SkeletonViewerExternalRegularAnimationEntry
            : SkeletonViewerRegularAnimationEntry,
                ISkeletonViewerExternalAnimationEntry
        {
            private int _segment;
            private F3DZEX.Memory.Segment _data;

            public SkeletonViewerExternalRegularAnimationEntry(
                AnimationHolder anim,
                int segment,
                F3DZEX.Memory.Segment data
            )
                : base(anim)
            {
                _segment = segment;
                _data = data;
            }

            public int GetExternalSegment()
            {
                return _segment;
            }

            public F3DZEX.Memory.Segment GetExternalData()
            {
                return _data;
            }
        }

        class SkeletonViewerExternalPlayerAnimationEntry
            : SkeletonViewerPlayerAnimationEntry,
                ISkeletonViewerExternalAnimationEntry
        {
            private int _segment;
            private F3DZEX.Memory.Segment _data;

            public SkeletonViewerExternalPlayerAnimationEntry(
                PlayerAnimationHolder playerAnim,
                int segment,
                F3DZEX.Memory.Segment data
            )
                : base(playerAnim)
            {
                _segment = segment;
                _data = data;
            }

            public int GetExternalSegment()
            {
                return _segment;
            }

            public F3DZEX.Memory.Segment GetExternalData()
            {
                return _data;
            }
        }

        bool _formClosing = false;
        System.Timers.Timer _timer;
        PlayState _playState;
        string? _dlistError = null;

        Z64Game? _game;
        F3DZEX.Render.Renderer _renderer;
        SegmentEditorForm? _segForm;
        DisasmForm? _disasForm;
        SettingsForm? _settingsForm;
        F3DZEX.Render.Renderer.Config _rendererCfg;

        int _curSegment;
        Z64Skeleton _skeleton;
        List<ISkeletonViewerAnimationEntry> _animations;
        List<F3DZEX.Command.Dlist?> _limbDlists;
        bool[] _limbDlistRenderFlags;

        Z64Animation? _curRegularAnim;
        Z64PlayerAnimation? _curPlayerAnim;
        Matrix4[] _curPose;
        int _curPoseFrame;

        string? _animationError;

        public SkeletonViewerForm(
            Z64Game? game,
            int curSegment,
            F3DZEX.Memory.Segment curSegmentData,
            SkeletonHolder skel,
            List<AnimationHolder> anims
        )
        {
            _game = game;
            _curSegment = curSegment;
            _rendererCfg = new F3DZEX.Render.Renderer.Config();

            InitializeComponent();

            _renderer = new F3DZEX.Render.Renderer(game, _rendererCfg);
            modelViewer.RenderCallback = RenderCallback;

            _timer = new System.Timers.Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = (int)numUpDown_playbackSpeed.Value;

            if ((Control.ModifierKeys & Keys.Control) == 0)
            {
                if (game != null)
                {
                    var gameplay_keepFile = game.GetFileByName("gameplay_keep");
                    if (gameplay_keepFile == null || !gameplay_keepFile.Valid())
                        MessageBox.Show(
                            "Could not find valid gameplay_keep file for setting segment 4"
                        );
                    else
                        _renderer.Memory.Segments[4] = F3DZEX.Memory.Segment.FromBytes(
                            "gameplay_keep",
                            gameplay_keepFile.Data
                        );
                }
                for (int i = 8; i < 16; i++)
                {
                    _renderer.Memory.Segments[i] = F3DZEX.Memory.Segment.FromFill(
                        "Empty Dlist",
                        new byte[] { 0xDF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
                    );
                }
            }

            NewRender();

            FormClosing += (s, e) =>
            {
                if (_timer.Enabled && !_formClosing)
                {
                    _formClosing = true;
                    e.Cancel = true;
                }
            };
            _playState = PlayState.Pause;

            SetSegment(curSegment, curSegmentData);

            if (skel is FlexSkeletonHolder flexSkel)
                _skeleton = Z64FlexSkeleton.Get(_renderer.Memory, flexSkel);
            else
                _skeleton = Z64Skeleton.Get(_renderer.Memory, skel);

            _animations = new();
            anims.ForEach(ah => _animations.Add(new SkeletonViewerRegularAnimationEntry(ah)));

            listBox_anims.Items.Clear();
            _animations.ForEach(a => listBox_anims.Items.Add(a.GetName()));

            SetIdentityPose();
            _curPoseFrame = -1;

            UpdateSkeleton();
            NewRender();
        }

        [MemberNotNull(nameof(_curPose))]
        void SetIdentityPose()
        {
            _curPose = new Matrix4[_skeleton.Limbs.Count];
            for (int i = 0; i < _curPose.Length; i++)
            {
                _curPose[i] = Matrix4.Identity;
            }
        }

        void UpdateCurPose()
        {
            if (_curRegularAnim == null && _curPlayerAnim == null)
            {
                SetIdentityPose();
            }
            else
            {
                if (_curPoseFrame != trackBar_anim.Value)
                {
                    if (_curRegularAnim != null)
                    {
                        _curPose = Z64SkeletonPose
                            .Get(_skeleton, _curRegularAnim, trackBar_anim.Value)
                            .LimbsPose;
                    }
                    else
                    {
                        Utils.Assert(_curPlayerAnim != null);
                        _curPose = Z64SkeletonPose
                            .Get(_skeleton, _curPlayerAnim, trackBar_anim.Value)
                            .LimbsPose;
                    }
                    _curPoseFrame = trackBar_anim.Value;
                }
            }
        }

        void RenderCallback(Matrix4 proj, Matrix4 view)
        {
            if (_dlistError != null)
            {
                toolStripErrorLabel.Text = _dlistError;
                return;
            }

            _renderer.RenderStart(proj, view);
            UpdateCurPose();
            _skeleton.Root.Visit(index =>
            {
                _renderer.RdpMtxStack.Load(_curPose[index]);

                var node = treeView_hierarchy.SelectedNode;
                _renderer.SetHightlightEnabled(node?.Tag?.Equals(_skeleton.Limbs[index]) ?? false);

                var dl = _limbDlists[index];
                if (dl != null && _limbDlistRenderFlags[index])
                    _renderer.RenderDList(dl);
            });

            if (_renderer.RenderFailed())
            {
                toolStripErrorLabel.Text =
                    $"RENDER ERROR AT 0x{_renderer.RenderErrorAddr:X8}! ({_renderer.ErrorMsg})";
            }
            else if (!string.IsNullOrEmpty(_animationError))
            {
                toolStripErrorLabel.Text = _animationError;
            }
            else
            {
                toolStripErrorLabel.Text = "";
            }
        }

        private void TreeView_hierarchy_AfterSelect(object sender, EventArgs e)
        {
            var tag = treeView_hierarchy.SelectedNode?.Tag ?? null;
            if (tag != null && tag is SkeletonLimbHolder)
            {
                var dlist = _limbDlists[_skeleton.Limbs.IndexOf((SkeletonLimbHolder)tag)];
                if (dlist != null)
                    _disasForm?.UpdateDlist(dlist);
                else
                    _disasForm?.SetMessage("Empty limb");
            }

            NewRender();
        }

        private void treeView_hierarchy_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var tag = e.Node.Tag ?? null;
                if (tag != null && tag is SkeletonLimbHolder)
                {
                    var index = _skeleton.Limbs.IndexOf((SkeletonLimbHolder)tag);
                    _limbDlistRenderFlags[index] = !_limbDlistRenderFlags[index];
                    if (!_limbDlistRenderFlags[index])
                        e.Node.ForeColor = Color.Gray;
                    else
                        e.Node.ForeColor = Color.Black;

                    treeView_hierarchy.SelectedNode = e.Node;
                }

                NewRender();
            }
        }

        private void NewRender(object? sender = null, EventArgs? e = null)
        {
            _renderer.ClearErrors();
            toolStripErrorLabel.Text = "";
            modelViewer.Render();
        }

        [MemberNotNull(nameof(_limbDlists))]
        void UpdateLimbsDlists()
        {
            _dlistError = null;
            _limbDlists = new();

            foreach (var limb in _skeleton.Limbs)
            {
                if (limb.Type != EntryType.StandardLimb && limb.Type != EntryType.LODLimb)
                    throw new Exception($"Unimplemented limb type in skeleton viewer {limb.Type}");
                Utils.Assert(limb.DListSeg != null); // always set for Standard and LOD limbs
                F3DZEX.Command.Dlist? dlist = null;
                try
                {
                    if (limb.DListSeg.VAddr != 0)
                        dlist = _renderer.GetDlist(limb.DListSeg);
                }
                catch (Exception ex)
                {
                    if (_dlistError == null)
                        _dlistError =
                            $"Error while decoding dlist 0x{limb.DListSeg.VAddr:X8} : {ex.Message}";
                }
                _limbDlists.Add(dlist);
            }
        }

        // Updates skeleton -> limbs / limbs dlists -> matrices
        [MemberNotNull(nameof(_limbDlistRenderFlags))]
        [MemberNotNull(nameof(_limbDlists))]
        void UpdateSkeleton()
        {
            treeView_hierarchy.Nodes.Clear();
            treeView_hierarchy.Nodes.Add("skeleton");

            _limbDlistRenderFlags = new bool[_skeleton.Limbs.Count];
            for (int i = 0; i < _limbDlistRenderFlags.Length; i++)
            {
                _limbDlistRenderFlags[i] = true;
            }

            UpdateLimbsDlists();
            UpdateLimbs();
        }

        // Updates limbs -> matrices
        void UpdateLimbs()
        {
            TreeNode skelNode = treeView_hierarchy.Nodes[0];

            AddLimbRoutine(skelNode, _skeleton.Root);

            UpdateMatrixBuf();
        }

        void AddLimbRoutine(TreeNode parent, Z64SkeletonTreeLimb treeLimb)
        {
            var node = parent.Nodes.Add($"limb_{treeLimb.Index}");
            node.Tag = _skeleton.Limbs[treeLimb.Index];

            if (treeLimb.Sibling != null)
                AddLimbRoutine(parent, treeLimb.Sibling);
            if (treeLimb.Child != null)
                AddLimbRoutine(node, treeLimb.Child);
        }

        // Update anims -> matrices
        void UpdateRegularAnim()
        {
            Utils.Assert(_curRegularAnim != null);

            trackBar_anim.Minimum = 0;
            trackBar_anim.Maximum = _curRegularAnim.FrameCount - 1;
            trackBar_anim.Value = 0;

            UpdateMatrixBuf();
        }

        void UpdatePlayerAnim()
        {
            Utils.Assert(_curPlayerAnim != null);

            trackBar_anim.Minimum = 0;
            trackBar_anim.Maximum = _curPlayerAnim.FrameCount - 1;
            trackBar_anim.Value = 0;

            UpdateMatrixBuf();
        }

        // Flex Only
        void UpdateMatrixBuf()
        {
            if (!(_skeleton is Z64FlexSkeleton flexSkeleton))
                return;

            UpdateCurPose();

            byte[] mtxBuff = new byte[flexSkeleton.DListCount * Mtx.SIZE];

            using (MemoryStream ms = new MemoryStream(mtxBuff))
            {
                BinaryStream bw = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);

                _skeleton.Root.Visit(index =>
                {
                    if (_limbDlists[index] != null)
                        Mtx.FromMatrix4(_curPose[index]).Write(bw);
                });
            }

            _renderer.Memory.Segments[0xD] = F3DZEX.Memory.Segment.FromBytes(
                "[RESERVED] Anim Matrices",
                mtxBuff
            );
        }

        private void ToolStripRenderCfgBtn_Click(object sender, System.EventArgs e)
        {
            if (_settingsForm != null)
            {
                _settingsForm.Activate();
            }
            else
            {
                _settingsForm = new SettingsForm(_rendererCfg);
                _settingsForm.FormClosed += (sender, e) =>
                {
                    _settingsForm = null;
                };
                _settingsForm.SettingsChanged += NewRender;
                _settingsForm.Show();
            }
        }

        private void ToolStripDisassemblyBtn_Click(object sender, System.EventArgs e)
        {
            if (_disasForm != null)
            {
                _disasForm.Activate();
            }
            else
            {
                _disasForm = new DisasmForm(defaultText: "No limb selected");

                _disasForm.FormClosed += (sender, e) => _disasForm = null;
                _disasForm.Show();
            }

            var tag = treeView_hierarchy.SelectedNode?.Tag ?? null;
            if (tag != null && tag is SkeletonLimbHolder)
            {
                var dlist = _limbDlists[_skeleton.Limbs.IndexOf((SkeletonLimbHolder)tag)];
                _disasForm.UpdateDlist(dlist);
            }
        }

        private void SkeletonViewerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _disasForm?.Close();
            _segForm?.Close();
            _settingsForm?.Close();
        }

        public void SetSegment(int idx, F3DZEX.Memory.Segment seg)
        {
            if (idx < 0 || idx > F3DZEX.Memory.Segment.COUNT)
                throw new IndexOutOfRangeException();

            _renderer.Memory.Segments[idx] = seg;

            // _skeleton can be null when SetSegment is called from the constructor
            if (_skeleton != null)
                UpdateLimbsDlists();
        }

        private void ToolStripSegmentsBtn_Click(object sender, System.EventArgs e)
        {
            if (_segForm != null)
            {
                _segForm.Activate();
            }
            else
            {
                _segForm = new SegmentEditorForm(_game, _renderer);
                _segForm.SegmentsChanged += (sender, e) =>
                {
                    if (e.SegmentID == 0xD && _skeleton is Z64FlexSkeleton)
                        MessageBox.Show(
                            "Error",
                            "Cannot set segment 13 (reserved for animation matrices)"
                        );
                    else
                    {
                        _renderer.Memory.Segments[e.SegmentID] = e.Segment;

                        UpdateLimbsDlists();
                        NewRender();
                    }
                };
                _segForm.FormClosed += (sender, e) => _segForm = null;
                _segForm.Show();
            }
        }

        private void listBox_anims_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_playAnim.Enabled =
                button_playbackAnim.Enabled =
                trackBar_anim.Enabled =
                    listBox_anims.SelectedIndex >= 0;

            _curRegularAnim = null;
            _curPlayerAnim = null;
            _curPoseFrame = -1; // Recompute the pose on animation change
            if (listBox_anims.SelectedIndex >= 0)
            {
                var curAnimEntry = _animations[listBox_anims.SelectedIndex];
                if (curAnimEntry is ISkeletonViewerRegularAnimationEntry curRegularAnimationEntry)
                {
                    int? savedSegment = null;
                    F3DZEX.Memory.Segment? savedData = null;

                    if (curAnimEntry is ISkeletonViewerExternalAnimationEntry curExternalAnimation)
                    {
                        savedSegment = curExternalAnimation.GetExternalSegment();
                        savedData = _renderer.Memory.Segments[
                            curExternalAnimation.GetExternalSegment()
                        ];

                        _renderer.Memory.Segments[curExternalAnimation.GetExternalSegment()] =
                            curExternalAnimation.GetExternalData();
                    }

                    try
                    {
                        _curRegularAnim = Z64Animation.Get(
                            _renderer.Memory,
                            curRegularAnimationEntry.GetAnim(),
                            _skeleton.Limbs.Count
                        );
                    }
                    catch (Exception)
                    {
                        _animationError =
                            "Animation is glitchy; displaying folded pose. To view this animation, load it in-game.";
                    }

                    if (savedSegment != null)
                    {
                        Utils.Assert(savedData != null);
                        _renderer.Memory.Segments[(int)savedSegment] = savedData;
                    }

                    if (_curRegularAnim != null)
                        UpdateRegularAnim();
                }
                else
                {
                    var curPlayerAnimationEntry =
                        curAnimEntry as ISkeletonViewerPlayerAnimationEntry;
                    Utils.Assert(curPlayerAnimationEntry != null);

                    var curPAH = curPlayerAnimationEntry.GetPlayerAnim();

                    if (_game == null)
                        return;

                    var Saved = _renderer.Memory.Segments[curPAH.PlayerAnimationSegment.SegmentId];
                    var link_animetionFile = _game.GetFileByName("link_animetion");
                    Utils.Assert(link_animetionFile != null);
                    Utils.Assert(link_animetionFile.Valid());
                    _renderer.Memory.Segments[curPAH.PlayerAnimationSegment.SegmentId] =
                        F3DZEX.Memory.Segment.FromBytes("link_animetion", link_animetionFile.Data);

                    _curPlayerAnim = Z64PlayerAnimation.Get(_renderer.Memory, curPAH);

                    _renderer.Memory.Segments[curPAH.PlayerAnimationSegment.SegmentId] = Saved;

                    UpdatePlayerAnim();
                }

                NewRender();
            }

            //if (_playState != PlayState.Pause)
            //    _timer.Start()
        }

        private void trackBar_anim_ValueChanged(object sender, EventArgs e)
        {
            label_anim.Text = $"{trackBar_anim.Value}/{trackBar_anim.Maximum}";
            UpdateMatrixBuf();
            NewRender();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.IsDisposed || _formClosing)
            {
                _timer.Stop();
                Invoke(new Action(Close));
                return;
            }

            Invoke(
                new Action(() =>
                {
                    if (_playState == PlayState.Forward)
                    {
                        trackBar_anim.Value =
                            trackBar_anim.Value < trackBar_anim.Maximum
                                ? trackBar_anim.Value + 1
                                : 0;
                    }
                    else
                    {
                        trackBar_anim.Value =
                            trackBar_anim.Value > 0
                                ? trackBar_anim.Value - 1
                                : trackBar_anim.Maximum;
                    }
                })
            );
        }

        private void button_playbackAnim_Click(object sender, EventArgs e)
        {
            if (_playState == PlayState.Backward)
            {
                _playState = PlayState.Pause;
                _timer.Stop();
                button_playbackAnim.BackgroundImage = Properties.Resources.playback_icon;
            }
            else
            {
                _playState = PlayState.Backward;
                _timer.Start();
                button_playbackAnim.BackgroundImage = Properties.Resources.pause_icon;
                button_playAnim.BackgroundImage = Properties.Resources.play_icon;
            }
        }

        private void button_playAnim_Click(object sender, EventArgs e)
        {
            if (_playState == PlayState.Forward)
            {
                _playState = PlayState.Pause;
                _timer.Stop();
                button_playAnim.BackgroundImage = Properties.Resources.play_icon;
            }
            else
            {
                _playState = PlayState.Forward;
                _timer.Start();
                button_playAnim.BackgroundImage = Properties.Resources.pause_icon;
                button_playbackAnim.BackgroundImage = Properties.Resources.playback_icon;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Interval = (int)numUpDown_playbackSpeed.Value;
            _playState = PlayState.Pause;
            button_playAnim.BackgroundImage = Properties.Resources.play_icon;
            button_playbackAnim.BackgroundImage = Properties.Resources.playback_icon;
        }

        private void listBox_anims_DoubleClick(object sender, EventArgs e)
        {
            _timer.Stop();
            _playState = PlayState.Pause;
            button_playAnim.BackgroundImage = Properties.Resources.play_icon;
            button_playbackAnim.BackgroundImage = Properties.Resources.playback_icon;

            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var animFileData = File.ReadAllBytes(openFileDialog.FileName);
                int segment = _curSegment;
                if (openFileDialog.FileName.Contains("gameplay_keep"))
                {
                    segment = 4;
                }
                else if (
                    openFileDialog.FileName.Contains("gameplay_dangeon_keep")
                    || openFileDialog.FileName.Contains("gameplay_field_keep")
                )
                {
                    segment = 5;
                }
                else if (
                    (segment == 4 || segment == 5) && !openFileDialog.FileName.Contains("keep")
                )
                {
                    segment = 6;
                }

                var data = F3DZEX.Memory.Segment.FromBytes("", animFileData);

                using (
                    var form = new ObjectAnalyzerForm(
                        _game,
                        animFileData,
                        openFileDialog.FileName,
                        segment
                    )
                )
                {
                    _animations.Clear();

                    form._obj.Entries.ForEach(e =>
                    {
                        if (e is Z64Object.AnimationHolder eAnim)
                        {
                            eAnim.Name = "ext_" + eAnim.Name;
                            _animations.Add(
                                new SkeletonViewerExternalRegularAnimationEntry(
                                    eAnim,
                                    segment,
                                    data
                                )
                            );
                        }
                        if (e is Z64Object.PlayerAnimationHolder ePlayerAnim)
                        {
                            ePlayerAnim.Name = "ext_" + ePlayerAnim.Name;
                            _animations.Add(
                                new SkeletonViewerExternalPlayerAnimationEntry(
                                    ePlayerAnim,
                                    segment,
                                    data
                                )
                            );
                        }
                    });
                }

                listBox_anims.Items.Clear();
                _animations.ForEach(a => listBox_anims.Items.Add(a.GetName()));
            }
        }
    }
}
