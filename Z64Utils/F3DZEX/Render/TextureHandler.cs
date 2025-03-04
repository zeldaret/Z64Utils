using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

#nullable enable

namespace F3DZEX.Render
{
    public class TextureHandler
    {
        int _texId;

        public int TexID => _texId;

        public TextureHandler()
        {
            GL.GenTextures(1, out _texId);
            Use();
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest
            );
            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest
            );
        }

        public void SetDataRGBA(byte[] data, int width, int height)
        {
            Use();
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                data
            );
        }

        public void SetTextureWrap(int wrapS, int wrapT)
        {
            Use();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrapT);
        }

        public void Use() => GL.BindTexture(TextureTarget.Texture2D, _texId);
    }
}
