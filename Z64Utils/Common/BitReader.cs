using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BitReader : BinaryReader
    {
        private byte _curByte;
        private int _curBitIdx = 8;

        public BitReader(Stream s)
            : base(s) { }

        private byte ReadBit()
        {
            if (_curBitIdx >= 8)
            {
                _curBitIdx = 0;
                _curByte = base.ReadByte();
            }
            byte b = (byte)((_curByte >> (7 - _curBitIdx)) & 1);
            _curBitIdx++;

            return b;
        }

        public byte[] ReadBits(int bits)
        {
            byte[] buff = new byte[bits / 8 + ((bits % 8) > 0 ? 1 : 0)];

            int firstRest = 8 - bits % 8;
            if (firstRest >= 8)
                firstRest = 0;
            int bitRead = 0;
            for (int i = 0; i < buff.Length; i++)
            {
                for (int j = i == 0 ? firstRest : 0; j < 8 && bitRead < bits; j++, bitRead++)
                    buff[i] |= (byte)(ReadBit() << (7 - j));
            }
            return buff;
        }

        public void SkipBits(int bits) => ReadBits(bits);

        public byte ReadByte(int bits)
        {
            if (bits < 0 || bits > 8)
                throw new ArgumentOutOfRangeException(nameof(bits));
            return ReadBits(bits)[0];
        }

        public ushort ReadUInt16(int bits)
        {
            if (bits < 0 || bits > 32)
                throw new ArgumentOutOfRangeException(nameof(bits));
            return (ushort)ReadUInt64(bits);
        }

        public uint ReadUInt32(int bits)
        {
            if (bits < 0 || bits > 32)
                throw new ArgumentOutOfRangeException(nameof(bits));
            return (uint)ReadUInt64(bits);
        }

        public ulong ReadUInt64(int bits)
        {
            if (bits < 0 || bits > 64)
                throw new ArgumentOutOfRangeException(nameof(bits));

            byte[] data = ReadBits(bits);

            byte[] buff = new byte[sizeof(ulong)];
            Buffer.BlockCopy(data, 0, buff, buff.Length - data.Length, data.Length);
            return BitConverter.ToUInt64(buff.Reverse().ToArray(), 0);
        }

        public int ReadSigned(int intbits)
        {
            if (intbits < 0 || intbits > 31)
                throw new ArgumentOutOfRangeException(nameof(intbits));
            bool sign = ReadBoolean();
            int v = ReadInt32(intbits);
            if (sign)
                v *= -1;
            return v;
        }

        public sbyte ReadSByte(int bits) => unchecked((sbyte)ReadByte(bits));

        public short ReadInt16(int bits) => unchecked((short)ReadUInt16(bits));

        public int ReadInt32(int bits) => unchecked((int)ReadUInt32(bits));

        public long ReadInt64(int bits) => unchecked((long)ReadUInt64(bits));

        public override sbyte ReadSByte() => ReadSByte(8);

        public override byte ReadByte() => ReadByte(8);

        public override short ReadInt16() => ReadInt16(16);

        public override ushort ReadUInt16() => ReadUInt16(16);

        public override int ReadInt32() => ReadInt32(32);

        public override uint ReadUInt32() => ReadUInt32(32);

        public override long ReadInt64() => ReadInt64(64);

        public override ulong ReadUInt64() => ReadUInt64(64);

        public override bool ReadBoolean() => Convert.ToBoolean(ReadBit());
    }
}
