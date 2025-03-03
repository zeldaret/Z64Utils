using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using RDP;

#nullable enable

namespace F3DZEX.Command
{
    [Serializable]
    public class F3DZEXOpCodeSizeException : Exception
    {
        public F3DZEXOpCodeSizeException() { }

        public F3DZEXOpCodeSizeException(string message)
            : base(message) { }

        public F3DZEXOpCodeSizeException(string message, Exception inner)
            : base(message, inner) { }

        protected F3DZEXOpCodeSizeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context) { }
    }

    [Serializable]
    public class InvalidF3DZEXOpCodeException : Exception
    {
        public InvalidF3DZEXOpCodeException() { }

        public InvalidF3DZEXOpCodeException(string message)
            : base(message) { }

        public InvalidF3DZEXOpCodeException(string message, Exception inner)
            : base(message, inner) { }

        protected InvalidF3DZEXOpCodeException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context) { }
    }

    public static partial class CmdEncoding
    {
        public static List<CmdInfo> DecodeCmds(byte[] ucode, int off = 0)
        {
            var dlist = new List<CmdInfo>();

            using (MemoryStream ms = new MemoryStream(ucode))
            {
                ms.Position = off;
                BitReader br = new BitReader(ms);
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    CmdID id = (CmdID)br.ReadByte();

                    if (DEC_TABLE.ContainsKey(id))
                    {
                        var info = DEC_TABLE[id](br);
                        dlist.Add(info);
                    }
                    else
                        new InvalidF3DZEXOpCodeException($"Invalid OpCode : {id:X}");

                    if (id == CmdID.G_ENDDL)
                        break;
                }
            }
            return dlist;
        }

        public static byte[] EncodeCmds(List<CmdInfo> dlist)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BitWriter bw = new BitWriter(ms);
                foreach (var info in dlist)
                {
                    if (ENC_TABLE.ContainsKey(info.ID))
                        ENC_TABLE[info.ID](info, bw);
                    else
                        new InvalidF3DZEXOpCodeException($"Invalid OpCode : {info.ID:X}");
                }
                return ms.GetBuffer().Take((int)ms.Length).ToArray();
            }
        }

        public static readonly Dictionary<CmdID, Func<BitReader, CmdInfo>> DEC_TABLE =
            new Dictionary<CmdID, Func<BitReader, CmdInfo>>()
            {
                {
                    CmdID.G_NOOP,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        args.Add("tag", br.ReadUInt32());

                        return new CmdInfo(CmdID.G_NOOP, args);
                    }
                },
                {
                    CmdID.G_VTX,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(4);
                        int numv = br.ReadByte();
                        br.SkipBits(4);
                        int vbidx = (br.ReadByte() >> 1) - numv;
                        uint vaddr = br.ReadUInt32();

                        args.Add("numv", numv);
                        args.Add("vbidx", vbidx);
                        args.Add("vaddr", vaddr);

                        return new CmdInfo(CmdID.G_VTX, args);
                    }
                },
                {
                    CmdID.G_MODIFYVTX,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int where = br.ReadByte();
                        int vbidx = br.ReadUInt16() / 2;
                        uint val = br.ReadUInt32();

                        args.Add("where", where);
                        args.Add("vbidx", vbidx);
                        args.Add("val", val);

                        return new CmdInfo(CmdID.G_MODIFYVTX, args);
                    }
                },
                {
                    CmdID.G_CULLDL,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        int vfirst = br.ReadUInt16() / 2;
                        br.ReadUInt16();
                        int vlast = br.ReadUInt16() / 2;

                        args.Add("vfirst", vfirst);
                        args.Add("vlast", vlast);

                        return new CmdInfo(CmdID.G_CULLDL, args);
                    }
                },
                {
                    CmdID.G_BRANCH_Z,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int vbidx = br.ReadUInt16(12) / 5;
                        br.ReadUInt16(12);
                        uint zval = br.ReadUInt32();

                        args.Add("vbidx", vbidx);
                        args.Add("zval", zval);
                        return new CmdInfo(CmdID.G_BRANCH_Z, args);
                    }
                },
                {
                    CmdID.G_TRI1,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        byte v0 = (byte)(br.ReadByte() / 2);
                        byte v1 = (byte)(br.ReadByte() / 2);
                        byte v2 = (byte)(br.ReadByte() / 2);
                        br.ReadUInt32();

                        args.Add("v0", v0);
                        args.Add("v1", v1);
                        args.Add("v2", v2);

                        return new CmdInfo(CmdID.G_TRI1, args);
                    }
                },
                {
                    CmdID.G_TRI2,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        byte v00 = (byte)(br.ReadByte() / 2);
                        byte v01 = (byte)(br.ReadByte() / 2);
                        byte v02 = (byte)(br.ReadByte() / 2);
                        br.ReadByte();
                        byte v10 = (byte)(br.ReadByte() / 2);
                        byte v11 = (byte)(br.ReadByte() / 2);
                        byte v12 = (byte)(br.ReadByte() / 2);

                        args.Add("v00", v00);
                        args.Add("v01", v01);
                        args.Add("v02", v02);
                        args.Add("v10", v10);
                        args.Add("v11", v11);
                        args.Add("v12", v12);

                        return new CmdInfo(CmdID.G_TRI2, args);
                    }
                },
                {
                    CmdID.G_QUAD,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int v0 = br.ReadByte() / 2;
                        int v1 = br.ReadByte() / 2;
                        int v2 = br.ReadByte() / 2;
                        br.ReadByte();
                        br.ReadByte(); //v0 ?
                        br.ReadByte(); //v2 ?
                        int v3 = br.ReadByte() / 2;

                        args.Add("v0", v0);
                        args.Add("v1", v1);
                        args.Add("v2", v2);
                        args.Add("v3", v3);

                        return new CmdInfo(CmdID.G_QUAD, args);
                    }
                },
                {
                    CmdID.G_DMA_IO,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        bool flag = br.ReadBoolean();
                        uint dmem = br.ReadUInt32(10) * 8;
                        br.ReadByte(1);
                        int size = br.ReadInt32(12) + 1;
                        uint dram = br.ReadUInt32();

                        args.Add("flag", flag);
                        args.Add("dmem", dmem);
                        args.Add("size", size);
                        args.Add("dram", dram);

                        return new CmdInfo(CmdID.G_DMA_IO, args);
                    }
                },
                {
                    CmdID.G_TEXTURE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        br.ReadByte(2);
                        int level = br.ReadByte(3);
                        var tile = (G_TX_TILE)br.ReadByte(3);
                        var on = (G_TEX_ENABLE)br.ReadByte(7);
                        br.ReadByte(1);
                        ushort scaleS = br.ReadUInt16();
                        ushort scaleT = br.ReadUInt16();

                        args.Add("level", level);
                        args.Add("tile", tile);
                        args.Add("on", on);
                        args.Add("scaleS", scaleS);
                        args.Add("scaleT", scaleT);

                        return new CmdInfo(CmdID.G_TEXTURE, args);
                    }
                },
                {
                    CmdID.G_POPMTX,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        uint num = br.ReadUInt32() / 64;

                        args.Add("num", num);

                        return new CmdInfo(CmdID.G_POPMTX, args);
                    }
                },
                {
                    CmdID.G_GEOMETRYMODE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int clearbits = br.ReadInt32(24);
                        int setbits = br.ReadInt32();

                        args.Add("clearbits", clearbits);
                        args.Add("setbits", setbits);

                        return new CmdInfo(CmdID.G_GEOMETRYMODE, args);
                    }
                },
                {
                    CmdID.G_MTX,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadUInt16();
                        G_MTX_PARAM param = (G_MTX_PARAM)(br.ReadByte() ^ 1);
                        uint mtxaddr = br.ReadUInt32();

                        args.Add("mtxaddr", mtxaddr);
                        args.Add("param", param);

                        return new CmdInfo(CmdID.G_MTX, args);
                    }
                },
                {
                    CmdID.G_MOVEWORD,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int index = br.ReadByte();
                        int offset = br.ReadInt16();
                        uint data = br.ReadUInt32();

                        args.Add("index", index);
                        args.Add("offset", offset);
                        args.Add("data", data);

                        return new CmdInfo(CmdID.G_MOVEWORD, args);
                    }
                },
                {
                    CmdID.G_MOVEMEM,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        int size = ((br.ReadByte() >> 3) + 1) * 8;
                        int offset = br.ReadByte() * 8;
                        int index = br.ReadByte();
                        uint address = br.ReadUInt32();

                        args.Add("size", size);
                        args.Add("offset", offset);
                        args.Add("index", index);
                        args.Add("address", address);

                        return new CmdInfo(CmdID.G_MOVEMEM, args);
                    }
                },
                {
                    CmdID.G_LOAD_UCODE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        int dsize = br.ReadUInt16();
                        uint tstart = br.ReadUInt32();

                        args.Add("dsize", dsize);
                        args.Add("tstart", tstart);
                        return new CmdInfo(CmdID.G_LOAD_UCODE, args);
                    }
                },
                {
                    CmdID.G_DL,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        bool branch = br.ReadByte() != 0;
                        br.ReadUInt16();
                        uint dl = br.ReadUInt32();

                        args.Add("branch", branch);
                        args.Add("dl", dl);

                        return new CmdInfo(CmdID.G_DL, args);
                    }
                },
                {
                    CmdID.G_ENDDL,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24 + 32);

                        return new CmdInfo(CmdID.G_ENDDL, args);
                    }
                },
                {
                    CmdID.G_SPNOOP,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24 + 32);

                        return new CmdInfo(CmdID.G_SPNOOP, args);
                    }
                },
                {
                    CmdID.G_RDPHALF_1,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        uint word = br.ReadUInt32();

                        args.Add("word", word);
                        return new CmdInfo(CmdID.G_RDPHALF_1, args);
                    }
                },
                {
                    CmdID.G_SETOTHERMODE_L,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        byte ss = br.ReadByte();
                        int len = br.ReadByte() + 1;
                        int shift = (int)(32 - len - ss);
                        uint data = br.ReadUInt32();

                        args.Add("shift", shift);
                        args.Add("len", len);
                        args.Add("data", data);

                        return new CmdInfo(CmdID.G_SETOTHERMODE_L, args);
                    }
                },
                {
                    CmdID.G_SETOTHERMODE_H,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        byte ss = br.ReadByte();
                        int len = br.ReadByte() + 1;
                        int shift = (int)(32 - len - ss);
                        uint data = br.ReadUInt32();

                        args.Add("shift", shift);
                        args.Add("len", len);
                        args.Add("data", data);

                        return new CmdInfo(CmdID.G_SETOTHERMODE_H, args);
                    }
                },
                {
                    CmdID.G_TEXRECT,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        FixedPoint lrx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        FixedPoint lry = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(4);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        FixedPoint ulx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        FixedPoint uly = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(); //E1
                        br.SkipBits(3 * 8);
                        FixedPoint uls = new FixedPoint(br.ReadInt32(16), 10, 5, true);
                        FixedPoint ult = new FixedPoint(br.ReadInt32(16), 10, 5, true);
                        br.ReadByte(); //F1
                        br.SkipBits(3 * 8);
                        FixedPoint dsdx = new FixedPoint(br.ReadInt32(16), 5, 10, true);
                        FixedPoint dtdy = new FixedPoint(br.ReadInt32(16), 5, 10, true);

                        args.Add("lrx", lrx);
                        args.Add("lry", lry);
                        args.Add("tile", tile);
                        args.Add("ulx", ulx);
                        args.Add("uly", uly);
                        args.Add("uls", uls);
                        args.Add("ult", ult);
                        args.Add("dsdx", dsdx);
                        args.Add("dtdy", dtdy);
                        return new CmdInfo(CmdID.G_TEXRECT, args);
                    }
                },
                {
                    CmdID.G_TEXRECTFLIP,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        FixedPoint lrx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        FixedPoint lry = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(4);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        FixedPoint ulx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        FixedPoint uly = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(); //E1
                        br.SkipBits(3 * 8);
                        FixedPoint uls = new FixedPoint(br.ReadInt32(16), 10, 5, true);
                        FixedPoint ult = new FixedPoint(br.ReadInt32(16), 10, 5, true);
                        br.ReadByte(); //F1
                        br.SkipBits(3 * 8);
                        FixedPoint dsdx = new FixedPoint(br.ReadInt32(16), 5, 10, true);
                        FixedPoint dtdy = new FixedPoint(br.ReadInt32(16), 5, 10, true);

                        args.Add("lrx", lrx);
                        args.Add("lry", lry);
                        args.Add("tile", tile);
                        args.Add("ulx", ulx);
                        args.Add("uly", uly);
                        args.Add("uls", uls);
                        args.Add("ult", ult);
                        args.Add("dsdx", dsdx);
                        args.Add("dtdy", dtdy);
                        return new CmdInfo(CmdID.G_TEXRECTFLIP, args);
                    }
                },
                {
                    CmdID.G_RDPLOADSYNC,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();
                        br.SkipBits(56);
                        return new CmdInfo(CmdID.G_RDPLOADSYNC, args);
                    }
                },
                {
                    CmdID.G_RDPPIPESYNC,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();
                        br.SkipBits(56);
                        return new CmdInfo(CmdID.G_RDPPIPESYNC, args);
                    }
                },
                {
                    CmdID.G_RDPTILESYNC,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();
                        br.SkipBits(56);
                        return new CmdInfo(CmdID.G_RDPTILESYNC, args);
                    }
                },
                {
                    CmdID.G_RDPFULLSYNC,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();
                        br.SkipBits(56);
                        return new CmdInfo(CmdID.G_RDPFULLSYNC, args);
                    }
                },
                {
                    CmdID.G_SETKEYGB,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var widthG = new FixedPoint(br.ReadInt32(12), 4, 8);
                        var widthB = new FixedPoint(br.ReadInt32(12), 4, 8);
                        byte centerG = br.ReadByte();
                        byte scaleG = br.ReadByte();
                        byte centerB = br.ReadByte();
                        byte scaleB = br.ReadByte();

                        args.Add("widthG", widthG);
                        args.Add("widthB", widthB);
                        args.Add("centerG", centerG);
                        args.Add("scaleG", scaleG);
                        args.Add("centerB", centerB);
                        args.Add("scaleB", scaleB);
                        return new CmdInfo(CmdID.G_SETKEYGB, args);
                    }
                },
                {
                    CmdID.G_SETKEYR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(28);
                        var widthR = new FixedPoint(br.ReadInt32(12), 4, 8);
                        byte centerR = br.ReadByte();
                        byte scaleR = br.ReadByte();

                        args.Add("widthR", widthR);
                        args.Add("centerR", centerR);
                        args.Add("scaleR", scaleR);
                        return new CmdInfo(CmdID.G_SETKEYR, args);
                    }
                },
                {
                    CmdID.G_SETCONVERT,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte(2);
                        int k0 = br.ReadSigned(8);
                        int k1 = br.ReadSigned(8);
                        int k2 = br.ReadSigned(8);
                        int k3 = br.ReadSigned(8);
                        int k4 = br.ReadSigned(8);
                        int k5 = br.ReadSigned(8);

                        args.Add("k0", k0);
                        args.Add("k1", k1);
                        args.Add("k2", k2);
                        args.Add("k3", k3);
                        args.Add("k4", k4);
                        args.Add("k5", k5);
                        return new CmdInfo(CmdID.G_SETCONVERT, args);
                    }
                },
                {
                    CmdID.G_SETSCISSOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var ulx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var uly = new FixedPoint(br.ReadInt32(12), 10, 2);
                        int mode = br.ReadInt32(4);
                        br.ReadInt32(4);
                        var lrx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var lry = new FixedPoint(br.ReadInt32(12), 10, 2);

                        args.Add("ulx", ulx);
                        args.Add("uly", uly);
                        args.Add("mode", mode);
                        args.Add("lrx", lrx);
                        args.Add("lry", lry);
                        return new CmdInfo(CmdID.G_SETSCISSOR, args);
                    }
                },
                {
                    CmdID.G_SETPRIMDEPTH,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        short z = br.ReadInt16();
                        short dz = br.ReadInt16();

                        args.Add("z", z);
                        args.Add("dz", dz);
                        return new CmdInfo(CmdID.G_SETPRIMDEPTH, args);
                    }
                },
                {
                    CmdID.G_RDPSETOTHERMODE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        uint omodeH = br.ReadUInt32(24);
                        uint omodeL = br.ReadUInt32();

                        args.Add("omodeH", omodeH);
                        args.Add("omodeL", omodeL);
                        return new CmdInfo(CmdID.G_RDPSETOTHERMODE, args);
                    }
                },
                {
                    CmdID.G_LOADTLUT,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(28);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        int count = br.ReadUInt16(12) >> 2;
                        br.ReadUInt16(12);

                        args.Add("tile", tile);
                        args.Add("count", count);
                        return new CmdInfo(CmdID.G_LOADTLUT, args);
                    }
                },
                {
                    CmdID.G_RDPHALF_2,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        uint word = br.ReadUInt32();

                        args.Add("word", word);
                        return new CmdInfo(CmdID.G_RDPHALF_2, args);
                    }
                },
                {
                    CmdID.G_SETTILESIZE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var uls = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var ult = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(4);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        var lrs = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var lrt = new FixedPoint(br.ReadInt32(12), 10, 2);

                        args.Add("uls", uls);
                        args.Add("ult", ult);
                        args.Add("tile", tile);
                        args.Add("lrs", lrs);
                        args.Add("lrt", lrt);
                        return new CmdInfo(CmdID.G_SETTILESIZE, args);
                    }
                },
                {
                    CmdID.G_LOADBLOCK,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var uls = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var ult = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(4);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        int texels = br.ReadInt32(12);
                        var dxt = new FixedPoint(br.ReadInt32(12), 1, 11);

                        args.Add("uls", uls);
                        args.Add("ult", ult);
                        args.Add("tile", tile);
                        args.Add("texels", texels);
                        args.Add("dxt", dxt);
                        return new CmdInfo(CmdID.G_LOADBLOCK, args);
                    }
                },
                {
                    CmdID.G_LOADTILE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var uls = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var ult = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte(4);
                        var tile = (G_TX_TILE)br.ReadByte(4);
                        var lrs = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var lrt = new FixedPoint(br.ReadInt32(12), 10, 2);

                        args.Add("uls", uls);
                        args.Add("ult", ult);
                        args.Add("tile", tile);
                        args.Add("lrs", lrs);
                        args.Add("lrt", lrt);
                        return new CmdInfo(CmdID.G_LOADTILE, args);
                    }
                },
                {
                    CmdID.G_SETTILE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var fmt = (G_IM_FMT)br.ReadByte(3);
                        var siz = (G_IM_SIZ)br.ReadByte(2);
                        br.ReadByte(1);
                        int line = br.ReadInt32(9);
                        int tmem = br.ReadInt32(9);
                        br.ReadByte(5);
                        var tile = (G_TX_TILE)br.ReadByte(3);
                        int palette = br.ReadByte(4);
                        var cmT = (G_TX_TEXWRAP)br.ReadByte(2);
                        int maskT = br.ReadByte(4);
                        int shiftT = br.ReadByte(4);
                        var cmS = (G_TX_TEXWRAP)br.ReadByte(2);
                        int maskS = br.ReadByte(4);
                        int shiftS = br.ReadByte(4);

                        args.Add("fmt", fmt);
                        args.Add("siz", siz);
                        args.Add("line", line);
                        args.Add("tmem", tmem);
                        args.Add("tile", tile);
                        args.Add("palette", palette);
                        args.Add("cmT", cmT);
                        args.Add("maskT", maskT);
                        args.Add("shiftT", shiftT);
                        args.Add("cmS", cmS);
                        args.Add("maskS", maskS);
                        args.Add("shiftS", shiftS);
                        return new CmdInfo(CmdID.G_SETTILE, args);
                    }
                },
                {
                    CmdID.G_FILLRECT,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var lrx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var lry = new FixedPoint(br.ReadInt32(12), 10, 2);
                        br.ReadByte();
                        var ulx = new FixedPoint(br.ReadInt32(12), 10, 2);
                        var uly = new FixedPoint(br.ReadInt32(12), 10, 2);

                        args.Add("lrx", lrx);
                        args.Add("lry", lry);
                        args.Add("ulx", ulx);
                        args.Add("uly", uly);
                        return new CmdInfo(CmdID.G_FILLRECT, args);
                    }
                },
                {
                    CmdID.G_SETFILLCOLOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        uint color = br.ReadUInt32();

                        args.Add("color", color);
                        return new CmdInfo(CmdID.G_SETFILLCOLOR, args);
                    }
                },
                {
                    CmdID.G_SETFOGCOLOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        byte R = br.ReadByte();
                        byte G = br.ReadByte();
                        byte B = br.ReadByte();
                        byte A = br.ReadByte();

                        args.Add("R", R);
                        args.Add("G", G);
                        args.Add("B", B);
                        args.Add("A", A);
                        return new CmdInfo(CmdID.G_SETFOGCOLOR, args);
                    }
                },
                {
                    CmdID.G_SETBLENDCOLOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        byte R = br.ReadByte();
                        byte G = br.ReadByte();
                        byte B = br.ReadByte();
                        byte A = br.ReadByte();

                        args.Add("R", R);
                        args.Add("G", G);
                        args.Add("B", B);
                        args.Add("A", A);
                        return new CmdInfo(CmdID.G_SETBLENDCOLOR, args);
                    }
                },
                {
                    CmdID.G_SETPRIMCOLOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.ReadByte();
                        byte minlevel = br.ReadByte();
                        byte lodfrac = br.ReadByte();
                        byte R = br.ReadByte();
                        byte G = br.ReadByte();
                        byte B = br.ReadByte();
                        byte A = br.ReadByte();

                        args.Add("minlevel", minlevel);
                        args.Add("lodfrac", lodfrac);
                        args.Add("R", R);
                        args.Add("G", G);
                        args.Add("B", B);
                        args.Add("A", A);
                        return new CmdInfo(CmdID.G_SETPRIMCOLOR, args);
                    }
                },
                {
                    CmdID.G_SETENVCOLOR,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        byte R = br.ReadByte();
                        byte G = br.ReadByte();
                        byte B = br.ReadByte();
                        byte A = br.ReadByte();

                        args.Add("R", R);
                        args.Add("G", G);
                        args.Add("B", B);
                        args.Add("A", A);
                        return new CmdInfo(CmdID.G_SETENVCOLOR, args);
                    }
                },
                {
                    CmdID.G_SETCOMBINE,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var a0 = (G_CCMUX)br.ReadInt32(4);
                        var c0 = (G_CCMUX)br.ReadInt32(5);
                        var Aa0 = (G_ACMUX)br.ReadInt32(3);
                        var Ac0 = (G_ACMUX)br.ReadInt32(3);
                        var a1 = (G_CCMUX)br.ReadInt32(4);
                        var c1 = (G_CCMUX)br.ReadInt32(5);
                        var b0 = (G_CCMUX)br.ReadInt32(4);
                        var b1 = (G_CCMUX)br.ReadInt32(4);
                        var Aa1 = (G_ACMUX)br.ReadInt32(3);
                        var Ac1 = (G_ACMUX)br.ReadInt32(3);
                        var d0 = (G_CCMUX)br.ReadInt32(3);
                        var Ab0 = (G_ACMUX)br.ReadInt32(3);
                        var Ad0 = (G_ACMUX)br.ReadInt32(3);
                        var d1 = (G_CCMUX)br.ReadInt32(3);
                        var Ab1 = (G_ACMUX)br.ReadInt32(3);
                        var Ad1 = (G_ACMUX)br.ReadInt32(3);

                        args.Add("a0", a0);
                        args.Add("b0", b0);
                        args.Add("c0", c0);
                        args.Add("d0", d0);
                        args.Add("Aa0", Aa0);
                        args.Add("Ab0", Ab0);
                        args.Add("Ac0", Ac0);
                        args.Add("Ad0", Ad0);
                        args.Add("a1", a1);
                        args.Add("b1", b1);
                        args.Add("c1", c1);
                        args.Add("d1", d1);
                        args.Add("Aa1", Aa1);
                        args.Add("Ab1", Ab1);
                        args.Add("Ac1", Ac1);
                        args.Add("Ad1", Ad1);
                        return new CmdInfo(CmdID.G_SETCOMBINE, args);
                    }
                },
                {
                    CmdID.G_SETTIMG,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var fmt = (G_IM_FMT)br.ReadByte(3);
                        var siz = (G_IM_SIZ)br.ReadByte(2);
                        br.SkipBits(3 + 4);
                        int width = br.ReadInt32(12) + 1;
                        uint imgaddr = br.ReadUInt32();

                        args.Add("fmt", fmt);
                        args.Add("siz", siz);
                        args.Add("width", width);
                        args.Add("imgaddr", imgaddr);
                        return new CmdInfo(CmdID.G_SETTIMG, args);
                    }
                },
                {
                    CmdID.G_SETZIMG,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        br.SkipBits(24);
                        uint imgaddr = br.ReadUInt32();

                        args.Add("imgaddr", imgaddr);
                        return new CmdInfo(CmdID.G_SETZIMG, args);
                    }
                },
                {
                    CmdID.G_SETCIMG,
                    (br) =>
                    {
                        Dictionary<string, object> args = new Dictionary<string, object>();

                        var fmt = (G_IM_FMT)br.ReadByte(3);
                        var siz = (G_IM_SIZ)br.ReadByte(2);
                        br.ReadByte(3 + 4);
                        int width = br.ReadUInt16(12) + 1;
                        uint imgaddr = br.ReadUInt32();

                        args.Add("fmt", fmt);
                        args.Add("siz", siz);
                        args.Add("width", width);
                        args.Add("imgaddr", imgaddr);
                        return new CmdInfo(CmdID.G_SETCIMG, args);
                    }
                },
            };

        public static readonly Dictionary<CmdID, Action<CmdInfo, BitWriter>> ENC_TABLE =
            new Dictionary<CmdID, Action<CmdInfo, BitWriter>>()
            {
                {
                    CmdID.G_NOOP,
                    (info, bw) =>
                    {
                        uint tag = (uint)info.Args["tag"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(tag);
                    }
                },
                {
                    CmdID.G_VTX,
                    (info, bw) =>
                    {
                        int numv = (int)info.Args["numv"];
                        int vbidx = (int)info.Args["vbidx"];
                        uint vaddr = (uint)info.Args["vaddr"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 4);
                        bw.Write(numv, 8);
                        bw.Write(0, 4);
                        bw.Write((byte)(((vbidx + numv) & 0x7F) << 1));
                        bw.Write(vaddr);
                    }
                },
                {
                    CmdID.G_MODIFYVTX,
                    (info, bw) =>
                    {
                        int where = (int)info.Args["where"];
                        int vbidx = (int)info.Args["vbidx"];
                        uint val = (uint)info.Args["val"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)where);
                        bw.Write(vbidx * 2);
                        bw.Write(val);
                    }
                },
                {
                    CmdID.G_CULLDL,
                    (info, bw) =>
                    {
                        int vfirst = (int)info.Args["vfirst"];
                        int vlast = (int)info.Args["vlast"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)0);
                        bw.Write((ushort)(vfirst * 2));
                        bw.Write((ushort)0);
                        bw.Write((ushort)(vlast * 2));
                    }
                },
                {
                    CmdID.G_BRANCH_Z,
                    (info, bw) =>
                    {
                        int vbidx = (int)info.Args["vbidx"];
                        uint zval = (uint)info.Args["zval"];

                        bw.Write((byte)info.ID);
                        bw.Write(vbidx * 5, 3 * 4);
                        bw.Write(vbidx * 2, 3 * 4);
                        bw.Write(zval);
                    }
                },
                {
                    CmdID.G_TRI1,
                    (info, bw) =>
                    {
                        byte v0 = (byte)info.Args["v0"];
                        byte v1 = (byte)info.Args["v1"];
                        byte v2 = (byte)info.Args["v2"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)(v0 * 2));
                        bw.Write((byte)(v1 * 2));
                        bw.Write((byte)(v2 * 2));
                        bw.Write(0);
                    }
                },
                {
                    CmdID.G_TRI2,
                    (info, bw) =>
                    {
                        byte v00 = (byte)info.Args["v00"];
                        byte v01 = (byte)info.Args["v01"];
                        byte v02 = (byte)info.Args["v02"];
                        byte v10 = (byte)info.Args["v10"];
                        byte v11 = (byte)info.Args["v11"];
                        byte v12 = (byte)info.Args["v12"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)(v00 * 2));
                        bw.Write((byte)(v01 * 2));
                        bw.Write((byte)(v02 * 2));
                        bw.Write((byte)0);
                        bw.Write((byte)(v10 * 2));
                        bw.Write((byte)(v11 * 2));
                        bw.Write((byte)(v12 * 2));
                    }
                },
                {
                    CmdID.G_QUAD,
                    (info, bw) =>
                    {
                        int v0 = (int)info.Args["v0"];
                        int v1 = (int)info.Args["v1"];
                        int v2 = (int)info.Args["v2"];
                        int v3 = (int)info.Args["v3"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)(v0 * 2));
                        bw.Write((byte)(v1 * 2));
                        bw.Write((byte)(v2 * 2));
                        bw.Write((byte)0);
                        bw.Write((byte)(v0 * 2));
                        bw.Write((byte)(v2 * 2));
                        bw.Write((byte)(v3 * 2));
                    }
                },
                {
                    CmdID.G_DMA_IO,
                    (info, bw) =>
                    {
                        bool flag = (bool)info.Args["flag"];
                        uint dmem = (uint)info.Args["dmem"];
                        int size = (int)info.Args["size"];
                        uint dram = (uint)info.Args["dram"];

                        bw.Write((byte)info.ID);
                        bw.Write(flag);
                        bw.Write(dmem / 8 & 0x3FF, 10);
                        bw.WriteBit(false);
                        bw.Write(size - 1, 12);
                        bw.Write(dram);
                    }
                },
                {
                    CmdID.G_TEXTURE,
                    (info, bw) =>
                    {
                        int level = (int)info.Args["level"];
                        int tile = (int)info.Args["tile"];
                        G_TEX_ENABLE on = (G_TEX_ENABLE)info.Args["on"];
                        ushort scaleS = (ushort)info.Args["scaleS"];
                        ushort scaleT = (ushort)info.Args["scaleT"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 8 + 2);
                        bw.Write(level & 7, 3);
                        bw.Write(tile & 7, 3);
                        bw.Write((byte)on, 7);
                        bw.Write(false);
                        bw.Write(scaleS);
                        bw.Write(scaleT);
                    }
                },
                {
                    CmdID.G_POPMTX,
                    (info, bw) =>
                    {
                        uint num = (uint)info.Args["num"];

                        bw.Write((byte)info.ID);
                        bw.Write(0x38_00_02, 3 * 8);
                        bw.Write(num * 64);
                    }
                },
                {
                    CmdID.G_GEOMETRYMODE,
                    (info, bw) =>
                    {
                        int clearbits = (int)info.Args["clearbits"];
                        int setbits = (int)info.Args["setbits"];

                        bw.Write((byte)info.ID);
                        bw.Write(clearbits, 24);
                        bw.Write(setbits, 24);
                    }
                },
                {
                    CmdID.G_MTX,
                    (info, bw) =>
                    {
                        uint mtxaddr = (uint)info.Args["mtxaddr"];
                        int param = (int)info.Args["param"];

                        bw.Write((byte)info.ID);
                        bw.Write((ushort)0x3800);
                        bw.Write((byte)(param ^ 1));
                        bw.Write(mtxaddr);
                    }
                },
                {
                    CmdID.G_MOVEWORD,
                    (info, bw) =>
                    {
                        int index = (int)info.Args["index"];
                        int offset = (int)info.Args["offset"];
                        uint data = (uint)info.Args["data"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)index);
                        bw.Write(offset, 24);
                        bw.Write(data);
                    }
                },
                {
                    CmdID.G_MOVEMEM,
                    (info, bw) =>
                    {
                        int size = (int)info.Args["size"];
                        int offset = (int)info.Args["offset"];
                        int index = (int)info.Args["index"];
                        uint address = (uint)info.Args["address"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)(((size - 1) / 8 & 0x1F) << 3));
                        bw.Write((byte)(offset / 8));
                        bw.Write((byte)index);
                        bw.Write(address);
                    }
                },
                {
                    CmdID.G_LOAD_UCODE,
                    (info, bw) =>
                    {
                        int dsize = (int)info.Args["dsize"];
                        uint tstart = (uint)info.Args["tstart"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)0);
                        bw.Write((ushort)dsize);
                        bw.Write(tstart);
                    }
                },
                {
                    CmdID.G_DL,
                    (info, bw) =>
                    {
                        bool branch = (bool)info.Args["branch"];
                        uint dl = (uint)info.Args["dl"];

                        bw.Write((byte)info.ID);
                        bw.Write(Convert.ToByte(branch));
                        bw.Write((ushort)0);
                        bw.Write(dl);
                    }
                },
                {
                    CmdID.G_ENDDL,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(0);
                    }
                },
                {
                    CmdID.G_SPNOOP,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(0);
                    }
                },
                {
                    CmdID.G_RDPHALF_1,
                    (info, bw) =>
                    {
                        uint word = (uint)info.Args["word"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(word);
                    }
                },
                {
                    CmdID.G_SETOTHERMODE_L,
                    (info, bw) =>
                    {
                        int shift = (int)info.Args["shift"];
                        int len = (int)info.Args["len"];
                        uint data = (uint)info.Args["data"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)0);
                        bw.Write((byte)(32 - (int)shift - len));
                        bw.Write((byte)len - 1);
                        bw.Write(data);
                    }
                },
                {
                    CmdID.G_SETOTHERMODE_H,
                    (info, bw) =>
                    {
                        int shift = (int)info.Args["shift"];
                        int len = (int)info.Args["len"];
                        uint data = (uint)info.Args["data"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)0);
                        bw.Write((byte)(32 - (int)shift - len));
                        bw.Write((byte)len - 1);
                        bw.Write(data);
                    }
                },
                {
                    CmdID.G_TEXRECT,
                    (info, bw) =>
                    {
                        var lrx = (FixedPoint)info.Args["lrx"];
                        var lry = (FixedPoint)info.Args["lry"];
                        int tile = (int)info.Args["tile"];
                        var ulx = (FixedPoint)info.Args["ulx"];
                        var uly = (FixedPoint)info.Args["uly"];
                        var uls = (FixedPoint)info.Args["uls"];
                        var ult = (FixedPoint)info.Args["ult"];
                        var dsdx = (FixedPoint)info.Args["dsdx"];
                        var dtdy = (FixedPoint)info.Args["dtdy"];

                        bw.Write((byte)info.ID);
                        bw.Write(lrx.Raw, 12);
                        bw.Write(lry.Raw, 12);
                        bw.Write(0, 4);
                        bw.Write(tile, 4);
                        bw.Write(ulx.Raw, 12);
                        bw.Write(uly.Raw, 12);
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(uls.Raw, 16);
                        bw.Write(ult.Raw, 16);
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(dsdx.Raw, 16);
                        bw.Write(dtdy.Raw, 16);
                    }
                },
                {
                    CmdID.G_TEXRECTFLIP,
                    (info, bw) =>
                    {
                        var lrx = (FixedPoint)info.Args["lrx"];
                        var lry = (FixedPoint)info.Args["lry"];
                        int tile = (int)info.Args["tile"];
                        var ulx = (FixedPoint)info.Args["ulx"];
                        var uly = (FixedPoint)info.Args["uly"];
                        var uls = (FixedPoint)info.Args["uls"];
                        var ult = (FixedPoint)info.Args["ult"];
                        var dsdx = (FixedPoint)info.Args["dsdx"];
                        var dtdy = (FixedPoint)info.Args["dtdy"];

                        bw.Write((byte)info.ID);
                        bw.Write(lrx.Raw, 12);
                        bw.Write(lry.Raw, 12);
                        bw.Write(0, 4);
                        bw.Write(tile, 4);
                        bw.Write(ulx.Raw, 12);
                        bw.Write(uly.Raw, 12);
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(uls.Raw, 16);
                        bw.Write(ult.Raw, 16);
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(dsdx.Raw, 16);
                        bw.Write(dtdy.Raw, 16);
                    }
                },
                {
                    CmdID.G_RDPLOADSYNC,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(32);
                    }
                },
                {
                    CmdID.G_RDPPIPESYNC,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(32);
                    }
                },
                {
                    CmdID.G_RDPTILESYNC,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(32);
                    }
                },
                {
                    CmdID.G_RDPFULLSYNC,
                    (info, bw) =>
                    {
                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(32);
                    }
                },
                {
                    CmdID.G_SETKEYGB,
                    (info, bw) =>
                    {
                        var widthG = (FixedPoint)info.Args["widthG"];
                        var widthB = (FixedPoint)info.Args["widthB"];
                        byte centerG = (byte)info.Args["centerG"];
                        byte scaleG = (byte)info.Args["scaleG"];
                        byte centerB = (byte)info.Args["centerB"];
                        byte scaleB = (byte)info.Args["scaleB"];

                        bw.Write((byte)info.ID);
                        bw.Write(widthG.Raw, 12);
                        bw.Write(widthB.Raw, 12);
                        bw.Write(centerG);
                        bw.Write(centerG);
                        bw.Write(scaleG);
                        bw.Write(centerB);
                        bw.Write(scaleB);
                    }
                },
                {
                    CmdID.G_SETKEYR,
                    (info, bw) =>
                    {
                        var widthR = (FixedPoint)info.Args["widthR"];
                        byte centerR = (byte)info.Args["centerR"];
                        byte scaleR = (byte)info.Args["scaleR"];

                        bw.Write(0, 28);
                        bw.Write((byte)info.ID);
                        bw.Write(widthR.Raw, 12);
                        bw.Write(centerR);
                        bw.Write(scaleR);
                    }
                },
                {
                    CmdID.G_SETCONVERT,
                    (info, bw) =>
                    {
                        int k0 = (int)info.Args["k0"];
                        int k1 = (int)info.Args["k1"];
                        int k2 = (int)info.Args["k2"];
                        int k3 = (int)info.Args["k3"];
                        int k4 = (int)info.Args["k4"];
                        int k5 = (int)info.Args["k5"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 2);
                        bw.WriteSigned(k0, 8);
                        bw.WriteSigned(k1, 8);
                        bw.WriteSigned(k2, 8);
                        bw.WriteSigned(k3, 8);
                        bw.WriteSigned(k4, 8);
                        bw.WriteSigned(k5, 8);
                    }
                },
                {
                    CmdID.G_SETSCISSOR,
                    (info, bw) =>
                    {
                        var ulx = (FixedPoint)info.Args["ulx"];
                        var uly = (FixedPoint)info.Args["uly"];
                        int mode = (int)info.Args["mode"];
                        var lrx = (FixedPoint)info.Args["lrx"];
                        var lry = (FixedPoint)info.Args["lry"];

                        bw.Write((byte)info.ID);
                        bw.Write(ulx.Raw, 12);
                        bw.Write(uly.Raw, 12);
                        bw.Write(mode, 4);
                        bw.Write(0, 4);
                        bw.Write(lrx.Raw, 12);
                        bw.Write(lry.Raw, 12);
                    }
                },
                {
                    CmdID.G_SETPRIMDEPTH,
                    (info, bw) =>
                    {
                        int z = (int)info.Args["z"];
                        int dz = (int)info.Args["dz"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write((short)z);
                        bw.Write((short)dz);
                    }
                },
                {
                    CmdID.G_RDPSETOTHERMODE,
                    (info, bw) =>
                    {
                        uint omodeH = (uint)info.Args["omodeH"];
                        uint omodeL = (uint)info.Args["omodeL"];

                        bw.Write((byte)info.ID);
                        bw.Write(omodeH, 24);
                        bw.Write(omodeL, 24);
                    }
                },
                {
                    CmdID.G_LOADTLUT,
                    (info, bw) =>
                    {
                        int tile = (int)info.Args["tile"];
                        int count = (int)info.Args["count"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 28);
                        bw.Write(tile, 4);
                        bw.Write((count & 0x3FF) << 2, 12);
                        bw.Write(0, 12);
                    }
                },
                {
                    CmdID.G_RDPHALF_2,
                    (info, bw) =>
                    {
                        uint word = (uint)info.Args["word"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(word);
                    }
                },
                {
                    CmdID.G_SETTILESIZE,
                    (info, bw) =>
                    {
                        var uls = (FixedPoint)info.Args["uls"];
                        var ult = (FixedPoint)info.Args["ult"];
                        int tile = (int)info.Args["tile"];
                        var lrs = (FixedPoint)info.Args["lrs"];
                        var lrt = (FixedPoint)info.Args["lrt"];

                        bw.Write((byte)info.ID);
                        bw.Write(uls.Raw, 12);
                        bw.Write(ult.Raw, 12);
                        bw.Write(0, 4);
                        bw.Write(tile, 4);
                        bw.Write(lrs.Raw, 12);
                        bw.Write(lrt.Raw, 12);
                    }
                },
                {
                    CmdID.G_LOADBLOCK,
                    (info, bw) =>
                    {
                        var uls = (FixedPoint)info.Args["uls"];
                        var ult = (FixedPoint)info.Args["ult"];
                        int tile = (int)info.Args["tile"];
                        int texels = (int)info.Args["texels"];
                        var dxt = (FixedPoint)info.Args["dxt"];

                        bw.Write((byte)info.ID);
                        bw.Write(uls.Raw, 12);
                        bw.Write(ult.Raw, 12);
                        bw.Write(0, 4);
                        bw.Write(tile, 4);
                        bw.Write(texels, 12);
                        bw.Write(dxt.Raw, 12);
                    }
                },
                {
                    CmdID.G_LOADTILE,
                    (info, bw) =>
                    {
                        var uls = (FixedPoint)info.Args["uls"];
                        var ult = (FixedPoint)info.Args["ult"];
                        int tile = (int)info.Args["tile"];
                        var lrs = (FixedPoint)info.Args["lrs"];
                        var lrt = (FixedPoint)info.Args["lrt"];

                        bw.Write((byte)info.ID);
                        bw.Write(uls.Raw, 12);
                        bw.Write(ult.Raw, 12);
                        bw.Write(0, 4);
                        bw.Write(tile, 4);
                        bw.Write(lrs.Raw, 12);
                        bw.Write(lrt.Raw, 12);
                    }
                },
                {
                    CmdID.G_SETTILE,
                    (info, bw) =>
                    {
                        var fmt = (G_IM_FMT)info.Args["fmt"];
                        var siz = (G_IM_SIZ)info.Args["siz"];
                        int line = (int)info.Args["line"];
                        int tmem = (int)info.Args["tmem"];
                        int tile = (int)info.Args["tile"];
                        int palette = (int)info.Args["palette"];
                        int cmT = (int)info.Args["cmT"];
                        int maskT = (int)info.Args["maskT"];
                        int shiftT = (int)info.Args["shiftT"];
                        int cmS = (int)info.Args["cmS"];
                        int maskS = (int)info.Args["maskS"];
                        int shiftS = (int)info.Args["shiftS"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)fmt, 3);
                        bw.Write((byte)siz, 2);
                        bw.Write(false);
                        bw.Write(line, 9);
                        bw.Write(tmem, 9);
                        bw.Write(0, 5);
                        bw.Write(tile, 3);
                        bw.Write(palette, 3);
                        bw.Write(cmT, 2);
                        bw.Write(maskT, 4);
                        bw.Write(shiftT, 4);
                        bw.Write(cmS, 2);
                        bw.Write(maskS, 4);
                        bw.Write(shiftS, 4);
                    }
                },
                {
                    CmdID.G_FILLRECT,
                    (info, bw) =>
                    {
                        var lrx = (FixedPoint)info.Args["lrx"];
                        var lry = (FixedPoint)info.Args["lry"];
                        var ulx = (FixedPoint)info.Args["ulx"];
                        var uly = (FixedPoint)info.Args["uly"];

                        bw.Write((byte)info.ID);
                        bw.Write(lrx.Raw, 12);
                        bw.Write(lry.Raw, 12);
                        bw.Write((byte)0);
                        bw.Write(ulx.Raw, 12);
                        bw.Write(uly.Raw, 12);
                    }
                },
                {
                    CmdID.G_SETFILLCOLOR,
                    (info, bw) =>
                    {
                        uint color = (uint)info.Args["color"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(color);
                    }
                },
                {
                    CmdID.G_SETFOGCOLOR,
                    (info, bw) =>
                    {
                        byte R = (byte)info.Args["R"];
                        byte G = (byte)info.Args["G"];
                        byte B = (byte)info.Args["B"];
                        byte A = (byte)info.Args["A"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(R);
                        bw.Write(G);
                        bw.Write(B);
                        bw.Write(A);
                    }
                },
                {
                    CmdID.G_SETBLENDCOLOR,
                    (info, bw) =>
                    {
                        byte R = (byte)info.Args["R"];
                        byte G = (byte)info.Args["G"];
                        byte B = (byte)info.Args["B"];
                        byte A = (byte)info.Args["A"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(R);
                        bw.Write(G);
                        bw.Write(B);
                        bw.Write(A);
                    }
                },
                {
                    CmdID.G_SETPRIMCOLOR,
                    (info, bw) =>
                    {
                        byte minlevel = (byte)info.Args["minlevel"];
                        byte lodfrac = (byte)info.Args["lodfrac"];
                        byte R = (byte)info.Args["R"];
                        byte G = (byte)info.Args["G"];
                        byte B = (byte)info.Args["B"];
                        byte A = (byte)info.Args["A"];

                        bw.Write((byte)info.ID);
                        bw.Write((byte)0);
                        bw.Write(minlevel);
                        bw.Write(lodfrac);
                        bw.Write(R);
                        bw.Write(G);
                        bw.Write(B);
                        bw.Write(A);
                    }
                },
                {
                    CmdID.G_SETENVCOLOR,
                    (info, bw) =>
                    {
                        byte R = (byte)info.Args["R"];
                        byte G = (byte)info.Args["G"];
                        byte B = (byte)info.Args["B"];
                        byte A = (byte)info.Args["A"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(R);
                        bw.Write(G);
                        bw.Write(B);
                        bw.Write(A);
                    }
                },
                {
                    CmdID.G_SETCOMBINE,
                    (info, bw) =>
                    {
                        int a0 = (int)info.Args["a0"];
                        int c0 = (int)info.Args["c0"];
                        int Aa0 = (int)info.Args["Aa0"];
                        int Ac0 = (int)info.Args["Ac0"];
                        int a1 = (int)info.Args["a1"];
                        int c1 = (int)info.Args["c1"];
                        int b0 = (int)info.Args["b0"];
                        int b1 = (int)info.Args["b1"];
                        int Aa1 = (int)info.Args["Aa1"];
                        int Ac1 = (int)info.Args["Ac1"];
                        int d0 = (int)info.Args["d0"];
                        int Ab0 = (int)info.Args["Ab0"];
                        int Ad0 = (int)info.Args["Ad0"];
                        int d1 = (int)info.Args["d1"];
                        int Ab1 = (int)info.Args["Ab1"];
                        int Ad1 = (int)info.Args["Ad1"];

                        bw.Write((byte)info.ID);
                        bw.Write(a0, 4);
                        bw.Write(c0, 5);
                        bw.Write(Aa0, 3);
                        bw.Write(Ac0, 3);
                        bw.Write(a1, 4);
                        bw.Write(c1, 5);
                        bw.Write(b0, 4);
                        bw.Write(b1, 4);
                        bw.Write(Aa1, 3);
                        bw.Write(Ac1, 3);
                        bw.Write(d0, 3);
                        bw.Write(Ab0, 3);
                        bw.Write(Ad0, 3);
                        bw.Write(d1, 3);
                        bw.Write(Ab1, 3);
                        bw.Write(Ad1, 3);
                    }
                },
                {
                    CmdID.G_SETTIMG,
                    (info, bw) =>
                    {
                        var fmt = (G_IM_FMT)info.Args["fmt"];
                        var siz = (G_IM_FMT)info.Args["siz"];
                        int width = (int)info.Args["width"];
                        uint imgaddr = (uint)info.Args["imgaddr"];

                        bw.Write((byte)info.ID);
                        bw.Write((int)fmt, 3);
                        bw.Write((int)fmt, 2);
                        bw.Write(0, 12);
                        bw.Write(width - 1, 12);
                        bw.Write(imgaddr);
                    }
                },
                {
                    CmdID.G_SETZIMG,
                    (info, bw) =>
                    {
                        uint imgaddr = (uint)info.Args["imgaddr"];

                        bw.Write((byte)info.ID);
                        bw.Write(0, 24);
                        bw.Write(imgaddr);
                    }
                },
                {
                    CmdID.G_SETCIMG,
                    (info, bw) =>
                    {
                        var fmt = (G_IM_FMT)info.Args["fmt"];
                        var siz = (G_IM_FMT)info.Args["siz"];
                        int width = (int)info.Args["width"];
                        uint imgaddr = (uint)info.Args["imgaddr"];

                        bw.Write((byte)info.ID);
                        bw.Write((int)fmt, 3);
                        bw.Write((int)fmt, 2);
                        bw.Write(0, 12);
                        bw.Write(width - 1, 12);
                        bw.Write(imgaddr);
                    }
                },
            };
    }
}
