using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace R2SaveEditor
{
    class PSARC
    {
        private const int HeaderSize = 0x20;
        private const int TocEntrySize = 30;
        private const int BlockSize = 0x10000;
        private const string CompressionTag = "zlib";

        private sealed class Entry
        {
            public byte[] HashNames;
            public int ZSizeIndex;
            public long UncompressedSize;
            public long Offset;
        }

        // --------- Unpack ---------
        public static void Unpack(string psarcPath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            using (var fs = File.OpenRead(psarcPath))
            {
                // ---- Header ----
                string magic = ReadAscii4(fs);
                if (magic != "PSAR") throw new InvalidDataException("Pas un PSARC (magic).");

                ushort major = ReadU16BE(fs);
                ushort minor = ReadU16BE(fs);

                string compression = ReadAscii4(fs);           // "zlib"
                int startOfDatas = ReadS32BE(fs);            // offset data start (ex 0x806)
                int sizeOfEntry = ReadS32BE(fs);            // ex 30
                int filesCount = ReadS32BE(fs);            // ex 51
                int blockSize = ReadS32BE(fs);            // ex 0x10000
                int zero = ReadS32BE(fs);

                if (compression != "zlib")
                    throw new NotSupportedException("Ce dépacker ne gère que zlib pour le moment.");

                // ---- Entries ----
                var entries = new Entry[filesCount];
                for (int i = 0; i < filesCount; i++)
                {
                    entries[i] = ReadEntry(fs);
                }

                int zSizeCount = (startOfDatas - (sizeOfEntry * filesCount) + 32) / 2;
                if (zSizeCount < 0) throw new InvalidDataException("ZSizeCount invalide.");

                var zSizes = new ushort[zSizeCount];
                for (int i = 0; i < zSizeCount; i++)
                    zSizes[i] = ReadU16BE(fs);

                // ---- Entry 0 = table des noms ----
                byte[] namesBytes = UnpackEntry(fs, entries[0], zSizes, blockSize);
                var nameDict = BuildMd5NameDictionary(namesBytes);
                var manifest = new List<string>();

                // ---- Extraction fichiers ----
                for (int i = 1; i < filesCount; i++)
                {
                    var e = entries[i];

                    // skip “empty”
                    if (e.Offset == 0 || IsAllZero16(e.HashNames))
                        continue;

                    string hash = BitConverter.ToString(e.HashNames);
                    string relName;

                    if (!nameDict.TryGetValue(hash, out relName))
                        relName = "_unknowns/" + hash.Replace("-", "").ToLowerInvariant() + ".bin";

                    relName = relName.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
                    manifest.Add(relName);
                    R2Form.AppLog.WriteLine?.Invoke($"Extracting: {relName}");


                    string outPath = Path.Combine(outputDir, relName.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                    byte[] plain = UnpackEntry(fs, e, zSizes, blockSize);
                    File.WriteAllBytes(outPath, plain);
                }
                manifest.Sort(StringComparer.Ordinal);
                File.WriteAllLines(Path.Combine(outputDir, "fileslist.txt"), manifest);
            }
        }
        private static byte[] UnpackEntry(Stream s, Entry e, ushort[] zSizes, int blockSize)
        {
            long remaining = e.UncompressedSize;
            int zIndex = e.ZSizeIndex;
            long blockOffset = e.Offset;

            using (var ms = new MemoryStream((int)Math.Min(e.UncompressedSize, 64 * 1024)))
            {
                while (ms.Length < e.UncompressedSize)
                {
                    if (zIndex < 0 || zIndex >= zSizes.Length)
                        throw new InvalidDataException("ZSizeIndex hors limites.");

                    int compressedSize = zSizes[zIndex++];
                    if (compressedSize == 0) compressedSize = blockSize;

                    int expectedPlain = (int)Math.Min((long)blockSize, remaining);

                    byte[] block = ReadAt(s, blockOffset, compressedSize);
                    byte[] plain = TryInflateZlib(block, expectedPlain);

                    // si pas compressé, plain==block
                    ms.Write(plain, 0, expectedPlain);

                    blockOffset += compressedSize;
                    remaining -= blockSize;
                }

                return ms.ToArray();
            }
        }
        private static byte[] TryInflateZlib(byte[] data, int expectedSize)
        {
            // zlib header typique: 0x78 0xDA (ou 0x78 0x9C)
            bool looksZlib = data.Length >= 2 && data[0] == 0x78;
            if (!looksZlib) return data;

            using (var input = new MemoryStream(data))
            using (var inflater = new InflaterInputStream(input, new Inflater(noHeader: false)))
            {
                byte[] outBuf = new byte[expectedSize];
                int total = 0;

                while (total < expectedSize)
                {
                    int r = inflater.Read(outBuf, total, expectedSize - total);
                    if (r <= 0) break;
                    total += r;
                }

                if (total != expectedSize)
                {
                    return data;
                }

                return outBuf;
            }
        }
        private static Dictionary<string, string> BuildMd5NameDictionary(byte[] file)
        {
            string[] names = Encoding.UTF8.GetString(file)
                .Split(new[] { "\n", "\0" }, StringSplitOptions.None);

            var dict = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var n in names)
            {
                var name = (n ?? "").Trim();
                if (name.Length == 0) continue;

                AddMd5(dict, name);
                AddMd5(dict, name.ToUpperInvariant());
                AddMd5(dict, name.ToLowerInvariant());
            }

            return dict;
        }
        private static void AddMd5(Dictionary<string, string> dict, string name)
        {
            string key = BitConverter.ToString(Md5Bytes(name));
            if (!dict.ContainsKey(key))
                dict.Add(key, name);
        }
        private static byte[] Md5Bytes(string s)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(Encoding.ASCII.GetBytes(s));
        }
        private static Entry ReadEntry(Stream s)
        {
            var e = new Entry();
            e.HashNames = ReadExact(s, 0x10);
            e.ZSizeIndex = ReadS32BE(s);

            // u40 = (byte << 32) | u32
            e.UncompressedSize = ((long)s.ReadByte() << 32) | (long)ReadU32BE(s);
            e.Offset = ((long)s.ReadByte() << 32) | (long)ReadU32BE(s);

            return e;
        }
        private static byte[] ReadAt(Stream s, long offset, int length)
        {
            long pos = s.Position;
            s.Position = offset;
            byte[] buf = ReadExact(s, length);
            s.Position = pos;
            return buf;
        }
        private static byte[] ReadExact(Stream s, int len)
        {
            byte[] b = new byte[len];
            int off = 0;
            while (len > 0)
            {
                int r = s.Read(b, off, len);
                if (r <= 0) throw new EndOfStreamException();
                off += r;
                len -= r;
            }
            return b;
        }
        private static string ReadAscii4(Stream s)
        {
            return Encoding.ASCII.GetString(ReadExact(s, 4));
        }
        private static ushort ReadU16BE(Stream s)
        {
            int b0 = s.ReadByte(), b1 = s.ReadByte();
            if ((b0 | b1) < 0) throw new EndOfStreamException();
            return (ushort)((b0 << 8) | b1);
        }
        private static uint ReadU32BE(Stream s)
        {
            int b0 = s.ReadByte(), b1 = s.ReadByte(), b2 = s.ReadByte(), b3 = s.ReadByte();
            if ((b0 | b1 | b2 | b3) < 0) throw new EndOfStreamException();
            return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
        }
        private static int ReadS32BE(Stream s) => unchecked((int)ReadU32BE(s));
        private static bool IsAllZero16(byte[] b)
        {
            if (b == null || b.Length != 16) return false;
            for (int i = 0; i < 16; i++) if (b[i] != 0) return false;
            return true;
        }

        // --------- Repack ---------
        private sealed class EntryBuild
        {
            public byte[] Hash = new byte[16];
            public int ZIndex;
            public long UncompressedSize;
            public long TempDataOffset;
            public long FinalOffset;
            public int BlockCount;
        }
        public static void RepackFromManifest(string extractedDir, string manifestPath, string outputPsarcPath)
        {
            if (!Directory.Exists(extractedDir))
                throw new DirectoryNotFoundException(extractedDir);

            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("fileslist.txt introuvable", manifestPath);

            var files = File.ReadAllLines(manifestPath)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#"))
                .Select(l => l.Replace('\\', '/').TrimStart('/').ToLowerInvariant())
                .ToList();

            var namesText = string.Join("\n", files) + "\n";
            byte[] namesBytes = Encoding.UTF8.GetBytes(namesText);

            var entries = new List<EntryBuild>();

            entries.Add(new EntryBuild
            {
                Hash = new byte[16],
                UncompressedSize = namesBytes.Length
            });

            foreach (var rel in files)
            {
                string abs = Path.Combine(extractedDir, rel.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(abs))
                    throw new FileNotFoundException("Fichier manquant pour repack: " + rel, abs);

                entries.Add(new EntryBuild
                {
                    Hash = Md5Ascii(rel),
                    UncompressedSize = new FileInfo(abs).Length
                });
            }

            string tmpDataPath = Path.Combine(Path.GetTempPath(), "psarc_data_" + Guid.NewGuid().ToString("N") + ".bin");
            var zSizes = new List<ushort>(capacity: 4096);

            try
            {
                using (var tmp = File.Create(tmpDataPath))
                {
                    BuildEntryDataFromBytes(entries[0], namesBytes, tmp, zSizes);

                    for (int i = 0; i < files.Count; i++)
                    {
                        string rel = files[i];
                        string abs = Path.Combine(extractedDir, rel.Replace('/', Path.DirectorySeparatorChar));
                        BuildEntryDataFromFile(entries[i + 1], abs, tmp, zSizes);
                        R2Form.AppLog.WriteLine?.Invoke($"Repacking: {rel}");
                    }
                }

                int entryCount = entries.Count;
                int tocLen = entryCount * TocEntrySize + zSizes.Count * 2;
                int dataStart = HeaderSize + tocLen;

                foreach (var e in entries)
                    e.FinalOffset = dataStart + e.TempDataOffset;

                using (var outFs = File.Create(outputPsarcPath))
                {
                    WriteHeader(outFs, dataStart, entryCount);

                    // entries
                    foreach (var e in entries)
                    {
                        outFs.Write(e.Hash, 0, 16);

                        WriteS32BE(outFs, e.ZIndex);
                        WriteU40BE(outFs, e.UncompressedSize);
                        WriteU40BE(outFs, e.FinalOffset);
                    }

                    foreach (var zs in zSizes)
                        WriteU16BE(outFs, zs);

                    using (var tmp = File.OpenRead(tmpDataPath))
                        tmp.CopyTo(outFs);
                }
            }
            finally
            {
                try { if (File.Exists(tmpDataPath)) File.Delete(tmpDataPath); } catch { /* ignore */ }
            }
        }
        private static void BuildEntryDataFromBytes(EntryBuild e, byte[] data, Stream tmp, List<ushort> zSizes)
        {
            e.ZIndex = zSizes.Count;
            e.TempDataOffset = tmp.Position;

            using (var ms = new MemoryStream(data, writable: false))
                e.BlockCount = WriteBlocks(ms, tmp, zSizes);
        }
        private static void BuildEntryDataFromFile(EntryBuild e, string absPath, Stream tmp, List<ushort> zSizes)
        {
            e.ZIndex = zSizes.Count;
            e.TempDataOffset = tmp.Position;

            using (var fs = File.OpenRead(absPath))
                e.BlockCount = WriteBlocks(fs, tmp, zSizes);
        }
        private static int WriteBlocks(Stream input, Stream tmp, List<ushort> zSizes)
        {
            int blocks = 0;
            byte[] plainBuf = new byte[BlockSize];

            while (true)
            {
                int read = ReadSome(input, plainBuf, 0, BlockSize);
                if (read <= 0) break;

                blocks++;

                byte[] compressed = DeflateZlib(plainBuf, read);

                bool canUseCompressed =
                    compressed.Length > 0 &&
                    compressed.Length < BlockSize &&
                    compressed.Length <= ushort.MaxValue;

                if (canUseCompressed)
                {
                    zSizes.Add((ushort)compressed.Length);
                    tmp.Write(compressed, 0, compressed.Length);
                }
                else
                {
                    zSizes.Add(0);
                    if (read < BlockSize)
                        Array.Clear(plainBuf, read, BlockSize - read);

                    tmp.Write(plainBuf, 0, BlockSize);
                }

                if (read < BlockSize) break;
            }

            return blocks;
        }
        private static int ReadSome(Stream s, byte[] buf, int off, int len)
        {
            int total = 0;
            while (total < len)
            {
                int r = s.Read(buf, off + total, len - total);
                if (r <= 0) break;
                total += r;
                if (r == 0) break;
            }
            return total;
        }
        private static byte[] DeflateZlib(byte[] plain, int count)
        {
            using (var input = new MemoryStream(plain, 0, count, writable: false))
            using (var output = new MemoryStream())
            {
                var deflater = new Deflater(level: 9, noZlibHeaderOrFooter: false);
                using (var ds = new DeflaterOutputStream(output, deflater))
                {
                    input.CopyTo(ds);
                    ds.Finish();
                }
                return output.ToArray();
            }
        }
        private static void WriteHeader(Stream s, int dataStart, int entryCount)
        {
            WriteAscii4(s, "PSAR");
            WriteU16BE(s, 1);
            WriteU16BE(s, 4);
            WriteAscii4(s, CompressionTag);
            WriteS32BE(s, dataStart);
            WriteS32BE(s, TocEntrySize);
            WriteS32BE(s, entryCount);
            WriteS32BE(s, BlockSize);
            WriteS32BE(s, 0);

            while (s.Position < HeaderSize) s.WriteByte(0);
        }
        private static void WriteAscii4(Stream s, string tag4)
        {
            byte[] b = Encoding.ASCII.GetBytes(tag4);
            if (b.Length != 4) throw new ArgumentException("tag4 doit faire 4 bytes");
            s.Write(b, 0, 4);
        }
        private static void WriteU16BE(Stream s, ushort v)
        {
            s.WriteByte((byte)((v >> 8) & 0xFF));
            s.WriteByte((byte)(v & 0xFF));
        }
        private static void WriteS32BE(Stream s, int v)
        {
            unchecked
            {
                s.WriteByte((byte)((v >> 24) & 0xFF));
                s.WriteByte((byte)((v >> 16) & 0xFF));
                s.WriteByte((byte)((v >> 8) & 0xFF));
                s.WriteByte((byte)(v & 0xFF));
            }
        }
        private static void WriteU40BE(Stream s, long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            ulong v = (ulong)value;
            byte hi = (byte)((v >> 32) & 0xFF);
            uint lo = (uint)(v & 0xFFFFFFFF);

            s.WriteByte(hi);
            s.WriteByte((byte)((lo >> 24) & 0xFF));
            s.WriteByte((byte)((lo >> 16) & 0xFF));
            s.WriteByte((byte)((lo >> 8) & 0xFF));
            s.WriteByte((byte)(lo & 0xFF));
        }
        private static byte[] Md5Ascii(string s)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(Encoding.ASCII.GetBytes(s));
        }
    }
}
