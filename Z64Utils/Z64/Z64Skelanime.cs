using System;
using System.Collections.Generic;
using F3DZEX.Render;
using OpenTK.Mathematics;

namespace Z64;

public class Z64SkeletonTreeLimb
{
    public Z64SkeletonTreeLimb? Child;
    public Z64SkeletonTreeLimb? Sibling;
    public int Index;

    public Z64SkeletonTreeLimb(int index)
    {
        Index = index;
    }

    public void Visit(Action<int> action)
    {
        action(Index);
        Child?.Visit(action);
        Sibling?.Visit(action);
    }
}

public class Z64Skeleton
{
    public List<Z64Object.SkeletonLimbHolder> Limbs;
    public Z64SkeletonTreeLimb Root;

    public Z64Skeleton(List<Z64Object.SkeletonLimbHolder> limbs, Z64SkeletonTreeLimb root)
    {
        Limbs = limbs;
        Root = root;
    }

    public static Z64Skeleton Get(F3DZEX.Memory mem, Z64Object.SkeletonHolder skeletonHolder)
    {
        byte[] limbsData = mem.ReadBytes(skeletonHolder.LimbsSeg, skeletonHolder.LimbCount * 4);

        var skeletonLimbsHolder = new Z64Object.SkeletonLimbsHolder("limbs", limbsData);

        var limbs = new List<Z64Object.SkeletonLimbHolder>();
        for (int i = 0; i < skeletonLimbsHolder.LimbSegments.Length; i++)
        {
            byte[] limbData = mem.ReadBytes(
                skeletonLimbsHolder.LimbSegments[i],
                Z64Object.SkeletonLimbHolder.STANDARD_LIMB_SIZE
            );
            var limb = new Z64Object.SkeletonLimbHolder(
                $"limb_{i}",
                limbData,
                Z64Object.EntryType.StandardLimb
            );
            limbs.Add(limb);
        }

        var treeRoot = new Z64SkeletonTreeLimb(0);

        void RenderLimb(Z64SkeletonTreeLimb treeLimb)
        {
            if (limbs[treeLimb.Index].Child != 0xFF)
            {
                treeLimb.Child = new(limbs[treeLimb.Index].Child);
                RenderLimb(treeLimb.Child);
            }

            if (limbs[treeLimb.Index].Sibling != 0xFF)
            {
                treeLimb.Sibling = new(limbs[treeLimb.Index].Sibling);
                RenderLimb(treeLimb.Sibling);
            }
        }

        RenderLimb(treeRoot);

        return new(limbs, treeRoot);
    }
}

public class Z64FlexSkeleton : Z64Skeleton
{
    public int DListCount;

    public Z64FlexSkeleton(
        List<Z64Object.SkeletonLimbHolder> limbs,
        Z64SkeletonTreeLimb root,
        int dListCount
    )
        : base(limbs, root)
    {
        DListCount = dListCount;
    }

    public static Z64FlexSkeleton Get(
        F3DZEX.Memory mem,
        Z64Object.FlexSkeletonHolder flexSkeletonHolder
    )
    {
        var skel = Z64Skeleton.Get(mem, flexSkeletonHolder);
        return new(skel.Limbs, skel.Root, flexSkeletonHolder.DListCount);
    }
}

public class Z64Animation
{
    public int FrameCount;
    public int StaticIndexMax;
    public Z64Object.AnimationJointIndicesHolder.JointIndex[] Joints;
    public short[] FrameData;

    public Z64Animation(
        int frameCount,
        int staticIndexMax,
        Z64Object.AnimationJointIndicesHolder.JointIndex[] joints,
        short[] frameData
    )
    {
        FrameCount = frameCount;
        StaticIndexMax = staticIndexMax;
        Joints = joints;
        FrameData = frameData;
    }

    public short GetFrameData(int frameDataIdx, int frame)
    {
        return FrameData[frameDataIdx < StaticIndexMax ? frameDataIdx : frameDataIdx + frame];
    }

    public static Z64Animation Get(
        F3DZEX.Memory mem,
        Z64Object.AnimationHolder animationHolder,
        int limbsCount
    )
    {
        byte[] buff = mem.ReadBytes(
            animationHolder.JointIndices,
            (limbsCount + 1) * Z64Object.AnimationJointIndicesHolder.ENTRY_SIZE
        );
        var joints = new Z64Object.AnimationJointIndicesHolder("joints", buff).JointIndices;

        int max = 0;
        foreach (var joint in joints)
        {
            max = Math.Max(max, joint.X);
            max = Math.Max(max, joint.Y);
            max = Math.Max(max, joint.Z);
        }

        int bytesToRead =
            (max < animationHolder.StaticIndexMax ? max + 1 : animationHolder.FrameCount + max) * 2;

        buff = mem.ReadBytes(animationHolder.FrameData, bytesToRead);
        var frameData = new Z64Object.AnimationFrameDataHolder("framedata", buff).FrameData;

        return new(animationHolder.FrameCount, animationHolder.StaticIndexMax, joints, frameData);
    }
}

public class Z64PlayerAnimation
{
    public int FrameCount;
    public Z64Object.PlayerAnimationJointTableHolder.JointTableEntry[,] JointTable;

    public Z64PlayerAnimation(
        int frameCount,
        Z64Object.PlayerAnimationJointTableHolder.JointTableEntry[,] jointTable
    )
    {
        FrameCount = frameCount;
        JointTable = jointTable;
    }

    public static Z64PlayerAnimation Get(
        F3DZEX.Memory mem,
        Z64Object.PlayerAnimationHolder playerAnimationHolder
    )
    {
        byte[] buff = mem.ReadBytes(
            playerAnimationHolder.PlayerAnimationSegment,
            ((Z64Object.PlayerAnimationHolder.PLAYER_LIMB_COUNT * 3) + 1)
                * playerAnimationHolder.FrameCount
                * 2
        );
        var playerJointTable = new Z64Object.PlayerAnimationJointTableHolder(
            "joints",
            buff
        ).JointTable;
        return new(playerAnimationHolder.FrameCount, playerJointTable);
    }
}

public class Z64SkeletonPose
{
    public Matrix4[] LimbsPose;

    public Z64SkeletonPose(Matrix4[] limbsPose)
    {
        LimbsPose = limbsPose;
    }

    private static float S16ToRad(short x) => x * (float)Math.PI / 0x7FFF;

    public static Z64SkeletonPose Get(Z64Skeleton skeleton, Z64Animation anim, int frame)
    {
        MatrixStack matrixStack = new();

        var limbsPose = new Matrix4[skeleton.Limbs.Count];

        void RenderLimb(Z64SkeletonTreeLimb treeLimb)
        {
            matrixStack.Push();

            matrixStack.Load(CalcMatrix(matrixStack.Top(), treeLimb.Index));

            limbsPose[treeLimb.Index] = matrixStack.Top();

            if (treeLimb.Child != null)
                RenderLimb(treeLimb.Child);

            matrixStack.Pop();

            if (treeLimb.Sibling != null)
                RenderLimb(treeLimb.Sibling);
        }

        Matrix4 CalcMatrix(Matrix4 src, int limbIdx)
        {
            Vector3 pos = GetLimbPos(limbIdx);

            short rotX = anim.GetFrameData(anim.Joints[limbIdx + 1].X, frame);
            short rotY = anim.GetFrameData(anim.Joints[limbIdx + 1].Y, frame);
            short rotZ = anim.GetFrameData(anim.Joints[limbIdx + 1].Z, frame);

            src =
                Matrix4.CreateRotationX(S16ToRad(rotX))
                * Matrix4.CreateRotationY(S16ToRad(rotY))
                * Matrix4.CreateRotationZ(S16ToRad(rotZ))
                * Matrix4.CreateTranslation(pos)
                * src;

            return src;
        }

        Vector3 GetLimbPos(int limbIdx)
        {
            return (limbIdx == 0)
                ? new Vector3(
                    anim.Joints[limbIdx].X,
                    anim.Joints[limbIdx].Y,
                    anim.Joints[limbIdx].Z
                )
                : new Vector3(
                    skeleton.Limbs[limbIdx].JointX,
                    skeleton.Limbs[limbIdx].JointY,
                    skeleton.Limbs[limbIdx].JointZ
                );
        }

        RenderLimb(skeleton.Root);

        return new(limbsPose);
    }

    public static Z64SkeletonPose Get(
        Z64Skeleton skeleton,
        Z64PlayerAnimation playerAnim,
        int frame
    )
    {
        MatrixStack matrixStack = new();

        var limbsPose = new Matrix4[skeleton.Limbs.Count];

        void RenderLimb(Z64SkeletonTreeLimb treeLimb)
        {
            matrixStack.Push();

            matrixStack.Load(CalcMatrixPlayer(matrixStack.Top(), treeLimb.Index));

            limbsPose[treeLimb.Index] = matrixStack.Top();

            if (treeLimb.Child != null)
                RenderLimb(treeLimb.Child);

            matrixStack.Pop();

            if (treeLimb.Sibling != null)
                RenderLimb(treeLimb.Sibling);
        }

        Matrix4 CalcMatrixPlayer(Matrix4 src, int limbIdx)
        {
            Vector3 pos = GetLimbPos(limbIdx);

            short rotX = playerAnim.JointTable[frame, limbIdx + 1].X;
            short rotY = playerAnim.JointTable[frame, limbIdx + 1].Y;
            short rotZ = playerAnim.JointTable[frame, limbIdx + 1].Z;

            src =
                Matrix4.CreateRotationX(S16ToRad(rotX))
                * Matrix4.CreateRotationY(S16ToRad(rotY))
                * Matrix4.CreateRotationZ(S16ToRad(rotZ))
                * Matrix4.CreateTranslation(pos)
                * src;

            return src;
        }

        Vector3 GetLimbPos(int limbIdx)
        {
            return new Vector3(
                skeleton.Limbs[limbIdx].JointX,
                skeleton.Limbs[limbIdx].JointY,
                skeleton.Limbs[limbIdx].JointZ
            );
        }

        RenderLimb(skeleton.Root);

        return new(limbsPose);
    }
}
