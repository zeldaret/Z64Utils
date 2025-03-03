using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.IO;

namespace F3DZEX.Render
{
    public class CollisionVertexDrawer : VertexDrawer
    {
        public CollisionVertexDrawer()
        {
            _shader = new ShaderHandler(File.ReadAllText("Shaders/collisionVtx.vert"), File.ReadAllText("Shaders/collisionVtx.frag"));
            _attrs = new VertexAttribs();

            _attrs.LayoutAddFloat(3, VertexAttribPointerType.Float, false);
            byte[] fakeData = new byte[] { 0, 0, 0 };
            SetVertexData(fakeData, fakeData.Length, BufferUsageHint.StaticDraw);
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
    }
}
