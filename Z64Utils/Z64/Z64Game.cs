using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Syroot.BinaryData;
using N64;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using Common;

namespace Z64
{

    [Serializable]
    public class Z64GameException : Exception
    {
        public Z64GameException() { }
        public Z64GameException(string message) : base(message) { }
        public Z64GameException(string message, Exception inner) : base(message, inner) { }
        protected Z64GameException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public class Z64File
    {
        public bool Compressed { get; set; }
        public int VRomStart { get; set; }
        public int VRomEnd { get; set; }
        public int RomStart { get; set; }
        public int RomEnd { get; set; }
        public byte[] Data { get; set; }
        public bool Deleted { get; set; }

        public Z64File()
        {

        }
        public Z64File(byte[] data, int vrom, int romStart, int romEnd, bool comp)
        {
            Data = data;
            VRomStart = vrom;
            VRomEnd = Data != null ? vrom + data.Length : vrom;
            RomStart = romStart;
            RomEnd = romEnd;
            Compressed = comp;
            Deleted = false;

        }
        public static Z64File DeletedFile(int vrom, int rom, int size)
        {
            return new Z64File()
            {
                Data = new byte[size],
                VRomStart = vrom,
                VRomEnd = vrom + size,
                RomStart = rom,
                RomEnd = rom,
                Compressed = false,
                Deleted = true,
            };
        }

        public bool Valid()
        {
            return Data != null;
        }
    }

    public class Z64Game
    {
        private struct DmadataEntry
        {
            public int VRomStart;
            public int VRomEnd;
            public int RomStart;
            public int RomEnd;

            public bool Valid()
            {
                return (VRomStart != 0 || VRomEnd != 0 || RomStart != 0 || RomEnd != 0);
            }
            public bool Exist()
            {
                return RomStart != -1 && RomEnd != -1;
            }

            public bool Compressed()
            {
                return RomEnd != 0;
            }

            public DmadataEntry(BinaryStream br)
            {
                VRomStart = br.ReadInt32();
                VRomEnd = br.ReadInt32();
                RomStart = br.ReadInt32();
                RomEnd = br.ReadInt32();
            }

            public int GetSize()
            {
                if (!Valid() || !Exist()) return 0;
                return Compressed() ? RomEnd - RomStart : VRomEnd - VRomStart;
            }

            public Z64File ToFile(Z64Game game)
            {
                if (!Valid())
                    return new Z64File(null, -1, -1, -1, false);

                if (!Exist())
                    return Z64File.DeletedFile(VRomStart, RomStart, VRomEnd - VRomStart);

                int len = GetSize();

                byte[] data = new byte[len];
                Buffer.BlockCopy(game.Rom.RawRom, RomStart, data, 0, len);

                int romEnd = RomStart + data.Length;

                if (Compressed())
                    data = game.Decompress(data, VRomEnd - VRomStart);

                return new Z64File(data, VRomStart, RomStart, romEnd, Compressed());
            }

        }

        public Z64Memory Memory { get; private set; }
        public N64Rom Rom { get; private set; }
        public Z64Version Version { get; set; }

        private List<Z64File> _files;

        public Z64Game(N64Rom rom, Action<float, string> progressCalback = null)
        {
            Rom = rom;

            Version = Z64Version.IdentifyRom(rom, out int fileTableOff);
            if (Version == null)
                throw new Z64GameException("Could not identify build. Please check your config files.");

            if (!N64CheckSum.Validate(Rom, Version.Cic))
                throw new Exception("Invalid CRC");

            using (MemoryStream ms = new MemoryStream(rom.RawRom))
            {
                BinaryStream br = new BinaryStream(ms, ByteConverter.Big);
                br.Position = fileTableOff;

                GetFs(br, progressCalback);
            }

            Z64Version.ProcessGame(this);

            Memory = new Z64Memory(this);

        }
        public Z64Game(string path, Action<float, string> progressCalback = null) : this(new N64Rom(path), progressCalback)
        {

        }

        
        public byte[] Compress(byte[] data)
        {
            return Version.Compression == Z64FileCompression.Yaz0
                ? Yaz0.Compress(data)
                : throw new NotImplementedException();
        }
        public byte[] Decompress(byte[] data, int decSize)
        {
            return Version.Compression == Z64FileCompression.Yaz0
                ? Yaz0.Decompress(data)
                : Utils.ZlibDecompress(data, decSize);
        }


        public string GetFileName(int vrom) => Version.GetFileName(vrom) ?? "";

        public Z64FileType GetFileType(int vrom) => Version.GetFileType(vrom);

        public bool GetVrom(string name, out int vrom)
        {
            var fileVrom = Version.GetVrom(name);
            vrom = fileVrom ?? 0;
            return fileVrom.HasValue;
        }
        public int GetVrom(string name) => Version.GetVrom(name) ?? throw new Exception();

        public Z64File GetFile(int vrom) => _files.Find((f) => f.VRomStart == vrom);

        public Z64File GetFileFromIndex(int index) => _files[index];

        public void InjectFile(int vrom, byte[] data)
        {
            var file = GetFile(vrom);
            if (file == null)
                throw new Z64GameException("Invalid VROM");

            int oldSize = file.Compressed ? file.RomEnd - file.RomStart : file.Data.Length;

            var restStart = file.RomEnd != 0
                ? file.RomEnd
                : file.RomStart + file.Data.Length;

            //find rom end
            Z64File last = null;
            for (int i = 0; i < GetFileCount(); i++)
            {
                var iter = GetFileFromIndex(i);

                if (iter.Valid() && !iter.Deleted && (last == null || iter.RomStart > last.RomStart))
                    last = iter;
            }
            if (last == null)
                throw new Exception("?");

            var restEnd = last.RomEnd;

            //save rest
            byte[] rest = new byte[restEnd - restStart];
            Buffer.BlockCopy(Rom.RawRom, restStart, rest, 0, rest.Length);

            var encData = file.Compressed ? Compress(data) : data;
            int off = encData.Length - oldSize;

            //copy new file in rom
            Buffer.BlockCopy(encData, 0, Rom.RawRom, file.RomStart, encData.Length);

            file.Data = data;
            file.VRomEnd = (file.VRomStart + data.Length) + 0xF & ~0xF;
            if (file.Compressed)
            {
                file.RomEnd = (file.RomStart + encData.Length) + 0xF & ~0xF;
            }

            //copy rest
            Buffer.BlockCopy(rest, 0, Rom.RawRom, file.RomEnd, rest.Length);

            //offset each file rom addresses
            for (int i = _files.IndexOf(file) + 1; i < _files.Count; i++)
            {
                _files[i].RomStart = (_files[i].RomStart + off) + 0xF & ~0xF;
                _files[i].RomEnd = (_files[i].RomEnd + off) + 0xF & ~0xF;
            }

            file.Data = data;
            file.Deleted = false;
        }


        private void FixDmaDataTable()
        {
            var dmatable = GetFile(GetVrom("dmadata"));

            byte[] newTable;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryStream bw = new BinaryStream(ms, ByteConverter.Big);
                foreach (var file in _files)
                {
                    bw.Write(file.VRomStart);
                    bw.Write(file.VRomEnd);
                    bw.Write(file.Deleted ? -1 : file.RomStart);
                    bw.Write(file.Deleted ? -1 : (file.Compressed ? file.RomEnd : 0));
                }
                newTable = ms.GetBuffer().Take((int)ms.Length).ToArray();
            }
            if (newTable.Length != dmatable.Data.Length)
                throw new Exception("dmadata size missmatch??");

            Buffer.BlockCopy(newTable, 0, Rom.RawRom, dmatable.RomStart, newTable.Length);
            dmatable.Data = newTable;
        }

        public int GetFileCount()
        {
            return _files.Count;
        }


        public void ExtractFiles(string dir, Action<float, string> progressCalback = null)
        {
            for (int i = 0; i < _files.Count; i++)
            {
                string filename = GetFileName(_files[i].VRomStart);
                progressCalback?.Invoke((float)i / _files.Count, $"Extracting files... [{i}/{_files.Count}] \"{filename}\"");
                if (_files[i].Valid())
                    File.WriteAllBytes($"{dir}/{filename}.bin", _files[i].Data);
            }
        }

        public void FixRom()
        {
            FixDmaDataTable();
            N64CheckSum.Update(Rom, Version.Cic);
        }

        private void GetFs(BinaryStream br, Action<float, string> progressCalback = null)
        {
            _files = new List<Z64File>();
            int filecount = 3; //dmadata file

            DateTime lastProgressUpdate = DateTime.Now - new TimeSpan(1, 0, 0); // one hour ago
            int lastprogressI = 0;

            for (int i = 0; i < filecount; i++)
            {
                DmadataEntry entry = new DmadataEntry(br);

                DateTime now = DateTime.Now;
                // only update progress if 10% of the files were processed,
                // or 100 milliseconds elapsed, since the last progress update
                if (((float)(i - lastprogressI) / filecount) > 0.1
                    || (now - lastProgressUpdate).TotalMilliseconds > 100)
                {
                    if (i <= 2)
                        progressCalback?.Invoke(0, $"Processing files... [{i}/?] \"{GetFileName(entry.VRomStart)}\"");
                    else
                        progressCalback?.Invoke((float)i / filecount, $"Processing files... [{i}/{filecount}] \"{GetFileName(entry.VRomStart)}\"");
                    lastProgressUpdate = now;
                    lastprogressI = i;
                }

                var file = entry.ToFile(this);
                _files.Add(file);
                if (entry.Valid() && entry.Exist())
                {
                    if (i == 2) //dmadata
                    {
                        filecount = file.Data.Length / 0x10;

                        lastprogressI = -filecount; // force a progress update
                    }
                }
            }

            progressCalback?.Invoke(1, $"Done processing {filecount} files.");
        }

    }
}
