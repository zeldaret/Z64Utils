using System;
using System.Collections.Generic;
using System.Text;
using RDP;
using Z64;

namespace F3DZEX
{
    public class Memory
    {
        public enum SegmentType
        {
            Empty,
            Fill,
            Buffer,
            Vram,
        }

        public class Segment
        {
            public const int COUNT = 16;

            public SegmentType Type;

            public byte[] Data { get; set; }
            public uint Address { get; set; }
            public string Label { get; set; }

            public static Segment Empty() =>
                new Segment()
                {
                    Type = SegmentType.Empty,
                    Data = null,
                    Address = 0,
                    Label = "[Empty]",
                };

            public static Segment FromVram(string label, uint addr) =>
                new Segment()
                {
                    Type = SegmentType.Vram,
                    Data = null,
                    Address = addr,
                    Label = label,
                };

            public static Segment FromBytes(string label, byte[] data) =>
                new Segment()
                {
                    Type = SegmentType.Buffer,
                    Data = data,
                    Address = 0,
                    Label = label,
                };

            public static Segment FromFill(string label, byte[] pattern = null) =>
                new Segment()
                {
                    Type = SegmentType.Fill,
                    Data = pattern ?? new byte[1],
                    Address = 0,
                    Label = label,
                };
        }

        public Segment[] Segments { get; private set; }

        Z64Game _game;

        public Memory(Z64Game game)
        {
            _game = game;

            Segments = new Segment[Segment.COUNT];
            for (int i = 0; i < Segment.COUNT; i++)
                Segments[i] = Segment.Empty();
        }

        public SegmentedAddress ResolveAddress(uint vaddr) => ResolveAddress(vaddr, out _);

        public SegmentedAddress ResolveAddress(uint vaddr, out string path)
        {
            SegmentedAddress addr = new SegmentedAddress(vaddr);
            path = $"{vaddr:X8}";

            // resolve address
            int resolveCount = 0;
            while (addr.Segmented && Segments[addr.SegmentId].Type == SegmentType.Vram)
            {
                if (
                    resolveCount > 16
                    || new SegmentedAddress(Segments[addr.SegmentId].Address).SegmentId
                        == addr.SegmentId
                )
                    throw new Exception($"Could not resolve address 0x{vaddr:X}. Path: {path}");

                path += $" -> {Segments[addr.SegmentId].Label}+0x{addr.SegmentOff:X}";
                addr = new SegmentedAddress(Segments[addr.SegmentId].Address + addr.SegmentOff);
                resolveCount++;
            }
            return addr;
        }

        public byte[] ReadBytes(SegmentedAddress addr, int count) => ReadBytes(addr.VAddr, count);

        public byte[] ReadBytes(uint vaddr, int count)
        {
            SegmentedAddress addr = ResolveAddress(vaddr, out string path);

            // read data
            try
            {
                if (addr.Segmented)
                {
                    var seg = Segments[addr.SegmentId];
                    switch (seg.Type)
                    {
                        case SegmentType.Fill:
                        {
                            byte[] buff = new byte[count];
                            int rest = count;
                            int dstOff = 0;
                            while (rest > 0)
                            {
                                int srcOff = ((int)addr.SegmentOff + dstOff) % seg.Data.Length;
                                int curCount = Math.Min(seg.Data.Length - srcOff, rest);
                                System.Buffer.BlockCopy(
                                    seg.Data,
                                    srcOff,
                                    buff,
                                    count - rest,
                                    curCount
                                );
                                rest -= curCount;
                                dstOff += curCount;
                            }
                            return buff;
                        }
                        case SegmentType.Buffer:
                            if (addr.SegmentOff + count <= seg.Data.Length)
                            {
                                byte[] buff = new byte[count];
                                System.Buffer.BlockCopy(
                                    seg.Data,
                                    (int)addr.SegmentOff,
                                    buff,
                                    0,
                                    count
                                );
                                return buff;
                            }
                            break;
                        case SegmentType.Vram:
                            if (_game != null)
                                return _game.Memory.ReadBytes(seg.Address + addr.SegmentOff, count);
                            break;
                        case SegmentType.Empty:
                        default:
                            break;
                    }
                }
                else if (_game != null)
                {
                    return _game.Memory.ReadBytes(vaddr, count);
                }
            }
            catch { }
            throw new Exception($"Could not read 0x{count:X} bytes at address {path}");
        }
    }
}
