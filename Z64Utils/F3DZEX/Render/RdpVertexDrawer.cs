using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using F3DZEX.Command;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace F3DZEX.Render
{
    public class RdpVertexDrawer
    {
        public enum ModelRenderMode
        {
            Wireframe,
            Textured,
            Surface,
            Normal,
        }

        private ModelRenderMode _mode;

        private ShaderHandler _nrmShader;
        private ShaderHandler _wireframeShader;
        private ShaderHandler _shader;
        private VertexAttribs _attrs;

        public RdpVertexDrawer()
        {
            _shader = ShaderHandler.FromSrcFilesInShadersDir("rdpVtx.vert", "rdpVtx.frag");
            _wireframeShader = ShaderHandler.FromSrcFilesInShadersDir(
                "rdpVtx.vert",
                "wireframe.frag",
                "wireframe.geom"
            );
            _nrmShader = ShaderHandler.FromSrcFilesInShadersDir(
                "rdpVtx.vert",
                "coloredVtx.frag",
                "rdpVtxNrm.geom"
            );
            _attrs = new VertexAttribs();
            // position
            //_attrs.LayoutAddFloat(3, VertexAttribPointerType.Short, false);
            _attrs.LayoutAddInt(3, VertexAttribIntegerType.UnsignedShort);
            //flag
            _attrs.LayoutAddInt(1, VertexAttribIntegerType.UnsignedShort);
            // tex coords
            _attrs.LayoutAddInt(2, VertexAttribIntegerType.Short);
            // color/normal
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Byte, true);
            // matrix
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Float, false);
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Float, false);
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Float, false);
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Float, false);
        }

        public void RecompileRdpShader(string vertSrc, string fragSrc)
        {
            _shader.RecompileShaders(vertSrc, fragSrc);
        }

        public void SetData(byte[] data, BufferUsageHint hint) => _attrs.SetData(data, true, hint);

        public void SetSubData(byte[] data, int off) => _attrs.SetSubData(data, off, true);

        #region uniform

        public void SendProjViewMatrices(ref Matrix4 proj, ref Matrix4 view)
        {
            _shader.Send("u_Projection", proj);
            _shader.Send("u_View", view);
            _nrmShader.Send("u_Projection", proj);
            _nrmShader.Send("u_View", view);
            _wireframeShader.Send("u_Projection", proj);
            _wireframeShader.Send("u_View", view);
        }

        public void SendModelMatrix(Matrix4 model)
        {
            _shader.Send("u_Model", model);
            _nrmShader.Send("u_Model", model);
            _wireframeShader.Send("u_Model", model);
        }

        public void SendTile(int idx, Render.Renderer.Tile tile)
        {
            _shader.Send($"u_Tiles[{idx}].tex", idx);
            _shader.Send($"u_Tiles[{idx}].cm", (int)tile.cmS, (int)tile.cmT);
            _shader.Send($"u_Tiles[{idx}].mask", tile.maskS, tile.maskT);
            _shader.Send($"u_Tiles[{idx}].shift", tile.shiftS, tile.shiftT);
            _shader.Send($"u_Tiles[{idx}].ul", tile.uls.Float(), tile.ult.Float());
            _shader.Send($"u_Tiles[{idx}].lr", tile.lrs.Float(), tile.lrt.Float());
        }

        public void SendHighlightEnabled(bool enabled)
        {
            _shader.Send("u_HighlightEnabled", enabled);
            _wireframeShader.Send("u_HighlightEnabled", enabled);
        }

        public void SendHighlightColor(Color color)
        {
            _shader.Send("u_HighlightColor", color);
            _wireframeShader.Send("u_HighlightColor", color);
        }

        public void SendPrimColor(GSetPrimColor cmd)
        {
            _shader.Send("u_RdpState.color.prim", Color.FromArgb(cmd.A, cmd.R, cmd.G, cmd.B));
            _shader.Send("u_RdpState.color.primLod", cmd.lodfrac / 255.0f);
        }

        public void SendColor(CmdID id, GSetColor setColor)
        {
            string name = id switch
            {
                CmdID.G_SETBLENDCOLOR => "u_RdpState.color.blend",
                CmdID.G_SETENVCOLOR => "u_RdpState.color.env",
                CmdID.G_SETFOGCOLOR => "u_RdpState.color.fog",
                _ => throw new ArgumentException($"Invalid Command for {nameof(SendColor)} : {id}"),
            };
            _shader.Send(name, Color.FromArgb(setColor.A, setColor.R, setColor.G, setColor.B));
        }

        public void SendInitialColors(Renderer.Config cfg)
        {
            _shader.Send("u_RdpState.color.prim", cfg.InitialPrimColor);
            _shader.Send("u_RdpState.color.blend", cfg.InitialBlendColor);
            _shader.Send("u_RdpState.color.env", cfg.InitialEnvColor);
            _shader.Send("u_RdpState.color.fog", cfg.InitialFogColor);
        }

        public void SendChromaKey(Renderer.ChromaKey key)
        {
            _shader.Send(
                "u_ChromaKeyCenter",
                key.r.center / 255.0f,
                key.g.center / 255.0f,
                key.b.center / 255.0f
            );
            _shader.Send(
                "u_ChromaKeyScale",
                key.r.scale / 255.0f,
                key.g.scale / 255.0f,
                key.b.scale / 255.0f
            );
        }

        public void SendCombiner(Renderer.ColorCombiner combiner)
        {
            _shader.Send(
                "u_RdpState.combiner.c1",
                (int)combiner.a.c1,
                (int)combiner.b.c1,
                (int)combiner.c.c1,
                (int)combiner.d.c1
            );
            _shader.Send(
                "u_RdpState.combiner.c2",
                (int)combiner.a.c2,
                (int)combiner.b.c2,
                (int)combiner.c.c2,
                (int)combiner.d.c2
            );
            _shader.Send(
                "u_RdpState.combiner.a1",
                (int)combiner.a.a1,
                (int)combiner.b.a1,
                (int)combiner.c.a1,
                (int)combiner.d.a1
            );
            _shader.Send(
                "u_RdpState.combiner.a2",
                (int)combiner.a.a2,
                (int)combiner.b.a2,
                (int)combiner.c.a2,
                (int)combiner.d.a2
            );
        }

        public void SendGeometryMode(uint mode)
        {
            _shader.Send("u_RdpState.geoMode", mode);
        }

        public void SendOtherMode(CmdID id, uint word)
        {
            string name = id switch
            {
                CmdID.G_SETOTHERMODE_H => "u_RdpState.otherMode.hi",
                CmdID.G_SETOTHERMODE_L => "u_RdpState.otherMode.lo",
                _ => throw new ArgumentException(
                    $"Invalid Command for {nameof(SendOtherMode)} : {id}"
                ),
            };
            _shader.Send(name, word);
        }

        public void SendWireFrameColor(Color color)
        {
            _wireframeShader.Send("u_WireFrameColor", color);
        }

        public void SetModelRenderMode(ModelRenderMode mode)
        {
            _mode = mode;
            _shader.Send("u_ModelRenderMode", (int)mode);
            _wireframeShader.Send("u_ModelRenderMode", (int)mode);
        }

        public void SendNormalColor(Color color)
        {
            _nrmShader.Send("u_NrmColor", color);
        }

        public void SendLightingEnabled(bool enabled)
        {
            _shader.Send("u_LigthingEnabled", enabled);
        }
        #endregion


        public void UseNormalShader() => _nrmShader.Use();

        public void UseVertexShader() => _shader.Use();

        public void UseWireFrameShader() => _wireframeShader.Use();

        public void Draw(PrimitiveType type, byte[] indices, bool drawNormals)
        {
            if (_mode == ModelRenderMode.Wireframe)
                UseWireFrameShader();
            else
                UseVertexShader();

            _attrs.Draw(type, indices);

            if (drawNormals)
            {
                UseNormalShader();
                _attrs.Draw(type, indices);
            }
        }
    }
}
