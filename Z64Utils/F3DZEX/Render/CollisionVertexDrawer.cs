using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Z64.Forms;

namespace F3DZEX.Render
{
    public class CollisionVertexDrawer : VertexDrawer
    {
        public CollisionVertexDrawer()
            : base(
                new ShaderHandler(
                    File.ReadAllText("Shaders/collisionVtx.vert"),
                    File.ReadAllText("Shaders/collisionVtx.frag")
                ),
                new VertexAttribs()
            )
        {
            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
        }

        public void SendProjViewMatrices(ref Matrix4 proj, ref Matrix4 view)
        {
            _shader.Send("u_Projection", proj);
            _shader.Send("u_View", view);
        }

        public void SendModelMatrix(Matrix4 model) => _shader.Send("u_Model", model);

        public void SendColor(Color color)
        {
            _shader.Send("u_Color", color);
        }

        public void SetDataTriangles(
            CollisionViewerForm.RenderColPoly[] polys,
            BufferUsageHint hint
        )
        {
            float[] data = new float[3 * (3 + 3) * polys.Length];
            int i = 0;
            foreach (var poly in polys)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i++] = poly.Points[j][0];
                    data[i++] = poly.Points[j][1];
                    data[i++] = poly.Points[j][2];
                    data[i++] = (float)poly.Normal.X / 0x7FFF;
                    data[i++] = (float)poly.Normal.Y / 0x7FFF;
                    data[i++] = (float)poly.Normal.Z / 0x7FFF;
                }
            }
            SetVertexData(data, sizeof(float) * data.Length, hint);
        }

        public void SetDataLines(CollisionViewerForm.RenderColPoly[] polys, BufferUsageHint hint)
        {
            float[] data = new float[3 * 2 * (3 + 3) * polys.Length];
            int i = 0;
            foreach (var poly in polys)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i++] = poly.Points[j][0];
                    data[i++] = poly.Points[j][1];
                    data[i++] = poly.Points[j][2];
                    data[i++] = (float)poly.Normal.X / 0x7FFF;
                    data[i++] = (float)poly.Normal.Y / 0x7FFF;
                    data[i++] = (float)poly.Normal.Z / 0x7FFF;

                    data[i++] = poly.Points[(j + 1) % 3][0];
                    data[i++] = poly.Points[(j + 1) % 3][1];
                    data[i++] = poly.Points[(j + 1) % 3][2];
                    data[i++] = (float)poly.Normal.X / 0x7FFF;
                    data[i++] = (float)poly.Normal.Y / 0x7FFF;
                    data[i++] = (float)poly.Normal.Z / 0x7FFF;
                }
            }
            SetVertexData(data, sizeof(float) * data.Length, hint);
        }
    }
}
