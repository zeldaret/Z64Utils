using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{

    [Serializable]
    public class BitWriterException : Exception
    {
        public BitWriterException() { }
        public BitWriterException(string message) : base(message) { }
        public BitWriterException(string message, Exception inner) : base(message, inner) { }
        protected BitWriterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class BitWriter : BinaryWriter
    {
        private byte _curByte;
        private int _curBitIdx = 0;

        public BitWriter(Stream s) : base(s) { }

        public override void Flush()
        {
            base.Write(_curByte);
            base.Flush();
        }

        public void WriteBit(bool bit)
        {
            _curByte |= (byte)(Convert.ToByte(bit) << (7 - _curBitIdx));
            _curBitIdx++;

            if (_curBitIdx >= 8)
            {
                _curBitIdx = 0;
                base.Write(_curByte);
                _curByte = 0;
            }
        }
        public void WriteBits(byte[] data, int bits)
        {
            if (bits < 0 || bits > data.Length * 8)
                throw new ArgumentOutOfRangeException(nameof(bits));

            int diff = data.Length * 8 - bits;
            int bitWritten = 0;
            for (int i = diff/8; i < data.Length; i++)
            {
                int start = i == diff / 8 ? diff % 8 : 0;
                for (int j = start; j < 8 && bitWritten < bits; j++, bitWritten++)
                    WriteBit(Convert.ToBoolean((data[i] >> (7 - j)) & 1));
            }
        }

        public void WriteSigned(int value, int intBits)
        {
            if (intBits < 0 || intBits > 31)
                throw new ArgumentOutOfRangeException(nameof(intBits));
            Write(value < 0);
            Write(value & int.MaxValue, intBits);
        }


        public void Write(byte value, int bits)
        {
            if (bits < 0 || bits > 8)
                throw new ArgumentOutOfRangeException(nameof(bits));
            WriteBits(new byte[] { value }, bits);
        }
        public void Write(ushort value, int bits)
        {
            if (bits < 0 || bits > 16)
                throw new ArgumentOutOfRangeException(nameof(bits));
            WriteBits(BitConverter.GetBytes(value).Reverse().ToArray(), bits);
        }
        public void Write(uint value, int bits)
        {
            if (bits < 0 || bits > 32)
                throw new ArgumentOutOfRangeException(nameof(bits));
            WriteBits(BitConverter.GetBytes(value).Reverse().ToArray(), bits);
        }
        public void Write(ulong value, int bits)
        {
            if (bits < 0 || bits > 64)
                throw new ArgumentOutOfRangeException(nameof(bits));
            WriteBits(BitConverter.GetBytes(value).Reverse().ToArray(), bits);
        }

        public void Write(sbyte value, int bits) => Write(unchecked((byte)value), bits);
        public void Write(short value, int bits) => Write(unchecked((ushort)value), bits);
        public void Write(int value, int bits) => Write(unchecked((uint)value), bits);
        public void Write(long value, int bits) => Write(unchecked((ulong)value), bits);

        public override void Write(sbyte value) => Write(value, 8);
        public override void Write(byte value) => Write(value, 8);
        public override void Write(short value) => Write(value, 16);
        public override void Write(ushort value) => Write(value, 16);
        public override void Write(int value) => Write(value, 32);
        public override void Write(uint value) => Write(value, 32);
        public override void Write(long value) => Write(value, 64);
        public override void Write(ulong value) => Write(value, 64);
        public override void Write(bool value) => WriteBit(value);
    }
}
