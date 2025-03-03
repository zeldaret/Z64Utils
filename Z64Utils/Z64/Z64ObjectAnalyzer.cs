using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using F3DZEX;
using N64;
using System.Text.RegularExpressions;
using System.IO;
using F3DZEX.Command;
using RDP;

namespace Z64
{
    [Serializable]
    public class Z64ObjectAnalyzerException : Exception
    {
        public Z64ObjectAnalyzerException() { }
        public Z64ObjectAnalyzerException(string message) : base(message) { }
        public Z64ObjectAnalyzerException(string message, Exception inner) : base(message, inner) { }
        protected Z64ObjectAnalyzerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public static class Z64ObjectAnalyzer
    {
        public class Config
        {
            public class OpCodePattern
            {
                public CmdID ID { get; private set; }
                private List<Tuple<int, List<CmdID>>> _pattern;

                private OpCodePattern()
                {

                }

                public bool Check(byte[] data, int off)
                {
                    if (data[off] != (byte)ID)
                        return false;

                    foreach (var item in _pattern)
                    {
                        int off2 = off + item.Item1*8;
                        if (off2 < 0 || off2 >= data.Length)
                            return false;

                        if (item.Item2.Count == 0)
                            continue;

                        if (item.Item2.FindAll(id => (byte)id == data[off2]).Count == 0)
                            return false;
                    }

                    return true;
                }

                private static bool ValidOpCodeID(string id)
                {
                    var values = Enum.GetValues(typeof(CmdID));
                    foreach (var v in values)
                        if (v.ToString() == id)
                            return true;
                    return false;
                }
                public static OpCodePattern Parse(string exp)
                {
                    var ret = new OpCodePattern();

                    exp = exp.Replace(" ", "");
                    var parts = exp.Split(':');
                    if (parts.Length != 2)
                        return null;

                    if (!ValidOpCodeID(parts[0]))
                        return null;

                    ret.ID = (CmdID)Enum.Parse(typeof(CmdID), parts[0]);
                    var entries = parts[1].Split(',').ToList();
                    if (entries.FindAll(s => s == "*").Count != 1)
                        return null;

                    int idx = entries.IndexOf("*");

                    ret._pattern = new List<Tuple<int, List<CmdID>>>();
                    for (int i = 0; i < entries.Count; i++)
                    {
                        if (entries[i] == "*")
                        {
                            ret._pattern.Add(new Tuple<int, List<CmdID>>(0, new List<CmdID>() { ret.ID }));
                            continue;
                        }

                        if (entries[i] == "?")
                        {
                            ret._pattern.Add(new Tuple<int, List<CmdID>>(i-idx, new List<CmdID>()));
                            continue;
                        }

                        var ids = new List<CmdID>();

                        var idStr = entries[i].Split('|').ToList();
                        if (!idStr.TrueForAll(s => ValidOpCodeID(s)))
                            return null;
                        foreach (var s in idStr)
                            ids.Add((CmdID)Enum.Parse(typeof(CmdID), s));

                        ret._pattern.Add(new Tuple<int, List<CmdID>>(i-idx, ids));
                    }

                    return ret;
                }
                public override string ToString()
                {
                    StringWriter sw = new StringWriter();

                    sw.Write($"{ID}: ");
                    _pattern.Sort((a, b) => a.Item1 >= b.Item1 ? 1 : -1);
                    for (int i = 0; i < _pattern.Count; i++)
                    {
                        var ids = _pattern[i].Item2;


                        if (_pattern[i].Item1 == 0)
                            sw.Write("*");
                        else if (ids.Count == 0)
                            sw.Write("?");
                        else
                            sw.Write(string.Join("|", ids.ToArray()));

                        if (i + 1 < _pattern.Count)
                            sw.Write(", ");
                    }

                    return sw.ToString();
                }
            }

            public List<CmdID> ImprobableOpCodes { get; set; } = new List<CmdID>();
            public List<OpCodePattern> Patterns { get; set; } = new List<OpCodePattern>();

        }

        private class ReservedRegion
        {
            public int Off;
            public int Size;
            public ReservedRegion(int off, int size)
            {
                Off = off;
                Size = size;
            }
            public override string ToString() => $"0x{Off:X8}(0x{Size:X})";
        }
        private static bool IsOverlap(int off, List<ReservedRegion> regions) => GetRegionIndex(off, regions) != -1;
        private static int GetRegionIndex(int off, List<ReservedRegion> regions)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (off >= regions[i].Off && off < regions[i].Off + regions[i].Size)
                    return i;
            }
            return -1;
        }
        private static void AddRegion(List<ReservedRegion> regions, int curSegId, uint vaddr, int size, int totalSize)
        {
            var addr = new SegmentedAddress(vaddr);
            if (addr.Segmented && addr.SegmentId == curSegId && ((int)addr.SegmentOff + size) <= totalSize)
                regions.Add(new ReservedRegion((int)addr.SegmentOff, size));
        }
        private static void AddDlist(List<int> dlists, int curSegId, uint vaddr, int totalSize)
        {
            var addr = new SegmentedAddress(vaddr);
            if (addr.Segmented && addr.SegmentId == curSegId && (int)addr.SegmentOff < totalSize)
                dlists.Add((int)addr.SegmentOff);
        }


        private static Tuple<List<int>, List<ReservedRegion>> FindRegions(byte[] data, int segmentId, Config cfg)
        {
            List<ReservedRegion> regions = new List<ReservedRegion>();
            List<int> dlists = new List<int>();

            var codeEnds = Utils.FindData(data, new byte[] { (byte)CmdID.G_ENDDL, 0, 0, 0, 0, 0, 0, 0, }, 8);
            codeEnds.Insert(0, 0);
            for (int i = 1; i < codeEnds.Count; i++)
            {
                int end = codeEnds[i];
                int texels = -1;
                uint half1 = 0xFFFFFFFF;

                for (int off = end; off >= codeEnds[i-1]+8; off -= 8)
                {
                    CmdID op = (CmdID)data[off];

                    if (IsOverlap(off, regions) || !IsOpCodeCorrect(data, off, cfg))
                        break;

                    switch (op)
                    {
                        case CmdID.G_RDPHALF_1:
                            {
                                half1 = CmdInfo.DecodeCommand<GRdpHalf>(data, off).word;
                                break;
                            }
                        case CmdID.G_BRANCH_Z:
                            {
                                AddDlist(dlists, segmentId, half1, data.Length);
                                break;
                            }
                        case CmdID.G_DL:
                            {
                                var gdl = CmdInfo.DecodeCommand<GDl>(data, off);
                                AddDlist(dlists, segmentId, gdl.dl, data.Length);
                                break;
                            }
                        case CmdID.G_VTX:
                            {
                                var gmtx = CmdInfo.DecodeCommand<GVtx>(data, off);
                                AddRegion(regions, segmentId, gmtx.vaddr, gmtx.numv * 0x10, data.Length);
                                break;
                            }
                        case CmdID.G_SETTIMG:
                            {
                                var settimg = CmdInfo.DecodeCommand<GSetTImg>(data, off);
                                if (texels == -1)
                                    break;
                                AddRegion(regions, segmentId, settimg.imgaddr, N64Texture.GetTexSize(texels, settimg.siz), data.Length);
                                texels = -1;
                                break;
                            }
                        case CmdID.G_LOADBLOCK:
                            {
                                var loadblock = CmdInfo.DecodeCommand<GLoadBlock>(data, off);
                                texels = loadblock.texels+1;
                                break;
                            }
                        case CmdID.G_LOADTLUT:
                            {
                                var loadtlut = CmdInfo.DecodeCommand<GLoadTlut>(data, off);
                                texels = loadtlut.count+1;
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            return new Tuple<List<int>, List<ReservedRegion>>(dlists, regions);
        }

        private static bool IsOpCodeCorrect(byte[] data, int off, Config cfg)
        {
            CmdID id = (CmdID)data[off];

            // check invalid opcodes
            if (!CmdEncoding.DEC_TABLE.ContainsKey(id))
                return false;

            // check improbable opcodes
            if (cfg.ImprobableOpCodes.Contains(id))
                return false;

            var patterns = cfg.Patterns.FindAll(p => p.ID == id);
            if (patterns.Count == 0)
                return true;
            foreach (var pattern in patterns)
                if (pattern.Check(data, off))
                    return true;

            return false;

        }
        
        private static List<int> FindDlistEntries(List<ReservedRegion> regions, List<int> confirmedDlists, byte[] data, Config cfg)
        {
            List<int> dlists = new List<int>();

            var codeEnds = Utils.FindData(data, new byte[] { (byte)CmdID.G_ENDDL, 0, 0, 0, 0, 0, 0, 0, }, 8);
            codeEnds.Insert(0, 0);

            regions.Sort((a, b) => a.Off >= b.Off ? 1 : -1);

            for (int i = 1; i < codeEnds.Count; i++)
            {
                int end = codeEnds[i]+8;
                int start = codeEnds[i-1]+8;

                bool skip = false;
                foreach (var dl in confirmedDlists)
                {
                    if (dl >= start && dl < end)
                    {
                        dlists.Add(dl);
                        skip = true;
                        break;
                    }
                }
                if (skip)
                    continue;

                // check reserved regions
                foreach (var region in regions)
                {
                    int regionEnd = region.Off + region.Size;
                    if (regionEnd <= end && regionEnd >= start)
                        start = regionEnd;
                }

                // check opcodes
                for (int off = start; off < end; off += 8)
                {
                    if (!IsOpCodeCorrect(data, off, cfg))
                        start = off+8;
                }


                dlists.Add(start);
            }

            return dlists;
        }
        
        private static int GetTlutWidth(int texels)
        {
            for (int i = (int)Math.Sqrt(texels); i >=1; i--)
                if (texels % i == 0)
                    return Math.Max(i, texels / i);
            return 0;
        }

        public static void FindSkeletons(Z64Object obj, byte[] data, int segmentId)
        {
            // Search for Skeleton Headers
            // Structure: SS OO OO OO XX 00 00 00 [XX 00 00 00]
            for (int i = 0; i <= data.Length - Z64Object.SkeletonHolder.HEADER_SIZE; i += 4)
            {
                var segment = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i));
                // check for segmentId match, check for valid segment offset,
                // check for Limbs 0x4 alignment, check for nonzero limb count,
                // check for zeroes in struct padding
                if (segment.SegmentId == segmentId && segment.SegmentOff < data.Length &&
                    (segment.SegmentOff % 4) == 0 && data[i + 5] == 0 && data[i + 6] == 0 && data[i + 7] == 0)
                {
                    if (!obj.IsOffsetFree(i))
                        continue;

                    int nLimbs = data[i + 4];
                    byte[] limbsData = new byte[nLimbs * 4];
                    Buffer.BlockCopy(data, (int)segment.SegmentOff, limbsData, 0, nLimbs * 4);

                    // check for limbs array ending at the start of the skeleton header,
                    if (segment.SegmentOff + nLimbs * 4 != i)
                        continue;

                    // find the type of limb
                    // the checks are not very rigorous as they do not appear to need to be
                    Z64Object.EntryType limbType;

                    var firstLimbSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(limbsData, 0));
                    // Quick fix for one-limb skeletons
                    // TODO: Maybe do this right somehow
                    if (limbsData.Length <= 4)
                    {
                        limbType = Z64Object.EntryType.StandardLimb;
                        goto found_limb_type;
                    }
                    var secondLimbSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(limbsData, 4));

                    if (secondLimbSeg.VAddr - firstLimbSeg.VAddr == Z64Object.SkeletonLimbHolder.STANDARD_LIMB_SIZE)
                    {
                        limbType = Z64Object.EntryType.StandardLimb;
                        goto found_limb_type;
                    }
                    // The difference in structure size resolves most of these, however skin and lod limbs have the same
                    // size, so one of these needs a more in-depth test to differentiate them.
                    if (secondLimbSeg.VAddr - firstLimbSeg.VAddr == Z64Object.SkeletonLimbHolder.SKIN_LIMB_SIZE)
                    {
                        bool limbsTest = true;

                        for (int j = 0; j < nLimbs * 4; j += 4)
                        {
                            var limbSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(limbsData, j));

                            var value08 = ArrayUtil.ReadUint32BE(data, (int)limbSeg.SegmentOff + 0x8);
                            var segment0C = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, (int)limbSeg.SegmentOff + 0xC));

                            if (value08 > 255 ||
                                !(segment0C.SegmentId == segmentId || segment0C.VAddr == 0) || segment0C.VAddr % 4 != 0)
                            {
                                limbsTest = false;
                                break;
                            }
                        }

                        if (limbsTest)
                        {
                            limbType = Z64Object.EntryType.SkinLimb;
                            goto found_limb_type;
                        }
                    }
                    if (secondLimbSeg.VAddr - firstLimbSeg.VAddr == Z64Object.SkeletonLimbHolder.LOD_LIMB_SIZE)
                    {
                        limbType = Z64Object.EntryType.LODLimb;
                        goto found_limb_type;
                    }
                    // failed to find any valid limb type
                    continue;
                    found_limb_type:
                    int nNonNullDlists = 0;

                    obj.AddSkeletonLimbs(nLimbs, off: (int)segment.SegmentOff);

                    for (int j = 0; j < nLimbs * 4; j += 4)
                    {
                        var limbSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(limbsData, j));
                        if (limbSeg.SegmentId != segmentId)
                            throw new Z64ObjectAnalyzerException(
                                $"Limb segment {limbSeg.Segmented} is not the correct segment id, mis-detected SkeletonHeader?");
                        obj.AddSkeletonLimb(limbType, off: (int)limbSeg.SegmentOff);
                        // check if dlist is non-null (dlists may be null in general, this is only for FlexSkeletonHeader detection)
                        if (ArrayUtil.ReadUint32BE(data, (int)(limbSeg.SegmentOff + 0x8)) != 0)
                            nNonNullDlists++;
                    }
                    // try to detect flex headers over normal headers
                    // check for the existence of extra bytes beyond standard header size,
                    // check if nothing is already assumed to occupy that space,
                    // check if the number of dlists is equal to the actual number of non-null dlists,
                    // check struct padding
                    if (i <= data.Length - Z64Object.FlexSkeletonHolder.HEADER_SIZE && obj.IsOffsetFree(i + Z64Object.SkeletonHolder.HEADER_SIZE) &&
                        data[i + 8] == nNonNullDlists && data[i + 9] == 0 && data[i + 10] == 0 && data[i + 11] == 0)
                    {
                        obj.AddFlexSkeleton(off: i);
                    }
                    else
                    {
                        obj.AddSkeleton(off: i);
                    }
                }
            }
        }
        public static void FindAnimations(Z64Object obj, byte[] data, int segmentId)
        {
            // Search for Animation Headers
            // Structure: FF FF 00 00 SS OO OO OO SS OO OO OO II II 00 00
            for (int i = 0; i <= data.Length - Z64Object.AnimationHolder.HEADER_SIZE; i += 4)
            {
   
                var frameCount = ArrayUtil.ReadInt16BE(data, i);
                // check positive nonzero frame count, check struct padding zeroes
                if (frameCount > 0 &&
                    data[i + 2] == 0 && data[i + 3] == 0 && data[i + 14] == 0 && data[i + 15] == 0)
                {
                    if (!(obj.IsOffsetFree(i) && obj.IsOffsetFree(i + Z64Object.AnimationHolder.HEADER_SIZE - 1)))
                        continue;
                    
                    var frameDataSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i+4));
                    var jointIndicesSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i+8));
                    // check for segmentId match, check for valid segment offsets
                    if (frameDataSeg.SegmentId == segmentId && jointIndicesSeg.SegmentId == segmentId &&
                        frameDataSeg.SegmentOff < data.Length && jointIndicesSeg.SegmentOff < data.Length &&
                        jointIndicesSeg.SegmentOff > frameDataSeg.SegmentOff)
                    {
                        // Assumes these are all in order and end at the start of the next, which seems to be the case so far
                        int frameDataSize = (int)(jointIndicesSeg.SegmentOff - frameDataSeg.SegmentOff);
                        int jointIndicesSize = (int)(i - jointIndicesSeg.SegmentOff);

                        // Require at least one entry to be considered valid
                        if (frameDataSize < 2 || jointIndicesSize < Z64Object.AnimationJointIndicesHolder.ENTRY_SIZE)
                            continue;

                        // if not a multiple of 2, check for struct padding
                        if ((frameDataSize % 2) != 0)
                        {
                            byte[] possiblePadding = new byte[frameDataSize % 2];
                            Buffer.BlockCopy(data, (int)(frameDataSeg.SegmentOff + frameDataSize - (frameDataSize % 2)), 
                                possiblePadding, 0, frameDataSize % 2);
                            // if assumed struct padding is nonzero, consider invalid
                            if (possiblePadding.Any(b => b != 0))
                                continue;
                            frameDataSize -= (frameDataSize % 2);
                        }
                        // if not a multiple of 6, check for struct padding
                        if ((jointIndicesSize % 6) != 0)
                        {
                            byte[] possiblePadding = new byte[jointIndicesSize % 6];
                            Buffer.BlockCopy(data, (int)(jointIndicesSeg.SegmentOff + jointIndicesSize - (jointIndicesSize % 6)),
                                possiblePadding, 0, jointIndicesSize % 6);
                            // if assumed struct padding is nonzero, consider invalid
                            if (possiblePadding.Any(b => b != 0))
                                continue;
                            jointIndicesSize -= (jointIndicesSize % 6);
                        }
                        obj.AddAnimation(off: i);
                        obj.AddFrameData(frameDataSize/2, off:(int)frameDataSeg.SegmentOff);
                        obj.AddJointIndices(jointIndicesSize/Z64Object.AnimationJointIndicesHolder.ENTRY_SIZE, off:(int)jointIndicesSeg.SegmentOff);
                    }
                }
            }
        }

        public static void FindPlayerAnimations(Z64Object obj, byte[] data, int segmentId)
        {
            // only search in gameplay_keep
            if (!obj.FileName.Contains("gameplay_keep"))
                return;

            if (obj.Game == null)
                return;

            Z64File linkAnimetion = obj.Game.GetFileByName("link_animetion");
            // only search if the link_animetion file is known
            if (linkAnimetion == null)
                return;

            int linkAnimetionSize = linkAnimetion.VRomEnd - linkAnimetion.VRomStart;

            for (int i = 0; i <= data.Length - Z64Object.PlayerAnimationHolder.SIZE; i += 4)
            {
                var frameCount = ArrayUtil.ReadInt16BE(data, i);

                // Check frame count > 0 and struct padding
                if (frameCount > 0 && data[i + 2] == 0 && data[i + 3] == 0)
                {
                    var animationSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 4));

                    // Check if the segment address is a valid pointer into link_animetion
                    if (animationSeg.SegmentId == 7 && animationSeg.SegmentOff < linkAnimetionSize)
                    {
                        // Check if free
                        if (!(obj.IsOffsetFree(i) && obj.IsOffsetFree(i + Z64Object.PlayerAnimationHolder.SIZE - 1)))
                            continue;

                        // Check if the data pointed to in link_animetion is valid? TODO

                        obj.AddPlayerAnimation(off: i);
                    }
                }
            }
        }
        public static void FindCollisionData(Z64Object obj, byte[] data, int segmentId)
        {
            // Search for Collision Headers
            // Structure: XX XX YY YY ZZ ZZ XX XX YY YY ZZ ZZ NN NN 00 00
            //            SS OO OO OO NN NN 00 00 SS OO OO OO SS OO OO OO
            //            SS OO OO OO NN NN 00 00 SS OO OO OO
            for (int i = 0; i < data.Length - Z64Object.ColHeaderHolder.COLHEADER_SIZE; i += 4)
            {
                // check free and skip whole region if occupied
                if (!obj.IsOffsetFree(i))
                {
                    Z64Object.ObjectHolder holder = obj.GetHolderAtOffset(i);
                    i = obj.OffsetOf(holder) + holder.GetSize();
                    if (i % 4 != 0)
                        i += (4 - i % 4);
                    i -= 4; // gets incremented by 4 in continue
                    continue;
                }

                // check struct padding zeroes and that MinBounds <= MaxBounds
                if (data[i + 14] == 0 && data[i + 15] == 0 && data[i + 22] == 0 && data[i + 23] == 0 &&
                    data[i + 38] == 0 && data[i + 39] == 0 &&
                    ArrayUtil.ReadInt16BE(data, i + 0) <= ArrayUtil.ReadInt16BE(data, i + 6 + 0) &&
                    ArrayUtil.ReadInt16BE(data, i + 2) <= ArrayUtil.ReadInt16BE(data, i + 6 + 2) &&
                    ArrayUtil.ReadInt16BE(data, i + 4) <= ArrayUtil.ReadInt16BE(data, i + 6 + 4))
                {
                    var verticesSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 0x10));
                    var polygonsSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 0x18));
                    var surfaceTypesSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 0x1C));
                    var camDataSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 0x20));
                    var waterBoxesSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 0x28));

                    var nVertices = ArrayUtil.ReadInt16BE(data, i + 0xC);
                    var nPolygons = ArrayUtil.ReadInt16BE(data, i + 0x14);
                    var nWaterBoxes = ArrayUtil.ReadInt16BE(data, i + 0x24);

                    int surfaceTypesSize = (int)(polygonsSeg.SegmentOff - surfaceTypesSeg.SegmentOff);
                    int camDataSize = (int)(surfaceTypesSeg.SegmentOff - camDataSeg.SegmentOff);
                    if (camDataSeg.VAddr == 0)
                        camDataSize = 0;

                    int surfaceTypesPadLen = surfaceTypesSize % Z64Object.CollisionSurfaceTypesHolder.ENTRY_SIZE;
                    int camDataPadLen = camDataSize % Z64Object.CollisionCamDataHolder.ENTRY_SIZE;

                    surfaceTypesSize -= surfaceTypesPadLen;
                    camDataSize -= camDataPadLen;

                    // check segment address validity
                    // (same segment number and points to in-bounds free space or null with 0 count, has suitable alignment)
                    Func<SegmentedAddress, int, int, bool, bool> validSegmentAddr = (seg, num, align, allowNull) =>
                        ((seg.SegmentId == segmentId && seg.SegmentOff < data.Length &&
                          obj.IsOffsetFree((int)seg.SegmentOff) && seg.SegmentOff % align == 0) ||
                         (allowNull && num == 0 && seg.VAddr == 0));

                    if (validSegmentAddr(verticesSeg, nVertices, 2, false) &&
                        validSegmentAddr(polygonsSeg, nPolygons, 2, false) &&
                        validSegmentAddr(surfaceTypesSeg, surfaceTypesSize / Z64Object.CollisionSurfaceTypesHolder.ENTRY_SIZE, 4, false) &&
                        validSegmentAddr(camDataSeg, camDataSize / Z64Object.CollisionCamDataHolder.ENTRY_SIZE, 2, true) &&
                        validSegmentAddr(waterBoxesSeg, nWaterBoxes, 4, true))
                    {
                        // check padding between surface types and polygons
                        if (surfaceTypesSize != 0 && surfaceTypesPadLen != 0)
                        {
                            byte[] possiblePadding = new byte[surfaceTypesPadLen];

                            // 'Offset and length were out of bounds for the array or count is greater than the number of
                            // elements from index to the end of the source collection.'
                            Buffer.BlockCopy(data, (int)surfaceTypesSeg.SegmentOff + surfaceTypesSize, possiblePadding,
                                0, surfaceTypesPadLen);
                            // if assumed struct padding is nonzero, consider invalid
                            if (possiblePadding.Any(b => b != 0))
                                continue;
                        }
                        // check padding between cam data and surface types
                        if (camDataSeg.VAddr != 0 && camDataSize != 0 && camDataPadLen != 0)
                        {
                            byte[] possiblePadding = new byte[camDataPadLen];
                            Buffer.BlockCopy(data, (int)surfaceTypesSeg.SegmentOff + camDataSize, possiblePadding,
                                0, camDataPadLen);
                            // if assumed struct padding is nonzero, consider invalid
                            if (possiblePadding.Any(b => b != 0))
                                continue;
                        }

                        Z64Object.ColHeaderHolder colHeader = obj.AddCollisionHeader(off: i);

                        colHeader.VerticesHolder = obj.AddCollisionVertices(nVertices, off: (int)verticesSeg.SegmentOff);
                        colHeader.PolygonsHolder = obj.AddCollisionPolygons(nPolygons, off: (int)polygonsSeg.SegmentOff);
                        colHeader.SurfaceTypesHolder = obj.AddCollisionSurfaceTypes(
                            surfaceTypesSize / Z64Object.CollisionSurfaceTypesHolder.ENTRY_SIZE,
                            off: (int)surfaceTypesSeg.SegmentOff);

                        if (camDataSeg.VAddr != 0)
                            colHeader.CamDataHolder = obj.AddCollisionCamData(
                                camDataSize / Z64Object.CollisionCamDataHolder.ENTRY_SIZE,
                                off: (int)camDataSeg.SegmentOff);

                        if (waterBoxesSeg.VAddr != 0)
                            colHeader.WaterBoxHolder = obj.AddWaterBoxes(nWaterBoxes, off: (int)waterBoxesSeg.SegmentOff);

                        i += Z64Object.ColHeaderHolder.COLHEADER_SIZE;
                    }
                }
            }
        }

        public struct SegmentedTextureAttrs
        {
            public int Width;
            public int Height;
            public N64TexFormat Fmt;
        }

        public static void FindMaterialAnimations(Z64Object obj, byte[] data, int segmentId, Dictionary<int, SegmentedTextureAttrs> textureLoads)
        {
            if (obj.Game == null || obj.Game.Version.Game != Z64GameType.Mm) // material animations are MM only
                return;

            // Search for Material Animations
            // Header Structure: SS 00 TT TT SS OO OO OO
            // Segment address points to 1 of 6 possible parameter structures

            for (int i = 0; i <= data.Length - Z64Object.MatAnimHeaderHolder.SIZE; i += 4)
            {
                // check free and skip whole region if occupied
                if (!obj.IsOffsetFree(i))
                {
                    Z64Object.ObjectHolder holder = obj.GetHolderAtOffset(i);
                    i = obj.OffsetOf(holder) + holder.GetSize();
                    if (i % 4 != 0)
                        i += (4 - i % 4);
                    i -= 4; // gets incremented by 4 in continue
                    continue;
                }

                int texSegment = (sbyte)data[i];

                texSegment = ((texSegment < 0) ? -texSegment : texSegment) + 7;

                short paramsType = ArrayUtil.ReadInt16BE(data, i + 2);

                if (texSegment > 7 && texSegment < 14 &&
                    paramsType >= 0 && paramsType < 6 &&
                    data[i + 1] == 0)
                {
                    var paramsSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(data, i + 4));

                    // check valid segment address
                    if (paramsSeg.SegmentId != segmentId || paramsSeg.SegmentOff >= data.Length ||
                        !obj.IsOffsetFree((int)paramsSeg.SegmentOff))
                        continue;

                    // params segment should always be before the header
                    if (paramsSeg.SegmentOff > i)
                        continue;

                    // do params
                    if (paramsType == 0 || paramsType == 1)
                    {   // Tex Scroll
                        // XX YY WW HH

                        // Check if params fits inside the object
                        if (paramsSeg.SegmentOff + Z64Object.MatAnimTexScrollParamsHolder.SIZE > data.Length ||
                            !obj.IsOffsetFree((int)paramsSeg.SegmentOff + Z64Object.MatAnimTexScrollParamsHolder.SIZE - 1))
                            continue;

                        obj.AddMatAnimTexScrollParams(off: (int)paramsSeg.SegmentOff);
                        // Two Tex Scroll types appear to have another params immediately following the first
                        if (paramsType == 1)
                            obj.AddMatAnimTexScrollParams(off: (int)paramsSeg.SegmentOff + 4);
                    }
                    else if (paramsType == 5)
                    {   // Tex Cycle
                        // KK KK 00 00 SS OO OO OO SS OO OO OO

                        // Check if params fits inside the object
                        if (paramsSeg.SegmentOff + Z64Object.MatAnimTexCycleParamsHolder.SIZE > data.Length ||
                            !obj.IsOffsetFree((int)paramsSeg.SegmentOff + Z64Object.MatAnimTexCycleParamsHolder.SIZE - 1))
                            continue;

                        // Get params data
                        byte[] paramsData = new byte[Z64Object.MatAnimTexCycleParamsHolder.SIZE];
                        Buffer.BlockCopy(data, (int)paramsSeg.SegmentOff, paramsData, 0, Z64Object.MatAnimTexCycleParamsHolder.SIZE);

                        var keyFrameLength = ArrayUtil.ReadUInt16BE(paramsData, 0);
                        var textureListSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(paramsData, 4));
                        var textureIndexListSeg = new SegmentedAddress(ArrayUtil.ReadUint32BE(paramsData, 8));

                        // check struct padding and valid segmented addresses
                        if (!(paramsData[2] == 0 && paramsData[3] == 0 &&
                              textureListSeg.SegmentId == segmentId && textureListSeg.SegmentOff < data.Length &&
                              textureIndexListSeg.SegmentId == segmentId && textureIndexListSeg.SegmentOff < data.Length &&
                              textureIndexListSeg.SegmentOff + keyFrameLength <= data.Length))
                            continue;

                        // get texture indices data
                        byte[] textureIndices = new byte[keyFrameLength];
                        Buffer.BlockCopy(data, (int)textureIndexListSeg.SegmentOff, textureIndices, 0, keyFrameLength);

                        // the number of textures in the texture list is assumed to be the largest index used + 1
                        int nTextures = textureIndices.Max() + 1;

                        // check if the texture list length fits within the object
                        if (textureListSeg.SegmentOff + nTextures * 4 > data.Length)
                            continue;

                        // get texture list data (list of segmented addresses to textures)
                        byte[] textureListData = new byte[nTextures * 4];
                        Buffer.BlockCopy(data, (int)textureListSeg.SegmentOff, textureListData, 0, nTextures * 4);

                        // check the texture segments for validity
                        bool texturesOk = true;
                        for (int j = 0; j < nTextures * 4; j += 4)
                        {
                            var textureSegment = new SegmentedAddress(ArrayUtil.ReadUint32BE(textureListData, j));
                            if (!(textureSegment.SegmentId == segmentId && textureSegment.SegmentOff < data.Length))
                            {
                                texturesOk = false;
                                break;
                            }
                        }
                        if (!texturesOk)
                            continue;

                        // add the textures if the format is known from any display lists
                        // this assumes the segment is only used for the texture, which may not always be the case
                        //  in practice
                        SegmentedTextureAttrs texAttrs;

                        if (textureLoads.TryGetValue(texSegment, out texAttrs))
                        {
                            for (int j = 0; j < nTextures * 4; j += 4)
                            {
                                var textureSegment = new SegmentedAddress(ArrayUtil.ReadUint32BE(textureListData, j));
                                obj.AddTexture(texAttrs.Width, texAttrs.Height, texAttrs.Fmt, off: (int)textureSegment.SegmentOff);
                            }
                        }

                        obj.AddMatAnimTextureIndexList(keyFrameLength, off: (int)textureIndexListSeg.SegmentOff);
                        obj.AddMatAnimTextureList(nTextures, off: (int)textureListSeg.SegmentOff);

                        obj.AddMatAnimTexCycleParams(off: (int)paramsSeg.SegmentOff);
                    }
                    else
                    {   // Color
                        // LL LL CC CC SS OO OO OO SS OO OO OO SS OO OO OO

                        // Check if params fits inside the object
                        if (paramsSeg.SegmentOff + Z64Object.MatAnimColorParamsHolder.SIZE > data.Length ||
                            !obj.IsOffsetFree((int)paramsSeg.SegmentOff + Z64Object.MatAnimColorParamsHolder.SIZE - 1))
                            continue;

                        // Get params data
                        byte[] paramsData = new byte[Z64Object.MatAnimColorParamsHolder.SIZE];
                        Buffer.BlockCopy(data, (int)paramsSeg.SegmentOff, paramsData, 0, Z64Object.MatAnimColorParamsHolder.SIZE);

                        var keyFrameLength = ArrayUtil.ReadUInt16BE(paramsData, 0);
                        var keyFrameCount = ArrayUtil.ReadUInt16BE(paramsData, 2);
                        var primColors = new SegmentedAddress(ArrayUtil.ReadUint32BE(paramsData, 4)); // size 5 per elem
                        var envColors = new SegmentedAddress(ArrayUtil.ReadUint32BE(paramsData, 8)); // size 4 per elem
                        var keyFrames = new SegmentedAddress(ArrayUtil.ReadUint32BE(paramsData, 0xC)); // size 2 per elem

                        var arrayLengths = (paramsType == 2) ? keyFrameLength : keyFrameCount;

                        // check valid & unoccupied segmented addresses
                        if (!(primColors.SegmentId == segmentId && primColors.SegmentOff < data.Length))
                            continue;
                        if (!obj.IsOffsetFree((int)primColors.SegmentOff) ||
                            primColors.SegmentOff + arrayLengths * 5 > data.Length ||
                            !obj.IsOffsetFree((int)primColors.SegmentOff + arrayLengths * 5 - 1))
                            continue;
                        // envColors may be null
                        if (!((envColors.SegmentId == segmentId && envColors.SegmentOff < data.Length) || envColors.VAddr == 0))
                            continue;
                        if (envColors.VAddr != 0 &&
                            (!obj.IsOffsetFree((int)envColors.SegmentOff) ||
                            envColors.SegmentOff + arrayLengths * 4 > data.Length ||
                            !obj.IsOffsetFree((int)envColors.SegmentOff + arrayLengths * 4 - 1)))
                            continue;
                        // type 2 doesn't use keyFrames, so it may be null in that case only
                        if (!((keyFrames.SegmentId == segmentId && keyFrames.SegmentOff < data.Length) || (paramsType == 2 && keyFrames.VAddr == 0)))
                            continue;
                        if (keyFrames.VAddr != 0 &&
                            (!obj.IsOffsetFree((int)keyFrames.SegmentOff) ||
                            keyFrames.SegmentOff + arrayLengths * 2 > data.Length ||
                            !obj.IsOffsetFree((int)keyFrames.SegmentOff + arrayLengths * 2 - 1) ||
                            keyFrames.SegmentOff % 2 != 0))
                            continue;

                        if (primColors.VAddr != 0)
                            obj.AddMatAnimPrimColors(arrayLengths, off: (int)primColors.SegmentOff);
                        if (envColors.VAddr != 0)
                            obj.AddMatAnimEnvColors(arrayLengths, off: (int)envColors.SegmentOff);
                        if (keyFrames.VAddr != 0)
                            obj.AddMatAnimKeyFrames(arrayLengths, off: (int)keyFrames.SegmentOff);

                        obj.AddMatAnimColorParams(off: (int)paramsSeg.SegmentOff);
                    }

                    obj.AddMatAnimHeader(off: i);
                }
            }
        }

        public static void FindDlists(Z64Object obj, byte[] data, int segmentId, Config cfg)
        {
            obj.Entries.Clear();
            obj.AddUnknow(data.Length);

            var ret = FindRegions(data, segmentId, cfg);
            List<int> entries = FindDlistEntries(ret.Item2, ret.Item1, data, cfg);
            foreach (var start in entries)
            {
                for (int off = start; off < data.Length; off += 8)
                {
                    if (data[off] == (byte)CmdID.G_ENDDL)
                    {
                        obj.AddDList(off + 8 - start, off: start);
                        break;
                    }
                }
            }

            obj.GroupUnkEntries();
            obj.FixNames();
            obj.SetData(data);
        }
        public static List<string> AnalyzeDlists(Z64Object obj, byte[] data, int segmentId)
        {
            List<string> errors = new List<string>();
            Dictionary<int, SegmentedTextureAttrs> segmentedTextureLoads = new Dictionary<int, SegmentedTextureAttrs>();
            bool[] ambigiousSegments = new bool[16];

            List<int> dlists = new List<int>();
            for (int i = 0; i < obj.Entries.Count; i++)
            {
                var entry = obj.Entries[i];
                if (entry.GetEntryType() == Z64Object.EntryType.DList)
                    dlists.Add(obj.OffsetOf(entry));
                else
                    obj.Entries[i] = new Z64Object.UnknowHolder($"unk_{obj.OffsetOf(entry):X8}", entry.GetData());
            }

            foreach (var dlist in dlists)
            {
                uint lastTexAddr = 0xFFFFFFFF;
                G_IM_FMT lastFmt = (G_IM_FMT)(-1);
                G_IM_SIZ lastSiz = (G_IM_SIZ)(-1);
                Z64Object.TextureHolder lastTlut = null;
                Z64Object.TextureHolder lastCiTex = null;

                bool exit = false;
                for (int i = dlist; i < data.Length && !exit; i += 8)
                {
                    CmdID op = (CmdID)data[i];
                    switch (op)
                    {
                        case CmdID.G_QUAD:
                        case CmdID.G_TRI2:
                        case CmdID.G_TRI1:
                        case CmdID.G_TEXRECTFLIP:
                        case CmdID.G_TEXRECT:
                            {
                                if (lastCiTex != null && lastTlut != null)
                                {
                                    lastCiTex.Tlut = lastTlut;
                                    lastCiTex = null;
                                }
                                break;
                            }
                        case CmdID.G_ENDDL:
                            {
                                exit = true;
                                break;
                            }
                        case CmdID.G_MTX:
                            {
                                var gmtx = CmdInfo.DecodeCommand<GMtx>(data, i);
                                var addr = new SegmentedAddress(gmtx.mtxaddr);
                                if (addr.Segmented && addr.SegmentId == segmentId)
                                {
                                    obj.AddMtx(1, off: (int)addr.SegmentOff);
                                }
                                break;
                            }
                        case CmdID.G_VTX:
                            {
                                var gvtx = CmdInfo.DecodeCommand<GVtx>(data, i);

                                var addr = new SegmentedAddress(gvtx.vaddr);
                                if (addr.Segmented && addr.SegmentId == segmentId)
                                {
                                    try
                                    {
                                        obj.AddVertices(gvtx.numv, off: (int)addr.SegmentOff);
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Error in Dlist 0x{new SegmentedAddress(segmentId, dlist).VAddr:X8} : {ex.Message}");
                                    }
                                }
                                break;
                            }
                        case CmdID.G_SETTIMG:
                            {
                                var settimg = CmdInfo.DecodeCommand<GSetTImg>(data, i);
                                lastTexAddr = settimg.imgaddr;
                                break;
                            }
                        case CmdID.G_SETTILE:
                            {
                                var settile = CmdInfo.DecodeCommand<GSetTile>(data, i);
                                if (settile.tile != G_TX_TILE.G_TX_LOADTILE)
                                {
                                    lastFmt = settile.fmt;
                                    lastSiz = settile.siz;
                                }
                                break;
                            }
                        case CmdID.G_SETTILESIZE:
                            {
                                var settilesize = CmdInfo.DecodeCommand<GLoadTile>(data, i);
                                var addr = new SegmentedAddress(lastTexAddr);

                                if ((int)lastFmt == -1 || (int)lastSiz == -1 || lastTexAddr == 0xFFFFFFFF)
                                    /* can't really thow an exception here since in some object files, there are two gsDPSetTileSize next to each other (see object_en_warp_uzu) */
                                    //throw new Z64ObjectAnalyzerException();
                                    break;

                                if (addr.Segmented && addr.SegmentId == segmentId)
                                {

                                    try
                                    {
                                        var tex = obj.AddTexture((int)(settilesize.lrs.Float() + 1), (int)(settilesize.lrt.Float() + 1), N64Texture.ConvertFormat(lastFmt, lastSiz), off: (int)addr.SegmentOff);
                                        if (lastFmt == G_IM_FMT.G_IM_FMT_CI)
                                            lastCiTex = tex;
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Error in Dlist 0x{new SegmentedAddress(segmentId, dlist).VAddr:X8} : {ex.Message}");
                                    }
                                }
                                else if (addr.Segmented && addr.SegmentId >= 8 && addr.SegmentOff == 0 && lastFmt != G_IM_FMT.G_IM_FMT_CI
                                        && !ambigiousSegments[addr.SegmentId])
                                { // enumerate texture loads from segment 8+ for animated material detection
                                    SegmentedTextureAttrs lastAttrs;
                                    SegmentedTextureAttrs attrs = new SegmentedTextureAttrs()
                                    {
                                        Width = (int)(settilesize.lrs.Float() + 1),
                                        Height = (int)(settilesize.lrt.Float() + 1),
                                        Fmt = N64Texture.ConvertFormat(lastFmt, lastSiz)
                                    };

                                    if (segmentedTextureLoads.TryGetValue(addr.SegmentId, out lastAttrs))
                                    {
                                        if (lastAttrs.Fmt != attrs.Fmt || lastAttrs.Width != attrs.Width || lastAttrs.Height != attrs.Height)
                                        {
                                            // Segment usage is ambiguous
                                            segmentedTextureLoads.Remove(addr.SegmentId);
                                            ambigiousSegments[addr.SegmentId] = true;
                                            errors.Add($"Error in Dlist 0x{new SegmentedAddress(segmentId, dlist).VAddr:X8} : Usage of segment 0x{addr.SegmentId:X2} is ambiguous");
                                        }
                                    }
                                    else
                                    {
                                        segmentedTextureLoads.Add(addr.SegmentId, attrs);
                                    }
                                }

                                lastFmt = (G_IM_FMT)(-1);
                                lastSiz = (G_IM_SIZ)(-1);
                                lastTexAddr = 0xFFFFFFFF;

                                break;
                            }
                        case CmdID.G_LOADTLUT:
                            {
                                var loadtlut = CmdInfo.DecodeCommand<GLoadTlut>(data, i);
                                var addr = new SegmentedAddress(lastTexAddr);

                                if (lastTexAddr == 0xFFFFFFFF)
                                    throw new Z64ObjectAnalyzerException();

                                int w = GetTlutWidth(loadtlut.count + 1);
                                if (addr.Segmented && addr.SegmentId == segmentId)
                                {
                                    try
                                    {
                                        lastTlut = obj.AddTexture(w, (loadtlut.count + 1) / w, N64Texture.ConvertFormat(G_IM_FMT.G_IM_FMT_RGBA, G_IM_SIZ.G_IM_SIZ_16b), off: (int)addr.SegmentOff);
                                    }
                                    catch (Exception ex)
                                    {
                                        errors.Add($"Error in Dlist 0x{new SegmentedAddress(segmentId, dlist).VAddr:X8} : {ex.Message}");
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            // These are carried out here as they are dependent on a lot of heuristics.
            // Having lots of the object already mapped out reduces possible mis-identifications.
            FindSkeletons(obj, data, segmentId);
            FindAnimations(obj, data, segmentId);
            FindCollisionData(obj, data, segmentId);
            FindMaterialAnimations(obj, data, segmentId, segmentedTextureLoads);
            FindPlayerAnimations(obj, data, segmentId);

            obj.GroupUnkEntries();
            obj.FixNames();
            obj.SetData(data);

            return errors;
        }
    }
}
