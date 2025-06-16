using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace F3DZEX.Render
{
    public class ColoredVertexDrawer : VertexDrawer
    {
        public ColoredVertexDrawer()
            : base(
                ShaderHandler.FromSrcFilesInShadersDir("coloredVtx.vert", "coloredVtx.frag"),
                new VertexAttribs()
            )
        {
            // pos
            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
            // color
            _attrs.LayoutAddFloat(4, VertexAttribPointerType.Float, false);
        }

        public void SetData(float[] data, BufferUsageHint hint) =>
            SetVertexData(data, data.Length * sizeof(float), hint);

        public void SendProjViewMatrices(ref Matrix4 proj, ref Matrix4 view)
        {
            _shader.Send("u_Projection", proj);
            _shader.Send("u_View", view);
        }

        public void SendModelMatrix(Matrix4 model) => _shader.Send("u_Model", model);
    }
}
