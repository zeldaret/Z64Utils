using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using N64;

#nullable enable

namespace Z64
{
    public enum Z64FileType
    {
        Unknow,
        Code,
        Object,
        Room,
        Scene,
    }

    public enum Z64GameType
    {
        Unknow,
        Oot,
        Mm,
        Af,
    }

    public enum Z64FileCompression
    {
        Yaz0,
        Zlib,
    }

    public class Z64Version
    {
        #region JSON Data Type

        private class AddrToStringConverter : JsonConverter<uint?>
        {
            public override uint? Read(
                ref Utf8JsonReader reader,
                Type type,
                JsonSerializerOptions options
            )
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (
                        uint.TryParse(
                            reader.GetString(),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out uint number
                        )
                    )
                        return number;
                }
                if (reader.TokenType == JsonTokenType.Null)
                    return null;

                return reader.GetUInt32();
            }

            public override void Write(
                Utf8JsonWriter writer,
                uint? v,
                JsonSerializerOptions options
            )
            {
                writer.WriteStringValue(v.HasValue ? v.Value.ToString("X8") : "null");
            }
        }

        public class VersionIdentifier
        {
            [JsonPropertyName("build_team")]
            public string? BuildTeam { get; set; }

            [JsonPropertyName("build_date")]
            public string? BuildDate { get; set; }

            [JsonPropertyName("rom_name")]
            public string? RomName { get; set; }

            [JsonPropertyName("rom_code")]
            public string? RomCode { get; set; }

            [JsonPropertyName("rom_version")]
            public byte? RomVersion { get; set; }

            [MemberNotNull(nameof(BuildTeam))]
            [MemberNotNull(nameof(BuildDate))]
            [MemberNotNull(nameof(RomName))]
            [MemberNotNull(nameof(RomCode))]
            public void AssertValid()
            {
                Debug.Assert(BuildTeam != null);
                Debug.Assert(BuildDate != null);
                Debug.Assert(RomName != null);
                Debug.Assert(RomCode != null);
            }
        }

        public class MemoryInfo
        {
            [JsonPropertyName("code")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? CodeVram { get; set; }

            [JsonPropertyName("actor_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? ActorTable { get; set; }

            [JsonPropertyName("gamestate_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? GameStateTable { get; set; }

            [JsonPropertyName("effect_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? EffectTable { get; set; }

            [JsonPropertyName("kaleido_mgr_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? KaleidoMgrTable { get; set; }

            [JsonPropertyName("map_mark_data_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? MapMarkDataOvl { get; set; } // specific to oot

            [JsonPropertyName("fbdemo_table")]
            [JsonConverter(typeof(AddrToStringConverter))]
            public uint? FBDemoTable { get; set; } // specific to mm
        }

        public class FileEntry
        {
            [JsonConverter(typeof(AddrToStringConverter))]
            [JsonPropertyName("vrom")]
            public uint? Vrom { get; set; }

            [JsonPropertyName("name")]
            public string? Filename { get; set; }

            [JsonConverter(typeof(JsonStringEnumConverter))]
            [JsonPropertyName("type")]
            public Z64FileType FileType { get; set; }
        }

        #endregion JSON Data Type

        #region JSON Properties

        [JsonPropertyName("version_name")]
        public string? VersionName { get; set; }

        [JsonPropertyName("version_game")]
        public Z64GameType Game { get; set; }

        [JsonPropertyName("compression_method")]
        public Z64FileCompression Compression { get; set; }

        [JsonPropertyName("cic")]
        public int Cic { get; set; }

        [JsonPropertyName("identifier")]
        public VersionIdentifier? Identifier { get; set; }

        [JsonPropertyName("memory")]
        public MemoryInfo Memory { get; set; }

        [JsonPropertyName("files")]
        public List<FileEntry> Files { get; set; }

        [JsonConstructor]
        public Z64Version()
        {
            Memory = new MemoryInfo();
            Files = new List<FileEntry>();
            Compression = Z64FileCompression.Yaz0;
            Cic = 6105;
        }

        [MemberNotNull(nameof(VersionName))]
        [MemberNotNull(nameof(Identifier))]
        public void AssertValid()
        {
            Debug.Assert(VersionName != null);
            Debug.Assert(Identifier != null);
        }

        #endregion JSON Properties

        public int? GetVrom(string filename) =>
            (int?)Files.Find(f => f.Filename == filename)?.Vrom ?? null;

        public string GetFileName(int vrom) => Files.Find(f => f.Vrom == vrom)?.Filename ?? "";

        public Z64FileType GetFileType(int vrom) =>
            Files.Find(f => f.Vrom == vrom)?.FileType ?? Z64FileType.Unknow;

        public void RenameFile(int vrom, string name)
        {
            var file = Files.Find(f => f.Vrom == vrom);
            if (file == null)
                Files.Add(
                    new FileEntry()
                    {
                        Filename = name,
                        Vrom = (uint)vrom,
                        FileType = GuessFileType(name),
                    }
                );
            else
                file.Filename = name;
            Save();
        }

        private int FindBuildTeam(N64Rom rom)
        {
            AssertValid();
            Identifier.AssertValid();

            string team = Identifier.BuildTeam;
            int start = 0x1000;
            int end = 0x40000 - team.Length - Identifier.BuildDate.Length;

            for (int i = start; i < end; i += 4)
            {
                int count = 0;
                while (
                    count < team.Length && i + count < end && rom.RawRom[i + count] == team[count]
                )
                    count++;

                if (count >= team.Length)
                    return i;
            }
            return -1;
        }

        public bool Match(N64Rom rom, out int fileTableOff)
        {
            AssertValid();
            Identifier.AssertValid();

            fileTableOff = 0;

            // search team
            int off = FindBuildTeam(rom);
            if (off < 0)
                return false;

            // build team string
            off += Identifier.BuildTeam.Length + 1;
            off = off + 3 & ~3; // string padding

            // check date
            string date = Identifier.BuildDate;
            int count = 0;
            while (
                off + count < rom.RawRom.Length
                && count < date.Length
                && date[count] == rom.RawRom[off + count]
            )
                count++;

            if (count < date.Length)
                return false;

            // build date string
            off += Identifier.BuildDate.Length + 1;
            off = off + 3 & ~3; // string padding

            // build option string
            off++;
            off = off + 3 & ~3; // string padding

            // file boundary
            off = off + 0xF & ~0xF;

            fileTableOff = off;
            return true;
        }

        public void Save()
        {
            var options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());

            Debug.Assert(_versions != null);
            string path = _versions.First(v => v.Value == this).Key;

            // sort entries by vrom
            Files.Sort((a, b) => a.Vrom < b.Vrom ? -1 : 1);

            // remove duplicates
            for (int i = 0; i < Files.Count; i++)
            for (int j = i + 1; j < Files.Count && Files[j].Vrom == Files[i].Vrom; j++)
                Files.RemoveAt(j);

            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(path, json);
        }

        public static Z64Version? IdentifyRom(N64Rom rom, out int fileTableOff)
        {
            LoadVersions();
            foreach (var ver in _versions)
            {
                if (ver.Value.Match(rom, out fileTableOff))
                {
                    return ver.Value;
                }
            }

            fileTableOff = 0;
            return null;
        }

        private static Dictionary<string, Z64Version>? _versions = null;

        [MemberNotNull(nameof(_versions))]
        private static void LoadVersions()
        {
            string path = "versions";
            if (!Directory.Exists(path))
                throw new Exception($"Could load config files (\"{path}/*.json\").");

            var files = Directory.GetFiles(path, "*.json");
            if (files.Length == 0)
                throw new Exception($"Could load config files (\"{path}/*.json\").");

            _versions = new();
            var options = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());

            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var ver = JsonSerializer.Deserialize<Z64Version>(json, options);
                Debug.Assert(ver != null);
                ver.AssertValid();
                ver.Identifier.AssertValid();

                var existing = _versions
                    .ToList()
                    .FindAll(v =>
                    {
                        v.Value.AssertValid();
                        return v.Value.Identifier.BuildDate == ver.Identifier.BuildDate;
                    });
                if (existing.Count > 0)
                    throw new Exception($"build date conflict with \"{existing[0].Key}\"");

                if (
                    ver.Identifier == null
                    || ver.Identifier.BuildDate == null
                    || ver.Identifier.BuildTeam == null
                )
                    throw new InvalidDataException(
                        $"Error loading \"{file}\": identifier.build_date or identifier.build_team missing"
                    );

                ver.Files.RemoveAll(f => string.IsNullOrEmpty(f.Filename));

                _versions.Add(file, ver);
            }
        }

        private class FileHashEntry
        {
            public string fileName;
            public string sha256;
            public Z64FileType type;

            public FileHashEntry(string line)
            {
                var parts = line.Split("|");
                sha256 = parts[0];
                type = Enum.Parse<Z64FileType>(parts[1]);
                fileName = parts[2];
            }

            public FileHashEntry(Z64Game game, Z64File file)
            {
                if (!file.Valid())
                    throw new Exception("file is not valid");
                fileName = game.GetFileName(file.VRomStart);
                sha256 = Utils.BytesToHex(
                    System.Security.Cryptography.SHA256.Create().ComputeHash(file.Data),
                    ""
                );
                type = game.GetFileType(file.VRomStart);
            }

            public static List<FileHashEntry> ReadEntries(string path)
            {
                List<FileHashEntry> ret = new List<FileHashEntry>();
                if (!File.Exists(path))
                    return ret;

                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                    ret.Add(new FileHashEntry(line));
                return ret;
            }

            public static void WriteEntries(string path, List<FileHashEntry> entries)
            {
                string[] lines = new string[entries.Count];
                for (int i = 0; i < lines.Length; i++)
                    lines[i] = entries[i].ToString();
                File.WriteAllLines(path, lines);
            }

            public override string ToString() => $"{sha256}|{type}|{fileName}";
        }

        public static void ProcessGame(Z64Game game)
        {
            ExportHashes(game);
            ImportHashes(game);
        }

        private static Z64FileType GuessFileType(string name)
        {
            if (name == "code" || name == "boot" || name.StartsWith("ovl_"))
                return Z64FileType.Code;

            if (name.StartsWith("object_") || name.StartsWith("gameplay_"))
                return Z64FileType.Object;

            if (name.EndsWith("_scene"))
                return Z64FileType.Scene;

            if (name.Contains("_room_"))
                return Z64FileType.Room;

            return Z64FileType.Unknow;
        }

        public static void ImportFileList(Z64Game game, string path)
        {
            var lines = File.ReadAllLines(path);

            game.Version.Files.Clear();

            for (int i = 0; i < game.GetFileCount(); i++)
            {
                var file = game.GetFileFromIndex(i);
                if (!file.Valid())
                    continue;

                if (!string.IsNullOrEmpty(lines[i]))
                {
                    game.Version.Files.Add(
                        new FileEntry()
                        {
                            Filename = lines[i],
                            FileType = GuessFileType(lines[i]),
                            Vrom = (uint)file.VRomStart,
                        }
                    );
                }
            }

            game.Version.Save();
            ExportHashes(game);
        }

        public static void ExportFileList(Z64Game game, string path)
        {
            StringWriter sw = new StringWriter();

            for (int i = 0; i < game.GetFileCount(); i++)
            {
                var file = game.GetFileFromIndex(i);

                sw.WriteLine(game.Version.GetFileName(file.VRomStart));
            }

            File.WriteAllText(path, sw.ToString());
        }

        private static void ExportHashes(Z64Game game)
        {
#if DEBUG
            string hashPath = "hashes.txt";
            var entries = FileHashEntry.ReadEntries(hashPath);
            int addCount = 0;
            int modifCount = 0;

            for (int i = 0; i < game.GetFileCount(); i++)
            {
                var file = game.GetFileFromIndex(i);
                if (file.Valid() && !file.Deleted)
                {
                    string name = game.GetFileName(file.VRomStart);
                    if (string.IsNullOrEmpty(name))
                        continue;

                    if (file.Data.Length <= 0x10)
                    {
                        Debug.WriteLine($"Skipping small file {name}");
                        continue;
                    }

                    FileHashEntry entry = new FileHashEntry(game, file);

                    var existing = entries.Find(f => f.sha256 == entry.sha256);

                    if (existing != null)
                    {
                        if (existing.fileName != entry.fileName)
                        {
                            Debug.WriteLine(
                                $"name missmatch : {existing.fileName} ({existing.sha256}) -> {entry.fileName}"
                            );
                        }
                        else if (
                            entry.type != Z64FileType.Unknow
                            && existing.type == Z64FileType.Unknow
                        )
                        {
                            Debug.WriteLine($"new type found : {existing.type} -> {entry.type}");
                            entries.Remove(existing);
                            entries.Add(entry);
                            modifCount++;
                        }
                    }
                    else
                    {
                        entries.Add(entry);
                        addCount++;
                    }
                }
            }

            FileHashEntry.WriteEntries(hashPath, entries);

            Debug.WriteLine($"{addCount} hash exported and {modifCount} hashes modifed!");
#endif
        }

        private static void ImportHashes(Z64Game game)
        {
#if DEBUG
            var entries = FileHashEntry.ReadEntries("hashes.txt");

            int foundCount = 0;

            for (int i = 0; i < game.GetFileCount(); i++)
            {
                var file = game.GetFileFromIndex(i);
                if (file.Deleted || !file.Valid())
                    continue;

                string name = game.GetFileName(file.VRomStart);
                var type = game.GetFileType(file.VRomStart);
                string sha = Utils.BytesToHex(
                    System.Security.Cryptography.SHA256.Create().ComputeHash(file.Data),
                    ""
                );

                if (string.IsNullOrEmpty(name))
                {
                    var found = entries.Find(e => e.sha256 == sha);
                    if (found != null)
                    {
                        name = found.fileName;
                        type = found.type;
                        foundCount++;
                    }
                }

                name = i switch
                {
                    0 => "makerom",
                    1 => "boot",
                    2 => "dmadata",
                    _ => name,
                };

                if (!string.IsNullOrEmpty(name))
                {
                    game.Version.Files.Add(
                        new FileEntry()
                        {
                            Filename = name,
                            FileType = type,
                            Vrom = (uint)file.VRomStart,
                        }
                    );
                }
            }

            Debug.WriteLine($"{foundCount} hashes imported!");

            game.Version.Save();
#endif
        }
    }
}
