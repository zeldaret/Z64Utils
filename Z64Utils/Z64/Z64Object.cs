using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using N64;
using F3DZEX;
using Syroot.BinaryData;
using Common;
using RDP;

namespace Z64
{

    [Serializable]
    public class Z64ObjectException : Exception
    {
        public Z64ObjectException() { }
        public Z64ObjectException(string message) : base(message) { }
        public Z64ObjectException(string message, Exception inner) : base(message, inner) { }
        protected Z64ObjectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
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
            SkeletonLimb,
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

            public DListHolder(string name, byte[] ucode) : base(name) => UCode = ucode;

            public override EntryType GetEntryType() => EntryType.DList;
            public override byte[] GetData() => UCode;
            public override void SetData(byte[] data) => UCode = data;
            public override int GetSize() => UCode.Length;
        }
        public class VertexHolder : ObjectHolder
        {
            public List<Vertex> Vertices { get; set; }

            public VertexHolder(string name, List<Vertex> vtx) : base(name) => Vertices = vtx;

            public override EntryType GetEntryType() => EntryType.Vertex;
            public override void SetData(byte[] data)
            {
                if (data.Length % 0x10 != 0)
                    throw new Z64ObjectException($"Invalid size for a vertex buffer (0x{data.Length:X})");

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

            public UnknowHolder(string name, byte[] data) : base(name) => Data = data;

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
            public TextureHolder Tlut { get; set; }


            public TextureHolder(string name, int w, int h, N64TexFormat format, byte[] tex) : base(name)
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
            public override void SetData(byte[] data)
            {
                int validSize = N64Texture.GetTexSize(Width * Height, Format);
                if (data.Length != validSize)
                    throw new Z64ObjectException($"Invalid data size (0x{data.Length:X} instead of 0x{validSize:X})");

                Texture = data;
            }
            public override int GetSize() => Texture.Length;
        }
        public class MtxHolder : ObjectHolder
        {
            public List<Mtx> Matrices;

            public MtxHolder(string name, byte[] data) : base(name)
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
            public override void SetData(byte[] data)
            {
                if (data.Length % Mtx.SIZE != 0)
                    throw new Z64ObjectException($"Invalid data size (0x{data.Length:X}, should be a multiple of 0x{Mtx.SIZE:X})");

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
            public const int ENTRY_SIZE = 0xC;

            public short JointX { get; set; }
            public short JointY { get; set; }
            public short JointZ { get; set; }
            public byte Child { get; set; }
            public byte Sibling { get; set; }
            public SegmentedAddress DListSeg { get; set; }

            public SkeletonLimbHolder(string name, byte[] data) : base(name)
            {
                SetData(data);
            }

            public override EntryType GetEntryType() => EntryType.SkeletonLimb;

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
                    bw.Write(DListSeg.VAddr);
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
                    DListSeg = new SegmentedAddress(br.ReadUInt32());
                }
            }
            public override int GetSize() => ENTRY_SIZE;
        }
        public class SkeletonLimbsHolder : ObjectHolder
        {
            public SegmentedAddress[] LimbSegments { get; set; }

            public SkeletonLimbsHolder(string name, byte[] data) : base(name)
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

            public override void SetData(byte[] data)
            {
                if ((data.Length % 4) != 0)
                    throw new Z64ObjectException($"Invalid data size (0x{data.Length:X}) should be a multiple of 4");

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
            
            public SkeletonHolder(string name, byte[] data) : base(name)
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

            public FlexSkeletonHolder(string name, byte[] data) : base(name, data)
            {
            }
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

            public AnimationHolder(string name, byte[] data) : base(name)
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

            public AnimationFrameDataHolder(string name, byte[] data) : base(name)
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

            public override void SetData(byte[] data)
            {
                if ((data.Length % 2) != 0)
                    throw new Z64ObjectException($"Invalid data size (0x{data.Length:X}) should be a multiple of 2");


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
                public ushort X, Y, Z;
            };

            public JointIndex[] JointIndices { get; set; }

            public AnimationJointIndicesHolder(string name, byte[] data) : base(name)
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

            public override void SetData(byte[] data)
            {
                if ((data.Length % ENTRY_SIZE) != 0)
                    throw new Z64ObjectException($"Invalid data size (0x{data.Length:X}) should be a multiple of 6");

                JointIndices = new JointIndex[data.Length / ENTRY_SIZE];
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryStream br = new BinaryStream(ms, ByteConverter.Big);

                    for (int i = 0; i < JointIndices.Length; i++)
                        JointIndices[i] = new JointIndex()
                        {
                            X = br.ReadUInt16(),
                            Y = br.ReadUInt16(),
                            Z = br.ReadUInt16()
                        };
                }
            }
            public override int GetSize() => JointIndices.Length * ENTRY_SIZE;
        }

        public List<ObjectHolder> Entries { get; set; }

        public Z64Object()
        {
            Entries = new List<ObjectHolder>();
        }
        public Z64Object(byte[] data) : this()
        {
            AddUnknow(data.Length);
            SetData(data);
        }
        private bool HolderOverlaps(ObjectHolder holder, int holderOff)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (holderOff >= entryOff && holderOff + holder.GetSize() <= entryOff + entry.GetSize() && entry.GetEntryType() == EntryType.Unknown)
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
                if ((holderOff >= entryOff && holderOff < entryOff + entry.GetSize()) ||
                    (entryOff >= holderOff && entryOff + entry.GetSize() > holderOff + holder.GetSize()) ||
                    (holderOff <= entryOff && holderOff + holder.GetSize() >= entryOff + entry.GetSize())
                    )
                {
                    if (holder.GetEntryType() != EntryType.Vertex && holder.GetEntryType() != EntryType.Unknown)
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
                    if (holderOff >= entryOff && (holderOff + holder.GetSize()) <= entryOff + Entries[i].GetSize())
                    {
                        int startDiff = holderOff - entryOff;
                        int endDiff = (entryOff + Entries[i].GetSize()) - (holderOff + holder.GetSize());

                        List<ObjectHolder> newEntries = new List<ObjectHolder>()
                        {
                           new UnknowHolder($"unk_{entryOff:X8}", new byte[startDiff]),
                           holder,
                           new UnknowHolder($"unk_{(holderOff+holder.GetSize()):X8}", new byte[endDiff]),
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
                var existing = Entries.Find(e => e.GetSize() == holder.GetSize() && OffsetOf(e) == holderOff);
                if (existing != null)
                    return  existing;

                throw new Z64ObjectException($"Overlapping data (type={holder.GetEntryType()}, off=0x{holderOff:X}, size=0x{holder.GetSize():X})");

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
                        if (Entries[i].GetEntryType() == EntryType.Vertex || Entries[i].GetEntryType() == EntryType.Unknown)
                        {
                            if (entryOff + Entries[i].GetSize() >= holderOff + holder.GetSize())
                            {
                                int startDiff = holderOff - entryOff;
                                int endDiff = (entryOff + Entries[i].GetSize()) - (holderOff + holder.GetSize());
                                if ((Entries[i].GetEntryType() == EntryType.Vertex && startDiff % 0x10 != 0) || (Entries[i].GetEntryType() == EntryType.Vertex && endDiff % 0x10 != 0))
                                    throw new Z64ObjectException("Invalid size for a vertex buffer");

                                List<ObjectHolder> newEntries = new List<ObjectHolder>()
                                {
                                   Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)new UnknowHolder($"unk_{entryOff:X8}", new byte[startDiff])
                                        : new VertexHolder($"vtx_{entryOff:X8}", new Vertex[startDiff/0x10].ToList()),

                                   new VertexHolder($"vtx_{holderOff:X8}", holder.Vertices),

                                   Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)new UnknowHolder($"unk_{entryOff:X8}", new byte[endDiff])
                                        : new VertexHolder($"vtx_{(holderOff+holder.GetSize()):X8}", new Vertex[endDiff/0x10].ToList()),
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
                                if ((Entries[i].GetEntryType() == EntryType.Vertex && startDiff % 0x10 != 0) || (Entries[i].GetEntryType() == EntryType.Vertex && endDiff % 0x10 != 0))
                                    throw new Z64ObjectException("Invalid size for a vertex buffer");

                                List<ObjectHolder> newEntries = new List<ObjectHolder>()
                                {
                                   Entries[i].GetEntryType() == EntryType.Unknown
                                        ? (ObjectHolder)new UnknowHolder($"unk_{entryOff:X8}", new byte[startDiff])
                                        : new VertexHolder($"vtx_{entryOff:X8}", new Vertex[startDiff/0x10].ToList()),


                                   new VertexHolder($"vtx_{holderOff:X8}", new Vertex[endDiff/0x10].ToList()),
                                }.FindAll(e => e.GetSize() > 0);
                                holder = new VertexHolder($"vtx_{(entryOff + Entries[i].GetSize()):X8}", new Vertex[holder.Vertices.Count-(endDiff/0x10)].ToList());

                                Entries.RemoveAt(i);
                                Entries.InsertRange(i, newEntries);

                                newEntries.ForEach(o => entryOff += o.GetSize());

                                i += newEntries.Count - 1;
                                holderOff = entryOff;
                            }

                        }
                        else throw new Z64ObjectException($"Vertex did not fit (off=0x{holderOff:X}, size=0x{holder.GetSize():X})");
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

        public DListHolder AddDList(int size, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new DListHolder(name?? $"dlist_{off:X8}", new byte[size]);
            return (DListHolder)AddHolder(holder, off);
        }
        public UnknowHolder AddUnknow(int size, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new UnknowHolder(name?? $"unk_{off:X8}", new byte[size]);
            return (UnknowHolder)AddHolder(holder, off);
        }
        public TextureHolder AddTexture(int w, int h, N64TexFormat format, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new TextureHolder(name?? $"tex_{off:X8}", w, h, format, new byte[N64Texture.GetTexSize(w*h, format)]);
            return (TextureHolder)AddHolder(holder, off);
        }
        public VertexHolder AddVertices(int vtxCount, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new VertexHolder(name?? $"vtx_{off:X8}", new Vertex[vtxCount].ToList());
            AddVertexHolder(holder, off);
            return holder;
        }
        public MtxHolder AddMtx(int mtxCount, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new MtxHolder(name ?? $"mtx_{off:X8}", new byte[mtxCount * Mtx.SIZE]);
            return (MtxHolder)AddHolder(holder, off);
        }
        public SkeletonLimbHolder AddSkeletonLimb(string name = null, int off = -1, int skel_off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new SkeletonLimbHolder(name?? $"skel_{skel_off:X8}_limb_{off:X8}", new byte[SkeletonLimbHolder.ENTRY_SIZE]);
            return (SkeletonLimbHolder)AddHolder(holder, off);
        }
        public SkeletonLimbsHolder AddSkeletonLimbs(int count, string name = null, int off = -1, int skel_off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new SkeletonLimbsHolder(name?? $"skel_{skel_off:X8}_limbs_{off:X8}", new byte[count * 4]);
            return (SkeletonLimbsHolder)AddHolder(holder, off);
        }
        public SkeletonHolder AddSkeleton(string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new SkeletonHolder(name?? $"skel_{off:X8}", new byte[SkeletonHolder.HEADER_SIZE]);
            return (SkeletonHolder)AddHolder(holder, off);
        }
        public FlexSkeletonHolder AddFlexSkeleton(string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new FlexSkeletonHolder(name ?? $"skel_{off:X8}", new byte[FlexSkeletonHolder.HEADER_SIZE]);
            return (FlexSkeletonHolder)AddHolder(holder, off);
        }
        public AnimationHolder AddAnimation(string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new AnimationHolder(name?? $"anim_{off:X8}", new byte[AnimationHolder.HEADER_SIZE]);
            return (AnimationHolder)AddHolder(holder, off);
        }
        public AnimationFrameDataHolder AddFrameData(int count, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new AnimationFrameDataHolder(name ?? $"framedata_{off:X8}", new byte[count*2]);
            return (AnimationFrameDataHolder)AddHolder(holder, off);
        }
        public AnimationJointIndicesHolder AddJointIndices(int count, string name = null, int off = -1)
        {
            if (off == -1) off = GetSize();
            var holder = new AnimationJointIndicesHolder(name ?? $"jointindices_{off:X8}", new byte[count*AnimationJointIndicesHolder.ENTRY_SIZE]);
            return (AnimationJointIndicesHolder)AddHolder(holder, off);
        }

        public void FixNames()
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                switch (entry.GetEntryType())
                {
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
                    case EntryType.SkeletonLimb:
                        entry.Name = "limb_" + entryOff.ToString("X8");
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
                if (Entries[i].GetEntryType() == EntryType.Unknown && Entries[i-1].GetEntryType() == EntryType.Unknown)
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
        public bool IsOffsetFree(int off)
        {
            int entryOff = 0;
            foreach (var entry in Entries)
            {
                if (off >= entryOff && off < entryOff + entry.GetSize() && entry.GetEntryType() != EntryType.Unknown)
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
                throw new Exception($"Invalid data size (0x{data.Length:X} instead of 0x{GetSize():X})");

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
            public string Name { get; set; }
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
            public string Tlut { get; set; }
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
                            list.Add(new JsonUCodeHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Size = iter.GetSize()
                            });
                            break;
                        }
                    case EntryType.Vertex:
                        {
                            list.Add(new JsonVertexHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                VertexCount = ((VertexHolder)iter).Vertices.Count,
                            });
                            break;
                        }
                    case EntryType.Texture:
                        {
                            var holder = (TextureHolder)iter;
                            list.Add(new JsonTextureHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Width = holder.Width,
                                Height = holder.Height,
                                Format = holder.Format,
                                Tlut = holder.Tlut?.Name,
                            });
                            break;
                        }
                    case EntryType.Unknown:
                        {
                            list.Add(new JsonUnknowHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Size = iter.GetSize()
                            });
                            break;
                        }
                    case EntryType.SkeletonLimbs:
                        {
                            var holder = (SkeletonLimbsHolder)iter;
                            list.Add(new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.LimbSegments.Length
                            });
                            break;
                        }
                    case EntryType.JointIndices:
                        {
                            var holder = (AnimationJointIndicesHolder)iter;
                            list.Add(new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.JointIndices.Length
                            }); ;
                            break;
                        }
                    case EntryType.FrameData:
                        {
                            var holder = (AnimationFrameDataHolder)iter;
                            list.Add(new JsonArrayHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType(),
                                Count = holder.FrameData.Length
                            });
                            break;
                        }
                    case EntryType.Mtx:
                    case EntryType.SkeletonHeader:
                    case EntryType.FlexSkeletonHeader:
                    case EntryType.SkeletonLimb:
                    case EntryType.AnimationHeader:
                        {
                            list.Add(new JsonObjectHolder()
                            {
                                Name = iter.Name,
                                EntryType = iter.GetEntryType()
                            });
                            break;
                        }
                    default:
                        throw new Z64ObjectException($"Invalid entry type ({iter.GetEntryType()})");
                }
            }
            return JsonSerializer.Serialize<object>(list, new JsonSerializerOptions() { WriteIndented = true }) ;
        }
        public static Z64Object FromJson(string json)
        {
            Z64Object obj = new Z64Object();
            var list = JsonSerializer.Deserialize<List<object>>(json);

            foreach (JsonElement iter in list)
            {
                var type = (EntryType)Enum.Parse(typeof(EntryType), iter.GetProperty(nameof(JsonObjectHolder.EntryType)).GetString());
                switch (type)
                {
                    case EntryType.DList:
                        {
                            var holder = iter.ToObject<JsonUCodeHolder>();
                            obj.AddDList(holder.Size, holder.Name);
                            break;
                        }
                    case EntryType.Vertex:
                        {
                            var holder = iter.ToObject<JsonVertexHolder>();
                            obj.AddVertices(holder.VertexCount, holder.Name);
                            break;
                        }
                    case EntryType.Texture:
                        {
                            var holder = iter.ToObject<JsonTextureHolder>();
                            obj.AddTexture(holder.Width, holder.Height, holder.Format, holder.Name);
                            break;
                        }
                    case EntryType.Unknown:
                        {
                            var holder = iter.ToObject<JsonUnknowHolder>();
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
                    case EntryType.SkeletonLimb:
                        {
                            obj.AddSkeletonLimb();
                            break;
                        }
                    case EntryType.AnimationHeader:
                        {
                            obj.AddAnimation();
                            break;
                        }
                    case EntryType.SkeletonLimbs:
                        {
                            var holder = iter.ToObject<JsonArrayHolder>();
                            obj.AddSkeletonLimbs(holder.Count);
                            break;
                        }
                    case EntryType.JointIndices:
                        {
                            var holder = iter.ToObject<JsonArrayHolder>();
                            obj.AddJointIndices(holder.Count);
                            break;
                        }
                    case EntryType.FrameData:
                        {
                            var holder = iter.ToObject<JsonArrayHolder>();
                            obj.AddFrameData(holder.Count);
                            break;
                        }
                    case EntryType.Mtx:
                        {
                            obj.AddMtx(1);
                            break;
                        }
                    default: throw new Z64ObjectException($"Invalid entry type ({type})");
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                var holder = ((JsonElement)list[i]).ToObject<JsonTextureHolder>();
                if (holder.EntryType == EntryType.Texture)
                {
                    var tlut = (TextureHolder)obj.Entries.Find(e => e.GetEntryType() == EntryType.Texture && e.Name == holder.Tlut);
                    ((TextureHolder)obj.Entries[i]).Tlut = tlut;
                }
            }
            return obj;
        }

    }
}
