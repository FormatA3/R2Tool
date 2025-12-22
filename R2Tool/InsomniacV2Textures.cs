using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace R2SaveEditor
{
    public sealed class InsomniacV2Textures : IDisposable
    {
        private readonly string _folderPath;
        private readonly string _assetlookupPath;
        private readonly string _highmipsPath;
        private readonly List<TextureEntry> _textures = new List<TextureEntry>();
        private FileStream _fs;
        public enum DdsFormat { DXT1, DXT5, ARGB8888 }

        private struct SectionHeader
        {
            public uint Identifier;
            public uint Offset;
            public uint One;
            public uint Length;
        }
        private struct AssetPointer
        {
            public ulong Id;
            public uint Offset;
            public uint Length;

            public static AssetPointer Read(BEReader r)
            {
                return new AssetPointer
                {
                    Id = r.ReadU64(),
                    Offset = r.ReadU32(),
                    Length = r.ReadU32()
                };
            }
        }

        public sealed class BEReader
        {
            private readonly Stream _s;

            public BEReader(Stream s) { _s = s; }

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

            public uint ReadU32At(long offset)
            {
                long old = _s.Position;
                _s.Position = offset;
                uint v = ReadU32();
                _s.Position = old;
                return v;
            }

            public byte[] ReadBytes(int n)
            {
                byte[] b = new byte[n];
                int off = 0;
                while (n > 0)
                {
                    int r = _s.Read(b, off, n);
                    if (r <= 0) throw new EndOfStreamException();
                    off += r;
                    n -= r;
                }
                return b;
            }
        }
        private sealed class BcDecoder
        {
            public static Bitmap DecodeToBitmap(Stream src, uint width, uint height, uint dataSize, byte format)
            {
                byte[] data = new byte[dataSize];
                ReadExact(src, data, 0, (int)dataSize);

                int w = (int)width;
                int h = (int)height;

                byte[] bgra = new byte[w * h * 4];

                if (format == 0x06) // DXT1 / BC1
                {
                    DecodeBC1(data, w, h, bgra);
                }
                else if (format == 0x08 || format == 0x0B) // DXT5 / BC3
                {
                    DecodeBC3(data, w, h, bgra);
                }
                else if (format == 0x05)
                {
                    int expected = w * h * 4;
                    if (data.Length < expected)
                        throw new InvalidDataException("ARGB8888: dataSize trop petit vs w*h*4.");

                    for (int i = 0; i < w * h; i++)
                    {
                        byte r = data[i * 4 + 0];
                        byte g = data[i * 4 + 1];
                        byte b = data[i * 4 + 2];
                        byte a = data[i * 4 + 3];

                        bgra[i * 4 + 0] = b;
                        bgra[i * 4 + 1] = g;
                        bgra[i * 4 + 2] = r;
                        bgra[i * 4 + 3] = a;
                    }
                }
                else
                {
                    throw new InvalidDataException("Format non supporté: 0x" + format.ToString("X2"));
                }

                return CreateBitmapFromBgra(bgra, w, h);
            }
            private static Bitmap CreateBitmapFromBgra(byte[] bgra, int w, int h)
            {
                var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                var rect = new Rectangle(0, 0, w, h);

                var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    Marshal.Copy(bgra, 0, data.Scan0, bgra.Length);
                }
                finally
                {
                    bmp.UnlockBits(data);
                }

                return bmp;
            }
            // ===== BC1 (DXT1) =====
            private static void DecodeBC1(byte[] data, int w, int h, byte[] outBgra)
            {
                int blocksX = (w + 3) / 4;
                int blocksY = (h + 3) / 4;

                int pos = 0;
                for (int by = 0; by < blocksY; by++)
                {
                    for (int bx = 0; bx < blocksX; bx++)
                    {
                        ushort c0 = ReadU16LE(data, pos + 0);
                        ushort c1 = ReadU16LE(data, pos + 2);
                        uint idx = ReadU32LE(data, pos + 4);
                        pos += 8;

                        var p0 = Color565(c0);
                        var p1 = Color565(c1);

                        // couleurs (DXT1)
                        byte[] c = new byte[16]; // 4 colors * RGBA
                        WriteRGBA(c, 0, p0.r, p0.g, p0.b, 255);
                        WriteRGBA(c, 4, p1.r, p1.g, p1.b, 255);

                        if (c0 > c1)
                        {
                            // 4 couleurs
                            WriteRGBA(c, 8,
                                (byte)((2 * p0.r + p1.r) / 3),
                                (byte)((2 * p0.g + p1.g) / 3),
                                (byte)((2 * p0.b + p1.b) / 3),
                                255);

                            WriteRGBA(c, 12,
                                (byte)((p0.r + 2 * p1.r) / 3),
                                (byte)((p0.g + 2 * p1.g) / 3),
                                (byte)((p0.b + 2 * p1.b) / 3),
                                255);
                        }
                        else
                        {
                            // 3 couleurs + transparent
                            WriteRGBA(c, 8,
                                (byte)((p0.r + p1.r) / 2),
                                (byte)((p0.g + p1.g) / 2),
                                (byte)((p0.b + p1.b) / 2),
                                255);

                            WriteRGBA(c, 12, 0, 0, 0, 0);
                        }

                        // écrire pixels 4x4
                        for (int py = 0; py < 4; py++)
                        {
                            for (int px = 0; px < 4; px++)
                            {
                                int x = bx * 4 + px;
                                int y = by * 4 + py;
                                if (x >= w || y >= h) continue;

                                int sel = (int)((idx >> (2 * (py * 4 + px))) & 0x3);
                                int o = (y * w + x) * 4;

                                // RGBA -> BGRA
                                byte r = c[sel * 4 + 0];
                                byte g = c[sel * 4 + 1];
                                byte b = c[sel * 4 + 2];
                                byte a = c[sel * 4 + 3];

                                outBgra[o + 0] = b;
                                outBgra[o + 1] = g;
                                outBgra[o + 2] = r;
                                outBgra[o + 3] = a;
                            }
                        }
                    }
                }
            }
            // ===== BC3 (DXT5) =====
            private static void DecodeBC3(byte[] data, int w, int h, byte[] outBgra)
            {
                int blocksX = (w + 3) / 4;
                int blocksY = (h + 3) / 4;

                int pos = 0;
                for (int by = 0; by < blocksY; by++)
                {
                    for (int bx = 0; bx < blocksX; bx++)
                    {
                        byte a0 = data[pos + 0];
                        byte a1 = data[pos + 1];

                        // 48 bits d'indices alpha (6 bytes)
                        ulong aIdx = 0;
                        for (int i = 0; i < 6; i++)
                            aIdx |= ((ulong)data[pos + 2 + i]) << (8 * i);

                        // BC1 color part
                        ushort c0 = ReadU16LE(data, pos + 8);
                        ushort c1 = ReadU16LE(data, pos + 10);
                        uint cIdx = ReadU32LE(data, pos + 12);

                        pos += 16;

                        // palette alpha (8 valeurs)
                        byte[] ap = new byte[8];
                        ap[0] = a0;
                        ap[1] = a1;
                        if (a0 > a1)
                        {
                            ap[2] = (byte)((6 * a0 + 1 * a1) / 7);
                            ap[3] = (byte)((5 * a0 + 2 * a1) / 7);
                            ap[4] = (byte)((4 * a0 + 3 * a1) / 7);
                            ap[5] = (byte)((3 * a0 + 4 * a1) / 7);
                            ap[6] = (byte)((2 * a0 + 5 * a1) / 7);
                            ap[7] = (byte)((1 * a0 + 6 * a1) / 7);
                        }
                        else
                        {
                            ap[2] = (byte)((4 * a0 + 1 * a1) / 5);
                            ap[3] = (byte)((3 * a0 + 2 * a1) / 5);
                            ap[4] = (byte)((2 * a0 + 3 * a1) / 5);
                            ap[5] = (byte)((1 * a0 + 4 * a1) / 5);
                            ap[6] = 0;
                            ap[7] = 255;
                        }

                        var p0 = Color565(c0);
                        var p1 = Color565(c1);

                        // palette couleur 4 (BC3 = comme BC1 en mode 4 couleurs)
                        byte[] c = new byte[16];
                        WriteRGBA(c, 0, p0.r, p0.g, p0.b, 255);
                        WriteRGBA(c, 4, p1.r, p1.g, p1.b, 255);
                        WriteRGBA(c, 8,
                            (byte)((2 * p0.r + p1.r) / 3),
                            (byte)((2 * p0.g + p1.g) / 3),
                            (byte)((2 * p0.b + p1.b) / 3),
                            255);
                        WriteRGBA(c, 12,
                            (byte)((p0.r + 2 * p1.r) / 3),
                            (byte)((p0.g + 2 * p1.g) / 3),
                            (byte)((p0.b + 2 * p1.b) / 3),
                            255);

                        // pixels 4x4
                        for (int py = 0; py < 4; py++)
                        {
                            for (int px = 0; px < 4; px++)
                            {
                                int x = bx * 4 + px;
                                int y = by * 4 + py;
                                if (x >= w || y >= h) continue;

                                int pix = py * 4 + px;

                                int aSel = (int)((aIdx >> (3 * pix)) & 0x7);
                                byte alpha = ap[aSel];

                                int cSel = (int)((cIdx >> (2 * pix)) & 0x3);

                                int o = (y * w + x) * 4;

                                byte r = c[cSel * 4 + 0];
                                byte g = c[cSel * 4 + 1];
                                byte b = c[cSel * 4 + 2];

                                outBgra[o + 0] = b;
                                outBgra[o + 1] = g;
                                outBgra[o + 2] = r;
                                outBgra[o + 3] = alpha;
                            }
                        }
                    }
                }
            }
            private static (byte r, byte g, byte b) Color565(ushort c)
            {
                int r = (c >> 11) & 31;
                int g = (c >> 5) & 63;
                int b = (c >> 0) & 31;

                // expand to 8-bit
                byte rr = (byte)((r << 3) | (r >> 2));
                byte gg = (byte)((g << 2) | (g >> 4));
                byte bb = (byte)((b << 3) | (b >> 2));
                return (rr, gg, bb);
            }
            private static void WriteRGBA(byte[] c, int off, byte r, byte g, byte b, byte a)
            {
                c[off + 0] = r;
                c[off + 1] = g;
                c[off + 2] = b;
                c[off + 3] = a;
            }
            private static ushort ReadU16LE(byte[] b, int o)
                => (ushort)(b[o] | (b[o + 1] << 8));
            private static uint ReadU32LE(byte[] b, int o)
                => (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
            private static void ReadExact(Stream s, byte[] buf, int off, int len)
            {
                while (len > 0)
                {
                    int r = s.Read(buf, off, len);
                    if (r <= 0) throw new EndOfStreamException();
                    off += r;
                    len -= r;
                }
            }
        }
        public sealed class DdsInfo
        {
            public int Width;
            public int Height;
            public DdsFormat Format;
            public byte[] Data;
        }
        public sealed class TextureEntry
        {
            public int Index { get; internal set; }
            public ulong Id { get; internal set; }
            public byte Format { get; internal set; }
            public byte MipCount { get; internal set; }
            public uint Width { get; internal set; }
            public uint Height { get; internal set; }
            public uint HighOffset { get; internal set; }
            public uint HighLength { get; internal set; }

            public override string ToString()
                => $"{Index:D5}  fmt=0x{Format:X2}  {Width}x{Height}";
            //hmOff=0x{HighOffset:X}  hmLen=0x{HighLength:X}
        }

        public IReadOnlyList<TextureEntry> Textures => _textures;
        public static InsomniacV2Textures LoadFromFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) throw new ArgumentNullException(nameof(folderPath));
            if (!Directory.Exists(folderPath)) throw new DirectoryNotFoundException(folderPath);

            string assetlookup = Path.Combine(folderPath, "assetlookup.dat");
            string highmips = Path.Combine(folderPath, "highmips.dat");

            if (!File.Exists(assetlookup))
                throw new FileNotFoundException("assetlookup.dat introuvable dans ce dossier.", assetlookup);
            if (!File.Exists(highmips))
                throw new FileNotFoundException("highmips.dat introuvable dans ce dossier.", highmips);

            return new InsomniacV2Textures(folderPath, assetlookup, highmips);
        }
        public void Dispose()
        {
            _fs?.Dispose();
            _fs = null;
        }
        private void ParseAndBuildTextureList()
        {
            var r = new BEReader(_fs);
            uint magic = r.ReadU32();
            if (magic != 0x49474857) throw new InvalidDataException("assetlookup.dat: magic IGHW attendu.");

            uint numSections = r.ReadU32At(0x08);
            var sections = new SectionHeader[numSections];

            for (int i = 0; i < numSections; i++)
            {
                long baseOff = 0x20 + i * 0x10;
                sections[i] = new SectionHeader
                {
                    Identifier = r.ReadU32At(baseOff + 0x00),
                    Offset = r.ReadU32At(baseOff + 0x04),
                    One = r.ReadU32At(baseOff + 0x08),
                    Length = r.ReadU32At(baseOff + 0x0C),
                };
            }

            var sectionData = new Dictionary<uint, byte[]>();
            foreach (var s in sections)
            {
                _fs.Position = s.Offset;
                sectionData[s.Identifier] = r.ReadBytes((int)s.Length);
            }

            var ptrSections = sections.Where(s => (s.Length % 0x10) == 0 && s.Length >= 0x10).ToList();
            if (ptrSections.Count < 2)
                throw new InvalidDataException("Impossible de trouver 2 sections pointers candidates (len multiple de 0x10).");

            long highmipsLen = new FileInfo(_highmipsPath).Length;

            (uint texPtrsId, uint hmPtrsId, int count) = GuessPointerPair(ptrSections, sectionData, highmipsLen);

            var metaCandidates = sections.Where(s => s.Length == (uint)(count * 4)).Select(s => s.Identifier).Distinct().ToList();
            if (metaCandidates.Count == 0)
                throw new InvalidDataException("Aucune section metadata candidate (len == count*4).");

            var best = GuessBestMetaSectionAndDecoder(metaCandidates, sectionData, count);
            uint metaId = best.metaId;
            MetaDecoder dec = best.decoder;

            var texPtrs = ReadPointerArray(sectionData[texPtrsId], count);
            var hmPtrs = ReadPointerArray(sectionData[hmPtrsId], count);
            var meta = sectionData[metaId];

            _textures.Clear();

            for (int i = 0; i < count; i++)
            {
                byte b0 = meta[i * 4 + 0];
                byte b1 = meta[i * 4 + 1];
                byte b2 = meta[i * 4 + 2];
                byte b3 = meta[i * 4 + 3];

                dec.Decode(b0, b1, b2, b3, out byte fmt, out byte mip, out byte wPow, out byte hPow);

                uint width = Pow2Safe(wPow);
                uint height = Pow2Safe(hPow);

                var hp = hmPtrs[i];

                if (!IsSupportedFormat(fmt)) continue;
                if (!IsPlausibleDim(width, height)) continue;
                if (hp.Length == 0) continue;
                if ((long)hp.Offset + hp.Length > highmipsLen) continue;

                _textures.Add(new TextureEntry
                {
                    Index = i,
                    Id = hp.Id,
                    Format = fmt,
                    MipCount = (mip == 0 ? (byte)1 : mip),
                    Width = width,
                    Height = height,
                    HighOffset = hp.Offset,
                    HighLength = hp.Length
                });
            }

            if (_textures.Count == 0)
                throw new InvalidDataException("Aucune texture exportable détectée (metadata/sections peut-être différentes pour ce pack).");
        }
        private InsomniacV2Textures(string folder, string assetlookup, string highmips)
        {
            _folderPath = folder;
            _assetlookupPath = assetlookup;
            _highmipsPath = highmips;

            _fs = new FileStream(_assetlookupPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            ParseAndBuildTextureList();
        }
        private sealed class MetaDecoder
        {
            public int[] Map;
            public MetaDecoder(int[] map) { Map = map; }
            public void Decode(byte b0, byte b1, byte b2, byte b3, out byte fmt, out byte mip, out byte wPow, out byte hPow)
            {
                byte[] raw = { b0, b1, b2, b3 };
                fmt = raw[Map[0]];
                mip = raw[Map[1]];
                wPow = raw[Map[2]];
                hPow = raw[Map[3]];
            }
        } 
        private static int ScorePointersAgainstFile(AssetPointer[] ptrs, long fileLen)
        {
            int ok = 0;
            int step = Math.Max(1, ptrs.Length / 2048);
            for (int i = 0; i < ptrs.Length; i += step)
            {
                uint off = ptrs[i].Offset;
                uint len = ptrs[i].Length;
                if (len == 0) continue;
                if ((long)off + len <= fileLen) ok++;
            }
            return ok;
        }
        private static int ScoreMeta(byte[] meta, int count, MetaDecoder dec)
        {
            int ok = 0;
            int step = Math.Max(1, count / 2048);
            for (int i = 0; i < count; i += step)
            {
                byte b0 = meta[i * 4 + 0];
                byte b1 = meta[i * 4 + 1];
                byte b2 = meta[i * 4 + 2];
                byte b3 = meta[i * 4 + 3];

                dec.Decode(b0, b1, b2, b3, out byte fmt, out byte mip, out byte wPow, out byte hPow);

                uint w = Pow2Safe(wPow);
                uint h = Pow2Safe(hPow);

                if (IsSupportedFormat(fmt) && IsPlausibleDim(w, h) && mip <= 16)
                    ok++;
            }
            return ok;
        }
        private static (uint metaId, MetaDecoder decoder) GuessBestMetaSectionAndDecoder(List<uint> metaCandidates, Dictionary<uint, byte[]> sectionData, int count)
        {
            int[][] perms =
            {
                new[]{0,1,2,3},
                new[]{0,1,3,2},
                new[]{0,2,1,3},
                new[]{0,2,3,1},
                new[]{0,3,1,2},
                new[]{0,3,2,1},
                new[]{1,0,2,3},
                new[]{1,0,3,2},
                new[]{2,3,0,1},
                new[]{3,2,0,1},
            };

            int bestScore = -1;
            uint bestMeta = 0;
            MetaDecoder bestDec = null;

            foreach (var metaId in metaCandidates)
            {
                byte[] meta = sectionData[metaId];
                if (meta.Length != count * 4) continue;

                foreach (var p in perms)
                {
                    var dec = new MetaDecoder(p);
                    int score = ScoreMeta(meta, count, dec);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMeta = metaId;
                        bestDec = dec;
                    }
                }
            }

            if (bestMeta == 0 || bestDec == null || bestScore <= 0)
                throw new InvalidDataException("Impossible d'identifier la bonne section metadata / mapping bytes.");

            return (bestMeta, bestDec);
        }
        private static (uint texPtrsId, uint hmPtrsId, int count) GuessPointerPair(List<SectionHeader> ptrSections, Dictionary<uint, byte[]> sectionData, long highmipsLen)
        {
            int bestScore = -1;
            uint bestA = 0, bestB = 0;
            int bestCount = 0;

            foreach (var a in ptrSections)
            {
                int countA = (int)(a.Length / 0x10);
                var arrA = ReadPointerArray(sectionData[a.Identifier], countA);

                foreach (var b in ptrSections)
                {
                    if (b.Identifier == a.Identifier) continue;
                    int countB = (int)(b.Length / 0x10);
                    if (countB != countA) continue;

                    var arrB = ReadPointerArray(sectionData[b.Identifier], countB);

                    int scoreB = ScorePointersAgainstFile(arrB, highmipsLen);

                    if (scoreB > bestScore)
                    {
                        bestScore = scoreB;
                        bestA = a.Identifier;
                        bestB = b.Identifier;
                        bestCount = countA;
                    }
                }
            }

            if (bestScore < 1 || bestCount <= 0)
                throw new InvalidDataException("Impossible d'identifier les pointer sections textures vs highmips.");

            return (bestA, bestB, bestCount);
        }
        private static AssetPointer[] ReadPointerArray(byte[] buf, int count)
        {
            var ptrs = new AssetPointer[count];
            using (var ms = new MemoryStream(buf))
            {
                var r = new BEReader(ms);
                for (int i = 0; i < count; i++)
                    ptrs[i] = AssetPointer.Read(r);
            }
            return ptrs;
        }
        private static bool IsSupportedFormat(byte fmt)
        {
            return fmt == 0x06 || fmt == 0x08 || fmt == 0x0B || fmt == 0x05;
        }
        private static bool IsPlausibleDim(uint w, uint h)
        {
            if (w < 8 || h < 8) return false;
            if (w > 8192 || h > 8192) return false;
            return true;
        }
        private static uint Pow2Safe(byte p)
        {
            if (p > 15) return 0;
            return (uint)(1 << p);
        }
        private static void DdsWrite(Stream dst, Stream src, uint width, uint height, uint dataSize, uint mipCount, byte format)
        {
            using (var bw = new BinaryWriter(dst, Encoding.ASCII, leaveOpen: true))
            {
                bw.Write(Encoding.ASCII.GetBytes("DDS "));
                bw.Write(124u);
                bw.Write(0x0002100Fu);
                bw.Write(height);
                bw.Write(width);
                bw.Write(dataSize);
                bw.Write(0u);
                bw.Write(mipCount == 0 ? 1u : mipCount);

                for (int i = 0; i < 11; i++) bw.Write(0u);

                bw.Write(32u);

                if (format == 0x06)
                {
                    bw.Write(0x00000004u);
                    bw.Write(Encoding.ASCII.GetBytes("DXT1"));
                    bw.Write(0u); bw.Write(0u); bw.Write(0u); bw.Write(0u); bw.Write(0u);
                }
                else if (format == 0x08 || format == 0x0B)
                {
                    bw.Write(0x00000004u);
                    bw.Write(Encoding.ASCII.GetBytes("DXT5"));
                    bw.Write(0u); bw.Write(0u); bw.Write(0u); bw.Write(0u); bw.Write(0u);
                }
                else if (format == 0x05)
                {
                    bw.Write(0x00000041u);
                    bw.Write(0u);
                    bw.Write(32u);
                    bw.Write(0x000000FFu);
                    bw.Write(0x0000FF00u);
                    bw.Write(0x00FF0000u);
                    bw.Write(0xFF000000u);
                }
                else
                {
                    throw new InvalidDataException("Format texture non supporté: 0x" + format.ToString("X2"));
                }

                bw.Write(0x00001000u);
                bw.Write(mipCount > 1 ? 0x00400008u : 0u);
                bw.Write(0u);
                bw.Write(0u);
                bw.Write(0u);
            }

            CopyExactly(src, dst, dataSize);
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

        // ==== Extract Functions ====
        public void ExtractSelectedToDds(TextureEntry e, string outDdsPath)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (string.IsNullOrWhiteSpace(outDdsPath)) throw new ArgumentNullException(nameof(outDdsPath));

            Directory.CreateDirectory(Path.GetDirectoryName(outDdsPath) ?? ".");

            using (var hm = File.OpenRead(_highmipsPath))
            {
                if (e.HighLength == 0) throw new InvalidOperationException("Highmip length=0.");
                if ((long)e.HighOffset + e.HighLength > hm.Length)
                    throw new InvalidDataException("Offset/Length highmips invalide pour cette entrée.");

                hm.Position = e.HighOffset;

                using (var outFs = File.Create(outDdsPath))
                {
                    DdsWrite(outFs, hm, e.Width, e.Height, e.HighLength, 1, e.Format);
                }
            }
        }
        public void ExtractAllToFolder(string outDir, Func<TextureEntry, bool> filter = null, Action<int, int, TextureEntry> onProgress = null, Func<bool> shouldCancel = null)
        {
            if (string.IsNullOrWhiteSpace(outDir)) throw new ArgumentNullException(nameof(outDir));
            Directory.CreateDirectory(outDir);

            var list = _textures;
            int total = list.Count;
            int done = 0;

            using (var hm = File.OpenRead(_highmipsPath))
            {
                foreach (var e in list)
                {
                    if (shouldCancel?.Invoke() == true) break;
                    if (filter != null && !filter(e))
                    {
                        done++;
                        onProgress?.Invoke(done, total, e);
                        continue;
                    }

                    if (e.HighLength == 0 || (long)e.HighOffset + e.HighLength > hm.Length)
                    {
                        done++;
                        onProgress?.Invoke(done, total, e);
                        continue;
                    }

                    string outPath = Path.Combine(outDir, $"tex_{e.Index:D5}_{e.Id:X16}.dds");

                    hm.Position = e.HighOffset;
                    using (var outFs = File.Create(outPath))
                        DdsWrite(outFs, hm, e.Width, e.Height, e.HighLength, 1, e.Format);

                    done++;
                    onProgress?.Invoke(done, total, e);
                }
            }
        }

        // ==== Preview Functions ====
        public Bitmap GetPreviewBitmap(TextureEntry e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            using (var hm = File.OpenRead(_highmipsPath))
            {
                if (e.HighLength == 0) throw new InvalidOperationException("Highmip length=0.");
                if ((long)e.HighOffset + e.HighLength > hm.Length)
                    throw new InvalidDataException("Offset/Length highmips invalide.");

                hm.Position = e.HighOffset;

                return BcDecoder.DecodeToBitmap(hm, e.Width, e.Height, e.HighLength, e.Format);
            }
        }

        // ==== Replace DDS Functions ====
        public static DdsInfo Load(string path)
        {
            using (var fs = File.OpenRead(path))
            using (var br = new BinaryReader(fs, Encoding.ASCII))
            {
                var magic = br.ReadBytes(4);
                if (Encoding.ASCII.GetString(magic) != "DDS ")
                    throw new InvalidDataException("Pas un DDS.");

                uint headerSize = br.ReadUInt32(); // 124
                if (headerSize != 124) throw new InvalidDataException("DDS headerSize != 124.");

                uint flags = br.ReadUInt32();
                int height = (int)br.ReadUInt32();
                int width = (int)br.ReadUInt32();
                uint pitchOrLinear = br.ReadUInt32();
                br.ReadUInt32();
                uint mipCount = br.ReadUInt32();

                br.BaseStream.Position += 11 * 4;

                uint pfSize = br.ReadUInt32();
                uint pfFlags = br.ReadUInt32();
                uint pfFourCC = br.ReadUInt32();
                uint pfRGBBitCount = br.ReadUInt32();
                uint pfRMask = br.ReadUInt32();
                uint pfGMask = br.ReadUInt32();
                uint pfBMask = br.ReadUInt32();
                uint pfAMask = br.ReadUInt32();

                br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32(); br.ReadUInt32();

                long dataStart = 128;

                if (pfFourCC == 0x30315844)
                {
                    br.BaseStream.Position = 128 + 20;
                    dataStart = 148;
                    throw new InvalidDataException("DDS DX10 non supporté (exporte en DXT1/DXT5 classique).");
                }

                DdsFormat format;

                if ((pfFlags & 0x4) != 0)
                {
                    if (pfFourCC == 0x31545844) format = DdsFormat.DXT1;
                    else if (pfFourCC == 0x35545844) format = DdsFormat.DXT5;
                    else throw new InvalidDataException("FourCC DDS non supporté.");
                }
                else
                {
                    if (pfRGBBitCount == 32 && pfAMask != 0)
                        format = DdsFormat.ARGB8888;
                    else
                        throw new InvalidDataException("DDS non compressé non supporté.");
                }

                br.BaseStream.Position = dataStart;
                byte[] data = br.ReadBytes((int)(br.BaseStream.Length - dataStart));

                int mip0Size = ComputeMip0Size(width, height, format);
                if (data.Length < mip0Size)
                    throw new InvalidDataException("DDS trop petit (mip0 incomplet).");

                if (data.Length != mip0Size)
                {
                    Array.Resize(ref data, mip0Size);
                }

                return new DdsInfo
                {
                    Width = width,
                    Height = height,
                    Format = format,
                    Data = data
                };
            }
        }
        public static int ComputeMip0Size(int width, int height, DdsFormat fmt)
        {
            if (fmt == DdsFormat.ARGB8888) return checked(width * height * 4);

            int bw = (width + 3) / 4;
            int bh = (height + 3) / 4;

            int blockBytes = (fmt == DdsFormat.DXT1) ? 8 : 16;
            return checked(bw * bh * blockBytes);
        }
        public void ReplaceHighmipFromDds(TextureEntry target, string ddsPath, bool createBackupOnce)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrWhiteSpace(ddsPath)) throw new ArgumentNullException(nameof(ddsPath));
            if (!File.Exists(ddsPath)) throw new FileNotFoundException(ddsPath);

            var dds = Load(ddsPath);

            if (dds.Width != (int)target.Width || dds.Height != (int)target.Height)
                throw new InvalidOperationException($"Dimensions différentes. Attendu {target.Width}x{target.Height}, DDS={dds.Width}x{dds.Height}");

            byte expectedFmt = target.Format;
            byte ddsFmt = MapDdsToGameFormat(dds.Format);

            if (ddsFmt != expectedFmt)
                throw new InvalidOperationException($"Format différent. Attendu fmt=0x{expectedFmt:X2}, DDS={dds.Format}");

            if (dds.Data.Length != target.HighLength)
                throw new InvalidOperationException($"Taille data différente. Attendu {target.HighLength} bytes, DDS mip0={dds.Data.Length} bytes");

            if (createBackupOnce)
                EnsureBackupExists(_highmipsPath);

            using (var fs = new FileStream(_highmipsPath, FileMode.Open, FileAccess.Write, FileShare.Read))
            {
                fs.Position = target.HighOffset;
                fs.Write(dds.Data, 0, dds.Data.Length);
                fs.Flush();
            }
        }
        private static byte MapDdsToGameFormat(DdsFormat fmt)
        {
            switch (fmt)
            {
                case DdsFormat.DXT1: return 0x06;
                case DdsFormat.DXT5: return 0x08;
                case DdsFormat.ARGB8888: return 0x05;
                default: throw new InvalidOperationException("Format DDS non supporté.");
            }
        }
        private static void EnsureBackupExists(string highmipsPath)
        {
            string bak = highmipsPath + ".bak";
            if (!File.Exists(bak))
                File.Copy(highmipsPath, bak);
        }
    }
}