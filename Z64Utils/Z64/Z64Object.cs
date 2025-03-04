using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common;
using F3DZEX;
using N64;
using RDP;
using Syroot.BinaryData;

namespace Z64
{
    [Serializable]
    public class Z64ObjectException : Exception
    {
        public Z64ObjectException() { }

        public Z64ObjectException(string message)
            : base(message) { }

        public Z64ObjectException(string message, Exception inner)
            : base(message, inner) { }

        protected Z64ObjectException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context) { }
    }

    public struct Vec3s
    {
        public short X,
            Y,
            Z;
    };

    public class Z64Object
    {
        public enum EntryType
        {
            DList,
            Vertex,
            Texture,
            Mtx,
            AnimationHeader,
            FrameData,
            JointIndices,
            SkeletonHeader,
            FlexSkeletonHeader,
            SkeletonLimbs,
            StandardLimb,
            LODLimb,
            SkinLimb,
            PlayerAnimationHeader,
            PlayerAnimationJointTable,
            CollisionHeader,
            CollisionVertices,
            CollisionPolygons,
            CollisionSurfaceTypes,
            CollisionCamData,
            WaterBox,
            MatAnimHeader,
            MatAnimTexScrollParams,
            MatAnimColorParams,
            MatAnimPrimColors,
            MatAnimEnvColors,
            MatAnimKeyFrames,
            MatAnimTexCycleParams,
            MatAnimTextureIndexList,
            MatAnimTextureList,
            Unknown,
        }

        public abstract class ObjectHolder
        {
            public string Name { get; set; }

            protected ObjectHolder(string name)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("Invalid Name", nameof(name));
                Name = name;
            }

            public abstract EntryType GetEntryType();
            public abstract byte[] GetData();
            public abstract void SetData(byte[] data);
            public abstract int GetSize();

            public override string ToString() => $"{Name} ({GetEntryType()})";
        }

        public class DListHolder : ObjectHolder
        {
            public byte[] UCode { get; set; }

            public DListHolder(string name, byte[] ucode)
                : base(name) => UCode = ucode;

            public override EntryType GetEntryType() => EntryType.DList;

            public override byte[] GetData() => UCode;

            public override void SetData(byte[] data) => UCode = data;

            public override int GetSize() => UCode.Length;
        }

        public class VertexHolder : ObjectHolder
        {
            public List<Vertex> Vertices { get; set; }

            public VertexHolder(string name, List<Vertex> vtx)
                : base(name) => Vertices = vtx;

            public override EntryType GetEntryType() => EntryType.Vertex;

            public override void SetData(byte[] data)
            {
                if (data.Length % 0x10 != 0)
                    throw new Z64ObjectException(
                        $"Invalid size for a vertex buffer (0x{data.Length:X})"
                    );

                int count = data.Length / 0x10;

                Vertices = new List<Vertex>();
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms);
                    br.ByteConverter = ByteConverter.Big;

                    for (int i = 0; i < count; i++)
                        Vertices.Add(new Vertex(br));
                }
            }

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms);
                    bw.ByteConverter = ByteConverter.Big;

                    for (int i = 0; i < Vertices.Count; i++)
                        Vertices[i].Write(bw);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            public override int GetSize() => Vertices.Count * Vertex.SIZE;
        }

        public class UnknowHolder : ObjectHolder
        {
            public byte[] Data { get; set; }

            public UnknowHolder(string name, byte[] data)
                : base(name) => Data = data;

            public override EntryType GetEntryType() => EntryType.Unknown;

            public override byte[] GetData() => Data;

            public override void SetData(byte[] data) => Data = data;

            public override int GetSize() => Data.Length;
        }

        public class TextureHolder : ObjectHolder
        {
            public byte[] Texture { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public N64TexFormat Format { get; set; }
            public TextureHolder? Tlut { get; set; }

            public TextureHolder(string name, int w, int h, N64TexFormat format, byte[] tex)
                : base(name)
            {
                Width = w;
                Height = h;
                Format = format;
                Tlut = null;
                SetData(tex);
            }

            public void SetBitmap(Bitmap bmp, N64TexFormat format)
            {
                throw new NotImplementedException();
            }

            public Bitmap GetBitmap()
            {
                return N64Texture.DecodeBitmap(Width, Height, Format, Texture, Tlut?.Texture);
            }

            public override EntryType GetEntryType() => EntryType.Texture;

            public override byte[] GetData() => Texture;

            [MemberNotNull(nameof(Texture))]
            public override void SetData(byte[] data)
            {
                int validSize = N64Texture.GetTexSize(Width * Height, Format);
                if (data.Length != validSize)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X} instead of 0x{validSize:X})"
                    );

                Texture = data;
            }

            public override int GetSize() => Texture.Length;
        }

        public class MtxHolder : ObjectHolder
        {
            public List<Mtx> Matrices;

            public MtxHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.Mtx;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    foreach (Mtx mtx in Matrices)
                    {
                        mtx.Write(bw);
                    }
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(Matrices))]
            public override void SetData(byte[] data)
            {
                if (data.Length % Mtx.SIZE != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}, should be a multiple of 0x{Mtx.SIZE:X})"
                    );

                int count = data.Length / Mtx.SIZE;

                Matrices = new List<Mtx>(count);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms);
                    br.ByteConverter = ByteConverter.Big;

                    for (int i = 0; i < count; i++)
                        Matrices.Add(new Mtx(br));
                }
            }

            public override int GetSize() => Matrices.Count * Mtx.SIZE;
        }

        public class SkeletonLimbHolder : ObjectHolder
        {
            public const int STANDARD_LIMB_SIZE = 0xC;
            public const int LOD_LIMB_SIZE = 0x10;
            public const int SKIN_LIMB_SIZE = 0x10;

            public EntryType Type;

            public short JointX { get; set; }
            public short JointY { get; set; }
            public short JointZ { get; set; }
            public byte Child { get; set; }
            public byte Sibling { get; set; }

            // Standard and LOD Limb Only
            public SegmentedAddress? DListSeg { get; set; }

            // LOD Limb Only
            public SegmentedAddress? DListFarSeg { get; set; }

            // Skin Limb Only
            public int? SegmentType { get; set; } // indicates the type of data pointed to by SkinSeg
            public SegmentedAddress? SkinSeg { get; set; }

            public SkeletonLimbHolder(string name, byte[] data, EntryType type)
                : base(name)
            {
                Debug.Assert(
                    type == EntryType.StandardLimb
                        || type == EntryType.LODLimb
                        || type == EntryType.SkinLimb
                );
                Type = type;
                SetData(data);
            }

            public override EntryType GetEntryType() => Type;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(JointX);
                    bw.Write(JointY);
                    bw.Write(JointZ);
                    bw.Write(Child);
                    bw.Write(Sibling);

                    if (Type != EntryType.SkinLimb)
                    {
                        Debug.Assert(DListSeg != null);
                        bw.Write(DListSeg.VAddr);
                    }

                    if (Type == EntryType.LODLimb)
                    {
                        Debug.Assert(DListFarSeg != null);
                        bw.Write(DListFarSeg.VAddr);
                    }
                    else if (Type == EntryType.SkinLimb)
                    {
                        Debug.Assert(SegmentType != null);
                        Debug.Assert(SkinSeg != null);
                        bw.Write((int)SegmentType);
                        bw.Write(SkinSeg.VAddr);
                    }

                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    JointX = br.ReadInt16();
                    JointY = br.ReadInt16();
                    JointZ = br.ReadInt16();
                    Child = br.Read1Byte();
                    Sibling = br.Read1Byte();

                    if (Type != EntryType.SkinLimb)
                        DListSeg = new SegmentedAddress(br.ReadUInt32());

                    if (Type == EntryType.LODLimb)
                        DListFarSeg = new SegmentedAddress(br.ReadUInt32());
                    else if (Type == EntryType.SkinLimb)
                    {
                        SegmentType = br.ReadInt32();
                        SkinSeg = new SegmentedAddress(br.ReadUInt32());
                    }
                }
            }

            public override int GetSize() =>
                (Type == EntryType.StandardLimb)
                    ? STANDARD_LIMB_SIZE
                    : ((Type == EntryType.LODLimb) ? LOD_LIMB_SIZE : SKIN_LIMB_SIZE);
        }

        public class SkeletonLimbsHolder : ObjectHolder
        {
            public SegmentedAddress[] LimbSegments { get; set; }

            public SkeletonLimbsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.SkeletonLimbs;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < LimbSegments.Length; i++)
                        bw.Write(LimbSegments[i].VAddr);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(LimbSegments))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % 4) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 4"
                    );

                LimbSegments = new SegmentedAddress[data.Length / 4];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < LimbSegments.Length; i++)
                        LimbSegments[i] = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => LimbSegments.Length * 4;
        }

        public class SkeletonHolder : ObjectHolder
        {
            public const int HEADER_SIZE = 0x8;

            public byte LimbCount { get; set; }
            public SegmentedAddress LimbsSeg { get; set; }

            public SkeletonHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.SkeletonHeader;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(LimbsSeg.VAddr);
                    bw.Write(LimbCount);
                    bw.Write(new byte[3]); // padding
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(LimbsSeg))]
            public override void SetData(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    LimbsSeg = new SegmentedAddress(br.ReadUInt32());
                    LimbCount = br.Read1Byte();
                }
            }

            public override int GetSize() => HEADER_SIZE;
        }

        public class FlexSkeletonHolder : SkeletonHolder
        {
            public new const int HEADER_SIZE = SkeletonHolder.HEADER_SIZE + 0x4;

            public byte DListCount { get; set; }

            public FlexSkeletonHolder(string name, byte[] data)
                : base(name, data) { }

            public override EntryType GetEntryType() => EntryType.FlexSkeletonHeader;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(LimbsSeg.VAddr);
                    bw.Write(LimbCount);
                    bw.Write(new byte[3]); // padding
                    bw.Write(DListCount);
                    bw.Write(new byte[3]); // padding
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            public override void SetData(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    LimbsSeg = new SegmentedAddress(br.ReadUInt32());
                    LimbCount = br.Read1Byte();
                    br.ReadBytes(3); // padding
                    DListCount = br.Read1Byte();
                    br.ReadBytes(3); // padding
                }
            }

            public override int GetSize() => HEADER_SIZE;
        }

        public class AnimationHolder : ObjectHolder
        {
            public const int HEADER_SIZE = 0x10;

            public short FrameCount { get; set; }
            public SegmentedAddress FrameData { get; set; }
            public SegmentedAddress JointIndices { get; set; }
            public ushort StaticIndexMax { get; set; }

            public bool extAnim { get; set; }

            public AnimationHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.AnimationHeader;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(FrameCount);
                    bw.Write(new byte[2]); // padding
                    bw.Write(FrameData.VAddr);
                    bw.Write(JointIndices.VAddr);
                    bw.Write(StaticIndexMax);
                    bw.Write(new byte[2]); // padding
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(FrameData), nameof(JointIndices))]
            public override void SetData(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    FrameCount = br.ReadInt16();
                    br.ReadBytes(2); // padding
                    FrameData = new SegmentedAddress(br.ReadUInt32());
                    JointIndices = new SegmentedAddress(br.ReadUInt32());
                    StaticIndexMax = br.ReadUInt16();
                }
            }

            public override int GetSize() => HEADER_SIZE;
        }

        public class AnimationFrameDataHolder : ObjectHolder
        {
            public short[] FrameData { get; set; }

            public AnimationFrameDataHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.FrameData;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    foreach (var item in FrameData)
                        bw.Write(item);
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(FrameData))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % 2) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 2"
                    );

                using (var ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    FrameData = new short[data.Length / 2];
                    for (int i = 0; i < FrameData.Length; i++)
                        FrameData[i] = br.ReadInt16();
                }
            }

            public override int GetSize() => FrameData.Length * 2;
        }

        public class AnimationJointIndicesHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 6;

            public struct JointIndex
            {
                public ushort X,
                    Y,
                    Z;
            };

            public JointIndex[] JointIndices { get; set; }

            public AnimationJointIndicesHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.JointIndices;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < JointIndices.Length; i++)
                    {
                        bw.Write(JointIndices[i].X);
                        bw.Write(JointIndices[i].Y);
                        bw.Write(JointIndices[i].Z);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(JointIndices))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 6"
                    );

                JointIndices = new JointIndex[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < JointIndices.Length; i++)
                        JointIndices[i] = new JointIndex()
                        {
                            X = br.ReadUInt16(),
                            Y = br.ReadUInt16(),
                            Z = br.ReadUInt16(),
                        };
                }
            }

            public override int GetSize() => JointIndices.Length * ENTRY_SIZE;
        }

        public class PlayerAnimationHolder : ObjectHolder
        {
            public const int SIZE = 0x8;
            public const int PLAYER_LIMB_COUNT = 22;

            public short FrameCount { get; set; }
            public SegmentedAddress PlayerAnimationSegment { get; set; }
            public bool extAnim { get; set; }

            public PlayerAnimationHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.PlayerAnimationHeader;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(FrameCount);
                    bw.Write(new byte[2]); // padding
                    bw.Write(PlayerAnimationSegment.VAddr);
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(PlayerAnimationSegment))]
            public override void SetData(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    FrameCount = br.ReadInt16();
                    br.ReadBytes(2); // padding
                    PlayerAnimationSegment = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => SIZE;
        }

        public class PlayerAnimationJointTableHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 6;

            public struct JointTableEntry
            {
                public short X,
                    Y,
                    Z;
            };

            public JointTableEntry[,] JointTable { get; set; }
            public short[] Unknown { get; set; }

            public PlayerAnimationJointTableHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.PlayerAnimationJointTable;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    int frameCount = JointTable.GetLength(0);

                    for (int frame = 0; frame < frameCount; frame++)
                    {
                        for (int i = 0; i < JointTable.GetLength(1); i++)
                        {
                            bw.Write(JointTable[frame, i].X);
                            bw.Write(JointTable[frame, i].Y);
                            bw.Write(JointTable[frame, i].Z);
                        }

                        bw.Write(Unknown[frame]);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(JointTable), nameof(Unknown))]
            public override void SetData(byte[] data)
            {
                // TODO: Validate size

                int frameCount =
                    data.Length / (((PlayerAnimationHolder.PLAYER_LIMB_COUNT * 3) + 1) * 2);
                JointTable = new JointTableEntry[
                    frameCount,
                    PlayerAnimationHolder.PLAYER_LIMB_COUNT
                ];
                Unknown = new short[frameCount];

                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int frame = 0; frame < frameCount; frame++)
                    {
                        for (int i = 0; i < JointTable.GetLength(1); i++)
                        {
                            JointTable[frame, i] = new JointTableEntry()
                            {
                                X = br.ReadInt16(),
                                Y = br.ReadInt16(),
                                Z = br.ReadInt16(),
                            };
                        }

                        Unknown[frame] = br.ReadInt16();
                    }
                }
            }

            public override int GetSize() => JointTable.Length * ENTRY_SIZE;
        }

        public class ColHeaderHolder : ObjectHolder
        {
            public const int COLHEADER_SIZE = 0x2C;

            public Vec3s MinBounds { get; set; }
            public Vec3s MaxBounds { get; set; }
            public ushort NbVertices { get; set; }
            public SegmentedAddress VertexListSeg { get; set; }
            public ushort NbPolygons { get; set; }
            public SegmentedAddress PolyListSeg { get; set; }
            public SegmentedAddress SurfaceTypeSeg { get; set; }
            public SegmentedAddress CamDataSeg { get; set; }
            public ushort NbWaterBoxes { get; set; }
            public SegmentedAddress WaterBoxSeg { get; set; }

            public CollisionVerticesHolder? VerticesHolder { get; set; }
            public CollisionPolygonsHolder? PolygonsHolder { get; set; }
            public CollisionSurfaceTypesHolder? SurfaceTypesHolder { get; set; }
            public CollisionCamDataHolder? CamDataHolder { get; set; }
            public WaterBoxHolder? WaterBoxHolder { get; set; }

            public ColHeaderHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
                VerticesHolder = null;
                PolygonsHolder = null;
                SurfaceTypesHolder = null;
                CamDataHolder = null;
                WaterBoxHolder = null;
            }

            public override EntryType GetEntryType() => EntryType.CollisionHeader;

            public override byte[] GetData()
            {
                using (var ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                    bw.Write(MinBounds.X);
                    bw.Write(MinBounds.Y);
                    bw.Write(MinBounds.Z);
                    bw.Write(MaxBounds.X);
                    bw.Write(MaxBounds.Y);
                    bw.Write(MaxBounds.Z);
                    bw.Write(NbVertices);
                    bw.WriteInt16(0);
                    bw.Write(VertexListSeg.VAddr);
                    bw.Write(NbPolygons);
                    bw.WriteInt16(0);
                    bw.Write(PolyListSeg.VAddr);
                    bw.Write(SurfaceTypeSeg.VAddr);
                    bw.Write(CamDataSeg.VAddr);
                    bw.Write(NbWaterBoxes);
                    bw.WriteInt16(0);
                    bw.Write(WaterBoxSeg.VAddr);
                    return ms.ToArray().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(
                nameof(VertexListSeg),
                nameof(PolyListSeg),
                nameof(SurfaceTypeSeg),
                nameof(CamDataSeg),
                nameof(WaterBoxSeg)
            )]
            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                    MinBounds = new Vec3s()
                    {
                        X = br.ReadInt16(),
                        Y = br.ReadInt16(),
                        Z = br.ReadInt16(),
                    };
                    MaxBounds = new Vec3s()
                    {
                        X = br.ReadInt16(),
                        Y = br.ReadInt16(),
                        Z = br.ReadInt16(),
                    };
                    NbVertices = br.ReadUInt16();
                    br.ReadUInt16();
                    VertexListSeg = new SegmentedAddress(br.ReadUInt32());
                    NbPolygons = br.ReadUInt16();
                    br.ReadUInt16();
                    PolyListSeg = new SegmentedAddress(br.ReadUInt32());
                    SurfaceTypeSeg = new SegmentedAddress(br.ReadUInt32());
                    CamDataSeg = new SegmentedAddress(br.ReadUInt32());
                    NbWaterBoxes = br.ReadUInt16();
                    br.ReadUInt16();
                    WaterBoxSeg = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => COLHEADER_SIZE;
        }

        public class CollisionVerticesHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 6;

            public Vec3s[] Points { get; set; }

            public CollisionVerticesHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.CollisionVertices;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < Points.Length; i++)
                    {
                        bw.Write(Points[i].X);
                        bw.Write(Points[i].Y);
                        bw.Write(Points[i].Z);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(Points))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 6"
                    );

                Points = new Vec3s[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < Points.Length; i++)
                        Points[i] = new Vec3s()
                        {
                            X = br.ReadInt16(),
                            Y = br.ReadInt16(),
                            Z = br.ReadInt16(),
                        };
                }
            }

            public override int GetSize() => Points.Length * ENTRY_SIZE;
        }

        public class CollisionPolygonsHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 0x10;

            public struct CollisionPoly
            {
                public ushort Type;
                public ushort[] Data;
                public Vec3s Normal;
                public short Dist;
            }

            public CollisionPoly[] CollisionPolys { get; set; }

            public CollisionPolygonsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.CollisionPolygons;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < CollisionPolys.Length; i++)
                    {
                        bw.Write(CollisionPolys[i].Type);
                        bw.Write(CollisionPolys[i].Data[0]);
                        bw.Write(CollisionPolys[i].Data[1]);
                        bw.Write(CollisionPolys[i].Data[2]);
                        bw.Write(CollisionPolys[i].Normal.X);
                        bw.Write(CollisionPolys[i].Normal.Y);
                        bw.Write(CollisionPolys[i].Normal.Z);
                        bw.Write(CollisionPolys[i].Dist);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(CollisionPolys))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 6"
                    );

                CollisionPolys = new CollisionPoly[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < CollisionPolys.Length; i++)
                        CollisionPolys[i] = new CollisionPoly()
                        {
                            Type = br.ReadUInt16(),
                            Data = new ushort[3]
                            {
                                br.ReadUInt16(),
                                br.ReadUInt16(),
                                br.ReadUInt16(),
                            },
                            Normal = new Vec3s()
                            {
                                X = br.ReadInt16(),
                                Y = br.ReadInt16(),
                                Z = br.ReadInt16(),
                            },
                            Dist = br.ReadInt16(),
                        };
                }
            }

            public override int GetSize() => CollisionPolys.Length * ENTRY_SIZE;

            public ushort LargestPolyType()
            {
                return CollisionPolys.Max(poly => poly.Type);
            }
        }

        public class CollisionSurfaceTypesHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 8;

            public uint[][] SurfaceTypes { get; set; }

            public CollisionSurfaceTypesHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.CollisionSurfaceTypes;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < SurfaceTypes.Length; i++)
                    {
                        bw.Write(SurfaceTypes[i][0]);
                        bw.Write(SurfaceTypes[i][1]);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(SurfaceTypes))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 8"
                    );

                SurfaceTypes = new uint[data.Length / ENTRY_SIZE][];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < SurfaceTypes.Length; i++)
                        SurfaceTypes[i] = new uint[2] { br.ReadUInt32(), br.ReadUInt32() };
                }
            }

            public override int GetSize() => SurfaceTypes.Length * ENTRY_SIZE;
        }

        public class CollisionCamDataHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 8;

            public struct ColCamData
            {
                public ushort CameraSType;
                public short NumCameras;
                public SegmentedAddress CamPosData;
            };

            public ColCamData[] CamData { get; set; }

            public CollisionCamDataHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.CollisionCamData;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < CamData.Length; i++)
                    {
                        bw.Write(CamData[i].CameraSType);
                        bw.Write(CamData[i].NumCameras);
                        bw.Write(CamData[i].CamPosData.VAddr);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(CamData))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 8"
                    );

                CamData = new ColCamData[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < CamData.Length; i++)
                        CamData[i] = new ColCamData
                        {
                            CameraSType = br.ReadUInt16(),
                            NumCameras = br.ReadInt16(),
                            CamPosData = new SegmentedAddress(br.ReadUInt32()),
                        };
                }
            }

            public override int GetSize() => CamData.Length * ENTRY_SIZE;
        }

        public class WaterBoxHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 0x10;

            public struct WaterBox
            {
                public short XMin;
                public short YSurface;
                public short ZMin;
                public short XLength,
                    ZLength;
                public uint Properties;
            };

            public WaterBox[] WaterBoxes { get; set; }

            public WaterBoxHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.WaterBox;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < WaterBoxes.Length; i++)
                    {
                        bw.Write(WaterBoxes[i].XMin);
                        bw.Write(WaterBoxes[i].YSurface);
                        bw.Write(WaterBoxes[i].ZMin);
                        bw.Write(WaterBoxes[i].XLength);
                        bw.Write(WaterBoxes[i].ZLength);
                        bw.WriteUInt16(0);
                        bw.Write(WaterBoxes[i].Properties);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(WaterBoxes))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 0x10"
                    );

                WaterBoxes = new WaterBox[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < WaterBoxes.Length; i++)
                    {
                        WaterBoxes[i] = new WaterBox
                        {
                            XMin = br.ReadInt16(),
                            YSurface = br.ReadInt16(),
                            ZMin = br.ReadInt16(),
                            XLength = br.ReadInt16(),
                            ZLength = br.ReadInt16(),
                        };
                        br.ReadUInt16();
                        WaterBoxes[i].Properties = br.ReadUInt32();
                    }
                }
            }

            public override int GetSize() => WaterBoxes.Length * ENTRY_SIZE;
        }

        public class MatAnimHeaderHolder : ObjectHolder
        {
            public const int SIZE = 8;

            public byte SegmentId { get; set; }
            public short Type { get; set; }
            public SegmentedAddress ParamsSeg { get; set; }

            public MatAnimHeaderHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimHeader;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    bw.Write(SegmentId);
                    bw.Write((byte)0);
                    bw.Write(Type);
                    bw.Write(ParamsSeg.VAddr);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(ParamsSeg))]
            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    SegmentId = br.Read1Byte();
                    br.Read1Byte();
                    Type = br.ReadInt16();
                    ParamsSeg = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => SIZE;
        }

        public class MatAnimTexScrollParamsHolder : ObjectHolder
        {
            public const int SIZE = 4;

            public byte StepX { get; set; }
            public byte StepY { get; set; }
            public byte Width { get; set; }
            public byte Height { get; set; }

            public MatAnimTexScrollParamsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimTexScrollParams;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    bw.Write(StepX);
                    bw.Write(StepY);
                    bw.Write(Width);
                    bw.Write(Height);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    StepX = br.Read1Byte();
                    StepY = br.Read1Byte();
                    Width = br.Read1Byte();
                    Height = br.Read1Byte();
                }
            }

            public override int GetSize() => SIZE;
        }

        public class MatAnimColorParamsHolder : ObjectHolder
        {
            public const int SIZE = 0x10;

            public ushort KeyFrameLength { get; set; }
            public ushort KeyFrameCount { get; set; }
            public SegmentedAddress PrimColors { get; set; }
            public SegmentedAddress EnvColors { get; set; }
            public SegmentedAddress KeyFrames { get; set; }

            public MatAnimColorParamsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimColorParams;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    bw.Write(KeyFrameLength);
                    bw.Write(KeyFrameCount);
                    bw.Write(PrimColors.VAddr);
                    bw.Write(EnvColors.VAddr);
                    bw.Write(KeyFrames.VAddr);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(PrimColors), nameof(EnvColors), nameof(KeyFrames))]
            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    KeyFrameLength = br.ReadUInt16();
                    KeyFrameCount = br.ReadUInt16();
                    PrimColors = new SegmentedAddress(br.ReadUInt32());
                    EnvColors = new SegmentedAddress(br.ReadUInt32());
                    KeyFrames = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => SIZE;
        }

        public class MatAnimPrimColorsHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 5;

            public struct MatAnimPrimColor
            {
                public byte R,
                    G,
                    B,
                    A,
                    LodFrac;
            };

            public MatAnimPrimColor[] PrimColors { get; set; }

            public MatAnimPrimColorsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimPrimColors;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < PrimColors.Length; i++)
                    {
                        bw.Write(PrimColors[i].R);
                        bw.Write(PrimColors[i].G);
                        bw.Write(PrimColors[i].B);
                        bw.Write(PrimColors[i].A);
                        bw.Write(PrimColors[i].LodFrac);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(PrimColors))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 5"
                    );

                PrimColors = new MatAnimPrimColor[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < PrimColors.Length; i++)
                        PrimColors[i] = new MatAnimPrimColor()
                        {
                            R = br.Read1Byte(),
                            G = br.Read1Byte(),
                            B = br.Read1Byte(),
                            A = br.Read1Byte(),
                            LodFrac = br.Read1Byte(),
                        };
                }
            }

            public override int GetSize() => PrimColors.Length * ENTRY_SIZE;
        }

        public class MatAnimEnvColorsHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 4;

            public struct MatAnimEnvColor
            {
                public byte R,
                    G,
                    B,
                    A;
            };

            public MatAnimEnvColor[] EnvColors { get; set; }

            public MatAnimEnvColorsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimEnvColors;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < EnvColors.Length; i++)
                    {
                        bw.Write(EnvColors[i].R);
                        bw.Write(EnvColors[i].G);
                        bw.Write(EnvColors[i].B);
                        bw.Write(EnvColors[i].A);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(EnvColors))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 4"
                    );

                EnvColors = new MatAnimEnvColor[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < EnvColors.Length; i++)
                        EnvColors[i] = new MatAnimEnvColor()
                        {
                            R = br.Read1Byte(),
                            G = br.Read1Byte(),
                            B = br.Read1Byte(),
                            A = br.Read1Byte(),
                        };
                }
            }

            public override int GetSize() => EnvColors.Length * ENTRY_SIZE;
        }

        public class MatAnimKeyFramesHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 2;

            public ushort[] KeyFrames { get; set; }

            public MatAnimKeyFramesHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimKeyFrames;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < KeyFrames.Length; i++)
                    {
                        bw.Write(KeyFrames[i]);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(KeyFrames))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 2"
                    );

                KeyFrames = new ushort[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < KeyFrames.Length; i++)
                        KeyFrames[i] = br.ReadUInt16();
                }
            }

            public override int GetSize() => KeyFrames.Length * ENTRY_SIZE;
        }

        public class MatAnimTexCycleParamsHolder : ObjectHolder
        {
            public const int SIZE = 0xC;

            public ushort KeyFrameLength { get; set; }
            public SegmentedAddress TextureList { get; set; }
            public SegmentedAddress TextureIndexList { get; set; }

            public MatAnimTexCycleParamsHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimTexCycleParams;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    bw.Write(KeyFrameLength);
                    bw.WriteUInt16(0);
                    bw.Write(TextureList.VAddr);
                    bw.Write(TextureIndexList.VAddr);

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(TextureList), nameof(TextureIndexList))]
            public override void SetData(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    KeyFrameLength = br.ReadUInt16();
                    br.ReadUInt16();
                    TextureList = new SegmentedAddress(br.ReadUInt32());
                    TextureIndexList = new SegmentedAddress(br.ReadUInt32());
                }
            }

            public override int GetSize() => SIZE;
        }

        public class MatAnimTextureIndexListHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 1;

            public byte[] TextureIndices { get; set; }

            public MatAnimTextureIndexListHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimTextureIndexList;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < TextureIndices.Length; i++)
                    {
                        bw.Write(TextureIndices[i]);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(TextureIndices))]
            public override void SetData(byte[] data)
            {
                TextureIndices = new byte[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < TextureIndices.Length; i++)
                    {
                        TextureIndices[i] = br.Read1Byte();
                    }
                }
            }

            public override int GetSize() => TextureIndices.Length * ENTRY_SIZE;
        }

        public class MatAnimTextureListHolder : ObjectHolder
        {
            public const int ENTRY_SIZE = 4;

            public SegmentedAddress[] TextureSegments { get; set; }

            public MatAnimTextureListHolder(string name, byte[] data)
                : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.MatAnimTextureList;

            public override byte[] GetData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < TextureSegments.Length; i++)
                    {
                        bw.Write(TextureSegments[i].VAddr);
                    }

                    return ms.GetBuffer().Take((int)ms.Length).ToArray();
                }
            }

            [MemberNotNull(nameof(TextureSegments))]
            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException(
                        $"Invalid data size (0x{data.Length:X}) should be a multiple of 0x10"
                    );

                TextureSegments = new SegmentedAddress[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < TextureSegments.Length; i++)
                    {
                        TextureSegments[i] = new SegmentedAddress(br.ReadUInt32());
                    }
                }
            }

            public override int GetSize() => TextureSegments.Length * ENTRY_SIZE;
        }

        public Z64Game? Game;
        public string? FileName;
        public List<ObjectHolder> Entries { get; set; }

        public Z64Object()
        {
            Game = null;
            Entries = new List<ObjectHolder>();
        }

        public Z64Object(Z64Game? game, byte[] data, string fileName)
            : this()
        {
            Game = game;
            FileName = fileName;

            AddUnknow(data.Length);
            SetData(data);
        }

        private bool HolderOverlaps(ObjectHolder holder, int holderOff)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (
                    holderOff >= entryOff
                    && holderOff + holder.GetSize() <= entryOff + entry.GetSize()
                    && entry.GetEntryType() == EntryType.Unknown
                )
                    return false;

                entryOff += entry.GetSize();
            }
            return true;
        }

        private bool VertexHolderOverlaps(VertexHolder holder, int holderOff)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (
                    (holderOff >= entryOff && holderOff < entryOff + entry.GetSize())
                    || (
                        entryOff >= holderOff
                        && entryOff + entry.GetSize() > holderOff + holder.GetSize()
                    )
                    || (
                        holderOff <= entryOff
                        && holderOff + holder.GetSize() >= entryOff + entry.GetSize()
                    )
                )
                {
                    if (
                        holder.GetEntryType() != EntryType.Vertex
                        && holder.GetEntryType() != EntryType.Unknown
                    )
                        return true;
                }
                entryOff += entry.GetSize();
            }
            return false;
        }

        private ObjectHolder AddHolder(ObjectHolder holder, int holderOff = -1)
        {
            if (holder.GetSize() <= 0)
                throw new Exception("Invalid holder size");

            if (holderOff == -1)
                holderOff = GetSize();

            if (holderOff == GetSize())
            {
                Entries.Add(holder);
                return holder;
            }
            else if (holderOff > GetSize())
            {
                AddUnknow(holderOff - GetSize());
                Entries.Add(holder);
                return holder;
            }
            else if (!HolderOverlaps(holder, holderOff))
            {
                int entryOff = 0;
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (
                        holderOff >= entryOff
                        && (holderOff + holder.GetSize()) <= entryOff + Entries[i].GetSize()
                    )
                    {
                        int startDiff = holderOff - entryOff;
                        int endDiff =
                            (entryOff + Entries[i].GetSize()) - (holderOff + holder.GetSize());

                        List<ObjectHolder> newEntries = new List<ObjectHolder>()
                        {
                            new UnknowHolder($"unk_{entryOff:X8}", new byte[startDiff]),
                            holder,
                            new UnknowHolder(
                                $"unk_{(holderOff + holder.GetSize()):X8}",
                                new byte[endDiff]
                            ),
                        }.FindAll(e => e.GetSize() > 0);

                        Entries.RemoveAt(i);
                        Entries.InsertRange(i, newEntries);

                        break;
                    }

                    entryOff += Entries[i].GetSize();
                }
                return holder;
            }
            else
            {
                var existing = Entries.Find(e =>
                    e.GetSize() == holder.GetSize() && OffsetOf(e) == holderOff
                );
                if (existing != null)
                    return existing;

                throw new Z64ObjectException(
                    $"Overlapping data (type={holder.GetEntryType()}, off=0x{holderOff:X}, size=0x{holder.GetSize():X})"
                );
            }
        }

        // it's pretty common to see vertices overlap
        private void AddVertexHolder(VertexHolder holder, int holderOff = -1)
        {
            if (holder.GetSize() <= 0)
                return;

            if (holderOff == -1)
                holderOff = GetSize();

            if (holderOff == GetSize())
            {
                Entries.Add(holder);
            }
            else if (holderOff > GetSize())
            {
                AddUnknow(holderOff - GetSize());
                Entries.Add(holder);
            }
            else // Check if fits ?
            {
                int entryOff = 0;
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (holderOff >= entryOff && holderOff < entryOff + Entries[i].GetSize())
                    {
                        if (
                            Entries[i].GetEntryType() == EntryType.Vertex
                            || Entries[i].GetEntryType() == EntryType.Unknown
                        )
                        {
                            if (entryOff + Entries[i].GetSize() >= holderOff + holder.GetSize())
                            {
                                int startDiff = holderOff - entryOff;
                                int endDiff =
                                    (entryOff + Entries[i].GetSize())
                                    - (holderOff + holder.GetSize());
                                if (
                                    (
                                        Entries[i].GetEntryType() == EntryType.Vertex
                                        && startDiff % 0x10 != 0
                                    )
                                    || (
                                        Entries[i].GetEntryType() == EntryType.Vertex
                                        && endDiff % 0x10 != 0
                                    )
                                )
                                    throw new Z64ObjectException(
                                        "Invalid size for a vertex buffer"
                                    );

                                List<ObjectHolder> newEntries = new List<ObjectHolder>()
                                {
                                    Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)
                                            new UnknowHolder(
                                                $"unk_{entryOff:X8}",
                                                new byte[startDiff]
                                            )
                                        : new VertexHolder(
                                            $"vtx_{entryOff:X8}",
                                            new Vertex[startDiff / 0x10].ToList()
                                        ),
                                    new VertexHolder($"vtx_{holderOff:X8}", holder.Vertices),
                                    Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)
                                            new UnknowHolder(
                                                $"unk_{entryOff:X8}",
                                                new byte[endDiff]
                                            )
                                        : new VertexHolder(
                                            $"vtx_{(holderOff + holder.GetSize()):X8}",
                                            new Vertex[endDiff / 0x10].ToList()
                                        ),
                                }.FindAll(e => e.GetSize() > 0);

                                Entries.RemoveAt(i);
                                Entries.InsertRange(i, newEntries);

                                newEntries.ForEach(o => entryOff += o.GetSize());

                                return;
                            }
                            else
                            {
                                int startDiff = holderOff - entryOff;
                                int endDiff = (entryOff + Entries[i].GetSize()) - holderOff;
                                if (
                                    (
                                        Entries[i].GetEntryType() == EntryType.Vertex
                                        && startDiff % 0x10 != 0
                                    )
                                    || (
                                        Entries[i].GetEntryType() == EntryType.Vertex
                                        && endDiff % 0x10 != 0
                                    )
                                )
                                    throw new Z64ObjectException(
                                        "Invalid size for a vertex buffer"
                                    );

                                List<ObjectHolder> newEntries = new List<ObjectHolder>()
                                {
                                    Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)
                                            new UnknowHolder(
                                                $"unk_{entryOff:X8}",
                                                new byte[startDiff]
                                            )
                                        : new VertexHolder(
                                            $"vtx_{entryOff:X8}",
                                            new Vertex[startDiff / 0x10].ToList()
                                        ),
                                    new VertexHolder(
                                        $"vtx_{holderOff:X8}",
                                        new Vertex[endDiff / 0x10].ToList()
                                    ),
                                }.FindAll(e => e.GetSize() > 0);
                                holder = new VertexHolder(
                                    $"vtx_{(entryOff + Entries[i].GetSize()):X8}",
                                    new Vertex[holder.Vertices.Count - (endDiff / 0x10)].ToList()
                                );

                                Entries.RemoveAt(i);
                                Entries.InsertRange(i, newEntries);

                                newEntries.ForEach(o => entryOff += o.GetSize());

                                i += newEntries.Count - 1;
                                holderOff = entryOff;
                            }
                        }
                        else
                            throw new Z64ObjectException(
                                $"Vertex did not fit (off=0x{holderOff:X}, size=0x{holder.GetSize():X})"
                            );
                    }
                    else
                    {
                        entryOff += Entries[i].GetSize();
                    }
                }

                if (holder.GetSize() > 0)
                    Entries.Add(holder);
            }
        }

        public DListHolder AddDList(int size, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new DListHolder(name ?? $"dlist_{off:X8}", new byte[size]);
            return (DListHolder)AddHolder(holder, off);
        }

        public UnknowHolder AddUnknow(int size, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new UnknowHolder(name ?? $"unk_{off:X8}", new byte[size]);
            return (UnknowHolder)AddHolder(holder, off);
        }

        public TextureHolder AddTexture(
            int w,
            int h,
            N64TexFormat format,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new TextureHolder(
                name ?? $"tex_{off:X8}",
                w,
                h,
                format,
                new byte[N64Texture.GetTexSize(w * h, format)]
            );
            return (TextureHolder)AddHolder(holder, off);
        }

        public VertexHolder AddVertices(int vtxCount, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new VertexHolder(name ?? $"vtx_{off:X8}", new Vertex[vtxCount].ToList());
            AddVertexHolder(holder, off);
            return holder;
        }

        public MtxHolder AddMtx(int mtxCount, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new MtxHolder(name ?? $"mtx_{off:X8}", new byte[mtxCount * Mtx.SIZE]);
            return (MtxHolder)AddHolder(holder, off);
        }

        public SkeletonLimbHolder AddSkeletonLimb(
            EntryType type,
            string? name = null,
            int off = -1,
            int skel_off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var limbTypeName =
                (type == EntryType.StandardLimb)
                    ? "standardlimb"
                    : ((type == EntryType.LODLimb) ? "lodlimb" : "skinlimb");
            var limbTypeSize =
                (type == EntryType.StandardLimb)
                    ? SkeletonLimbHolder.STANDARD_LIMB_SIZE
                    : (
                        (type == EntryType.LODLimb)
                            ? SkeletonLimbHolder.LOD_LIMB_SIZE
                            : SkeletonLimbHolder.SKIN_LIMB_SIZE
                    );
            var holder = new SkeletonLimbHolder(
                name ?? $"skel_{skel_off:X8}_{limbTypeName}_{off:X8}",
                new byte[limbTypeSize],
                type
            );
            return (SkeletonLimbHolder)AddHolder(holder, off);
        }

        public SkeletonLimbsHolder AddSkeletonLimbs(
            int count,
            string? name = null,
            int off = -1,
            int skel_off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new SkeletonLimbsHolder(
                name ?? $"skel_{skel_off:X8}_limbs_{off:X8}",
                new byte[count * 4]
            );
            return (SkeletonLimbsHolder)AddHolder(holder, off);
        }

        public SkeletonHolder AddSkeleton(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new SkeletonHolder(
                name ?? $"skel_{off:X8}",
                new byte[SkeletonHolder.HEADER_SIZE]
            );
            return (SkeletonHolder)AddHolder(holder, off);
        }

        public FlexSkeletonHolder AddFlexSkeleton(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new FlexSkeletonHolder(
                name ?? $"skel_{off:X8}",
                new byte[FlexSkeletonHolder.HEADER_SIZE]
            );
            return (FlexSkeletonHolder)AddHolder(holder, off);
        }

        public PlayerAnimationHolder AddPlayerAnimation(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new PlayerAnimationHolder(
                name ?? $"playeranim_{off:X8}",
                new byte[PlayerAnimationHolder.SIZE]
            );
            return (PlayerAnimationHolder)AddHolder(holder, off);
        }

        public AnimationHolder AddAnimation(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new AnimationHolder(
                name ?? $"anim_{off:X8}",
                new byte[AnimationHolder.HEADER_SIZE]
            );
            return (AnimationHolder)AddHolder(holder, off);
        }

        public AnimationFrameDataHolder AddFrameData(int count, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new AnimationFrameDataHolder(
                name ?? $"framedata_{off:X8}",
                new byte[count * 2]
            );
            return (AnimationFrameDataHolder)AddHolder(holder, off);
        }

        public AnimationJointIndicesHolder AddJointIndices(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new AnimationJointIndicesHolder(
                name ?? $"jointindices_{off:X8}",
                new byte[count * AnimationJointIndicesHolder.ENTRY_SIZE]
            );
            return (AnimationJointIndicesHolder)AddHolder(holder, off);
        }

        public ColHeaderHolder AddCollisionHeader(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new ColHeaderHolder(
                name ?? $"colheader_{off:X8}",
                new byte[ColHeaderHolder.COLHEADER_SIZE]
            );
            return (ColHeaderHolder)AddHolder(holder, off);
        }

        public CollisionVerticesHolder AddCollisionVertices(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new CollisionVerticesHolder(
                name ?? $"colvertices_{off:X8}",
                new byte[count * CollisionVerticesHolder.ENTRY_SIZE]
            );
            return (CollisionVerticesHolder)AddHolder(holder, off);
        }

        public CollisionPolygonsHolder AddCollisionPolygons(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new CollisionPolygonsHolder(
                name ?? $"colpolys_{off:X8}",
                new byte[count * CollisionPolygonsHolder.ENTRY_SIZE]
            );
            return (CollisionPolygonsHolder)AddHolder(holder, off);
        }

        public CollisionSurfaceTypesHolder AddCollisionSurfaceTypes(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new CollisionSurfaceTypesHolder(
                name ?? $"colsurfacetypes_{off:X8}",
                new byte[count * CollisionSurfaceTypesHolder.ENTRY_SIZE]
            );
            return (CollisionSurfaceTypesHolder)AddHolder(holder, off);
        }

        public CollisionCamDataHolder AddCollisionCamData(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new CollisionCamDataHolder(
                name ?? $"colcamdata_{off:X8}",
                new byte[count * CollisionCamDataHolder.ENTRY_SIZE]
            );
            return (CollisionCamDataHolder)AddHolder(holder, off);
        }

        public WaterBoxHolder AddWaterBoxes(int count, string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new WaterBoxHolder(
                name ?? $"waterbox_{off:X8}",
                new byte[count * WaterBoxHolder.ENTRY_SIZE]
            );
            return (WaterBoxHolder)AddHolder(holder, off);
        }

        public MatAnimHeaderHolder AddMatAnimHeader(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimHeaderHolder(
                name ?? $"matanimheader_{off:X8}",
                new byte[MatAnimHeaderHolder.SIZE]
            );

            // TODO: REALLY stupid hack to get object_link_goron to load
            try
            {
                return (MatAnimHeaderHolder)AddHolder(holder, off);
            }
            catch (Exception)
            {
                return holder;
            }
        }

        public MatAnimTexScrollParamsHolder AddMatAnimTexScrollParams(
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimTexScrollParamsHolder(
                name ?? $"texscrollparams_{off:X8}",
                new byte[MatAnimTexScrollParamsHolder.SIZE]
            );
            return (MatAnimTexScrollParamsHolder)AddHolder(holder, off);
        }

        public MatAnimColorParamsHolder AddMatAnimColorParams(string? name = null, int off = -1)
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimColorParamsHolder(
                name ?? $"colorparams_{off:X8}",
                new byte[MatAnimColorParamsHolder.SIZE]
            );
            return (MatAnimColorParamsHolder)AddHolder(holder, off);
        }

        public MatAnimPrimColorsHolder AddMatAnimPrimColors(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimPrimColorsHolder(
                name ?? $"primcolors_{off:X8}",
                new byte[count * MatAnimPrimColorsHolder.ENTRY_SIZE]
            );
            return (MatAnimPrimColorsHolder)AddHolder(holder, off);
        }

        public MatAnimEnvColorsHolder AddMatAnimEnvColors(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimEnvColorsHolder(
                name ?? $"envcolors_{off:X8}",
                new byte[count * MatAnimEnvColorsHolder.ENTRY_SIZE]
            );
            return (MatAnimEnvColorsHolder)AddHolder(holder, off);
        }

        public MatAnimKeyFramesHolder AddMatAnimKeyFrames(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimKeyFramesHolder(
                name ?? $"keyframes_{off:X8}",
                new byte[count * MatAnimKeyFramesHolder.ENTRY_SIZE]
            );
            return (MatAnimKeyFramesHolder)AddHolder(holder, off);
        }

        public MatAnimTexCycleParamsHolder AddMatAnimTexCycleParams(
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimTexCycleParamsHolder(
                name ?? $"texcycleparams_{off:X8}",
                new byte[MatAnimTexCycleParamsHolder.SIZE]
            );
            return (MatAnimTexCycleParamsHolder)AddHolder(holder, off);
        }

        public MatAnimTextureIndexListHolder AddMatAnimTextureIndexList(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimTextureIndexListHolder(
                name ?? $"waterbox_{off:X8}",
                new byte[count * MatAnimTextureIndexListHolder.ENTRY_SIZE]
            );
            return (MatAnimTextureIndexListHolder)AddHolder(holder, off);
        }

        public MatAnimTextureListHolder AddMatAnimTextureList(
            int count,
            string? name = null,
            int off = -1
        )
        {
            if (off == -1)
                off = GetSize();
            var holder = new MatAnimTextureListHolder(
                name ?? $"waterbox_{off:X8}",
                new byte[count * MatAnimTextureListHolder.ENTRY_SIZE]
            );
            return (MatAnimTextureListHolder)AddHolder(holder, off);
        }

        public void FixNames()
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                switch (entry.GetEntryType())
                {
                    case EntryType.PlayerAnimationHeader:
                        entry.Name = "playeranim_" + entryOff.ToString("X8");
                        break;
                    case EntryType.AnimationHeader:
                        entry.Name = "anim_" + entryOff.ToString("X8");
                        break;
                    case EntryType.FrameData:
                        entry.Name = "framedata_" + entryOff.ToString("X8");
                        break;
                    case EntryType.JointIndices:
                        entry.Name = "jointindices_" + entryOff.ToString("X8");
                        break;
                    case EntryType.SkeletonHeader:
                    case EntryType.FlexSkeletonHeader:
                        entry.Name = "skel_" + entryOff.ToString("X8");
                        break;
                    case EntryType.SkeletonLimbs:
                        entry.Name = "limbs_" + entryOff.ToString("X8");
                        break;
                    case EntryType.StandardLimb:
                        entry.Name = "standardlimb_" + entryOff.ToString("X8");
                        break;
                    case EntryType.LODLimb:
                        entry.Name = "lodlimb_" + entryOff.ToString("X8");
                        break;
                    case EntryType.SkinLimb:
                        entry.Name = "skinlimb_" + entryOff.ToString("X8");
                        break;
                    case EntryType.DList:
                        entry.Name = "dlist_" + entryOff.ToString("X8");
                        break;
                    case EntryType.Texture:
                        entry.Name = "tex_" + entryOff.ToString("X8");
                        break;
                    case EntryType.Vertex:
                        entry.Name = "vtx_" + entryOff.ToString("X8");
                        break;
                    case EntryType.CollisionHeader:
                        entry.Name = "colheader_" + entryOff.ToString("X8");
                        break;
                    case EntryType.CollisionVertices:
                        entry.Name = "colvertices_" + entryOff.ToString("X8");
                        break;
                    case EntryType.CollisionPolygons:
                        entry.Name = "colpolys_" + entryOff.ToString("X8");
                        break;
                    case EntryType.CollisionSurfaceTypes:
                        entry.Name = "colsurfacetypes_" + entryOff.ToString("X8");
                        break;
                    case EntryType.CollisionCamData:
                        entry.Name = "colcamdata_" + entryOff.ToString("X8");
                        break;
                    case EntryType.WaterBox:
                        entry.Name = "waterbox_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimHeader:
                        entry.Name = "matanimheader_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimTexScrollParams:
                        entry.Name = "texscrollparams_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimColorParams:
                        entry.Name = "colorparams_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimPrimColors:
                        entry.Name = "primcolors_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimEnvColors:
                        entry.Name = "envcolors_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimKeyFrames:
                        entry.Name = "keyframes_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimTextureIndexList:
                        entry.Name = "texindexlist_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimTextureList:
                        entry.Name = "texlist_" + entryOff.ToString("X8");
                        break;
                    case EntryType.MatAnimTexCycleParams:
                        entry.Name = "texcycleparams_" + entryOff.ToString("X8");
                        break;
                    case EntryType.Mtx:
                        entry.Name = "mtx_" + entryOff.ToString("X8");
                        break;
                    default:
                        entry.Name = "unk_" + entryOff.ToString("X8");
                        break;
                }
                entryOff += entry.GetSize();
            }
            foreach (var entry in Entries)
            {
                if (entry.GetEntryType() == EntryType.Texture)
                {
                    var tex = (TextureHolder)entry;
                    if (tex.Tlut != null)
                        tex.Tlut.Name = tex.Tlut.Name.Replace("tex", "tlut");
                }
            }
        }

        public void GroupUnkEntries()
        {
            int count = Entries.Count;
            for (int i = 1; i < count; i++)
            {
                if (
                    Entries[i].GetEntryType() == EntryType.Unknown
                    && Entries[i - 1].GetEntryType() == EntryType.Unknown
                )
                {
                    List<byte> newData = Entries[i - 1].GetData().ToList();
                    newData.AddRange(Entries[i].GetData());
                    Entries[i - 1].SetData(newData.ToArray());
                    Entries.RemoveAt(i);
                    count--;
                    i--;
                }
            }
        }

        public ObjectHolder? GetHolderAtOffset(int target)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (target >= entryOff && target < entryOff + entry.GetSize())
                    return entry;
                entryOff += entry.GetSize();
            }
            return null;
        }

        public bool IsOffsetFree(int off)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (
                    off >= entryOff
                    && off < entryOff + entry.GetSize()
                    && entry.GetEntryType() != EntryType.Unknown
                )
                    return false;
                entryOff += entry.GetSize();
            }
            return true;
        }

        public int GetSize()
        {
            int size = 0;
            foreach (var entry in Entries)
                size += entry.GetSize();
            return size;
        }

        public int OffsetOf(ObjectHolder holder)
        {
            int off = 0;
            for (int i = 0; i < Entries.IndexOf(holder); i++)
                off += Entries[i].GetSize();

            return off;
        }

        public void SetData(byte[] data)
        {
            //if (((data.Length + 0xF) & ~0xF) != ((GetSize()+0xF) & ~0xF))
            if (data.Length != GetSize())
                throw new Exception(
                    $"Invalid data size (0x{data.Length:X} instead of 0x{GetSize():X})"
                );

            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryReader br = new BinaryReader(ms);

                foreach (var iter in Entries)
                    iter.SetData(br.ReadBytes(iter.GetSize()));
            }
        }

        public byte[] Build()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryStream bw = new BinaryStream(ms);

                foreach (var entry in Entries)
                    bw.Write(entry.GetData());

                bw.Align(0x10, true);

                return ms.GetBuffer().Take((int)ms.Length).ToArray();
            }
        }

        private class JsonObjectHolder
        {
            public string? Name { get; set; }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public EntryType EntryType { get; set; }
        }

        private class JsonUnknowHolder : JsonObjectHolder
        {
            public int Size { get; set; }
        }

        private class JsonUCodeHolder : JsonObjectHolder
        {
            public int Size { get; set; }
        }

        private class JsonVertexHolder : JsonObjectHolder
        {
            public int VertexCount { get; set; }
        }

        private class JsonTextureHolder : JsonObjectHolder
        {
            public int Width { get; set; }
            public int Height { get; set; }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public N64TexFormat Format { get; set; }
            public string? Tlut { get; set; }
        }

        private class JsonArrayHolder : JsonObjectHolder
        {
            public int Count { get; set; }
        }

        public string GetJSON()
        {
            var list = new List<object>();
            foreach (var iter in Entries)
            {
                switch (iter.GetEntryType())
                {
                    case EntryType.DList:
                    {
                        list.Add(
                            new JsonUCodeHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Size = iter.GetSize(),
                            }
                        );
                        break;
                    }
                    case EntryType.Vertex:
                    {
                        list.Add(
                            new JsonVertexHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                VertexCount = ((VertexHolder)iter).Vertices.Count,
                            }
                        );
                        break;
                    }
                    case EntryType.Texture:
                    {
                        var holder = (TextureHolder)iter;
                        list.Add(
                            new JsonTextureHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Width = holder.Width,
                                Height = holder.Height,
                                Format = holder.Format,
                                Tlut = holder.Tlut?.Name,
                            }
                        );
                        break;
                    }
                    case EntryType.Unknown:
                    {
                        list.Add(
                            new JsonUnknowHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Size = iter.GetSize(),
                            }
                        );
                        break;
                    }
                    case EntryType.SkeletonLimbs:
                    {
                        var holder = (SkeletonLimbsHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.LimbSegments.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.JointIndices:
                    {
                        var holder = (AnimationJointIndicesHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.JointIndices.Length,
                            }
                        );
                        ;
                        break;
                    }
                    case EntryType.FrameData:
                    {
                        var holder = (AnimationFrameDataHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.FrameData.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.CollisionVertices:
                    {
                        var holder = (CollisionVerticesHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.Points.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.CollisionPolygons:
                    {
                        var holder = (CollisionPolygonsHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.CollisionPolys.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.CollisionSurfaceTypes:
                    {
                        var holder = (CollisionSurfaceTypesHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.SurfaceTypes.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.CollisionCamData:
                    {
                        var holder = (CollisionCamDataHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.CamData.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.WaterBox:
                    {
                        var holder = (WaterBoxHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.WaterBoxes.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.MatAnimTextureIndexList:
                    {
                        var holder = (MatAnimTextureIndexListHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.TextureIndices.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.MatAnimTextureList:
                    {
                        var holder = (MatAnimTextureListHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.TextureSegments.Length,
                            }
                        );
                        break;
                    }
                    case EntryType.Mtx:
                    case EntryType.SkeletonHeader:
                    case EntryType.FlexSkeletonHeader:
                    case EntryType.StandardLimb:
                    case EntryType.LODLimb:
                    case EntryType.SkinLimb:
                    case EntryType.AnimationHeader:
                    case EntryType.PlayerAnimationHeader:
                    case EntryType.CollisionHeader:
                    case EntryType.MatAnimHeader:
                    case EntryType.MatAnimTexScrollParams:
                    case EntryType.MatAnimColorParams:
                    case EntryType.MatAnimTexCycleParams:
                    {
                        list.Add(
                            new JsonObjectHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                            }
                        );
                        break;
                    }
                    case EntryType.MatAnimPrimColors:
                    {
                        var holder = (MatAnimPrimColorsHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.PrimColors.Length,
                            }
                        );
                        ;
                        break;
                    }
                    case EntryType.MatAnimEnvColors:
                    {
                        var holder = (MatAnimEnvColorsHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.EnvColors.Length,
                            }
                        );
                        ;
                        break;
                    }
                    case EntryType.MatAnimKeyFrames:
                    {
                        var holder = (MatAnimKeyFramesHolder)iter;
                        list.Add(
                            new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.KeyFrames.Length,
                            }
                        );
                        ;
                        break;
                    }
                    default:
                        throw new Z64ObjectException($"Invalid entry type ({iter.GetEntryType()})");
                }
            }
            return JsonSerializer.Serialize<object>(
                list,
                new JsonSerializerOptions() { WriteIndented = true }
            );
        }

        public static Z64Object FromJson(string json)
        {
            Z64Object obj = new Z64Object();
            var list = JsonSerializer.Deserialize<List<object>>(json);
            if (list == null)
                throw new Exception("Could not deserialize json");

            foreach (JsonElement iter in list)
            {
                var typeStr = iter.GetProperty(nameof(JsonObjectHolder.EntryType)).GetString();
                if (typeStr == null)
                    throw new Exception("Could not deserialize json");
                var type = Enum.Parse<EntryType>(typeStr);
                switch (type)
                {
                    case EntryType.DList:
                    {
                        var holder = iter.ToObject<JsonUCodeHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddDList(holder.Size, holder.Name);
                        break;
                    }
                    case EntryType.Vertex:
                    {
                        var holder = iter.ToObject<JsonVertexHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddVertices(holder.VertexCount, holder.Name);
                        break;
                    }
                    case EntryType.Texture:
                    {
                        var holder = iter.ToObject<JsonTextureHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddTexture(holder.Width, holder.Height, holder.Format, holder.Name);
                        break;
                    }
                    case EntryType.Unknown:
                    {
                        var holder = iter.ToObject<JsonUnknowHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddUnknow(holder.Size, holder.Name);
                        break;
                    }
                    case EntryType.SkeletonHeader:
                    {
                        obj.AddSkeleton();
                        break;
                    }
                    case EntryType.FlexSkeletonHeader:
                    {
                        obj.AddFlexSkeleton();
                        break;
                    }
                    case EntryType.StandardLimb:
                    case EntryType.LODLimb:
                    case EntryType.SkinLimb:
                    {
                        obj.AddSkeletonLimb(type);
                        break;
                    }
                    case EntryType.AnimationHeader:
                    {
                        obj.AddAnimation();
                        break;
                    }
                    case EntryType.PlayerAnimationHeader:
                    {
                        obj.AddPlayerAnimation();
                        break;
                    }
                    case EntryType.SkeletonLimbs:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddSkeletonLimbs(holder.Count);
                        break;
                    }
                    case EntryType.JointIndices:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddJointIndices(holder.Count);
                        break;
                    }
                    case EntryType.FrameData:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddFrameData(holder.Count);
                        break;
                    }
                    case EntryType.Mtx:
                    {
                        obj.AddMtx(1);
                        break;
                    }
                    case EntryType.CollisionHeader:
                    {
                        obj.AddCollisionHeader();
                        break;
                    }
                    case EntryType.CollisionVertices:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddCollisionVertices(holder.Count);
                        break;
                    }
                    case EntryType.CollisionPolygons:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddCollisionPolygons(holder.Count);
                        break;
                    }
                    case EntryType.CollisionSurfaceTypes:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddCollisionSurfaceTypes(holder.Count);
                        break;
                    }
                    case EntryType.CollisionCamData:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddCollisionCamData(holder.Count);
                        break;
                    }
                    case EntryType.WaterBox:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddWaterBoxes(holder.Count);
                        break;
                    }
                    case EntryType.MatAnimHeader:
                    {
                        obj.AddMatAnimHeader();
                        break;
                    }
                    case EntryType.MatAnimTexScrollParams:
                    {
                        obj.AddMatAnimTexScrollParams();
                        break;
                    }
                    case EntryType.MatAnimColorParams:
                    {
                        obj.AddMatAnimColorParams();
                        break;
                    }
                    case EntryType.MatAnimTextureIndexList:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddMatAnimTextureIndexList(holder.Count);
                        break;
                    }
                    case EntryType.MatAnimTextureList:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddMatAnimTextureList(holder.Count);
                        break;
                    }
                    case EntryType.MatAnimTexCycleParams:
                    {
                        obj.AddMatAnimTexCycleParams();
                        break;
                    }
                    case EntryType.MatAnimPrimColors:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddMatAnimPrimColors(holder.Count);
                        break;
                    }
                    case EntryType.MatAnimEnvColors:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddMatAnimEnvColors(holder.Count);
                        break;
                    }
                    case EntryType.MatAnimKeyFrames:
                    {
                        var holder = iter.ToObject<JsonArrayHolder>();
                        if (holder == null)
                            throw new Exception("Could not deserialize json");
                        obj.AddMatAnimKeyFrames(holder.Count);
                        break;
                    }
                    default:
                        throw new Z64ObjectException($"Invalid entry type ({type})");
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                var holder = ((JsonElement)list[i]).ToObject<JsonTextureHolder>();
                if (holder == null)
                    throw new Exception("Could not deserialize json");
                if (holder.EntryType == EntryType.Texture)
                {
                    var tlut = (TextureHolder?)
                        obj.Entries.Find(e =>
                            e.GetEntryType() == EntryType.Texture && e.Name == holder.Tlut
                        );
                    ((TextureHolder)obj.Entries[i]).Tlut = tlut;
                }
            }
            return obj;
        }
    }
}
