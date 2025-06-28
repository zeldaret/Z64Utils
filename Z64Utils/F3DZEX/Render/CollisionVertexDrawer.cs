using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Z64;
using Z64Utils_Avalonia;

namespace F3DZEX.Render
{
    public class CollisionVertexDrawer : VertexDrawer
    {
        public CollisionVertexDrawer()
            : base(
                ShaderHandler.FromSrcFilesInShadersDir("collisionVtx.vert", "collisionVtx.frag"),
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

        public void SetDataTriangles(List<CollisionPolygon> polys, BufferUsageHint hint)
        {
            float[] data = new float[3 * (3 + 3) * polys.Count];
            int i = 0;
            foreach (var poly in polys)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i++] = poly.verts[j].X;
                    data[i++] = poly.verts[j].Y;
                    data[i++] = poly.verts[j].Z;
                    data[i++] = (float)poly.normal.X / 0x7FFF;
                    data[i++] = (float)poly.normal.Y / 0x7FFF;
                    data[i++] = (float)poly.normal.Z / 0x7FFF;
                }
            }
            SetVertexData(data, sizeof(float) * data.Length, hint);
        }

        public void SetDataLines(List<CollisionPolygon> polys, BufferUsageHint hint)
        {
            float[] data = new float[3 * 2 * (3 + 3) * polys.Count];
            int i = 0;
            foreach (var poly in polys)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i++] = poly.verts[j].X;
                    data[i++] = poly.verts[j].Y;
                    data[i++] = poly.verts[j].Z;
                    data[i++] = (float)poly.normal.X / 0x7FFF;
                    data[i++] = (float)poly.normal.Y / 0x7FFF;
                    data[i++] = (float)poly.normal.Z / 0x7FFF;

                    data[i++] = poly.verts[(j + 1) % 3].X;
                    data[i++] = poly.verts[(j + 1) % 3].Y;
                    data[i++] = poly.verts[(j + 1) % 3].Z;
                    data[i++] = (float)poly.normal.X / 0x7FFF;
                    data[i++] = (float)poly.normal.Y / 0x7FFF;
                    data[i++] = (float)poly.normal.Z / 0x7FFF;
                }
            }
            SetVertexData(data, sizeof(float) * data.Length, hint);
        }
    }
}
