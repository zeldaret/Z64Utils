using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using Common;
using OpenTK;
using OpenTK.Mathematics;
using Syroot.BinaryData;
using Z64;

namespace RDP
{
    public class Vertex
    {
        public const int SIZE = 0x10;

        public short X;
        public short Y;
        public short Z;
        public ushort Flag { get; set; }
        public short TexX;
        public short TexY;
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Vertex() { }

        public Vertex(BinaryStream br)
        {
            Parse(br);
        }

        public Vertex(byte[] rawVtx)
        {
            using (MemoryStream ms = new MemoryStream(rawVtx))
            {
                BinaryStream br = new BinaryStream(ms);
                br.ByteConverter = Syroot.BinaryData.ByteConverter.Big;
                Parse(br);
            }
        }

        private void Parse(BinaryStream br)
        {
            X = br.ReadInt16();
            Y = br.ReadInt16();
            Z = br.ReadInt16();
            Flag = br.ReadUInt16();
            TexX = br.ReadInt16();
            TexY = br.ReadInt16();
            R = br.Read1Byte();
            G = br.Read1Byte();
            B = br.Read1Byte();
            A = br.Read1Byte();
        }

        public void Write(BinaryStream bw)
        {
            bw.Write(X);
            bw.Write(Y);
            bw.Write(Z);
            bw.Write(Flag);
            bw.Write(TexX);
            bw.Write(TexY);
            bw.Write(R);
            bw.Write(G);
            bw.Write(B);
            bw.Write(A);
        }
    }

    public class SegmentedAddress
    {
        public bool Segmented { get; set; }
        public int SegmentId { get; set; }
        public uint SegmentOff { get; set; }
        public uint VAddr
        {
            get => Segmented ? ((uint)(SegmentId << 24) | SegmentOff) : SegmentOff;
        }

        public SegmentedAddress(int segId, int segOff)
            : this((uint)(segId << 24) | (uint)segOff) { }

        public SegmentedAddress(uint vaddr)
        {
            int segId = (int)(vaddr >> 24);
            Segmented = (segId >= 0 && segId < 16);
            if (Segmented)
            {
                SegmentId = (int)(vaddr >> 24);
                SegmentOff = vaddr & 0xFFFFFF;
            }
            else
            {
                SegmentId = -1;
                SegmentOff = vaddr;
            }
        }

        public static implicit operator uint(SegmentedAddress seg) => seg.VAddr;

        public static explicit operator SegmentedAddress(uint addr) => new SegmentedAddress(addr);

        public static SegmentedAddress? Parse(string text, bool acceptPrefix = true)
        {
            if (acceptPrefix && text.StartsWith("0x"))
                text = text.Substring(2);

            if (
                uint.TryParse(
                    text,
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out uint addr
                )
            )
                return new SegmentedAddress(addr);

            return null;
        }

        public static bool TryParse(
            string text,
            bool acceptPrefix,
            [NotNullWhen(true)] out SegmentedAddress? addr
        )
        {
            addr = Parse(text, acceptPrefix);

            return addr != null;
        }
    }

    public class Mtx
    {
        public const int SIZE = 0x40;

        public short[,] intPart = new short[4, 4];
        public ushort[,] fracPart = new ushort[4, 4];

        public Mtx() { }

        public Mtx(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryStream br = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);
                Parse(br);
            }
        }

        public Mtx(BinaryStream br)
        {
            Parse(br);
        }

        private void Parse(BinaryStream br)
        {
            for (int i = 0; i < intPart.GetLength(0); i++)
            for (int j = 0; j < intPart.GetLength(1); j++)
                intPart[i, j] = br.ReadInt16();

            for (int i = 0; i < fracPart.GetLength(0); i++)
            for (int j = 0; j < fracPart.GetLength(1); j++)
                fracPart[i, j] = br.ReadUInt16();
        }

        public void Write(BinaryStream bw)
        {
            for (int i = 0; i < intPart.GetLength(0); i++)
            for (int j = 0; j < intPart.GetLength(1); j++)
                bw.Write(intPart[i, j]);

            for (int i = 0; i < fracPart.GetLength(0); i++)
            for (int j = 0; j < fracPart.GetLength(1); j++)
                bw.Write(fracPart[i, j]);
        }

        public byte[] GetBuffer()
        {
            byte[] buff = new byte[SIZE];
            using (MemoryStream ms = new MemoryStream(buff))
            {
                BinaryStream bw = new BinaryStream(ms, Syroot.BinaryData.ByteConverter.Big);
                Write(bw);
            }
            return buff;
        }

        public Matrix4 ToMatrix4()
        {
            Matrix4 ret = new Matrix4();

            for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                ret[i, j] = ((intPart[i, j] << 16) | fracPart[i, j]) / (float)0x10000;

            return ret;
        }

        public static Mtx FromMatrix4(Matrix4 m4)
        {
            Mtx ret = new Mtx();

            for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
            {
                int v = (int)(m4[i, j] * 0x10000);
                ret.intPart[i, j] = (short)(v >> 16);
                ret.fracPart[i, j] = (ushort)(v & 0xFFFF);
            }

            return ret;
        }
    }

    public struct FixedPoint
    {
        public int Raw;
        public readonly int IntBits;
        public readonly int FracBits;
        public readonly bool Signed;

        public bool SignBit() => Signed ? (((Raw >> (FracBits + IntBits)) & 1) == 1) : false;

        public uint IntPart() => (uint)((Raw >> FracBits) & ((1 << IntBits) - 1));

        public uint FracPart() => (uint)(Raw & ((1 << FracBits) - 1));

        public float Float()
        {
            float ret = IntPart() + ((float)FracPart() / (1 << FracBits));
            if (SignBit())
                ret *= -1;

            return ret;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            if (SignBit())
                parts.Add($"(1<<{IntBits + FracBits})");

            uint intPart = IntPart();
            if (intPart != 0)
                parts.Add($"({intPart}<<{FracBits})");

            uint fracPart = FracPart();
            if (fracPart != 0)
                parts.Add($"{fracPart}");

            if (parts.Count == 0)
                return "0";
            return string.Join(" | ", parts.ToArray());
        }

        public FixedPoint(int raw, int intBits, int fracBits, bool signed = false)
        {
            Raw = raw;
            IntBits = intBits;
            FracBits = fracBits;
            Signed = signed;

            if (intBits < 0)
                throw new ArgumentOutOfRangeException(nameof(intBits));
            if (fracBits < 0)
                throw new ArgumentOutOfRangeException(nameof(fracBits));

            if ((intBits + fracBits) > 32)
                throw new ArgumentOutOfRangeException($"{nameof(intBits)}/{nameof(fracBits)}");
        }
    }
}
