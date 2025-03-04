using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using F3DZEX.Command;

namespace F3DZEX
{
    public partial class Disassembler
    {
        private static readonly List<MultiMacro> MultiMacros = new List<MultiMacro>()
        {
            new LoadTLutMacro(),
            new LoadTextureBlockMacro(),
        };

        private string? FindMultiCmdMacro(int index, out int cmdCount)
        {
            foreach (var entry in MultiMacros)
            {
                if (
                    entry.IsCandidate(this, index)
                    && entry.Disassemble(this, index, out string? res)
                )
                {
                    cmdCount = entry.GetCommandCount();
                    return res;
                }
            }
            cmdCount = 0;
            return null;
        }

        abstract class MultiMacro
        {
            CmdID[] _ids;

            protected T GetCmd<T>(Disassembler dis, int idx) =>
                dis._dlist.AtIndex(idx).cmd.Convert<T>();

            protected MultiMacro(params CmdID[] ids)
            {
                _ids = ids;
            }

            public bool IsCandidate(Disassembler dis, int idx)
            {
                for (int i = 0; i < _ids.Length; i++)
                {
                    if (dis._dlist.AtIndex(idx + i).cmd.ID != _ids[i])
                        return false;
                }
                return true;
            }

            public int GetCommandCount() => _ids.Length;

            public abstract bool Disassemble(
                Disassembler dis,
                int idx,
                [NotNullWhen(true)] out string? output
            );
        }

        class LoadTLutMacro : MultiMacro
        {
            public LoadTLutMacro()
                : base(
                    CmdID.G_SETTIMG,
                    CmdID.G_RDPTILESYNC,
                    CmdID.G_SETTILE,
                    CmdID.G_RDPLOADSYNC,
                    CmdID.G_LOADTLUT,
                    CmdID.G_RDPPIPESYNC
                ) { }

            public override bool Disassemble(
                Disassembler dis,
                int idx,
                [NotNullWhen(true)] out string? output
            )
            {
                output = null;

                var setTimg = GetCmd<GSetTImg>(dis, idx++);
                if (setTimg.fmt != G_IM_FMT.G_IM_FMT_RGBA || setTimg.width != 1)
                    return false;

                idx++; // G_RDPTILESYNC

                var setTile = GetCmd<GSetTile>(dis, idx++);

                idx++; // G_RDPLOADSYNC

                var loadTlut = GetCmd<GLoadTlut>(dis, idx++);

                idx++; // G_RDPPIPESYNC

                output =
                    $"gsDPLoadTLUT({loadTlut.count + 1}, 0x{setTile.tmem:X}, {dis.DisAddress(setTimg.imgaddr)})";
                return true;
            }
        }

        class LoadTextureBlockMacro : MultiMacro
        {
            const int G_TX_DXT_FRAC = 11;

            public LoadTextureBlockMacro()
                : base(
                    CmdID.G_SETTIMG,
                    CmdID.G_SETTILE,
                    CmdID.G_RDPLOADSYNC,
                    CmdID.G_LOADBLOCK,
                    CmdID.G_RDPPIPESYNC,
                    CmdID.G_SETTILE,
                    CmdID.G_SETTILESIZE
                ) { }

            private G_IM_SIZ SizLoadBlock(G_IM_SIZ siz)
            {
                switch (siz)
                {
                    case G_IM_SIZ.G_IM_SIZ_4b:
                    case G_IM_SIZ.G_IM_SIZ_8b:
                    case G_IM_SIZ.G_IM_SIZ_16b:
                        return G_IM_SIZ.G_IM_SIZ_16b;
                    case G_IM_SIZ.G_IM_SIZ_32b:
                        return G_IM_SIZ.G_IM_SIZ_32b;
                    default:
                        throw new ArgumentException();
                }
            }

            private int SizBytes(G_IM_SIZ siz)
            {
                switch (siz)
                {
                    case G_IM_SIZ.G_IM_SIZ_4b:
                        return 0;
                    case G_IM_SIZ.G_IM_SIZ_8b:
                        return 1;
                    case G_IM_SIZ.G_IM_SIZ_16b:
                        return 2;
                    case G_IM_SIZ.G_IM_SIZ_32b:
                        return 4;
                    default:
                        throw new ArgumentException();
                }
            }

            private int SizTileBytes(G_IM_SIZ siz)
            {
                switch (siz)
                {
                    case G_IM_SIZ.G_IM_SIZ_4b:
                        return 0;
                    case G_IM_SIZ.G_IM_SIZ_8b:
                        return 1;
                    case G_IM_SIZ.G_IM_SIZ_16b:
                        return 2;
                    case G_IM_SIZ.G_IM_SIZ_32b:
                        return 2;
                    default:
                        throw new ArgumentException();
                }
            }

            private int SizLineBytes(G_IM_SIZ siz)
            {
                return SizTileBytes(siz);
            }

            private int SizShift(G_IM_SIZ siz)
            {
                switch (siz)
                {
                    case G_IM_SIZ.G_IM_SIZ_4b:
                        return 2;
                    case G_IM_SIZ.G_IM_SIZ_8b:
                        return 1;
                    case G_IM_SIZ.G_IM_SIZ_16b:
                        return 0;
                    case G_IM_SIZ.G_IM_SIZ_32b:
                        return 0;
                    default:
                        throw new ArgumentException();
                }
            }

            private int SizIncr(G_IM_SIZ siz)
            {
                switch (siz)
                {
                    case G_IM_SIZ.G_IM_SIZ_4b:
                        return 3;
                    case G_IM_SIZ.G_IM_SIZ_8b:
                        return 1;
                    case G_IM_SIZ.G_IM_SIZ_16b:
                        return 0;
                    case G_IM_SIZ.G_IM_SIZ_32b:
                        return 0;
                    default:
                        throw new ArgumentException();
                }
            }

            private int Txl2Words(int txls, int b_txl)
            {
                return Math.Max(1, ((txls) * (b_txl) / 8));
            }

            private int CalcDxt(int width, int b_txl)
            {
                return (
                    ((1 << G_TX_DXT_FRAC) + Txl2Words(width, b_txl) - 1) / Txl2Words(width, b_txl)
                );
            }

            private int Txl2Words_4b(int txls)
            {
                return Math.Max(1, ((txls) / 16));
            }

            private int CalcDxt_4b(int width)
            {
                return (((1 << G_TX_DXT_FRAC) + Txl2Words_4b(width) - 1) / Txl2Words_4b(width));
            }

            public override bool Disassemble(
                Disassembler dis,
                int idx,
                [NotNullWhen(true)] out string? output
            )
            {
                output = null;

                var setTimg = GetCmd<GSetTImg>(dis, idx++);
                if (setTimg.width != 1)
                    return false;

                var setTile = GetCmd<GSetTile>(dis, idx++);

                idx++; // G_RDPLOADSYNC

                var loadBlock = GetCmd<GLoadBlock>(dis, idx++);

                idx++; // G_RDPPIPESYNC

                var setTile2 = GetCmd<GSetTile>(dis, idx++);

                var setTileSize = GetCmd<GLoadTile>(dis, idx++);

                var timg = setTimg.imgaddr;
                var fmt = setTile2.fmt;
                var siz = setTile2.siz;
                int width = (int)(setTileSize.lrs.IntPart() + 1);
                int height = (int)(setTileSize.lrt.IntPart() + 1);
                var pal = setTile2.palette;
                var cmt = setTile2.cmT;
                var maskt = setTile2.maskT;
                var shiftt = setTile2.shiftT;
                var cms = setTile2.cmS;
                var masks = setTile2.maskS;
                var shifts = setTile2.shiftS;
                int tmem = setTile2.tmem;
                G_TX_TILE rtile = setTile2.tile;

                if (
                    setTimg.fmt == fmt
                    && setTimg.siz == SizLoadBlock(siz)
                    && setTimg.width == 1
                    && setTile.fmt == fmt
                    && setTile.siz == SizLoadBlock(siz)
                    && setTile.line == 0
                    && setTile.tmem == tmem
                    && setTile.tile == G_TX_TILE.G_TX_LOADTILE
                    && setTile.palette == 0
                    && setTile.cmT == cmt
                    && setTile.maskT == maskt
                    && setTile.shiftT == shiftt
                    && setTile.cmS == cms
                    && setTile.maskS == masks
                    && setTile.shiftS == shifts
                    && loadBlock.tile == G_TX_TILE.G_TX_LOADTILE
                    && loadBlock.uls.Raw == 0
                    && loadBlock.ult.Raw == 0
                    && loadBlock.texels == ((width * height + SizIncr(siz)) >> SizShift(siz)) - 1
                    &&
                    //(loadBlock.dxt.Raw == 0 || loadBlock.dxt.Raw == CalcDxt(width, SizBytes(siz))) &&

                    //setTile2.line == ((width * SizLineBytes(siz)) + 7) >> 3 &&

                    setTileSize.uls.Raw == 0
                    && setTileSize.ult.Raw == 0
                )
                {
                    string s = loadBlock.dxt.Raw == 0 ? "S" : "";

                    if (siz == G_IM_SIZ.G_IM_SIZ_4b)
                    {
                        if (
                            (loadBlock.dxt.Raw == 0 || loadBlock.dxt.Raw == CalcDxt_4b(width))
                            && setTile2.line == ((width >> 1) + 7) >> 3
                        )
                        {
                            output =
                                (tmem == 0 && rtile == G_TX_TILE.G_TX_RENDERTILE)
                                    ? $"gsDPLoadTextureBlock_4b{s}({dis.DisAddress(timg)}, {fmt}, {width}, {height}, {pal}, {DisTexWrap(cms)}, {DisTexWrap(cmt)}, {masks}, {maskt}, {shifts}, {shiftt})"
                                    : $"gsDPLoadMultiBlock_4b{s}({dis.DisAddress(timg)}, 0x{tmem:X}, {rtile}, {fmt}, {width}, {height}, {pal}, {DisTexWrap(cms)}, {DisTexWrap(cmt)}, {masks}, {maskt}, {shifts}, {shiftt})";
                        }
                    }
                    else
                    {
                        if (
                            (
                                loadBlock.dxt.Raw == 0
                                || loadBlock.dxt.Raw == CalcDxt(width, SizBytes(siz))
                            )
                            && setTile2.line == ((width * SizLineBytes(siz)) + 7) >> 3
                        )
                        {
                            output =
                                (tmem == 0 && rtile == G_TX_TILE.G_TX_RENDERTILE)
                                    ? $"gsDPLoadTextureBlock{s}({dis.DisAddress(timg)}, {fmt}, {siz}, {width}, {height}, {pal}, {DisTexWrap(cms)}, {DisTexWrap(cmt)}, {masks}, {maskt}, {shifts}, {shiftt})"
                                    : $"gsDPLoadMultiBlock{s}({dis.DisAddress(timg)}, 0x{tmem:X}, {rtile}, {fmt}, {siz}, {width}, {height}, {pal}, {DisTexWrap(cms)}, {DisTexWrap(cmt)}, {masks}, {maskt}, {shifts}, {shiftt})";
                        }
                    }
                }

                if (output == null)
                    Debug.WriteLine("weird LoadTextureBlock detected");

                return output != null;
            }
        }
    }
}
