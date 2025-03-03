using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Syroot.BinaryData;

namespace N64
{
    [Serializable]
    public class N64RomException : Exception
    {
        public N64RomException() { }

        public N64RomException(string message)
            : base(message) { }

        public N64RomException(string message, Exception inner)
            : base(message, inner) { }

        protected N64RomException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        )
            : base(info, context) { }
    }

    public enum N64CountryCode
    {
        European = 0x50,
    }

    public class N64Rom
    {
        public int ClockRate
        {
            get { return Utils.BomSwap(BitConverter.ToInt32(RawRom, 4)); }
            set
            {
                byte[] data = BitConverter.GetBytes(Utils.BomSwap(value));
                Buffer.BlockCopy(data, 0, RawRom, 4, data.Length);
            }
        }
        public uint EntryPoint
        {
            get { return Utils.BomSwap(BitConverter.ToUInt32(RawRom, 8)); }
            set
            {
                byte[] data = BitConverter.GetBytes(Utils.BomSwap(value));
                Buffer.BlockCopy(data, 0, RawRom, 8, data.Length);
            }
        }
        public int ReleaseOffset
        {
            get { return Utils.BomSwap(BitConverter.ToInt32(RawRom, 0xc)); }
            set
            {
                byte[] data = BitConverter.GetBytes(Utils.BomSwap(value));
                Buffer.BlockCopy(data, 0, RawRom, 0xc, data.Length);
            }
        }
        public uint CRC1
        {
            get { return Utils.BomSwap(BitConverter.ToUInt32(RawRom, 0x10)); }
            set
            {
                byte[] data = BitConverter.GetBytes(Utils.BomSwap(value));
                Buffer.BlockCopy(data, 0, RawRom, 0x10, data.Length);
            }
        }
        public uint CRC2
        {
            get { return Utils.BomSwap(BitConverter.ToUInt32(RawRom, 0x14)); }
            set
            {
                byte[] data = BitConverter.GetBytes(Utils.BomSwap(value));
                Buffer.BlockCopy(data, 0, RawRom, 0x14, data.Length);
            }
        }
        public string Name
        {
            get { return Encoding.ASCII.GetString(RawRom, 0x20, 0x14).TrimEnd(' '); }
            set
            {
                byte[] data = Encoding.ASCII.GetBytes(value.ToArray());
                for (int i = 0x20; i < 0x20 + 0x14; i++)
                    RawRom[i] = (byte)' ';
                Buffer.BlockCopy(data, 0, RawRom, 0x20, Math.Min(0x14, data.Length));
            }
        }
        public string Developer
        {
            get { return BitConverter.ToChar(RawRom, 0x3B).ToString(); }
            set
            {
                if (value != null && value.Length > 0)
                    RawRom[0x3B] = (byte)value[0];
            }
        }
        public string CartID
        {
            get { return Encoding.ASCII.GetString(RawRom, 0x3C, 2); }
            set
            {
                byte[] data = Encoding.ASCII.GetBytes(value);
                if (data.Length == 2)
                    Buffer.BlockCopy(data, 0, RawRom, 0x3C, data.Length);
            }
        }
        public N64CountryCode CountryCode
        {
            get { return (N64CountryCode)RawRom[0x3E]; }
            set { RawRom[0x3E] = (byte)value; }
        }
        public byte Version
        {
            get { return RawRom[0x3F]; }
            set { RawRom[0x3F] = value; }
        }
        public byte[] BootStrap
        {
            get
            {
                byte[] data = new byte[0xFC0];
                Buffer.BlockCopy(RawRom, 0x40, data, 0, data.Length);
                return data;
            }
            set
            {
                if (value.Length == 0xFC0)
                    Buffer.BlockCopy(value, 0, RawRom, 0x40, value.Length);
            }
        }

        public byte[] RawRom { get; set; }

        public N64Rom(string file)
            : this(File.ReadAllBytes(file)) { }

        public N64Rom(byte[] data)
        {
            if (data.Length < 0x1000 || data.Length % 4 != 0)
                throw new N64RomException("Invalid ROM Size");

            //check for endian swap
            if (data[0] != 0x80 && data[1] == 0x80)
            {
                RawRom = new byte[data.Length];
                for (int i = 0; i < data.Length; i += 2)
                {
                    RawRom[i + 0] = data[i + 1];
                    RawRom[i + 1] = data[i + 0];
                }
            }
            else
                RawRom = data;
        }
    }
}
