using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.IO;
using Syroot.BinaryData;
using F3DZEX.Command;

namespace N64
{
    [Serializable]
    public class N64TextureException : Exception
    {
        public N64TextureException() { }
        public N64TextureException(string message) : base(message) { }
        public N64TextureException(string message, Exception inner) : base(message, inner) { }
        protected N64TextureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public enum N64TexFormat
    {
        RGBA16,
        RGBA32,
        IA16,
        IA8,
        IA4,
        I8,
        I4,
        CI8,
        CI4,
    }

    public static class N64Texture
    {
        public static Tuple<G_IM_FMT, G_IM_SIZ> ConvertFormat(N64TexFormat format)
        {
            switch (format)
            {
                case N64TexFormat.RGBA16: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_RGBA, G_IM_SIZ.G_IM_SIZ_16b);
                case N64TexFormat.RGBA32: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_RGBA, G_IM_SIZ.G_IM_SIZ_32b);
                case N64TexFormat.IA16: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_IA, G_IM_SIZ.G_IM_SIZ_16b);
                case N64TexFormat.IA8: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_IA, G_IM_SIZ.G_IM_SIZ_8b);
                case N64TexFormat.IA4: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_IA, G_IM_SIZ.G_IM_SIZ_4b);
                case N64TexFormat.I8: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_I, G_IM_SIZ.G_IM_SIZ_8b);
                case N64TexFormat.I4: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_I, G_IM_SIZ.G_IM_SIZ_4b);
                case N64TexFormat.CI8: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_CI, G_IM_SIZ.G_IM_SIZ_8b);
                case N64TexFormat.CI4: return new Tuple<G_IM_FMT, G_IM_SIZ>(G_IM_FMT.G_IM_FMT_CI, G_IM_SIZ.G_IM_SIZ_4b);
                default: throw new N64TextureException($"Invalid Texture Format : {format}");
            }
        }
        public static N64TexFormat ConvertFormat(G_IM_FMT fmt, G_IM_SIZ siz)
        {
            switch (fmt)
            {
                case G_IM_FMT.G_IM_FMT_RGBA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_16b: return N64TexFormat.RGBA16;
                        case G_IM_SIZ.G_IM_SIZ_32b: return N64TexFormat.RGBA32;
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_YUV:
                case G_IM_FMT.G_IM_FMT_CI:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return N64TexFormat.CI4;
                        case G_IM_SIZ.G_IM_SIZ_8b: return N64TexFormat.CI8;
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_IA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return N64TexFormat.IA4;
                        case G_IM_SIZ.G_IM_SIZ_8b: return N64TexFormat.IA8;
                        case G_IM_SIZ.G_IM_SIZ_16b: return N64TexFormat.IA16;
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_I:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return N64TexFormat.I4;
                        case G_IM_SIZ.G_IM_SIZ_8b: return N64TexFormat.I8;
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                default:
                    throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
            }
        }

        public static int GetTexSize(int texels, G_IM_SIZ siz)
        {
            switch (siz)
            {
                case G_IM_SIZ.G_IM_SIZ_4b: return texels / 2;
                case G_IM_SIZ.G_IM_SIZ_8b: return texels;
                case G_IM_SIZ.G_IM_SIZ_16b: return texels * 2;
                case G_IM_SIZ.G_IM_SIZ_32b: return texels * 4;
                default: throw new N64TextureException($"Invalid Texture Size : size={siz}");
            }
        }
        public static int GetTexSize(int texels, N64TexFormat format)
        {
            var a = ConvertFormat(format);
            return GetTexSize(texels, a.Item2);
        }

        public static Bitmap DecodeBitmap(int w, int h, N64TexFormat format, byte[] buff, byte[] tlut = null)
        {
            var a = ConvertFormat(format);
            return DecodeBitmap(w, h, a.Item1, a.Item2, buff, tlut);
        }
        public static unsafe Bitmap DecodeBitmap(int w, int h, G_IM_FMT fmt, G_IM_SIZ siz, byte[] buff, byte[] tlut = null)
        {
            Bitmap bmp = new Bitmap(w, h);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            byte* argb = (byte*)bmpData.Scan0;
            byte[] rgba = Decode(w * h, fmt, siz, buff, tlut);
            for (int i = 0; i < w * h; i++)
            {
                argb[4 * i + 3] = rgba[4 * i + 3]; //A
                argb[4 * i + 2] = rgba[4 * i + 0]; //R
                argb[4 * i + 1] = rgba[4 * i + 1]; //G
                argb[4 * i + 0] = rgba[4 * i + 2]; //B
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }
        public static byte[] Decode(int texels, G_IM_FMT fmt, G_IM_SIZ siz, byte[] buff, byte[] tlut)
        {
            switch (fmt)
            {
                case G_IM_FMT.G_IM_FMT_RGBA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_16b: return DecodeRgba16(texels, buff);
                        case G_IM_SIZ.G_IM_SIZ_32b: return DecodeRgba32(texels, buff);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_YUV:
                case G_IM_FMT.G_IM_FMT_CI:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return DecodeCI4(texels, buff, tlut);
                        case G_IM_SIZ.G_IM_SIZ_8b: return DecodeCI8(texels, buff, tlut);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_IA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return DecodeIA4(texels, buff);
                        case G_IM_SIZ.G_IM_SIZ_8b: return DecodeIA8(texels, buff);
                        case G_IM_SIZ.G_IM_SIZ_16b: return DecodeIA16(texels, buff);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_I:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return DecodeI4(texels, buff);
                        case G_IM_SIZ.G_IM_SIZ_8b: return DecodeI8(texels, buff);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                default:
                    throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
            }
        }

        public static unsafe byte[] EncodeBitmap(Bitmap bmp, N64TexFormat format)
        {
            var a = ConvertFormat(format);
            return EncodeBitmap(bmp, a.Item1, a.Item2);
        }
        public static unsafe byte[] EncodeBitmap(Bitmap bmp, G_IM_FMT fmt, G_IM_SIZ siz)
        {
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte* argb = (byte*)bmpData.Scan0;
            byte[] rgba = new byte[bmp.Width * bmp.Height * 4];
            for (int i = 0; i < bmp.Width*bmp.Height; i++)
            {
                rgba[4 * i + 0] = argb[4 * i + 2]; //R
                rgba[4 * i + 1] = argb[4 * i + 1]; //G
                rgba[4 * i + 2] = argb[4 * i + 0]; //B
                rgba[4 * i + 3] = argb[4 * i + 3]; //A
            }

            bmp.UnlockBits(bmpData);
            return Encode(rgba, fmt, siz);
        }

        public static byte[] Encode(byte[] rgba, G_IM_FMT fmt, G_IM_SIZ siz)
        {
            switch (fmt)
            {
                case G_IM_FMT.G_IM_FMT_RGBA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_16b: return EncodeRgba16(rgba);
                        case G_IM_SIZ.G_IM_SIZ_32b: return EncodeRgba32(rgba);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_YUV:
                case G_IM_FMT.G_IM_FMT_CI:
                    throw new NotImplementedException();
                    /*
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return EncodeCI4(rgba, tlut);
                        case G_IM_SIZ.G_IM_SIZ_8b: return EncodeCI8(rgba, tlut);
                        default: throw new N64TextureException("Invalid Size");
                    }
                    */
                case G_IM_FMT.G_IM_FMT_IA:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return EncodeIA4(rgba);
                        case G_IM_SIZ.G_IM_SIZ_8b: return EncodeIA8(rgba);
                        case G_IM_SIZ.G_IM_SIZ_16b: return EncodeIA16(rgba);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                case G_IM_FMT.G_IM_FMT_I:
                    switch (siz)
                    {
                        case G_IM_SIZ.G_IM_SIZ_4b: return EncodeI4(rgba);
                        case G_IM_SIZ.G_IM_SIZ_8b: return EncodeI8(rgba);
                        default: throw new N64TextureException($"Invalid Texture Format: fmt={fmt}; size={siz}");
                    }
                default:
                    throw new N64TextureException("Invalid Format");
            }
        }

        private static byte[] DecodeRgba16(int texels, byte[] inBuff)
        {
            if (inBuff.Length != texels * 2)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];
            //5551
            for (int t = 0; t < texels; t++)
            {
                byte b1 = inBuff[2 * t + 0];
                byte b2 = inBuff[2 * t + 1];
                ret[4 * t + 0] = (byte)((b1 >> 3) * 0xFF / 0x1F);
                ret[4 * t + 1] = (byte)((((b1 & 7) << 2) | (b2 >> 6)) * 0xFF / 0x1F);
                ret[4 * t + 2] = (byte)(((b2 >> 1) & 0x1F) * 0xFF / 0x1F);
                ret[4 * t + 3] = (byte)(0xFF * (b2 & 1));
            }

            return ret;
        }
        private static byte[] DecodeRgba32(int texels, byte[] inBuff)
        {
            if (inBuff.Length != texels * 4)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            return inBuff;
        }

        private static byte[] DecodeI4(int texels, byte[] inBuff)
        {
            if (inBuff.Length * 2 != texels)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];

            for (int t = 0; t < inBuff.Length; t++)
            {
                byte b = (byte)((inBuff[t] >> 4) * 0x11);
                ret[t * 8 + 0] = b;
                ret[t * 8 + 1] = b;
                ret[t * 8 + 2] = b;
                ret[t * 8 + 3] = 0xFF;
                //ret[t * 4 + 3] = b;

                b = (byte)((inBuff[t] & 0xF) * 0x11);
                ret[t * 8 + 4] = b;
                ret[t * 8 + 5] = b;
                ret[t * 8 + 6] = b;
                ret[t * 8 + 7] = 0xFF;
                //ret[t * 4 + 7] = b;
            }

            return ret;
        }
        private static byte[] DecodeI8(int texels, byte[] inBuff)
        {
            if (inBuff.Length != texels)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];
            for (int t = 0; t < texels; t++)
            {
                byte b = inBuff[t];
                ret[t * 4 + 0] = b;
                ret[t * 4 + 1] = b;
                ret[t * 4 + 2] = b;
                //ret[t * 4 + 3] = b;
                ret[t * 4 + 3] = 0xFF;
            }

            return ret;
        }

        private static byte[] DecodeIA4(int texels, byte[] inBuff)
        {
            if (inBuff.Length * 2 != texels)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];

            for (int t = 0; t < inBuff.Length; t++)
            {
                byte b = (byte)(inBuff[t] >> 4);
                byte i = (byte)((b >> 1) * 0xFF / 0b111);
                byte a = (byte)((b & 1) * 0xFF);
                ret[t * 8 + 0] = i;
                ret[t * 8 + 1] = i;
                ret[t * 8 + 2] = i;
                ret[t * 8 + 3] = a;

                b = (byte)((inBuff[t] & 0xF) * 0x11);
                i = (byte)((b >> 1) * 0xFF / 0b111);
                a = (byte)((b & 1) * 0xFF);
                ret[t * 8 + 4] = i;
                ret[t * 8 + 5] = i;
                ret[t * 8 + 6] = i;
                ret[t * 8 + 7] = a;
            }

            return ret;
        }
        private static byte[] DecodeIA8(int texels, byte[] inBuff)
        {
            if (texels != inBuff.Length)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];

            for (int t = 0; t < texels; t++)
            {
                byte b = inBuff[t];
                byte i = (byte)((b >> 4) * 0x11);
                byte a = (byte)((b & 0xF) * 0x11);
                ret[4 * t + 0] = i;
                ret[4 * t + 1] = i;
                ret[4 * t + 2] = i;
                ret[4 * t + 3] = a;
            }

            return ret;
        }
        private static byte[] DecodeIA16(int texels, byte[] inBuff)
        {
            if (inBuff.Length != texels * 2)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];

            for (int t = 0; t < texels; t++)
            {
                byte i = inBuff[2 * t + 0];
                byte a = inBuff[2 * t + 1];
                ret[4 * t + 0] = i;
                ret[4 * t + 1] = i;
                ret[4 * t + 2] = i;
                ret[4 * t + 3] = a;
            }

            return ret;
        }

        private static byte[] DecodeCI4(int texels, byte[] inBuff, byte[] tlut)
        {
            if (texels != inBuff.Length * 2)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];
            for (int t = 0; t < inBuff.Length; t++)
            {
                byte idx = (byte)(inBuff[t] >> 4 & 0xF);
                byte b1 = tlut[2 * idx + 0];
                byte b2 = tlut[2 * idx + 1];
                ret[8 * t + 0] = (byte)((b1 >> 3) * 0xFF / 0x1F);
                ret[8 * t + 1] = (byte)((((b1 & 7) << 2) | (b2 >> 6)) * 0xFF / 0x1F);
                ret[8 * t + 2] = (byte)(((b2 >> 1) & 0x1F) * 0xFF / 0x1F);
                ret[8 * t + 3] = (byte)(0xFF * (b2 & 1));

                idx = (byte)(inBuff[t] & 0xF);
                b1 = tlut[2 * idx + 0];
                b2 = tlut[2 * idx + 1];
                ret[8 * t + 4] = (byte)((b1 >> 3) * 0xFF / 0x1F);
                ret[8 * t + 5] = (byte)((((b1 & 7) << 2) | (b2 >> 6)) * 0xFF / 0x1F);
                ret[8 * t + 6] = (byte)(((b2 >> 1) & 0x1F) * 0xFF / 0x1F);
                ret[8 * t + 7] = (byte)(0xFF * (b2 & 1));
            }

            return ret;
        }
        private static byte[] DecodeCI8(int texels, byte[] inBuff, byte[] tlut)
        {
            if (texels != inBuff.Length)
                throw new N64TextureException($"Invalid Buffer Size for {texels} texels");

            byte[] ret = new byte[texels * 4];
            for (int t = 0; t < texels; t++)
            {
                byte idx = inBuff[t];
                byte b1 = tlut[2 * idx + 0];
                byte b2 = tlut[2 * idx + 1];

                ret[4 * t + 0] = (byte)((b1 >> 3) * 0xFF / 0x1F);
                ret[4 * t + 1] = (byte)((((b1 & 7) << 2) | (b2 >> 6)) * 0xFF / 0x1F);
                ret[4 * t + 2] = (byte)(((b2 >> 1) & 0x1F) * 0xFF / 0x1F);
                ret[4 * t + 3] = (byte)(0xFF * (b2 & 1));
            }

            return ret;
        }


        private static byte[] EncodeRgba16(byte[] rgba)
        {
            if (rgba.Length % 4 != 0)
                throw new N64TextureException("Invalid Size");

            int texels = rgba.Length / 4;

            byte[] ret = new byte[texels*2];

            for (int i = 0; i < texels; i++)
            {
                byte r = (byte)(rgba[i * 4 + 0] * 0x1F / 0xFF);
                byte g = (byte)(rgba[i * 4 + 1] * 0x1F / 0xFF);
                byte b = (byte)(rgba[i * 4 + 2] * 0x1F / 0xFF);
                byte a = (byte)(rgba[i * 4 + 3] > 0x7F ? 1 : 0);

                //RRRRRGGG
                ret[i * 2 + 0] = (byte)((r << 3) | (g >> 2));
                //GGBBBBBA
                ret[i * 2 + 1] = (byte)((g << 6) | (b << 1) | a);
            }
            return ret;
        }
        private static byte[] EncodeRgba32(byte[] rgba)
        {
            if (rgba.Length % 4 != 0)
                throw new N64TextureException("Invalid Size");
            return rgba;
        }

        //TODO: https://www.johndcook.com/blog/2009/08/24/algorithms-convert-color-grayscale/
        private static byte[] EncodeI4(byte[] rgba)
        {
            throw new NotImplementedException();
        }
        private static byte[] EncodeI8(byte[]rgba)
        {
            throw new NotImplementedException();
        }

        private static byte[] EncodeIA4(byte[] rgba)
        {
            throw new NotImplementedException();
        }
        private static byte[] EncodeIA8(byte[] rgba)
        {
            throw new NotImplementedException();
        }
        private static byte[] EncodeIA16(byte[] rgba)
        {
            throw new NotImplementedException();
        }

    }
}
