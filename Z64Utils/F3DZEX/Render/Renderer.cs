using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using OpenTK;
using System.Drawing;
using N64;
using Syroot.BinaryData;
using Z64;
using RDP;
using System.Diagnostics;
using F3DZEX.Command;

namespace F3DZEX.Render
{
    public class Renderer
    {
        public class Config
        {
            public event EventHandler OnGridScaleChanged;

            private float _gridScale = 5000;


            public float GridScale {
                get => _gridScale;
                set {
                    _gridScale = value;
                    OnGridScaleChanged?.Invoke(this, new EventArgs());
                }
            }
            public bool ShowGrid { get; set; } = true;
            public bool ShowAxis { get; set; } = true;
            public bool ShowGLInfo { get; set; } = false;
            public RdpVertexDrawer.ModelRenderMode RenderMode { get; set; } = RdpVertexDrawer.ModelRenderMode.Textured;
            public bool EnabledLighting { get; set; } = true;
            public bool DrawNormals { get; set; } = false;
            public Color NormalColor { get; set; } = Color.Yellow;
            public Color HighlightColor { get; set; } = Color.Red;
            public Color WireframeColor { get; set; } = Color.Black;
            public Color BackColor { get; set; } = Color.DodgerBlue;
            public Color InitialPrimColor { get; set; } = Color.White;
            public Color InitialEnvColor { get; set; } = Color.White;
            public Color InitialFogColor { get; set; } = Color.Transparent;
            public Color InitialBlendColor { get; set; } = Color.White;
        }

        public class Tile
        {
            public bool on;
            public ushort scaleS;
            public ushort scaleT;
            public int level;

            public FixedPoint uls;
            public FixedPoint ult;
            public FixedPoint lrs;
            public FixedPoint lrt;

            public G_IM_FMT fmt;
            public G_IM_SIZ siz;
            public int line;
            public int tmem;
            public int palette;
            public G_TX_TEXWRAP cmT;
            public G_TX_TEXWRAP cmS;
            public int maskS;
            public int maskT;
            public int shiftS;
            public int shiftT;

            public void SetTile(GSetTile tile)
            {
                fmt = tile.fmt;
                siz = tile.siz;
                line = tile.line;
                tmem = tile.tmem;
                palette = tile.palette;
                cmT = tile.cmT;
                cmS = tile.cmS;
                maskS = tile.maskS;
                maskT = tile.maskT;
                shiftS = tile.shiftS;
                shiftT = tile.shiftT;
            }

            public void SetTileSize(GLoadTile tileSize)
            {
                uls = tileSize.uls;
                ult = tileSize.ult;
                lrs = tileSize.lrs;
                lrt = tileSize.lrt;
            }

            public void Texture(GTexture tex)
            {
                on = tex.on.HasFlag(G_TEX_ENABLE.G_ON);
                if (on)
                {
                    scaleS = tex.scaleS;
                    scaleT = tex.scaleT;
                    level = tex.level;
                }
            }

            public Tuple<int, int> GetTextureWrap()
            {
                var mirrorV = cmT.HasFlag(G_TX_TEXWRAP.G_TX_MIRROR);
                var mirrorH = cmS.HasFlag(G_TX_TEXWRAP.G_TX_MIRROR);

                var wrapS = cmS.HasFlag(G_TX_TEXWRAP.G_TX_CLAMP)
                    ? TextureWrapMode.ClampToEdge
                    : (mirrorH ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat);

                var wrapT = cmT.HasFlag(G_TX_TEXWRAP.G_TX_CLAMP)
                    ? TextureWrapMode.ClampToEdge
                    : (mirrorV ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat);

                wrapS = cmT.HasFlag(G_TX_TEXWRAP.G_TX_CLAMP) ? TextureWrapMode.ClampToEdge : TextureWrapMode.Repeat;
                wrapT = cmT.HasFlag(G_TX_TEXWRAP.G_TX_CLAMP) ? TextureWrapMode.ClampToEdge : TextureWrapMode.Repeat;
                //wrapS = (mirrorH ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat);
                //wrapT = (mirrorV ? TextureWrapMode.MirroredRepeat : TextureWrapMode.Repeat);

                return new Tuple<int, int>((int)wrapS, (int)wrapT);
            }

            public int WrapWidth => 1 << maskS;
            public int WrapHeight => 1 << maskT;
        }

        public class ColorCombiner
        {
            public struct ColorInfo
            {
                public G_CCMUX c1;
                public G_ACMUX a1;
                public G_CCMUX c2;
                public G_ACMUX a2;
            }

            public ColorInfo a;
            public ColorInfo b;
            public ColorInfo c;
            public ColorInfo d;

            public ColorInfo this[int idx]
            {
                get
                {
                    return idx switch
                    {
                        0 => a,
                        1 => b,
                        2 => c,
                        3 => d,
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                }
            }

            public void SetCombine(GSetCombine combine)
            {
                a.c1 = combine.a0;
                a.a1 = combine.Aa0;
                b.c1 = combine.b0;
                b.a1 = combine.Ab0;
                c.c1 = combine.c0;
                c.a1 = combine.Ac0;
                d.c1 = combine.d0;
                d.a1 = combine.Ad0;

                a.c2 = combine.a1;
                a.a2 = combine.Aa1;
                b.c2 = combine.b1;
                b.a2 = combine.Ab1;
                c.c2 = combine.c1;
                c.a2 = combine.Ac1;
                d.c2 = combine.d1;
                d.a2 = combine.Ad1;
            }

            public bool UsesTex0()
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this[i].a1 == G_ACMUX.G_ACMUX_TEXEL0 ||
                        this[i].a2 == G_ACMUX.G_ACMUX_TEXEL0 ||
                        this[i].c1 == G_CCMUX.G_CCMUX_TEXEL0 ||
                        this[i].c1 == G_CCMUX.G_CCMUX_TEXEL0)
                        return true;
                }

                return false;
            }

            public bool UsesTex1()
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this[i].a1 == G_ACMUX.G_ACMUX_TEXEL1 ||
                        this[i].a2 == G_ACMUX.G_ACMUX_TEXEL1 ||
                        this[i].c1 == G_CCMUX.G_CCMUX_TEXEL1 ||
                        this[i].c1 == G_CCMUX.G_CCMUX_TEXEL1)
                        return true;
                }
                return false;
            }
        }

        public class ChromaKey
        {
            public struct Comp
            {
                public byte center;
                public byte scale;
                public FixedPoint width;
            }

            public Comp r;
            public Comp g;
            public Comp b;

            public Vector3 Center => new Vector3(r.center, g.center, b.center);
            public Vector3 Scale => new Vector3(r.scale, g.scale, b.scale);

            public void SetKeyGB(GSetKeyGB cmd)
            {
                g.center = cmd.centerG;
                g.scale = cmd.scaleG;
                g.width = cmd.widthG;
                b.center = cmd.centerB;
                b.scale = cmd.scaleB;
                b.width = cmd.widthB;
            }
            public void SetKeyR(GSetKeyR cmd)
            {
                r.center = cmd.centerR;
                r.scale = cmd.scaleR;
                r.width = cmd.widthR;
            }
        }

        public uint RenderErrorAddr { get; private set; } = 0xFFFFFFFF;
        public string ErrorMsg { get; private set; } = null;
        public Config CurrentConfig { get; set; }
        public Memory Memory { get; private set; }

        // matrix that gets transforms the vertices loaded with G_VTX
        public MatrixStack RdpMtxStack { get; }
        public MatrixStack ModelMtxStack { get; }


        uint _curImgAddr;
        bool _reqDecodeTex = false;
        Tile[] _tiles = new Tile[8];
        byte[] _tmem = new byte[0x1000];
        int _tlutTmem;
        int _tlutSize;
        int _selectedTile = 0;
        ColorCombiner _combiner;
        uint _otherModeHI = 0;
        uint _otherModeLO = 0;
        uint _geoMode = (uint)(G_GEO_MODE.G_FOG | G_GEO_MODE.G_LIGHTING);
        ChromaKey _chromaKey;

        bool _initialized;
        RdpVertexDrawer _rdpVtxDrawer;
        SimpleVertexDrawer _gridDrawer;
        ColoredVertexDrawer _axisDrawer;
        TextDrawer _textDrawer;
        TextureHandler _tex0;
        TextureHandler _tex1;

        public bool RenderFailed() => ErrorMsg != null;

        public Renderer(Z64Game game, Config cfg, int depth = 10) : this(new Memory(game), cfg, depth)
        {

        }
        public Renderer(Memory mem, Config cfg, int depth = 10)
        {
            Memory = mem;
            CurrentConfig = cfg;

            RdpMtxStack = new MatrixStack();
            ModelMtxStack = new MatrixStack();

            ModelMtxStack.OnTopMatrixChanged += (sender, e) => _rdpVtxDrawer.SendModelMatrix(e.newTop);

            _tiles = new Tile[8];
            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i] = new Tile();
            }
            _combiner = new ColorCombiner();
            _chromaKey = new ChromaKey();
        }
        
        public void ClearErrors() => ErrorMsg = null;




        private void CheckGLErros()
        {
            var err = GL.GetError();
            if (err != ErrorCode.NoError)
                throw new Exception($"GL.GetError() -> {err}");
        }
        private void GLWrapper(Action callback)
        {
            callback();
            CheckGLErros();
        }

        public void SetHightlightEnabled(bool enabled)
        {
            _rdpVtxDrawer.SendHighlightEnabled(enabled);
        }

        private void Init()
        {
            /* Init Texture */
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            _tex0 = new TextureHandler();
            _tex1 = new TextureHandler();

            /* Init Drawers */
            _rdpVtxDrawer = new RdpVertexDrawer();
            _gridDrawer = new SimpleVertexDrawer();
            _axisDrawer = new ColoredVertexDrawer();
            _textDrawer = new TextDrawer();

            float[] vertices = RenderHelper.GenerateGridVertices(CurrentConfig.GridScale, 6, false);
            _gridDrawer.SetData(vertices, BufferUsageHint.StaticDraw);

            vertices = RenderHelper.GenerateAxisvertices(CurrentConfig.GridScale);
            _axisDrawer.SetData(vertices, BufferUsageHint.StaticDraw);

            _rdpVtxDrawer.SetData(new byte[32 * (Vertex.SIZE + 4*4*4)], BufferUsageHint.DynamicDraw);

            CurrentConfig.OnGridScaleChanged += (o, e) =>
            {
                float[] vertices = RenderHelper.GenerateGridVertices(CurrentConfig.GridScale, 6, false);
                _gridDrawer.SetSubData(vertices, 0);
            };

            CheckGLErros();
            _initialized = true;
        }

        public void RenderStart(Matrix4 proj, Matrix4 view)
        {
            if (!_initialized)
                Init();

            CheckGLErros();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(CurrentConfig.BackColor);
            CheckGLErros();

            if (RenderFailed())
                return;

            CheckGLErros();
            RdpMtxStack.Clear();
            ModelMtxStack.Clear();

            GL.ActiveTexture(TextureUnit.Texture0);
            CheckGLErros();
            _tex0.Use();

            GL.ActiveTexture(TextureUnit.Texture1);
            CheckGLErros();
            _tex1.Use();
            CheckGLErros();


            _gridDrawer.SendProjViewMatrices(ref proj, ref view);
            _axisDrawer.SendProjViewMatrices(ref proj, ref view);
            _rdpVtxDrawer.SendProjViewMatrices(ref proj, ref view);
            _rdpVtxDrawer.SendInitialColors(CurrentConfig);
            CheckGLErros();
            Matrix4 id = Matrix4.Identity;
            _textDrawer.SendProjViewMatrices(ref id, ref id);

            _rdpVtxDrawer.SendModelMatrix(ModelMtxStack.Top());
            _gridDrawer.SendModelMatrix(Matrix4.Identity);
            _axisDrawer.SendModelMatrix(Matrix4.Identity);
            CheckGLErros();

            _rdpVtxDrawer.SendHighlightColor(CurrentConfig.HighlightColor);
            _rdpVtxDrawer.SendHighlightEnabled(false);
            _rdpVtxDrawer.SetModelRenderMode(CurrentConfig.RenderMode);
            _rdpVtxDrawer.SendNormalColor(CurrentConfig.NormalColor);
            _rdpVtxDrawer.SendWireFrameColor(CurrentConfig.WireframeColor);
            _rdpVtxDrawer.SendLightingEnabled(CurrentConfig.EnabledLighting);
            CheckGLErros();

            GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Lequal);
            //GL.DepthMask(false);
            //glTexEnvi(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_BLEND)
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)EnableCap.Blend);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Texture2D);
            CheckGLErros();

            if (CurrentConfig.ShowGrid)
                RenderHelper.DrawGrid(_gridDrawer);

            if (CurrentConfig.ShowAxis)
                RenderHelper.DrawAxis(_axisDrawer);

            if (CurrentConfig.ShowGLInfo)
            {
                _textDrawer.DrawString(
                    //$"Extensions: {GL.GetString(StringName.Extensions)}\n" + 
                    $"Shading Language Version: {GL.GetString(StringName.ShadingLanguageVersion)}\n" +
                    $"Version: {GL.GetString(StringName.Version)}\n" +
                    $"Renderer: {GL.GetString(StringName.Renderer)}\n" +
                    $"Vendor: {GL.GetString(StringName.Vendor)}");
            }

            CheckGLErros();
        }

        public Dlist GetDlist(uint vaddr)
        {
            return new Dlist(Memory, vaddr);
        }

        public void RenderDList(Dlist dlist)
        {
            if (!_initialized)
                Init();

            if (RenderFailed())
                return;

            uint addr = 0xFFFFFFFF;
            try
            {
                foreach (var entry in dlist)
                {
                    
                    addr = entry.addr;
                    ProcessInstruction(entry.cmd);
                }
            }
            catch (Exception ex)
            {
                RenderErrorAddr = addr;
                ErrorMsg = ex.Message;
            }
        }



        static int TexDecodeCount = 0;

        private void DecodeTex(TextureHandler tex, Tile tile, byte[] tlut)
        {
            int w = tile.WrapWidth;
            int h = tile.WrapHeight;

            // copy from tmem
            int size = N64Texture.GetTexSize(w * h, tile.siz);
            byte[] data = new byte[size];
            System.Buffer.BlockCopy(_tmem, tile.tmem * 8, data, 0, size);

            // decode data
            byte[] dec = N64Texture.Decode(w * h, tile.fmt, tile.siz, data, tlut);
            tex.SetDataRGBA(dec, w, h);


            // texture wrap
            var wrap = tile.GetTextureWrap();
            tex.SetTextureWrap(wrap.Item1, wrap.Item2);
        }
        private void DecodeTexIfRequired()
        {
            if (_reqDecodeTex)
            {
                //Debug.WriteLine($"Decoding texture... {TexDecodeCount++}");

                var tile0 = _combiner.UsesTex0() ? _tiles[_selectedTile + 0] : null;
                var tile1 = _combiner.UsesTex1() ? _tiles[_selectedTile + 1] : null;


                byte[] tlut = null;
                if ((tile0 != null && tile0.fmt == G_IM_FMT.G_IM_FMT_CI) || (tile1 != null && tile1.fmt == G_IM_FMT.G_IM_FMT_CI))
                {
                    tlut = new byte[_tlutSize];
                    System.Buffer.BlockCopy(_tmem, _tlutTmem, tlut, 0, tlut.Length);
                }


                if (tile0 != null && tile0.on)
                    DecodeTex(_tex0, tile0, tlut);
                
                // todo: handle hilite correctly
                if (tile1 != null && tile1.on)
                    DecodeTex(_tex1, tile1, tlut);

                _reqDecodeTex = false;
                
                _rdpVtxDrawer.SendTile(0, _tiles[0]);
                _rdpVtxDrawer.SendTile(1, _tiles[1]);
            }
        }

        private unsafe void ProcessInstruction(CmdInfo info)
        {
            switch (info.ID)
            {
                case CmdID.G_SETOTHERMODE_L:
                case CmdID.G_SETOTHERMODE_H:
                    {
                        var cmd = info.Convert<GSetOtherMode>();

                        ref uint x = ref _otherModeLO;
                        if (info.ID == CmdID.G_SETOTHERMODE_H)
                            x = ref _otherModeHI;

                        x = x & ~((uint)((1 << cmd.len) - 1) << cmd.shift) | cmd.data;

                        _rdpVtxDrawer.SendOtherMode(info.ID, x);
                        break;
                    }
                case CmdID.G_GEOMETRYMODE:
                    {
                        var cmd = info.Convert<GGeometryMode>();

                        _geoMode = (_geoMode & (uint)cmd.clearbits) | (uint)cmd.setbits;
                        _rdpVtxDrawer.SendGeometryMode(_geoMode);
                        
                        break;
                    }

                case CmdID.G_SETKEYR:
                    {
                        var cmd = info.Convert<GSetKeyR>();
                        _chromaKey.SetKeyR(cmd);

                        _rdpVtxDrawer.SendChromaKey(_chromaKey);
                        break;
                    }
                case CmdID.G_SETKEYGB:
                    {
                        var cmd = info.Convert<GSetKeyGB>();
                        _chromaKey.SetKeyGB(cmd);

                        _rdpVtxDrawer.SendChromaKey(_chromaKey);
                        break;
                    }


                case CmdID.G_SETPRIMCOLOR:
                    {
                        var cmd = info.Convert<GSetPrimColor>();

                        _rdpVtxDrawer.SendPrimColor(cmd);
                        
                        break;
                    }
                case CmdID.G_SETBLENDCOLOR:
                case CmdID.G_SETENVCOLOR:
                case CmdID.G_SETFOGCOLOR:
                    {
                        var cmd = info.Convert<GSetColor>();
                        _rdpVtxDrawer.SendColor(info.ID, cmd);
                        break;
                    }
                case CmdID.G_SETCOMBINE:
                    {
                        var cmd = info.Convert<GSetCombine>();
                        
                        _combiner.SetCombine(cmd);
                        _rdpVtxDrawer.SendCombiner(_combiner);
                        break;
                    }




                case CmdID.G_VTX:
                    {
                        var cmd = info.Convert<GVtx>();

                        /* We have to send the rdp model matrix here */
                        Matrix4 curMtx = RdpMtxStack.Top();

                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryWriter bw = new BinaryWriter(ms);
                            for (int i = 0; i < cmd.numv; i++)
                            {
                                byte[] data = Memory.ReadBytes(cmd.vaddr + (uint)(Vertex.SIZE * i), Vertex.SIZE);
                                bw.Write(data);

                                // send the rdp top matrix
                                for (int y = 0; y < 4; y++)
                                    for (int x = 0; x < 4; x++)
                                        bw.Write(curMtx[y, x]);
                            }

                            _rdpVtxDrawer.SetSubData(ms.ToArray(), cmd.vbidx * (Vertex.SIZE + 4 * 4 * 4));
                        }
                    }
                    break;
                case CmdID.G_TRI1:
                    {
                        var cmd = info.Convert<GTri1>();
                        
                        if (CurrentConfig.RenderMode == RdpVertexDrawer.ModelRenderMode.Textured)
                        {
                            DecodeTexIfRequired();
                        }

                        byte[] indices = new byte[] { cmd.v0, cmd.v1, cmd.v2 };
                        _rdpVtxDrawer.Draw(PrimitiveType.Triangles, indices, CurrentConfig.DrawNormals);

                    }
                    break;
                case CmdID.G_TRI2:
                    {
                        var cmd = info.Convert<GTri2>();

                        if (CurrentConfig.RenderMode == RdpVertexDrawer.ModelRenderMode.Textured)
                        {
                            DecodeTexIfRequired();
                        }

                        byte[] indices = new byte[] { cmd.v00, cmd.v01, cmd.v02, cmd.v10, cmd.v11, cmd.v12 };
                        _rdpVtxDrawer.Draw(PrimitiveType.Triangles, indices, CurrentConfig.DrawNormals);


                    }
                    break;

                case CmdID.G_TEXTURE:
                    {
                        var cmd = info.Convert<GTexture>();

                        _selectedTile = (int)cmd.tile;

                        _tiles[(int)cmd.tile].Texture(cmd);
                        break;
                    }
                case CmdID.G_SETTILESIZE:
                    {
                        if (CurrentConfig.RenderMode != RdpVertexDrawer.ModelRenderMode.Textured)
                            return;

                        var cmd = info.Convert<GLoadTile>();

                        _tiles[(int)cmd.tile].SetTileSize(cmd);
                        _reqDecodeTex = true;
                    }
                    break;
                case CmdID.G_LOADBLOCK:
                    {
                        if (CurrentConfig.RenderMode != RdpVertexDrawer.ModelRenderMode.Textured)
                            return;

                        var cmd = info.Convert<GLoadBlock>();

                        if (cmd.uls.Float() != 0 || cmd.ult.Float() != 0)
                            throw new NotImplementedException();

                        _reqDecodeTex = true;

                        int texels = cmd.texels + 1;
                        var tile = _tiles[(int)cmd.tile];
                        int size = N64Texture.GetTexSize(texels, tile.siz);
                        byte[] data = Memory.ReadBytes(_curImgAddr, size);
                        System.Buffer.BlockCopy(data, 0, _tmem, tile.tmem * 8, data.Length);
                    }
                    break;
                case CmdID.G_LOADTLUT:
                    {
                        if (CurrentConfig.RenderMode != RdpVertexDrawer.ModelRenderMode.Textured)
                            return;

                        var cmd = info.Convert<GLoadTlut>();

                        var tile = _tiles[(int)cmd.tile];
                        _tlutSize = (cmd.count + 1) * 2;
                        _tlutTmem = tile.tmem * 8;
                        byte[] data = Memory.ReadBytes(_curImgAddr, _tlutSize);
                        System.Buffer.BlockCopy(data, 0, _tmem, _tlutTmem, data.Length);

                        _reqDecodeTex = true;
                    }
                    break;
                case CmdID.G_SETTIMG:
                    {
                        var cmd = info.Convert<GSetTImg>();
                        _curImgAddr = cmd.imgaddr;
                        _reqDecodeTex = true;
                    }
                    break;
                case CmdID.G_SETTILE:
                    {
                        if (CurrentConfig.RenderMode != RdpVertexDrawer.ModelRenderMode.Textured)
                            return;

                        var settile = info.Convert<GSetTile>();

                        _reqDecodeTex = true;

                        _tiles[(int)settile.tile].SetTile(settile);
                    }
                    break;


                /*
            case Command.OpCodeID.G_DL:
                {
                    var cmd = info.Convert<Command.GDl>();
                    BranchFrame(cmd.dl, !cmd.branch);
                    break;
                }
                */




                case CmdID.G_POPMTX:
                    {
                        var cmd = info.Convert<GPopMtx>();
                        for (uint i = 0; i < cmd.num; i++)
                            RdpMtxStack.Pop();

                        break;
                    }
                case CmdID.G_MTX:
                    {
                        var cmd = info.Convert<GMtx>();
                        var mtx = new Mtx(Memory.ReadBytes(cmd.mtxaddr, Mtx.SIZE));
                        var mtxf = mtx.ToMatrix4();

                        if (cmd.param.HasFlag(G_MTX_PARAM.G_MTX_PUSH))
                            RdpMtxStack.Push(mtxf);

                        // check G_MTX_MUL
                        if (!cmd.param.HasFlag(G_MTX_PARAM.G_MTX_LOAD))
                            //mtxf = curMtx * mtxf;
                            mtxf *= RdpMtxStack.Top();

                        RdpMtxStack.Load(mtxf);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
