using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace N64
{
    [Serializable]
    public class Yaz0Exception : Exception
    {
        public Yaz0Exception() { }
        public Yaz0Exception(string message) : base(message) { }
        public Yaz0Exception(string message, Exception inner) : base(message, inner) { }
        protected Yaz0Exception(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class Yaz0
    {
        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream src = new MemoryStream(data))
            {
                BinaryStream br = new BinaryStream(src, ByteConverter.Big);

                string Magic = br.ReadString(4, Encoding.ASCII);
                if (Magic != "Yaz0")
                    throw new Exception("Invalid Header");

                uint fileSize = br.ReadUInt32();
                br.Position += 8; //pad

                byte[] dstBuffer = new byte[fileSize];
                using (MemoryStream dst = new MemoryStream(dstBuffer))
                {
                    BinaryWriter bw = new BinaryWriter(dst);

                    byte ChunkHeader = br.Read1Byte(); //to start the first chunk
                    int chunkIndex = 0;

                    while (bw.BaseStream.Position < fileSize)
                    {
                        //uncompressed byte
                        if ((ChunkHeader & 0x80) == 0x80) //if first bit is 1)
                        {
                            bw.Write(br.Read1Byte());
                        }
                        //compressed data
                        else
                        {

                            ushort raw = br.ReadUInt16();
                            byte nibble = (byte)(raw >> 12);

                            ushort backOff = (ushort)((ushort)(raw << 4) >> 4);
                            ushort size = (nibble != 0)
                                ? (ushort)(nibble + 2)              // 2 bytes NR RR
                                : (ushort)(br.Read1Byte() + 0x12);  // 3 bytes 0R RR NN

                            long tmpPos = bw.BaseStream.Position;
                            long newPos = tmpPos - backOff - 1;

                            for (int i = 0; i < size; i++)
                            {
                                byte b = 0; //if byte is out of stream
                                if (newPos + i >= 0)
                                {
                                    bw.BaseStream.Position = newPos + i;
                                    b = (byte)bw.BaseStream.Read1Byte();
                                    bw.BaseStream.Position = tmpPos + i;
                                }

                                bw.Write(b);
                            }
                        }

                        chunkIndex++;
                        ChunkHeader <<= 1;

                        //starts a new chunk
                        if ((chunkIndex == 8) && bw.BaseStream.Position < fileSize)
                        {
                            ChunkHeader = br.Read1Byte();
                            chunkIndex = 0;
                        }
                    }

                    return dstBuffer;
                }
            }
        }

        // quick and dirty port from https://gist.github.com/notwa/b2ddcd4999be54d39b9b6913c648dfe8#file-yaz0-c
        private const int MAX_RUNLEN = (0xFF + 0x12);
        public static byte[] Compress(byte[] src)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                bw.Write(Encoding.ASCII.GetBytes("Yaz0"));
                bw.Write(new byte[0xC]);

                int srcPos = 0;
                int bufPos = 0;

                byte[] buf = new byte[24]; // 8 codes * 3 bytes maximum

                int validBitCount = 0; // number of valid bits left in "code" byte
                byte currCodeByte = 0; // a bitfield, set bits meaning copy, unset meaning RLE

                while (srcPos < src.Length)
                {
                    int numBytes;
                    int matchPos;

                    numBytes = nintendoEnc(src, srcPos, out matchPos);
                    if (numBytes < 3)
                    {
                        // straight copy
                        buf[bufPos] = src[srcPos];
                        bufPos++;
                        srcPos++;
                        //set flag for straight copy
                        currCodeByte |= (byte)(0x80 >> validBitCount);
                    }
                    else
                    {
                        //RLE part
                        int dist = srcPos - matchPos - 1;
                        byte byte1, byte2, byte3;

                        if (numBytes >= 0x12)  // 3 byte encoding
                        {
                            byte1 = (byte)(0 | (dist >> 8));
                            byte2 = (byte)(dist & 0xFF);
                            buf[bufPos++] = byte1;
                            buf[bufPos++] = byte2;
                            // maximum runlength for 3 byte encoding
                            if (numBytes > MAX_RUNLEN)
                                numBytes = MAX_RUNLEN;
                            byte3 = (byte)(numBytes - 0x12);
                            buf[bufPos++] = byte3;
                        }
                        else  // 2 byte encoding
                        {
                            byte1 = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                            byte2 = (byte)(dist & 0xFF);
                            buf[bufPos++] = byte1;
                            buf[bufPos++] = byte2;
                        }
                        srcPos += numBytes;
                    }

                    validBitCount++;

                    // write eight codes
                    if (validBitCount == 8)
                    {
                        bw.Write(currCodeByte);
                        for (int j = 0; j < bufPos; j++)
                            bw.Write(buf[j]);

                        currCodeByte = 0;
                        validBitCount = 0;
                        bufPos = 0;
                    }
                }

                if (validBitCount > 0)
                {
                    bw.Write(currCodeByte);
                    for (int j = 0; j < bufPos; j++)
                        bw.Write(buf[j]);

                    currCodeByte = 0;
                    validBitCount = 0;
                    bufPos = 0;
                }

                bw.Position = 4;
                bw.Write(src.Length);

                return ms.ToArray().Take((int)ms.Length).ToArray();
            }
        }

        private static int nintendoEnc_numBytes1 = 0;
        private static int nintendoEnc_matchPos;
        private static int nintendoEnc_prevFlag = 0;
        // a lookahead encoding scheme for ngc Yaz0
        private static int nintendoEnc(byte[] src, int pos, out int pMatchPos)
        {
            int numBytes = 1;
            //static int numBytes1;
            //static int matchPos;
            //static int prevFlag = 0;

            // if prevFlag is set, it means that the previous position
            // was determined by look-ahead try.
            // so just use it. this is not the best optimization,
            // but nintendo's choice for speed.
            if (nintendoEnc_prevFlag == 1)
            {
                pMatchPos = nintendoEnc_matchPos;
                nintendoEnc_prevFlag = 0;
                return nintendoEnc_numBytes1;
            }

            nintendoEnc_prevFlag = 0;
            numBytes = simpleEnc(src, pos, out nintendoEnc_matchPos);
            pMatchPos = nintendoEnc_matchPos;

            // if this position is RLE encoded, then compare to copying 1 byte and next position(pos+1) encoding
            if (numBytes >= 3)
            {
                nintendoEnc_numBytes1 = simpleEnc(src, pos + 1, out nintendoEnc_matchPos);
                // if the next position encoding is +2 longer than current position, choose it.
                // this does not guarantee the best optimization, but fairly good optimization with speed.
                if (nintendoEnc_numBytes1 >= numBytes + 2)
                {
                    numBytes = 1;
                    nintendoEnc_prevFlag = 1;
                }
            }
            return numBytes;
        }
        private static int simpleEnc(byte[] src, int pos, out int pMatchPos)
        {
            int numBytes = 1;
            int matchPos = 0;

            int startPos = pos - 0x1000;
            int end = src.Length - pos;

            if (startPos < 0)
                startPos = 0;

            // maximum runlength for 3 byte encoding
            if (end > MAX_RUNLEN)
                end = MAX_RUNLEN;

            for (int i = startPos; i < pos; i++)
            {
                int j;

                for (j = 0; j < end; j++)
                {
                    if (src[i + j] != src[j + pos])
                        break;
                }
                if (j > numBytes)
                {
                    numBytes = j;
                    matchPos = i;
                }
            }

            pMatchPos = matchPos;

            if (numBytes == 2)
                numBytes = 1;

            return numBytes;
        }
    }
}
