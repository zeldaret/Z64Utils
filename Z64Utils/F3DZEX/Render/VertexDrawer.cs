using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace F3DZEX.Render
{
    public abstract class VertexDrawer
    {
        protected ShaderHandler _shader;
        protected VertexAttribs _attrs;

        protected void SetVertexData(
            byte[] buffer,
            bool bigEndian = false,
            BufferUsageHint hint = BufferUsageHint.StaticDraw
        ) => _attrs.SetData(buffer, bigEndian, hint);

        protected void SetVertexSubData(byte[] data, int off, bool bigEndian) =>
            _attrs.SetSubData(data, off, bigEndian);

        protected void SetVertexData<T>(
            T[] buffer,
            int size,
            BufferUsageHint hint = BufferUsageHint.StaticDraw
        )
            where T : struct, IComparable => _attrs.SetData(buffer, size, hint);

        public virtual void Draw(PrimitiveType type, byte[] indices)
        {
            _shader.Use();
            _attrs.Draw(type, indices);
        }

        public virtual void Draw(PrimitiveType type, uint[] indices)
        {
            _shader.Use();
            _attrs.Draw(type, indices);
        }

        public virtual void Draw(PrimitiveType type)
        {
            _shader.Use();
            _attrs.Draw(type);
        }
    }
}
