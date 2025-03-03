using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace F3DZEX.Render
{
    public class TextDrawer
    {
        TexturedVertexDrawer _vtxDrawer;
        TextureHandler _tex;
        Font _font;
        Color _color;

        //float _scale;
        byte[] _texData;
        int _texWidth;
        int _texHeight;
        RectangleF[] _charSpaces;
        string _lastStr;
        float[] _lastVertices;

        public Vector2 Position { get; set; }
        public Font TextFont
        {
            get => _font;
            set
            {
                _font = value;
                GenerateAlphabetTexture();
            }
        }
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                SendColor(_color);
            }
        }

        public float Scale { get; set; }
        public int TextHSpace { get; set; }
        public int TextVSpace { get; set; }

        public TextDrawer()
        {
            _vtxDrawer = new TexturedVertexDrawer();
            _tex = new TextureHandler();
            TextFont = new Font("Arial", 50);
            _lastStr = null;
            Scale = 0.35f;
            Color = Color.White;
            TextHSpace = -15;
            TextVSpace = -7;
        }

        public float GetTextWidth(string text)
        {
            using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                return g.MeasureString(text, TextFont).Width;
        }

        private SizeF GetCharSize(char c)
        {
            using (Graphics g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                return g.MeasureString(c.ToString(), TextFont);
            }
        }

        private RectangleF[] GetCharRange(string text)
        {
            RectangleF[] ret = new RectangleF[text.Length];

            using (var g = Graphics.FromImage(new Bitmap(1, 1)))
            {
                float x = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    var size = g.MeasureString(text[i].ToString(), _font);

                    ret[i] = new RectangleF(x, 0, size.Width, size.Height);
                    x += size.Width;
                }

                return ret;
            }
        }

        private void GenerateAlphabetTexture()
        {
            string alphabet = GetAlphabet();
            _charSpaces = GetCharRange(alphabet);

            // calculate texture size
            _texWidth = (int)(_charSpaces.Last().X + _charSpaces.Last().Width);
            _texHeight = 0;
            for (int i = 0; i < alphabet.Length; i++)
                _texHeight = Math.Max(_texHeight, (int)_charSpaces[i].Height);

            // draw alphabet
            Bitmap bmp = new Bitmap(_texWidth, _texHeight);
            using (var g = Graphics.FromImage(bmp))
                for (int i = 0; i < alphabet.Length; i++)
                    g.DrawString(
                        alphabet[i].ToString(),
                        _font,
                        new SolidBrush(Color.White),
                        _charSpaces[i].X,
                        0
                    );

            _texData = new byte[bmp.Width * bmp.Height * 4];
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            Marshal.Copy(bmpData.Scan0, _texData, 0, _texData.Length);

            bmp.UnlockBits(bmpData);

            _tex.SetDataRGBA(_texData, _texWidth, _texHeight);
        }

        private static string GetAlphabet()
        {
            string ret = "";
            for (int i = 0; i < 0x100; i++)
                ret += (char)i;
            return ret;
        }

        public float[] GenerateVertices(string text)
        {
            int stride = 3 + 2;
            float[] vertices = new float[stride * 4 * text.Replace("\n", "").Length];

            float x = 0;
            float y = 0;
            int vtxIdx = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    y -= _texHeight + TextVSpace;
                    x = 0;
                    continue;
                }

                RectangleF rec = _charSpaces[text[i] >= 0x100 ? 0 : text[i]];

                // top left
                vertices[vtxIdx++] = x;
                vertices[vtxIdx++] = y;
                vertices[vtxIdx++] = 0;
                vertices[vtxIdx++] = rec.X / _texWidth;
                vertices[vtxIdx++] = -(rec.Y / _texHeight);

                // top right
                vertices[vtxIdx++] = x + rec.Width;
                vertices[vtxIdx++] = y;
                vertices[vtxIdx++] = 0;
                vertices[vtxIdx++] = (rec.X + rec.Width) / _texWidth;
                vertices[vtxIdx++] = -(rec.Y / _texHeight);

                // bottom right
                vertices[vtxIdx++] = x + rec.Width;
                vertices[vtxIdx++] = y + rec.Height;
                vertices[vtxIdx++] = 0;
                vertices[vtxIdx++] = (rec.X + rec.Width) / _texWidth;
                vertices[vtxIdx++] = -((rec.Y + rec.Height) / _texHeight);

                // bottom left
                vertices[vtxIdx++] = x;
                vertices[vtxIdx++] = y + rec.Height;
                vertices[vtxIdx++] = 0;
                vertices[vtxIdx++] = rec.X / _texWidth;
                vertices[vtxIdx++] = -((rec.Y + rec.Height) / _texHeight);

                x += rec.Width;
                if (text[i] != ' ')
                    x += TextHSpace;
            }

            return vertices;
        }

        public void DrawString(string str)
        {
            _tex.Use();
            if (str != _lastStr)
            {
                _lastStr = str;
                _lastVertices = GenerateVertices(str);
                _vtxDrawer.SetData(_lastVertices, BufferUsageHint.DynamicDraw);
            }

            _vtxDrawer.SendModelMatrix(GetMatrix());
            _vtxDrawer.Draw(PrimitiveType.Quads);
        }

        public Matrix4 GetMatrix()
        {
            GL.GetFloat(GetPName.Viewport, out Vector4 view);

            Vector3 pos = new Vector3(Position);
            pos.X -= 1;
            pos.Y += 1 - (_texHeight / (view.W * 2));

            return Matrix4.CreateScale(Scale / view.Z, Scale / view.W, 1)
                * Matrix4.CreateTranslation(pos);
        }

        public void SendColor(Color color)
        {
            _vtxDrawer.SendColor(color);
        }

        public void SendProjViewMatrices(ref Matrix4 proj, ref Matrix4 view)
        {
            _vtxDrawer.SendProjViewMatrices(ref proj, ref view);
        }
    }
}
