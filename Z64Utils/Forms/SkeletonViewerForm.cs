﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
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

        bool _formClosing = false;
        System.Timers.Timer _timer;
        PlayState _playState;
        string _dlistError = null;

        Z64Game _game;
        F3DZEX.Render.Renderer _renderer;
        SegmentEditorForm _segForm;
        DisasmForm _disasForm;
        SettingsForm _settingsForm;
        F3DZEX.Render.Renderer.Config _rendererCfg;

        SkeletonHolder _skel;
        List<AnimationHolder> _anims;
        List<PlayerAnimationHolder> _playerAnims;
        List<SkeletonLimbHolder> _limbs;
        List<F3DZEX.Command.Dlist> _limbDlists;
        List<bool> _limbDlistRenderFlags;

        AnimationHolder _curAnim;
        short[] _frameData;
        AnimationJointIndicesHolder.JointIndex[] _curJoints;

        PlayerAnimationHolder _curPlayerAnim;
        PlayerAnimationJointTableHolder.JointTableEntry[,] _curPlayerJointTable;

        string _animationError;

        byte[] _animFile;
        int _curSegment = 6;
        int _extAnimSegment = 6;

        public SkeletonViewerForm(Z64Game game, int curSegment)
        {
            _game = game;
            _curSegment = curSegment;
            _rendererCfg = new F3DZEX.Render.Renderer.Config();

            InitializeComponent();
            Toolkit.Init();

            _renderer = new F3DZEX.Render.Renderer(game, _rendererCfg);
            modelViewer.RenderCallback = RenderCallback;

            _timer = new System.Timers.Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = (int)numUpDown_playbackSpeed.Value;

            if ((Control.ModifierKeys & Keys.Control) == 0)
            {
                _renderer.Memory.Segments[4] = F3DZEX.Memory.Segment.FromBytes(
                    "gameplay_keep",
                    game.GetFileByName("gameplay_keep").Data
                );
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
        }

        void RenderLimb(int limbIdx, bool overlay = false)
        {
            _renderer.RdpMtxStack.Push();

            _renderer.RdpMtxStack.Load(CalcMatrix(_renderer.RdpMtxStack.Top(), limbIdx));

            if (overlay)
            {
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(0, 0, 0);
                GL.End();

                if (_limbs[limbIdx].Child != 0xFF)
                {
                    Vector3 childPos;

                    if (_curAnim != null && _curJoints != null)
                        childPos = GetLimbPos(_limbs[limbIdx].Child);
                    else
                        childPos = new Vector3(0, 0, 0);

                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(childPos);
                    GL.End();
                }
            }
            else
            {
                var node = treeView_hierarchy.SelectedNode;
                _renderer.SetHightlightEnabled(node?.Tag?.Equals(_limbs[limbIdx]) ?? false);

                if (_limbDlists[limbIdx] != null && _limbDlistRenderFlags[limbIdx])
                    _renderer.RenderDList(_limbDlists[limbIdx]);
            }

            if (_limbs[limbIdx].Child != 0xFF)
                RenderLimb(_limbs[limbIdx].Child, overlay);

            _renderer.RdpMtxStack.Pop();

            if (_limbs[limbIdx].Sibling != 0xFF)
                RenderLimb(_limbs[limbIdx].Sibling, overlay);
        }

        void RenderCallback(Matrix4 proj, Matrix4 view)
        {
            if (_dlistError != null)
            {
                toolStripErrorLabel.Text = _dlistError;
                return;
            }

            _renderer.RenderStart(proj, view);
            RenderLimb(0);

            /*
            GL.PointSize(10.0f);
            GL.LineWidth(2.0f);
            GL.Color3(0xFF, 0, 0);
            RenderLimb(0, true);
            */

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
                var dlist = _limbDlists[_limbs.IndexOf((SkeletonLimbHolder)tag)];
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
                    var index = _limbs.IndexOf((SkeletonLimbHolder)tag);
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

        private void NewRender(object sender = null, EventArgs e = null)
        {
            _renderer.ClearErrors();
            toolStripErrorLabel.Text = "";
            modelViewer.Render();
        }

        public void SetSkeleton(SkeletonHolder skel, List<AnimationHolder> anims)
        {
            _skel = skel;
            _anims = anims;
            _playerAnims = new List<PlayerAnimationHolder>();

            listBox_anims.Items.Clear();
            _anims.ForEach(a => listBox_anims.Items.Add(a.Name));

            UpdateSkeleton();
            NewRender();
        }

        void UpdateLimbsDlists()
        {
            _dlistError = null;
            _limbDlists = new List<F3DZEX.Command.Dlist>();

            foreach (var limb in _limbs)
            {
                F3DZEX.Command.Dlist dlist = null;
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
        void UpdateSkeleton()
        {
            treeView_hierarchy.Nodes.Clear();
            var root = treeView_hierarchy.Nodes.Add("skeleton");
            root.Tag = _skel;

            byte[] limbsData = _renderer.Memory.ReadBytes(_skel.LimbsSeg, _skel.LimbCount * 4);

            var limbs = new SkeletonLimbsHolder("limbs", limbsData);

            _limbs = new List<SkeletonLimbHolder>();
            _limbDlistRenderFlags = new List<bool>();
            for (int i = 0; i < limbs.LimbSegments.Length; i++)
            {
                byte[] limbData = _renderer.Memory.ReadBytes(
                    limbs.LimbSegments[i],
                    SkeletonLimbHolder.STANDARD_LIMB_SIZE
                );
                var limb = new SkeletonLimbHolder($"limb_{i}", limbData, EntryType.StandardLimb); // TODO support other limb types
                _limbs.Add(limb);
                _limbDlistRenderFlags.Add(true);
            }

            UpdateLimbsDlists();
            UpdateLimbs();
        }

        // Updates limbs -> matrices
        void UpdateLimbs()
        {
            TreeNode skelNode = treeView_hierarchy.Nodes[0];

            if (_limbs.Count > 0)
                AddLimbRoutine(skelNode, 0, 0, 0, 0);

            UpdateMatrixBuf();
        }

        void AddLimbRoutine(TreeNode parent, int i, int x, int y, int z)
        {
            var node = parent.Nodes.Add($"limb_{i}");
            node.Tag = _limbs[i];

            if (_limbs[i].Sibling != 0xFF)
                AddLimbRoutine(parent, _limbs[i].Sibling, x, y, z);
            if (_limbs[i].Child != 0xFF)
                AddLimbRoutine(
                    node,
                    _limbs[i].Child,
                    x + _limbs[i].JointX,
                    y + _limbs[i].JointY,
                    z + _limbs[i].JointZ
                );
        }

        float S16ToRad(short x) => x * (float)Math.PI / 0x7FFF;

        float S16ToDeg(short x) => x * 360.0f / 0xFFFF;

        float DegToRad(float x) => x * (float)Math.PI / 180.0f;

        short GetFrameData(int frameDataIdx) =>
            _frameData[
                frameDataIdx < _curAnim.StaticIndexMax
                    ? frameDataIdx
                    : frameDataIdx + trackBar_anim.Value
            ];

        Vector3 GetLimbPos(int limbIdx)
        {
            if (_curPlayerAnim != null)
            {
                return new Vector3(
                    _limbs[limbIdx].JointX,
                    _limbs[limbIdx].JointY,
                    _limbs[limbIdx].JointZ
                );
            }

            return (_curJoints == null) ? new Vector3(0, 0, 0)
                : (limbIdx == 0)
                    ? new Vector3(
                        _curJoints[limbIdx].X,
                        _curJoints[limbIdx].Y,
                        _curJoints[limbIdx].Z
                    )
                : new Vector3(
                    _limbs[limbIdx].JointX,
                    _limbs[limbIdx].JointY,
                    _limbs[limbIdx].JointZ
                );
        }

        // Update anims -> matrices
        void UpdateAnim()
        {
            trackBar_anim.Minimum = 0;
            trackBar_anim.Maximum = _curAnim.FrameCount - 1;
            trackBar_anim.Value = 0;

            var Saved = _renderer.Memory.Segments[_curSegment];

            if (_curAnim.extAnim)
                _renderer.Memory.Segments[_extAnimSegment] = F3DZEX.Memory.Segment.FromBytes(
                    "",
                    _animFile
                );

            byte[] buff = _renderer.Memory.ReadBytes(
                _curAnim.JointIndices,
                (_limbs.Count + 1) * AnimationJointIndicesHolder.ENTRY_SIZE
            );
            _curJoints = new AnimationJointIndicesHolder("joints", buff).JointIndices;

            int max = 0;
            foreach (var joint in _curJoints)
            {
                max = Math.Max(max, joint.X);
                max = Math.Max(max, joint.Y);
                max = Math.Max(max, joint.Z);
            }

            int bytesToRead =
                (max < _curAnim.StaticIndexMax ? max + 1 : _curAnim.FrameCount + max) * 2;

            if (
                bytesToRead + _curAnim.FrameData.SegmentOff
                > _renderer.Memory.Segments[_curSegment].Data.Length
            )
            {
                _curAnim = null;
                _curJoints = null;
                _frameData = null;
                _animationError =
                    "Animation is glitchy; displaying folded pose. To view this animation, load it in-game.";
            }
            else
            {
                buff = _renderer.Memory.ReadBytes(_curAnim.FrameData, bytesToRead);
                _frameData = new AnimationFrameDataHolder("framedata", buff).FrameData;
                _animationError = "";
            }

            _renderer.Memory.Segments[_curSegment] = Saved;
            UpdateMatrixBuf();
        }

        void UpdatePlayerAnim()
        {
            trackBar_anim.Minimum = 0;
            trackBar_anim.Maximum = _curPlayerAnim.FrameCount - 1;
            trackBar_anim.Value = 0;

            var Saved = _renderer.Memory.Segments[_curPlayerAnim.PlayerAnimationSegment.SegmentId];
            _renderer.Memory.Segments[_curPlayerAnim.PlayerAnimationSegment.SegmentId] =
                F3DZEX.Memory.Segment.FromBytes(
                    "link_animetion",
                    _game.GetFileByName("link_animetion").Data
                );

            byte[] buff = _renderer.Memory.ReadBytes(
                _curPlayerAnim.PlayerAnimationSegment,
                ((PlayerAnimationHolder.PLAYER_LIMB_COUNT * 3) + 1) * _curPlayerAnim.FrameCount * 2
            );
            _curPlayerJointTable = new PlayerAnimationJointTableHolder("joints", buff).JointTable;

            _renderer.Memory.Segments[_curPlayerAnim.PlayerAnimationSegment.SegmentId] = Saved;
            UpdateMatrixBuf();
        }

        Matrix4 CalcMatrix(Matrix4 src, int limbIdx)
        {
            if (_curPlayerAnim != null)
            {
                return CalcMatrixPlayer(src, limbIdx);
            }

            return CalcMatrixNormal(src, limbIdx);
        }

        Matrix4 CalcMatrixNormal(Matrix4 src, int limbIdx)
        {
            if (_curAnim == null || _curJoints == null)
                return src;

            Vector3 pos = GetLimbPos(limbIdx);

            short rotX = GetFrameData(_curJoints[limbIdx + 1].X);
            short rotY = GetFrameData(_curJoints[limbIdx + 1].Y);
            short rotZ = GetFrameData(_curJoints[limbIdx + 1].Z);

            src =
                Matrix4.CreateRotationX(S16ToRad(rotX))
                * Matrix4.CreateRotationY(S16ToRad(rotY))
                * Matrix4.CreateRotationZ(S16ToRad(rotZ))
                * Matrix4.CreateTranslation(pos)
                * src;

            return src;
        }

        Matrix4 CalcMatrixPlayer(Matrix4 src, int limbIdx)
        {
            if (_curPlayerJointTable == null)
                return src;

            Vector3 pos = GetLimbPos(limbIdx);

            short rotX = _curPlayerJointTable[trackBar_anim.Value, limbIdx + 1].X;
            short rotY = _curPlayerJointTable[trackBar_anim.Value, limbIdx + 1].Y;
            short rotZ = _curPlayerJointTable[trackBar_anim.Value, limbIdx + 1].Z;

            src =
                Matrix4.CreateRotationX(S16ToRad(rotX))
                * Matrix4.CreateRotationY(S16ToRad(rotY))
                * Matrix4.CreateRotationZ(S16ToRad(rotZ))
                * Matrix4.CreateTranslation(pos)
                * src;

            return src;
        }

        // Flex Only
        void UpdateMatrixBuf()
        {
            if (!(_skel is FlexSkeletonHolder flexSkel))
                return;

            byte[] mtxBuff = new byte[flexSkel.DListCount * Mtx.SIZE];

            using (MemoryStream ms = new MemoryStream(mtxBuff))
            {
                BinaryStream bw = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);

                UpdateMatrixBuf(bw, 0, 0, Matrix4.Identity);
            }

            _renderer.Memory.Segments[0xD] = F3DZEX.Memory.Segment.FromBytes(
                "[RESERVED] Anim Matrices",
                mtxBuff
            );
        }

        int UpdateMatrixBuf(BinaryStream bw, int limbIdx, int dlistIdx, Matrix4 src)
        {
            Matrix4 mtx = CalcMatrix(src, limbIdx);

            if (_limbDlists[limbIdx] != null)
            {
                bw.Seek(dlistIdx++ * Mtx.SIZE, SeekOrigin.Begin);
                Mtx.FromMatrix4(mtx).Write(bw);
            }

            if (_limbs[limbIdx].Child != 0xFF)
                dlistIdx = UpdateMatrixBuf(bw, _limbs[limbIdx].Child, dlistIdx, mtx);

            if (_limbs[limbIdx].Sibling != 0xFF)
                dlistIdx = UpdateMatrixBuf(bw, _limbs[limbIdx].Sibling, dlistIdx, src);

            return dlistIdx;
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
                var dlist = _limbDlists[_limbs.IndexOf((SkeletonLimbHolder)tag)];
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

            if (_limbs != null)
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
                _segForm.SegmentsChanged += (sender, seg) =>
                {
                    int idx = (int)sender;

                    if (idx == 0xD && _skel is FlexSkeletonHolder)
                        MessageBox.Show(
                            "Error",
                            "Cannot set segment 13 (reserved for animation matrices)"
                        );
                    else
                    {
                        _renderer.Memory.Segments[(int)sender] = seg;

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

            _curAnim = null;
            _curPlayerAnim = null;
            if (listBox_anims.SelectedIndex >= 0)
            {
                if (listBox_anims.SelectedIndex < _anims.Count)
                {
                    _curAnim = _anims[listBox_anims.SelectedIndex];
                    UpdateAnim();
                }
                else
                {
                    _curPlayerAnim = _playerAnims[listBox_anims.SelectedIndex - _anims.Count];
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

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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

            OpenFileDialog of = new OpenFileDialog();
            DialogResult DR = of.ShowDialog();

            if (DR == DialogResult.OK)
            {
                _animFile = File.ReadAllBytes(of.FileName);
                int segment = _curSegment;
                if (of.FileName.Contains("gameplay_keep"))
                {
                    segment = 4;
                }
                else if (
                    of.FileName.Contains("gameplay_dangeon_keep")
                    || of.FileName.Contains("gameplay_field_keep")
                )
                {
                    segment = 5;
                }
                else if ((segment == 4 || segment == 5) && !of.FileName.Contains("keep"))
                {
                    segment = 6;
                }
                _extAnimSegment = segment;

                using (var form = new ObjectAnalyzerForm(_game, _animFile, of.FileName, segment))
                {
                    _anims.Clear();
                    _playerAnims.Clear();

                    form._obj.Entries.ForEach(e =>
                    {
                        if (e is Z64Object.AnimationHolder)
                        {
                            (e as Z64Object.AnimationHolder).extAnim = true;
                            (e as Z64Object.AnimationHolder).Name =
                                "ext_" + (e as Z64Object.AnimationHolder).Name;
                            _anims.Add((Z64Object.AnimationHolder)e);
                        }
                        if (e is Z64Object.PlayerAnimationHolder)
                        {
                            (e as Z64Object.PlayerAnimationHolder).extAnim = true;
                            (e as Z64Object.PlayerAnimationHolder).Name =
                                "ext_" + (e as Z64Object.PlayerAnimationHolder).Name;
                            _playerAnims.Add((Z64Object.PlayerAnimationHolder)e);
                        }
                    });
                }

                listBox_anims.Items.Clear();
                _anims.ForEach(a => listBox_anims.Items.Add(a.Name));
                _playerAnims.ForEach(a => listBox_anims.Items.Add(a.Name));
            }
        }
    }
}
