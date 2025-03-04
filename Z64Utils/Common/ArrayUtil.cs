using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class ArrayUtil
    {
        public static uint ReadUint32BE(byte[] data, int offset = 0)
        {
            byte[] buff = new byte[sizeof(uint)];
            Buffer.BlockCopy(data, offset, buff, 0, sizeof(uint));
            return BitConverter.ToUInt32(buff.Reverse().ToArray(), 0);
        }

        public static short ReadInt16BE(byte[] data, int offset = 0)
        {
            byte[] buff = new byte[sizeof(short)];
            Buffer.BlockCopy(data, offset, buff, 0, sizeof(short));
            return BitConverter.ToInt16(buff.Reverse().ToArray(), 0);
        }

        public static ushort ReadUInt16BE(byte[] data, int offset = 0)
        {
            byte[] buff = new byte[sizeof(ushort)];
            Buffer.BlockCopy(data, offset, buff, 0, sizeof(ushort));
            return BitConverter.ToUInt16(buff.Reverse().ToArray(), 0);
        }
    }
}
