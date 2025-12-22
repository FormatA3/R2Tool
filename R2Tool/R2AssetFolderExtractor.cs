using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace R2SaveEditor
{
    public static class R2AssetFolderExtractor
    {
        private const uint ID_ResourceLighting = 0x0001DB00;
        private const uint ID_ResourceZones = 0x0001DA00;
        private const uint ID_ResourceAnimsets = 0x0001D700;
        private const uint ID_ResourceMobys = 0x0001D600;
        private const uint ID_ResourceShrubs = 0x0001D500;
        private const uint ID_ResourceTies = 0x0001D300;
        private const uint ID_ResourceCubemaps = 0x0001D200;
        private const uint ID_ResourceCinematics = 0x0001D800;

        private const uint Lookup_MobyPath = 0x0000D200;
        private const uint Lookup_TiePath = 0x00003410;
        private const uint Lookup_ShrubPath = 0x0000B700;
        private const uint Lookup_CinematicPath = 0x00017D00;
        private const uint MAGIC_IGHW = 0x49474857;

        public sealed class FolderExtractResult
        {
            public string OutputDirectory { get; set; }
            public string Notes { get; set; }
        }
        private struct ResourceEntry
        {
            public uint Hash1;
            public uint Hash2;
            public uint Offset;
            public uint Size;

            public string HashName => $"{Hash1:X8}{Hash2:X8}";
        }
        public static FolderExtractResult ExtractFromFolder(string folder, string outRoot)
        {
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentNullException(nameof(folder));
            if (!Directory.Exists(folder)) throw new DirectoryNotFoundException(folder);

            string assetlookupPath = Path.Combine(folder, "assetlookup.dat");
            if (!File.Exists(assetlookupPath))
                throw new FileNotFoundException("assetlookup.dat introuvable dans le dossier.", assetlookupPath);

            Directory.CreateDirectory(outRoot);
            string outDir = Path.Combine(outRoot, new DirectoryInfo(folder).Name);
            Directory.CreateDirectory(outDir);

            var lookup = AssetLookup.Load(assetlookupPath);

            ExtractGroup(folder, outDir, lookup, ID_ResourceMobys, "mobys", "mobys.dat", Lookup_MobyPath);
            ExtractGroup(folder, outDir, lookup, ID_ResourceTies, "ties", "ties.dat", Lookup_TiePath);
            ExtractGroup(folder, outDir, lookup, ID_ResourceShrubs, "shrubs", "shrubs.dat", Lookup_ShrubPath);
            ExtractGroup(folder, outDir, lookup, ID_ResourceCinematics, "cinematics", "cinematics.dat", Lookup_CinematicPath);

            ExtractGroup(folder, outDir, lookup, ID_ResourceAnimsets, "animsets", "animsets.dat", lookupIdForName: null);
            ExtractGroup(folder, outDir, lookup, ID_ResourceZones, "zones", "zones.dat", lookupIdForName: null);
            ExtractGroup(folder, outDir, lookup, ID_ResourceLighting, "lighting", "lighting.dat", lookupIdForName: null);
            ExtractGroup(folder, outDir, lookup, ID_ResourceCubemaps, "cubemaps", "cubemaps.dat", lookupIdForName: null);

            return new FolderExtractResult
            {
                OutputDirectory = outDir,
                Notes = "Extraction terminée. Les dossiers mobys/ties/shrubs/cinematics tentent d’utiliser le lookup interne pour nommer les fichiers."
            };
        }
        private static void ExtractGroup(
            string folder,
            string outDir,
            AssetLookup lookup,
            uint resourceId,
            string groupName,
            string datFileName,
            uint? lookupIdForName)
        {
            if (!lookup.Resources.TryGetValue(resourceId, out var entries) || entries.Count == 0)
                return;

            string datPath = Path.Combine(folder, datFileName);
            if (!File.Exists(datPath))
                return;

            string groupOut = Path.Combine(outDir, groupName);
            Directory.CreateDirectory(groupOut);

            string listPath = Path.Combine(groupOut, "files_list.txt");
            using var list = new StreamWriter(listPath, false, Encoding.UTF8);

            using var fs = new FileStream(datPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e.Size == 0) continue;
                if ((long)e.Offset + e.Size > fs.Length) continue;

                string relPath;

                if (lookupIdForName.HasValue)
                    relPath = TryReadNameFromSubIghw(fs, e.Offset, lookupIdForName.Value);
                else
                    relPath = null;

                if (string.IsNullOrWhiteSpace(relPath))
                {
                    relPath = $"{groupName}/{e.Hash1:X8}.{e.Hash2:X8}.irb";
                    R2Form.AppLog.WriteLine?.Invoke($"[{groupName}] Pas de nom (lookup) pour entry {i}, fallback hash {e.HashName}");
                    relPath = NormalizeGroupPath(groupName, relPath);

                    if (groupName.Equals("mobys", StringComparison.OrdinalIgnoreCase))
                        relPath = EnsureEntityFolderLayout(relPath);
                }

                if (string.IsNullOrWhiteSpace(relPath))
                {
                    relPath = $"{groupName}/{e.Hash1:X8}.{e.Hash2:X8}.irb";
                }

                relPath = relPath.Replace('\\', '/').ToLowerInvariant();
                relPath = SanitizeRelativePathKeepDirs(relPath);

                string outPath = Path.Combine(outDir, relPath.Replace('/', Path.DirectorySeparatorChar));

                if (outPath.Length > 240)
                {
                    string compactDir = Path.Combine(outDir, groupName, "_longpaths");
                    Directory.CreateDirectory(compactDir);

                    string fileName = Path.GetFileName(outPath);
                    outPath = Path.Combine(compactDir, $"{i:D5}_{e.HashName}_{fileName}");
                    relPath = $"{groupName}/_longpaths/{Path.GetFileName(outPath)}";
                }


                Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

                fs.Position = e.Offset;
                using (var of = File.Create(outPath))
                    CopyExactly(fs, of, e.Size);

                list.WriteLine(relPath);

            }
        }

        private static string TryReadNameFromSubIghw(Stream datStream, uint blockOffset, uint lookupId)
        {
            long oldPos = datStream.Position;
            try
            {
                datStream.Position = blockOffset;

                var br = new BEReader(datStream);

                uint magic = br.ReadU32();
                if (magic != MAGIC_IGHW)
                    return null;

                // IGHWHeader layout (0x20)
                ushort verMaj = br.ReadU16();
                ushort verMin = br.ReadU16();
                uint numToc = br.ReadU32();
                uint tocEnd = br.ReadU32();
                uint dataEnd = br.ReadU32();
                uint numFixups = br.ReadU32();
                ulong dead = br.ReadU64();

                // TOC entries suivent immédiatement (0x10 chacun)
                for (int i = 0; i < numToc; i++)
                {
                    uint id = br.ReadU32();
                    uint dataPtr = br.ReadU32();   // offset relatif dans le bloc
                    uint count = br.ReadU32();     // pas utilisé ici
                    uint size = br.ReadU32();      // pas utilisé ici

                    if (id == lookupId)
                    {
                        // Dans le C++ : rd.Seek(toc.data) puis ReadString(fileName)
                        datStream.Position = blockOffset + dataPtr;
                        return ReadNullTerminatedAscii(datStream);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                datStream.Position = oldPos;
            }
        }
        private static string ReadNullTerminatedAscii(Stream s)
        {
            using var ms = new MemoryStream();
            while (true)
            {
                int b = s.ReadByte();
                if (b < 0) break;
                if (b == 0) break;
                ms.WriteByte((byte)b);
                if (ms.Length > 4096) break;
            }
            if (ms.Length == 0) return null;
            return Encoding.ASCII.GetString(ms.ToArray());
        }
        private static string SanitizeRelativePathKeepDirs(string rel)
        {
            rel = (rel ?? "").Replace('\\', '/').Trim();

            while (rel.StartsWith("/")) rel = rel.Substring(1);
            rel = rel.Replace("..", "__").Replace(":", "_");

            var parts = rel.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (parts.Count == 0) return "_";

            var bad = Path.GetInvalidFileNameChars();

            for (int i = 0; i < parts.Count; i++)
            {
                string p = parts[i];
                foreach (var c in bad) p = p.Replace(c, '_');
                if (string.IsNullOrWhiteSpace(p)) p = "_";
                parts[i] = p;
            }

            return string.Join("/", parts);
        }
        private static string NormalizeGroupPath(string groupName, string p)
        {
            if (string.IsNullOrWhiteSpace(p))
                return null;

            p = p.Replace('\\', '/').Trim();

            int idx = p.IndexOf(groupName + "/", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                p = p.Substring(idx + groupName.Length + 1);

            if (p.StartsWith("/")) p = p.TrimStart('/');

            return groupName + "/" + p;
        }
        private static string EnsureEntityFolderLayout(string rel)
        {
            rel = rel.Replace('\\', '/');

            var parts = rel.Split('/').ToList();
            if (parts.Count < 2) return rel;

            string file = parts.Last();
            string dir = string.Join("/", parts.Take(parts.Count - 1));

            const string suffix = ".entity.irb";
            if (file.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                string baseName = file.Substring(0, file.Length - suffix.Length);
                if (!dir.EndsWith("/" + baseName, StringComparison.OrdinalIgnoreCase))
                    dir = dir + "/" + baseName;

                return dir + "/" + file;
            }

            return rel;
        }
        private static void CopyExactly(Stream src, Stream dst, uint count)
        {
            byte[] buf = new byte[1024 * 1024];
            uint remaining = count;
            while (remaining > 0)
            {
                int toRead = (int)Math.Min((uint)buf.Length, remaining);
                int r = src.Read(buf, 0, toRead);
                if (r <= 0) throw new EndOfStreamException();
                dst.Write(buf, 0, r);
                remaining -= (uint)r;
            }
        }

        // ===== assetlookup reader (IGHW) =====
        private sealed class AssetLookup
        {
            public Dictionary<uint, List<ResourceEntry>> Resources { get; } = new Dictionary<uint, List<ResourceEntry>>();

            public static AssetLookup Load(string path)
            {
                var al = new AssetLookup();
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var br = new BEReader(fs);

                uint magic = br.ReadU32();
                if (magic != MAGIC_IGHW) throw new InvalidDataException("assetlookup.dat n'est pas un IGHW valide.");

                // header
                br.ReadU16(); // verMaj
                br.ReadU16(); // verMin
                uint numToc = br.ReadU32();
                br.ReadU32(); // tocEnd
                br.ReadU32(); // dataEnd
                br.ReadU32(); // numFixups
                br.ReadU64(); // dead

                // Pour chaque TOC, on lit et si l'id correspond à un ResourceLookup (0x1dxxx ou 0x1d?00 etc),
                // on charge la section en mémoire.
                // Ici on sait que chaque ResourceEntry fait 0x10.
                for (int i = 0; i < numToc; i++)
                {
                    uint id = br.ReadU32();
                    uint dataPtr = br.ReadU32();
                    uint count = br.ReadU32();
                    uint size = br.ReadU32();

                    // On ne veut que les tables de ResourceLookup (hash+offset+size)
                    // Heuristique : size multiple de 0x10 et non minuscule
                    // (ex: dans ton assetlookup_sections.txt, les gros blocs 0x920, 0x1350, 0x90 etc)
                    if (size >= 0x10 && (size % 0x10) == 0)
                    {
                        long old = fs.Position;
                        fs.Position = dataPtr;

                        int entryCount = (int)(size / 0x10);
                        var list = new List<ResourceEntry>(entryCount);

                        for (int e = 0; e < entryCount; e++)
                        {
                            uint h1 = br.ReadU32();
                            uint h2 = br.ReadU32();
                            uint off = br.ReadU32();
                            uint len = br.ReadU32();
                            list.Add(new ResourceEntry { Hash1 = h1, Hash2 = h2, Offset = off, Size = len });
                        }

                        fs.Position = old;

                        al.Resources[id] = list;
                    }
                }

                return al;
            }
        }

        // ===== Big Endian reader =====
        private sealed class BEReader
        {
            private readonly Stream _s;
            public BEReader(Stream s) { _s = s; }

            public ushort ReadU16()
            {
                int b0 = _s.ReadByte(), b1 = _s.ReadByte();
                if ((b0 | b1) < 0) throw new EndOfStreamException();
                return (ushort)((b0 << 8) | b1);
            }

            public uint ReadU32()
            {
                int b0 = _s.ReadByte(), b1 = _s.ReadByte(), b2 = _s.ReadByte(), b3 = _s.ReadByte();
                if ((b0 | b1 | b2 | b3) < 0) throw new EndOfStreamException();
                return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
            }

            public ulong ReadU64()
            {
                ulong hi = ReadU32();
                ulong lo = ReadU32();
                return (hi << 32) | lo;
            }
        }
    }
}
