using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDP;

namespace F3DZEX.Command
{
    public enum CmdID
    {
        G_NOOP = 0x00,
        G_VTX = 0x01,
        G_MODIFYVTX = 0x02,
        G_CULLDL = 0x03,
        G_BRANCH_Z = 0x04,
        G_TRI1 = 0x05,
        G_TRI2 = 0x06,
        G_QUAD = 0x07,

        G_DMA_IO = 0xD6,
        G_TEXTURE = 0xD7,
        G_POPMTX = 0xD8,
        G_GEOMETRYMODE = 0xD9,
        G_MTX = 0xDA,
        G_MOVEWORD = 0xDB,
        G_MOVEMEM = 0xDC,
        G_LOAD_UCODE = 0xDD,
        G_DL = 0xDE,
        G_ENDDL = 0xDF,

        G_SPNOOP = 0xE0,
        G_RDPHALF_1 = 0xE1,
        G_SETOTHERMODE_L = 0xE2,
        G_SETOTHERMODE_H = 0xE3,
        G_TEXRECT = 0xE4,
        G_TEXRECTFLIP = 0xE5,
        G_RDPLOADSYNC = 0xE6,
        G_RDPPIPESYNC = 0xE7,
        G_RDPTILESYNC = 0xE8,
        G_RDPFULLSYNC = 0xE9,
        G_SETKEYGB = 0xEA,
        G_SETKEYR = 0xEB,
        G_SETCONVERT = 0xEC,
        G_SETSCISSOR = 0xED,
        G_SETPRIMDEPTH = 0xEE,
        G_RDPSETOTHERMODE = 0xEF,

        G_LOADTLUT = 0xF0,
        G_RDPHALF_2 = 0xF1,
        G_SETTILESIZE = 0xF2,
        G_LOADBLOCK = 0xF3,
        G_LOADTILE = 0xF4,
        G_SETTILE = 0xF5,
        G_FILLRECT = 0xF6,
        G_SETFILLCOLOR = 0xF7,
        G_SETFOGCOLOR = 0xF8,
        G_SETBLENDCOLOR = 0xF9,
        G_SETPRIMCOLOR = 0xFA,
        G_SETENVCOLOR = 0xFB,
        G_SETCOMBINE = 0xFC,
        G_SETTIMG = 0xFD,
        G_SETZIMG = 0xFE,
        G_SETCIMG = 0xFF,
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class CmdAttribute : Attribute
    {
        readonly CmdID id;

        public CmdAttribute(CmdID id)
        {
            this.id = id;
        }

        public CmdID ID
        {
            get => id;
        }
    }

    [Cmd(CmdID.G_TEXTURE)]
    public struct GTexture
    {
        public int level { get; set; }
        public G_TX_TILE tile { get; set; }
        public G_TEX_ENABLE on { get; set; }
        public ushort scaleS { get; set; }
        public ushort scaleT { get; set; }
    }

    [Cmd(CmdID.G_GEOMETRYMODE)]
    public struct GGeometryMode
    {
        public int clearbits { get; set; }
        public int setbits { get; set; }
    }

    [Cmd(CmdID.G_VTX)]
    public struct GVtx
    {
        public int numv { get; set; }
        public int vbidx { get; set; }
        public uint vaddr { get; set; }
    }

    [Cmd(CmdID.G_SETTIMG)]
    public struct GSetTImg
    {
        public G_IM_FMT fmt { get; set; }
        public G_IM_SIZ siz { get; set; }
        public int width { get; set; }
        public uint imgaddr { get; set; }
    }

    [Cmd(CmdID.G_SETTILESIZE)]
    [Cmd(CmdID.G_LOADTILE)]
    public struct GLoadTile
    {
        public FixedPoint uls { get; set; }
        public FixedPoint ult { get; set; }
        public G_TX_TILE tile { get; set; }
        public FixedPoint lrs { get; set; }
        public FixedPoint lrt { get; set; }
    }

    [Cmd(CmdID.G_LOADTLUT)]
    public struct GLoadTlut
    {
        public G_TX_TILE tile { get; set; }
        public int count { get; set; }
    }

    [Cmd(CmdID.G_SETTILE)]
    public struct GSetTile
    {
        public G_IM_FMT fmt { get; set; }
        public G_IM_SIZ siz { get; set; }
        public int line { get; set; }
        public int tmem { get; set; }
        public G_TX_TILE tile { get; set; }
        public int palette { get; set; }
        public G_TX_TEXWRAP cmT { get; set; }
        public int maskT { get; set; }
        public int shiftT { get; set; }
        public G_TX_TEXWRAP cmS { get; set; }
        public int maskS { get; set; }
        public int shiftS { get; set; }
    }

    [Cmd(CmdID.G_LOADBLOCK)]
    public struct GLoadBlock
    {
        public FixedPoint uls { get; set; }
        public FixedPoint ult { get; set; }
        public G_TX_TILE tile { get; set; }
        public int texels { get; set; }
        public FixedPoint dxt { get; set; }
    }

    [Cmd(CmdID.G_BRANCH_Z)]
    public struct GBranchZ
    {
        public int vbidx { get; set; }
        public uint zval { get; set; }
    }

    [Cmd(CmdID.G_RDPHALF_1)]
    [Cmd(CmdID.G_RDPHALF_2)]
    public struct GRdpHalf
    {
        public uint word { get; set; }
    }

    [Cmd(CmdID.G_DL)]
    public struct GDl
    {
        public bool branch { get; set; }
        public uint dl { get; set; }
    }

    [Cmd(CmdID.G_SETFOGCOLOR)]
    [Cmd(CmdID.G_SETBLENDCOLOR)]
    [Cmd(CmdID.G_SETENVCOLOR)]
    public struct GSetColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
    }

    [Cmd(CmdID.G_SETPRIMCOLOR)]
    public struct GSetPrimColor
    {
        public byte minlevel { get; set; }
        public byte lodfrac { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
    }

    [Cmd(CmdID.G_TRI1)]
    public struct GTri1
    {
        public byte v0 { get; set; }
        public byte v1 { get; set; }
        public byte v2 { get; set; }
    }

    [Cmd(CmdID.G_TRI2)]
    public struct GTri2
    {
        public byte v00 { get; set; }
        public byte v01 { get; set; }
        public byte v02 { get; set; }
        public byte v10 { get; set; }
        public byte v11 { get; set; }
        public byte v12 { get; set; }
    }

    [Cmd(CmdID.G_TEXRECT)]
    [Cmd(CmdID.G_TEXRECTFLIP)]
    public struct GTexRect
    {
        public FixedPoint lrx { get; set; }
        public FixedPoint lry { get; set; }
        public G_TX_TILE tile { get; set; }
        public FixedPoint ulx { get; set; }
        public FixedPoint uly { get; set; }
        public FixedPoint uls { get; set; }
        public FixedPoint ult { get; set; }
        public FixedPoint dsdx { get; set; }
        public FixedPoint dtdy { get; set; }
    }

    [Cmd(CmdID.G_SETKEYGB)]
    public struct GSetKeyGB
    {
        public FixedPoint widthG { get; set; }
        public FixedPoint widthB { get; set; }
        public byte centerG { get; set; }
        public byte scaleG { get; set; }
        public byte centerB { get; set; }
        public byte scaleB { get; set; }
    }

    [Cmd(CmdID.G_SETKEYR)]
    public struct GSetKeyR
    {
        public FixedPoint widthR { get; set; }
        public byte centerR { get; set; }
        public byte scaleR { get; set; }
    }

    [Cmd(CmdID.G_SETSCISSOR)]
    public struct GSetScissor
    {
        public FixedPoint ulx { get; set; }
        public FixedPoint uly { get; set; }
        public int mode { get; set; }
        public FixedPoint lrx { get; set; }
        public FixedPoint lry { get; set; }
    }

    [Cmd(CmdID.G_FILLRECT)]
    public struct GFillRect
    {
        public FixedPoint lrx { get; set; }
        public FixedPoint lry { get; set; }
        public FixedPoint ulx { get; set; }
        public FixedPoint uly { get; set; }
    }

    [Cmd(CmdID.G_SETCOMBINE)]
    public struct GSetCombine
    {
        public G_CCMUX a0 { get; set; }
        public G_CCMUX b0 { get; set; }
        public G_CCMUX c0 { get; set; }
        public G_CCMUX d0 { get; set; }
        public G_ACMUX Aa0 { get; set; }
        public G_ACMUX Ab0 { get; set; }
        public G_ACMUX Ac0 { get; set; }
        public G_ACMUX Ad0 { get; set; }
        public G_CCMUX a1 { get; set; }
        public G_CCMUX b1 { get; set; }
        public G_CCMUX c1 { get; set; }
        public G_CCMUX d1 { get; set; }
        public G_ACMUX Aa1 { get; set; }
        public G_ACMUX Ab1 { get; set; }
        public G_ACMUX Ac1 { get; set; }
        public G_ACMUX Ad1 { get; set; }
    }

    [Cmd(CmdID.G_SETOTHERMODE_L)]
    [Cmd(CmdID.G_SETOTHERMODE_H)]
    public struct GSetOtherMode
    {
        public int shift { get; set; }
        public int len { get; set; }
        public uint data { get; set; }
    }

    [Cmd(CmdID.G_POPMTX)]
    public struct GPopMtx
    {
        public uint num { get; set; }
    }

    [Cmd(CmdID.G_MTX)]
    public struct GMtx
    {
        public G_MTX_PARAM param { get; set; }
        public uint mtxaddr { get; set; }
    }

    public class CmdInfo
    {
        public CmdID ID { get; private set; }
        public Dictionary<string, object> Args { get; private set; }

        public CmdInfo(CmdID id, Dictionary<string, object> args)
        {
            ID = id;
            Args = args;
        }

        public int GetSize()
        {
            switch (ID)
            {
                case CmdID.G_NOOP:
                case CmdID.G_VTX:
                case CmdID.G_MODIFYVTX:
                case CmdID.G_CULLDL:
                case CmdID.G_BRANCH_Z:
                case CmdID.G_TRI1:
                case CmdID.G_TRI2:
                case CmdID.G_QUAD:
                case CmdID.G_DMA_IO:
                case CmdID.G_TEXTURE:
                case CmdID.G_POPMTX:
                case CmdID.G_GEOMETRYMODE:
                case CmdID.G_MTX:
                case CmdID.G_MOVEWORD:
                case CmdID.G_MOVEMEM:
                case CmdID.G_LOAD_UCODE:
                case CmdID.G_DL:
                case CmdID.G_ENDDL:
                case CmdID.G_SPNOOP:
                case CmdID.G_RDPHALF_1:
                case CmdID.G_SETOTHERMODE_L:
                case CmdID.G_SETOTHERMODE_H:
                case CmdID.G_RDPLOADSYNC:
                case CmdID.G_RDPPIPESYNC:
                case CmdID.G_RDPTILESYNC:
                case CmdID.G_RDPFULLSYNC:
                case CmdID.G_SETKEYGB:
                case CmdID.G_SETKEYR:
                case CmdID.G_SETCONVERT:
                case CmdID.G_SETSCISSOR:
                case CmdID.G_SETPRIMDEPTH:
                case CmdID.G_RDPSETOTHERMODE:
                case CmdID.G_LOADTLUT:
                case CmdID.G_RDPHALF_2:
                case CmdID.G_SETTILESIZE:
                case CmdID.G_LOADBLOCK:
                case CmdID.G_LOADTILE:
                case CmdID.G_SETTILE:
                case CmdID.G_FILLRECT:
                case CmdID.G_SETFILLCOLOR:
                case CmdID.G_SETFOGCOLOR:
                case CmdID.G_SETBLENDCOLOR:
                case CmdID.G_SETPRIMCOLOR:
                case CmdID.G_SETENVCOLOR:
                case CmdID.G_SETCOMBINE:
                case CmdID.G_SETTIMG:
                case CmdID.G_SETZIMG:
                case CmdID.G_SETCIMG:
                    return 0x8;
                case CmdID.G_TEXRECT:
                case CmdID.G_TEXRECTFLIP:
                    return 0x18;
                default:
                    return -1;
            }
        }

        public T GetArg<T>(string param) => (T)Args[param];

        public T Convert<T>() => (T)ConvertCommand(typeof(T), this);

        private static object ConvertCommand(Type t, CmdInfo cmd)
        {
            // TODO: check parent class

            var attr = (
                (CmdAttribute[])t.GetCustomAttributes(typeof(CmdAttribute), false)
            ).ToList();

            var match = attr.Find(a => a.ID == cmd.ID);
            if (match == null)
                throw new InvalidF3DZEXOpCodeException("Invalid ID");

            object? obj = Activator.CreateInstance(t);
            Debug.Assert(obj != null);

            foreach (var prop in t.GetProperties())
            {
                if (
                    !cmd.Args.ContainsKey(prop.Name)
                    || cmd.Args[prop.Name].GetType() != prop.PropertyType
                )
                    throw new Exception("???");

                prop.SetValue(obj, cmd.Args[prop.Name]);
            }
            return obj;
        }

        public static T DecodeCommand<T>(byte[] ucode, int off) =>
            (T)ConvertCommand(typeof(T), CmdEncoding.DecodeCmds(ucode, off).First());
    }
}
