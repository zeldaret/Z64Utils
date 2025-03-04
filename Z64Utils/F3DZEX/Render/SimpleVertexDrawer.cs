using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace F3DZEX.Render
{
    public class SimpleVertexDrawer : VertexDrawer
    {
        public SimpleVertexDrawer()
            : base(
                new ShaderHandler(
                    File.ReadAllText("Shaders/simpleVtx.vert"),
                    File.ReadAllText("Shaders/coloredVtx.frag")
                ),
                new VertexAttribs()
            )
        {
            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
        }

        public void SendProjViewMatrices(ref Matrix4 proj, ref Matrix4 view)
        {
            _shader.Send("u_Projection", proj);
            _shader.Send("u_View", view);
        }

        public void SendModelMatrix(Matrix4 model)
        {
            _shader.Send("u_Model", model);
        }

        public void SendColor(Color color)
        {
            _shader.Send("u_Color", color);
        }

        public void SetData(float[] data, BufferUsageHint hint)
        {
            _attrs.SetData(data, data.Length * sizeof(float), hint);
        }

        public void SetSubData(float[] data, int off = 0)
        {
            _attrs.SetSubData(data, off, data.Length * sizeof(float));
        }
    }
}
