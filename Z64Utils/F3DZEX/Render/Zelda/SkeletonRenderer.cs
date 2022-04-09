using OpenTK;
using OpenTK.Graphics.OpenGL;
using RDP;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using static Z64.Z64Object;

namespace F3DZEX.Render.Zelda
{
    public class SkeletonRenderer
    {
        SkeletonHolder _skel;
        List<SkeletonLimbHolder> _limbs;
        List<Command.Dlist> _limbDlists;

        AnimationHolder _curAnim;
        short[] _frameData;
        AnimationJointIndicesHolder.JointIndex[] _curJoints;
        int _curFrameIdx;


        public SkeletonHolder Skeleton => _skel;
        public List<SkeletonLimbHolder> Limbs=> _limbs;
        public List<Command.Dlist> LimbDlists=> _limbDlists;


        public void RenderFrame(Renderer renderer, List<SkeletonLimbHolder> selectedLimbs, bool overlay = false)
        {
            RenderLimb(renderer, 0, selectedLimbs, overlay);
        }

        void RenderLimb(Renderer renderer, int limbIdx, List<SkeletonLimbHolder> selectedLimbs, bool overlay)
        {
            renderer.RdpMtxStack.Push();


            if (_curAnim != null)
            {
                renderer.RdpMtxStack.Load(CalcMatrix(renderer.RdpMtxStack.Top(), limbIdx, _curFrameIdx));

                if (overlay)
                {
                    Matrix4 mat = renderer.RdpMtxStack.Top();
                    var vec = mat.ExtractTranslation();
                    
                    GL.Begin(PrimitiveType.Points);
                    GL.Vertex3(vec);
                    GL.End();

                    if (_limbs[limbIdx].Child != 0xFF)
                    {
                        var mat2 = CalcMatrix(mat, _limbs[limbIdx].Child, _curFrameIdx);
                        Vector3 vec2 = mat2.ExtractTranslation();
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(vec);
                        GL.Vertex3(vec2);
                        GL.End();
                    }
                }
                else
                {
                    renderer.SetHightlightEnabled(selectedLimbs?.Contains(_limbs[limbIdx]) ?? false);

                    if (_limbDlists[limbIdx] != null)
                        renderer.RenderDList(_limbDlists[limbIdx]);
                }


            }

            if (_limbs[limbIdx].Child != 0xFF)
                RenderLimb(renderer, _limbs[limbIdx].Child, selectedLimbs, overlay);

            renderer.RdpMtxStack.Pop();

            if (_limbs[limbIdx].Sibling != 0xFF)
                RenderLimb(renderer, _limbs[limbIdx].Sibling, selectedLimbs, overlay);
        }

        public void SetAnim(AnimationHolder anim, Memory mem)
        {
            _curAnim = anim;

            byte[] buff = mem.ReadBytes(_curAnim.JointIndices, (_limbs.Count + 1) * AnimationJointIndicesHolder.ENTRY_SIZE);
            _curJoints = new AnimationJointIndicesHolder("joints", buff).JointIndices;

            int max = 0;
            foreach (var joint in _curJoints)
            {
                max = Math.Max(max, joint.X);
                max = Math.Max(max, joint.Y);
                max = Math.Max(max, joint.Z);
            }

            buff = mem.ReadBytes(_curAnim.FrameData, (max < _curAnim.StaticIndexMax ? max + 1 : _curAnim.FrameCount + max) * 2);
            _frameData = new AnimationFrameDataHolder("framedata", buff).FrameData;

            SetFrame(mem, 0);
        }

        public void SetSkeleton(SkeletonHolder skel, Memory mem, Func<int, SegmentedAddress, string> limbNameCallback = null)
        {
            _skel = skel;

            byte[] limbsData = mem.ReadBytes(_skel.LimbsSeg, _skel.LimbCount * 4);

            var limbs = new SkeletonLimbsHolder("limbs", limbsData);

            _limbs = new List<SkeletonLimbHolder>();
            for (int i = 0; i < limbs.LimbSegments.Length; i++)
            {
                string limbNameBase = limbNameCallback != null
                    ? limbNameCallback(i, limbs.LimbSegments[i])
                    : null;
                string limbName = limbNameBase != null
                    // prepend limb index to the callback-provided limb name
                    ? $"{i + 1} {limbNameBase}"
                    // no callback or the callback didn't provide a name, use a default limb name
                    : $"limb_{i + 1}";
                byte[] limbData = mem.ReadBytes(limbs.LimbSegments[i], SkeletonLimbHolder.ENTRY_SIZE);
                var limb = new SkeletonLimbHolder(limbName, limbData);
                _limbs.Add(limb);
            }

            UpdateLimbsDlists(mem);
            SetFrame(mem, 0);
        }
        void UpdateLimbsDlists(Memory mem)
        {
            _limbDlists = new List<Command.Dlist>();

            foreach (var limb in _limbs)
            {
                Command.Dlist dlist = null;
                try
                {
                    if (limb.DListSeg.VAddr != 0)
                        dlist = new Command.Dlist(mem, limb.DListSeg);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Error while decoding dlist 0x{limb.DListSeg.VAddr:X8} : {ex.Message}");
                }
                _limbDlists.Add(dlist);
            }
        }


        // updates the matrix buffer in segment 13 (has to be done each new frame) (Flex skeleton Only)
        public void SetFrame(Memory mem, int frameIdx)
        {
            _curFrameIdx = frameIdx;
            if (!(_skel is FlexSkeletonHolder flexSkel))
                return;

            byte[] mtxBuff = new byte[flexSkel.DListCount * Mtx.SIZE];

            using (MemoryStream ms = new MemoryStream(mtxBuff))
            {
                BinaryStream bw = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);

                UpdateMatrixBuf(bw, 0, 0, Matrix4.Identity, frameIdx);
            }

            mem.Segments[0xD] = Memory.Segment.FromBytes("[RESERVED] Anim Matrices", mtxBuff);
        }

        int UpdateMatrixBuf(BinaryStream bw, int limbIdx, int dlistIdx, Matrix4 src, int frameIdx)
        {
            Matrix4 mtx = CalcMatrix(src, limbIdx, frameIdx);

            if (_limbDlists[limbIdx] != null)
            {
                bw.Seek(dlistIdx++ * Mtx.SIZE, SeekOrigin.Begin);
                Mtx.FromMatrix4(mtx).Write(bw);
            }


            if (_limbs[limbIdx].Child != 0xFF)
                dlistIdx = UpdateMatrixBuf(bw, _limbs[limbIdx].Child, dlistIdx, mtx, frameIdx);

            if (_limbs[limbIdx].Sibling != 0xFF)
                dlistIdx = UpdateMatrixBuf(bw, _limbs[limbIdx].Sibling, dlistIdx, src, frameIdx);

            return dlistIdx;
        }

        Matrix4 CalcMatrix(Matrix4 src, int limbIdx, int frameIdx)
        {
            if (_curAnim == null)
                return src;

            Vector3 pos = GetLimbPos(limbIdx);

            short rotX = GetFrameData(_curJoints[limbIdx + 1].X, frameIdx);
            short rotY = GetFrameData(_curJoints[limbIdx + 1].Y, frameIdx);
            short rotZ = GetFrameData(_curJoints[limbIdx + 1].Z, frameIdx);

            src = Matrix4.CreateRotationX(S16ToRad(rotX)) *
                Matrix4.CreateRotationY(S16ToRad(rotY)) *
                Matrix4.CreateRotationZ(S16ToRad(rotZ)) *
                Matrix4.CreateTranslation(pos) *
                src;

            return src;
        }


        float S16ToRad(short x) => x * (float)Math.PI / 0x7FFF;
        float S16ToDeg(short x) => x * 360.0f / 0xFFFF;
        float DegToRad(float x) => x * (float)Math.PI / 180.0f;

        short GetFrameData(int frameDataIdx, int frameIdx) => _frameData[frameDataIdx < _curAnim.StaticIndexMax ? frameDataIdx : frameDataIdx + frameIdx];
        Vector3 GetLimbPos(int limbIdx) => (limbIdx == 0)
                ? new Vector3(_curJoints[limbIdx].X, _curJoints[limbIdx].Y, _curJoints[limbIdx].Z)
                : new Vector3(_limbs[limbIdx].JointX, _limbs[limbIdx].JointY, _limbs[limbIdx].JointZ);

    }
}
