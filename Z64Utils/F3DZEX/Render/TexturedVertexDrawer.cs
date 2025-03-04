using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace F3DZEX.Render
{
    public class TexturedVertexDrawer : VertexDrawer
    {
        public TexturedVertexDrawer()
            : base(
                new ShaderHandler(
                    File.ReadAllText("Shaders/texturedVtx.vert"),
                    File.ReadAllText("Shaders/texturedVtx.frag")
                ),
                new VertexAttribs()
            )
        {
            // pos
            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
            // texture coordinates
            _attrs.LayoutAddFloat(2, VertexAttribPointerType.Float, false);
        }

        public void SendColor(Color color)
        {
            _shader.Send("u_Color", color);
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
