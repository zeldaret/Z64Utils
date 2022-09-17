using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Z64
{

    [Serializable]
    public class Z64MemoryException : Exception
    {
        public Z64MemoryException() { }
        public Z64MemoryException(string message) : base(message) { }
        public Z64MemoryException(string message, Exception inner) : base(message, inner) { }
        protected Z64MemoryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class Z64Memory
    {
        private struct MemBlock
        {
            public uint Vram;
            public int Vrom;
            public MemBlock(uint vram, int vrom)
            {
                Vram = vram;
                Vrom = vrom;
            }
        }

        Z64Game _game;
        List<MemBlock> _blocks;

        public Z64Memory(Z64Game game)
        {
            _game = game;
            _blocks = new List<MemBlock>();

            try
            {
                if (game.GetVrom("boot", out int vrom))
                    _blocks.Add(new MemBlock(game.Rom.EntryPoint + 0x60, vrom));

                if (game.Version.Memory.CodeVram.HasValue && game.GetVrom("code", out vrom))
                    _blocks.Add(new MemBlock(game.Version.Memory.CodeVram.Value, vrom));

                LoadOvls();
            }
            catch (Exception)
            {
                throw new Z64MemoryException("Error while creating the memory map. Please check your config files (versions/*.json)");
            }
        }

        private void LoadOvls()
        {
            //GameStates
            int count = _game.Version.Game switch
            {
                Z64GameType.Oot => 6,
                Z64GameType.Mm => 7,
                _ => 0,
            };
            if (_game.Version.Memory.GameStateTable.HasValue)
                LoadOvlTable(_game.Version.Memory.GameStateTable.Value, count, 0x30, 4, 0xC);

            //Actors
            count = _game.Version.Game switch
            {
                Z64GameType.Oot => 471,
                Z64GameType.Mm => 690,
                _ => 0,
            };
            if (_game.Version.Memory.ActorTable.HasValue)
                LoadOvlTable(_game.Version.Memory.ActorTable.Value, count, 0x20, 0, 8);

            //EffectSS2
            count = _game.Version.Game switch
            {
                Z64GameType.Oot => 37,
                Z64GameType.Mm => 39,
                _ => 0,
            };
            if (_game.Version.Memory.EffectTable.HasValue)
                LoadOvlTable(_game.Version.Memory.EffectTable.Value, count, 0x1C, 0, 8);

            //KaleidoMgr
            if (_game.Version.Memory.KaleidoMgrTable.HasValue)
                LoadOvlTable(_game.Version.Memory.KaleidoMgrTable.Value, 2, 0x1C, 4, 0xC);

            //map_mark_data
            if (_game.Version.Memory.MapMarkDataOvl.HasValue)
                LoadOvlTable(_game.Version.Memory.MapMarkDataOvl.Value, 1, 0x18, 4, 0xC);

            //FBDemo
            if (_game.Version.Memory.FBDemoTable.HasValue)
                LoadOvlTable(_game.Version.Memory.FBDemoTable.Value, 7, 0x1C, 0xC, 4);
        }

        private void LoadOvlTable(uint tableAddr, int count, int entrySize, int vromOff, int vramOff)
        {
            byte[] data = ReadBytes(tableAddr, count*entrySize);

            for (int off = 0; off < count*entrySize; off += entrySize)
            {
                uint entryVram = Utils.BomSwap(BitConverter.ToUInt32(data, off + vramOff));
                int entryVrom = (int)Utils.BomSwap(BitConverter.ToUInt32(data, off + vromOff));
                if (entryVram != 0)
                    _blocks.Add(new MemBlock(entryVram, entryVrom));
            }
        }

        public byte[] ReadBytes(uint addr, int count)
        {
            byte[] ret = new byte[count];

            foreach (var block in _blocks)
            {
                var file = _game.GetFile(block.Vrom);
                if (addr >= block.Vram && addr < block.Vram + file.Data.Length)
                {
                    if (addr+count > block.Vram + file.Data.Length)
                        throw new Z64MemoryException($"Could not read 0x{count:X} bytes at address 0x{addr:X8}");

                    Buffer.BlockCopy(file.Data, (int)(addr - block.Vram), ret, 0, count);
                    return ret;
                }
            }
            throw new Z64MemoryException($"Could not read 0x{count:X} bytes at address 0x{addr:X8}");
        }
        public bool VromToVram(uint vrom, out uint vram)
        {
            foreach (var block in _blocks)
            {
                var file = _game.GetFile(block.Vrom);
                if (vrom >= block.Vrom && vrom < block.Vrom + file.Data.Length)
                {
                    vram = block.Vram + (vrom - (uint)block.Vrom);
                    return true;
                }
            }

            vram = 0;
            return false;
        }
        public bool VramToVrom(uint vram, out uint vrom)
        {
            foreach (var block in _blocks)
            {
                var file = _game.GetFile(block.Vrom);
                if (vram >= block.Vram && vram < block.Vram + file.Data.Length)
                {
                    vrom = (uint)block.Vrom + (vram - block.Vram);
                    return true;
                }
            }

            vrom = 0;
            return false;
        }

    }
}
