using System;

namespace R2SaveEditor
{
    class SaveEditor
    {
        // ===== CRC =====
        private const int CrcWriteOffset = 0x0C;
        private const int CrcStart = 0x20;
        private const int CrcTailMinus = 8;


        // ===== Save Address =====
        public const int CampaignXp = 0x0000B050;
        public const int CompXp = 0x0000B0FC;

        public const int BlackWraith_Solo = 0x0000B098;
        public const int BlackWraith_Coop = 0x0000B0E9;
        public const int BlackWraith_Comp = 0x0000B144;

        public const int SoldierHeadId = 0x0000B0DD;
        public const int MedicHeadId = 0x0000B0DF;
        public const int SpecHeadId = 0x0000B0E1;


        // ==== Byte Utils ====
        public static byte ReadByte(byte[] data, int offset) => data[offset];
        public static int ReadInt32BE(byte[] data, int offset) => (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
        public static void WriteByte(byte[] data, int offset, byte value) => data[offset] = value;
        public static void WriteInt32BE(byte[] data, int offset, int value)
        {
            data[offset] = (byte)((value >> 24) & 0xFF);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)(value & 0xFF);
        }
        public static void WriteUInt32BE(byte[] data, int offset, uint value)
        {
            data[offset + 0] = (byte)((value >> 24) & 0xFF);
            data[offset + 1] = (byte)((value >> 16) & 0xFF);
            data[offset + 2] = (byte)((value >> 8) & 0xFF);
            data[offset + 3] = (byte)(value & 0xFF);
        }


        // ===== Save Functions =====
        public static int GetCampaignXp(byte[] raw) => ReadInt32BE(raw, CampaignXp);
        public static int GetCompXp(byte[] raw) => ReadInt32BE(raw, CompXp);
        public static byte GetMedicHeadId(byte[] raw) => ReadByte(raw, MedicHeadId);
        public static byte GetSoldierHeadId(byte[] raw) => ReadByte(raw, SoldierHeadId);
        public static byte GetSpecHeadId(byte[] raw) => ReadByte(raw, SpecHeadId);

        public static void SetCampaignXp(byte[] raw, int value) => WriteInt32BE(raw, CampaignXp, value);
        public static void SetCompXp(byte[] raw, int value) => WriteInt32BE(raw, CompXp, value);

        public static void SetMedicHeadId(byte[] raw, byte headId) => WriteByte(raw, MedicHeadId, headId);
        public static void SetSoldierHeadId(byte[] raw, byte headId) => WriteByte(raw, SoldierHeadId, headId);
        public static void SetSpecHeadId(byte[] raw, byte headId) => WriteByte(raw, SpecHeadId, headId);

        public static bool IsBlackWraithEnabled(byte[] raw)
        {
            bool solo = ReadByte(raw, BlackWraith_Solo) == 1;
            bool coop = ReadByte(raw, BlackWraith_Coop) == 1;
            bool comp = ReadByte(raw, BlackWraith_Comp) == 1;
            return solo && coop && comp;
        }
        public static void SetBlackWraith(byte[] raw, bool enabled)
        {
            byte v = enabled ? (byte)1 : (byte)0;
            WriteByte(raw, BlackWraith_Solo, v);
            WriteByte(raw, BlackWraith_Coop, v);
            WriteByte(raw, BlackWraith_Comp, v);
        }


        // ===== CRC Functions =====
        private static readonly uint[] CrcTable = GenerateCrc32Table();
        private static uint[] GenerateCrc32Table()
        {
            const uint poly = 0xEDB88320;
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ poly : (crc >> 1);
                table[i] = crc;
            }
            return table;
        }
        private static uint ComputeCrc32_InitFFFFFFFF_NoFinalXor(byte[] data, int offset, int length)
        {
            uint crc = 0xFFFFFFFF;
            int end = offset + length;

            for (int i = offset; i < end; i++)
                crc = (crc >> 8) ^ CrcTable[(crc ^ data[i]) & 0xFF];

            return crc;
        }
        public static uint ComputeExpectedR2Crc(byte[] raw)
        {
            int endExclusive = raw.Length - CrcTailMinus;
            int len = endExclusive - CrcStart;
            if (len <= 0) throw new InvalidOperationException("GAME.SAV invalide.");

            return ComputeCrc32_InitFFFFFFFF_NoFinalXor(raw, CrcStart, len);
        }
        public static uint UpdateR2Crc(byte[] raw)
        {
            uint crc = ComputeExpectedR2Crc(raw);
            WriteUInt32BE(raw, CrcWriteOffset, crc);
            return crc;
        }
    }
}
