using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using F3DZEX.Command;
using RDP;

#nullable enable

namespace F3DZEX
{
    public partial class Disassembler
    {
        public class Config
        {
            public bool ShowAddress { get; set; }
            public bool RelativeAddress { get; set; }
            public bool DisasMultiCmdMacro { get; set; }
            public bool AddressLiteral { get; set; }
            public bool Static { get; set; }
        }

        public static Config StaticConfig { get; private set; } =
            new Config()
            {
                ShowAddress = true,
                RelativeAddress = false,
                DisasMultiCmdMacro = true,
                AddressLiteral = false,
                Static = true,
            };

        uint _wordLo;
        uint _wordHi;

        Config _cfg;
        Dlist _dlist = new Dlist();

        public Disassembler(Dlist dlist, Config? cfg = null)
        {
            _cfg = cfg ?? StaticConfig;
            _dlist = dlist ?? new Dlist();
        }

        public List<string> Disassemble()
        {
            List<string> lines = new List<string>();
            _wordLo = _wordHi = 0;

            int off = 0;
            int i = 0;
            int toSkip = 0;
            foreach (var cmd in _dlist)
            {
                if (toSkip > 0)
                {
                    toSkip--;
                }
                else
                {
                    uint addr = (_cfg.RelativeAddress) ? (uint)off : cmd.addr;
                    string prefix = (_cfg.ShowAddress ? $"{addr:X8}: " : "");

                    for (int j = 0; j < cmd.depth; j++)
                        prefix += "  ";

                    var (dis, comments) = DisassembleInstruction(cmd.cmd);
                    if (_cfg.DisasMultiCmdMacro)
                    {
                        string? macroDis = FindMultiCmdMacro(i, out int cmdCount);
                        if (macroDis != null)
                        {
                            dis = macroDis;
                            Debug.Assert(cmdCount > 0);
                            toSkip = cmdCount - 1;
                            comments = new List<string>()
                            {
                                $"Multi Command Macro Found ({cmdCount} instructions)",
                            };
                        }
                    }

                    if (!_cfg.Static)
                    {
                        dis = dis.Remove(1, 1);
                        dis = dis.Insert(dis.IndexOf('(') + 1, "gfx++, ");
                        dis = dis.Replace("(gfx++, )", "(gfx++)");
                    }
                    dis += _cfg.Static ? "," : ";";

                    if (comments != null)
                        foreach (var str in comments)
                            lines.Add($"// {str}");

                    lines.Add(prefix + dis);
                }

                i++;
                off += cmd.cmd.GetSize();
            }
            return lines;
        }

        private string DisAddress(object addr) =>
            _cfg.AddressLiteral ? $"0x{addr:X8}" : $"D_{addr:X8}";

        private (string, List<string>?) DisassembleInstruction(CmdInfo info)
        {
            switch (info.ID)
            {
                case CmdID.G_NOOP:
                    return ($"gsDPNoOpTag(0x{(uint)info.Args["tag"]:X})", null);
                case CmdID.G_VTX:
                    return (
                        $"gsSPVertex({DisAddress(info.Args["vaddr"])}, {info.Args["numv"]}, {info.Args["vbidx"]})",
                        null
                    );
                case CmdID.G_MODIFYVTX:
                    return (
                        $"gsSPModifyVertex({info.Args["where"]}, {info.Args["vbidx"]}, {info.Args["val"]})",
                        null
                    );
                case CmdID.G_CULLDL:
                    return (
                        $"gsSPCullDisplayList({info.Args["vfirst"]}, {info.Args["vlast"]})",
                        null
                    );
                case CmdID.G_BRANCH_Z:
                    return (
                        $"gsSPBranchLessZraw({DisAddress(_wordHi)}, {info.Args["vbidx"]}, 0x{info.Args["zval"]:X})",
                        null
                    );
                case CmdID.G_TRI1:
                    return (
                        $"gsSP1Triangle({info.Args["v0"]}, {info.Args["v1"]}, {info.Args["v2"]}, 0)",
                        null
                    );
                case CmdID.G_TRI2:
                    return (
                        $"gsSP2Triangles({info.Args["v00"]}, {info.Args["v01"]}, {info.Args["v02"]}, 0, {info.Args["v10"]}, {info.Args["v11"]}, {info.Args["v12"]}, 0)",
                        null
                    );
                case CmdID.G_QUAD:
                    return (
                        $"gsSPQuadrangle({info.Args["v0"]}, {info.Args["v1"]}, {info.Args["v2"]}, {info.Args["v3"]}, 0)",
                        null
                    );
                case CmdID.G_DMA_IO:
                    return (
                        $"gsSPDma_io(0x{info.Args["flag"]:X}, 0x{info.Args["dmem"]:X}, 0x{info.Args["dram"]:X}, 0x{info.Args["size"]:X})",
                        null
                    );
                case CmdID.G_TEXTURE:
                    return (
                        $"gsSPTexture(0x{info.Args["scaleS"]:X}, 0x{info.Args["scaleT"]:X}, {info.Args["level"]}, {(G_TX_TILE)info.Args["tile"]}, {info.Args["on"]})",
                        null
                    );
                case CmdID.G_POPMTX:
                    return ($"gsSPPopMatrixN(G_MTX_MODELVIEW, {info.Args["num"]})", null);
                case CmdID.G_GEOMETRYMODE:
                {
                    int clearbits = (int)info.Args["clearbits"];
                    int setbits = (int)info.Args["setbits"];

                    if (clearbits == 0)
                    {
                        var flag = new BitFlag<G_GEO_MODE>((G_GEO_MODE)setbits);
                        return ($"gsSPLoadGeometryMode({flag})", null);
                    }
                    else if (setbits == 0)
                    {
                        var flag = new BitFlag<G_GEO_MODE>((G_GEO_MODE)~clearbits);
                        return ($"gsSPClearGeometryMode({flag})", null);
                    }
                    else if (clearbits == 0xFFFFFF)
                    {
                        var flag = new BitFlag<G_GEO_MODE>((G_GEO_MODE)setbits);
                        return ($"gsSPSetGeometryMode({flag})", null);
                    }
                    else
                    {
                        var clearFlag = new BitFlag<G_GEO_MODE>((G_GEO_MODE)~clearbits);
                        var setFlag = new BitFlag<G_GEO_MODE>((G_GEO_MODE)setbits);
                        return ($"gsSPGeometryMode({clearFlag}, {setFlag})", null);
                    }
                }
                case CmdID.G_MTX:
                    return (
                        $"gsSPMatrix({DisAddress(info.Args["mtxaddr"])}, {DisMtxParams((int)info.Args["param"])})",
                        null
                    );
                case CmdID.G_MOVEWORD:
                    break;
                case CmdID.G_MOVEMEM:
                    break;
                case CmdID.G_LOAD_UCODE:
                    return (
                        $"gsSPLoadUcodeEx({DisAddress(info.Args["tstart"])}, {DisAddress(_wordHi)}, 0x{info.Args["dsize"]:X})",
                        null
                    );
                case CmdID.G_DL:
                {
                    var branch = info.GetArg<bool>("branch");
                    return (
                        (
                            branch
                                ? $"gsSPBranchList({DisAddress(info.Args["dl"])})"
                                : $"gsSPDisplayList({DisAddress(info.Args["dl"])})"
                        ),
                        null
                    );
                }
                case CmdID.G_ENDDL:
                    return ($"gsSPEndDisplayList()", null);
                case CmdID.G_SPNOOP:
                    return ($"gsSPNoOp()", null);
                case CmdID.G_RDPHALF_1:
                {
                    _wordHi = (uint)info.Args["word"];
                    break;
                }
                case CmdID.G_SETOTHERMODE_L:
                {
                    var cmd = info.Convert<GSetOtherMode>();

                    var macro = OtherModeMacro.MacrosL.Find(m => m.Match(cmd));
                    if (macro != null)
                    {
                        var value = macro.values.Find(v => (uint)v.Item2 == cmd.data);
                        return (
                            $"gsDP{macro.Name}({value?.Item1 ?? ("0x" + cmd.data.ToString("X"))})",
                            null
                        );
                    }

                    if (cmd.shift == (int)G_MDSFT_L.G_MDSFT_RENDERMODE && cmd.len == 29)
                    {
                        return OtherModeMacro.DisasRenderMode(cmd);
                    }

                    return (
                        $"gsSPSetOtherMode(G_SETOTHERMODE_L, {(G_MDSFT_L)cmd.shift}, {cmd.len}, 0x{cmd.data:X})",
                        null
                    );
                }
                case CmdID.G_SETOTHERMODE_H:
                {
                    var cmd = info.Convert<GSetOtherMode>();

                    var macro = OtherModeMacro.MacrosH.Find(m => m.Match(cmd));
                    if (macro != null)
                    {
                        var value = macro.values.Find(v => (uint)v.Item2 == cmd.data);
                        return (
                            $"gsDP{macro.Name}({value?.Item1 ?? ("0x" + cmd.data.ToString("X"))})",
                            null
                        );
                    }
                    return (
                        $"gsSPSetOtherMode(G_SETOTHERMODE_H, {(G_MDSFT_H)cmd.shift}, {cmd.len}, 0x{cmd.data:X})",
                        null
                    );
                }
                case CmdID.G_TEXRECT:
                {
                    var cmd = info.Convert<GTexRect>();
                    return (
                        $"gsSPTextureRectangle({cmd.ulx}, {cmd.uly}, {cmd.lrx}, {cmd.lry}, {cmd.tile}, {cmd.uls}, {cmd.ult}, {cmd.dsdx}, {cmd.dtdy})",
                        null
                    );
                }
                case CmdID.G_TEXRECTFLIP:
                {
                    var cmd = info.Convert<GTexRect>();
                    return (
                        $"gsSPTextureRectangleFlip({cmd.ulx}, {cmd.uly}, {cmd.lrx}, {cmd.lry}, {cmd.tile}, {cmd.uls}, {cmd.ult}, {cmd.dsdx}, {cmd.dtdy})",
                        null
                    );
                }
                case CmdID.G_RDPLOADSYNC:
                    return ("gsDPLoadSync()", null);
                case CmdID.G_RDPPIPESYNC:
                    return ("gsDPPipeSync()", null);
                case CmdID.G_RDPTILESYNC:
                    return ("gsDPTileSync()", null);
                case CmdID.G_RDPFULLSYNC:
                    return ("gsDPFullSync()", null);
                case CmdID.G_SETKEYGB:
                {
                    var cmd = info.Convert<GSetKeyGB>();
                    return (
                        $"gsDPSetKeyGB({cmd.centerG}, {cmd.scaleG}, {cmd.widthG}, {cmd.centerB}, {cmd.scaleB}, {cmd.widthB})",
                        null
                    );
                }
                case CmdID.G_SETKEYR:
                {
                    var cmd = info.Convert<GSetKeyR>();
                    return ($"gsDPSetKeyR({cmd.centerR}, {cmd.widthR}, {cmd.scaleR})", null);
                }
                case CmdID.G_SETCONVERT:
                    return (
                        $"gsDPSetConvert({info.Args["k0"]}, {info.Args["k1"]}, {info.Args["k2"]}, {info.Args["k3"]}, {info.Args["k4"]}, {info.Args["k5"]})",
                        null
                    );
                case CmdID.G_SETSCISSOR:
                {
                    var cmd = info.Convert<GSetScissor>();
                    if (
                        cmd.lrx.FracPart() == 0
                        && cmd.lry.FracPart() == 0
                        && cmd.ulx.FracPart() == 0
                        && cmd.uly.FracPart() == 0
                    )
                        return (
                            $"gsDPSetScissor({cmd.mode}, {cmd.ulx.IntPart()}, {cmd.uly.IntPart()}, {cmd.lrx.IntPart()}, {cmd.uly.IntPart()})",
                            null
                        );
                    else
                        return (
                            $"gsDPSetScissorFrac({cmd.mode}, {cmd.ulx}, {cmd.uly}, {cmd.lrx}, {cmd.lry})",
                            null
                        );
                }
                case CmdID.G_SETPRIMDEPTH:
                    return ($"gsDPSetPrimDepth({info.Args["z"]}, {info.Args["dz"]})", null);
                case CmdID.G_RDPSETOTHERMODE:
                    return (
                        $"gsDPSetOtherMode(0x{info.Args["omodeH"]:X}, 0x{info.Args["omodeL"]:X})",
                        null
                    );
                case CmdID.G_LOADTLUT:
                    return ($"gsDPLoadTLUTCmd({info.Args["tile"]}, {info.Args["count"]})", null);
                case CmdID.G_RDPHALF_2:
                {
                    _wordLo = (uint)info.Args["word"];
                    break;
                }
                case CmdID.G_SETTILESIZE:
                {
                    var cmd = info.Convert<GLoadTile>();
                    return (
                        $"gsDPSetTileSize({cmd.tile}, {cmd.uls}, {cmd.ult}, {cmd.lrs}, {cmd.lrt})",
                        null
                    );
                }
                case CmdID.G_LOADBLOCK:
                {
                    var cmd = info.Convert<GLoadBlock>();
                    return (
                        $"gsDPLoadBlock({cmd.tile}, {cmd.uls}, {cmd.ult}, {cmd.texels}, {cmd.dxt})",
                        null
                    );
                }
                case CmdID.G_LOADTILE:
                {
                    var cmd = info.Convert<GLoadTile>();
                    return (
                        $"gsDPLoadTile({cmd.tile}, {cmd.uls}, {cmd.ult}, {cmd.lrs}, {cmd.lrt})",
                        null
                    );
                }
                case CmdID.G_SETTILE:
                {
                    var cmt = DisTexWrap((int)info.Args["cmT"]);
                    var cmS = DisTexWrap((int)info.Args["cmS"]);
                    return (
                        $"gsDPSetTile({info.Args["fmt"]}, {info.Args["siz"]}, {info.Args["line"]}, 0x{info.Args["tmem"]:X}, {(G_TX_TILE)info.Args["tile"]}, {info.Args["palette"]}, {cmt}, {info.Args["maskT"]}, {info.Args["shiftT"]}, {cmS}, {info.Args["maskS"]}, {info.Args["shiftS"]})",
                        null
                    );
                }
                case CmdID.G_FILLRECT:
                {
                    var cmd = info.Convert<GFillRect>();
                    return ($"gsDPFillRectangle({cmd.ulx}, {cmd.uly}, {cmd.lrx}, {cmd.lry})", null);
                }
                case CmdID.G_SETFILLCOLOR:
                    return ($"gsDPSetFillColor(0x{info.Args["color"]:X8})", null);
                case CmdID.G_SETFOGCOLOR:
                    return (
                        $"gsDPSetFogColor({info.Args["R"]}, {info.Args["G"]}, {info.Args["B"]}, {info.Args["A"]})",
                        null
                    );
                case CmdID.G_SETBLENDCOLOR:
                    return (
                        $"gsDPBlendColor({info.Args["R"]}, {info.Args["G"]}, {info.Args["B"]}, {info.Args["A"]})",
                        null
                    );
                case CmdID.G_SETPRIMCOLOR:
                    return (
                        $"gsDPSetPrimColor(0x{info.Args["minlevel"]:X2}, 0x{info.Args["lodfrac"]:X2}, {info.Args["R"]}, {info.Args["G"]}, {info.Args["B"]}, {info.Args["A"]})",
                        null
                    );
                case CmdID.G_SETENVCOLOR:
                    return (
                        $"gsDPSetEnvColor({info.Args["R"]}, {info.Args["G"]}, {info.Args["B"]}, {info.Args["A"]})",
                        null
                    );
                case CmdID.G_SETCOMBINE:
                {
                    var cmd = info.Convert<GSetCombine>();

                    string a0 = DisCCM(cmd.a0, 1);
                    string b0 = DisCCM(cmd.b0, 2);
                    string c0 = DisCCM(cmd.c0, 3);
                    string d0 = DisCCM(cmd.d0, 4);

                    string Aa0 = DisACM(cmd.Aa0, 1);
                    string Ab0 = DisACM(cmd.Ab0, 2);
                    string Ac0 = DisACM(cmd.Ac0, 3);
                    string Ad0 = DisACM(cmd.Ad0, 4);

                    string a1 = DisCCM(cmd.a1, 1);
                    string b1 = DisCCM(cmd.b1, 2);
                    string c1 = DisCCM(cmd.c1, 3);
                    string d1 = DisCCM(cmd.d1, 4);

                    string Aa1 = DisACM(cmd.Aa1, 1);
                    string Ab1 = DisACM(cmd.Ab1, 2);
                    string Ac1 = DisACM(cmd.Ac1, 3);
                    string Ad1 = DisACM(cmd.Ad1, 4);

                    // (a - b) * c + d
                    Func<string, string, string, string, string> formatExpr = (a, b, c, d) =>
                    {
                        if (c == "0")
                            return d;

                        StringWriter sw = new StringWriter();

                        sw.Write(b == "0" ? a : $"({a} - {b})");
                        if (c != "1")
                            sw.Write($" * {c}");

                        if (d != "0")
                            sw.Write($" + {d}");

                        return sw.ToString();
                    };

                    string ccm0 = formatExpr(a0, b0, c0, d0);
                    string acm0 = formatExpr(Aa0, Ab0, Ac0, Ad0);
                    int max0 = Math.Max(ccm0.Length, acm0.Length);

                    string ccm1 = formatExpr(a1, b1, c1, d1);
                    string acm1 = formatExpr(Aa1, Ab1, Ac1, Ad1);
                    int max1 = Math.Max(ccm1.Length, acm1.Length);

                    List<string> comments = new List<string>()
                    {
                        $"CCM: {ccm0.PadRight(max0)}   ->   {ccm1.PadRight(max1)}",
                        $"ACM: {acm0.PadRight(max0)}   ->   {acm1.PadRight(max1)}",
                    };

                    var mode0 = CCMode.Modes.Find(m => m.Match(cmd, 0));
                    var mode1 = CCMode.Modes.Find(m => m.Match(cmd, 1));
                    if (mode0 != null && mode1 != null)
                        return ($"gsDPSetCombineMode({mode0.Name}, {mode1.Name})", comments);

                    return (
                        $"gsDPSetCombineLERP({a0}, {b0}, {c0}, {d0}, {Aa0}, {Ab0}, {Ac0}, {Ad0}, {a1}, {b1}, {c1}, {d1}, {Aa1}, {Ab1}, {Ac1}, {Ad1})",
                        comments
                    );
                }
                case CmdID.G_SETTIMG:
                    return (
                        $"gsDPSetTextureImage({info.Args["fmt"]}, {info.Args["siz"]}, {info.Args["width"]}, {DisAddress(info.Args["imgaddr"])})",
                        null
                    );
                case CmdID.G_SETZIMG:
                    return ($"gsDPSetDepthImage({DisAddress(info.Args["imgaddr"])})", null);
                case CmdID.G_SETCIMG:
                    return (
                        $"gsDPSetColorImage({info.Args["fmt"]}, {info.Args["siz"]}, {info.Args["width"]}, {DisAddress(info.Args["imgaddr"])})",
                        null
                    );
                default:
                    break;
            }

            return ($"Unsupported Instruction {info.ID}", null);
        }
    }
}
