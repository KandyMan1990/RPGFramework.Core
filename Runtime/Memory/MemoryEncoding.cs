namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Little-endian reads and writes of the multi-byte widths, shared by every memory bank.
    /// </summary>
    internal static class MemoryEncoding
    {
        internal static ushort ReadUshort(byte[] mem, ushort address)
        {
            ushort value = (ushort)(mem[address] | mem[address + 1] << 8);

            return value;
        }

        internal static void WriteUshort(byte[] mem, ushort address, ushort value)
        {
            mem[address]     = (byte)(value & 0xFF);
            mem[address + 1] = (byte)(value >> 8);
        }

        internal static int ReadInt(byte[] mem, ushort address)
        {
            int value = mem[address]           |
                        mem[address + 1] << 8  |
                        mem[address + 2] << 16 |
                        mem[address + 3] << 24;

            return value;
        }

        internal static void WriteInt(byte[] mem, ushort address, int value)
        {
            mem[address]     = (byte)(value       & 0xFF);
            mem[address + 1] = (byte)(value >> 8  & 0xFF);
            mem[address + 2] = (byte)(value >> 16 & 0xFF);
            mem[address + 3] = (byte)(value >> 24 & 0xFF);
        }

        internal static ulong ReadUlong(byte[] mem, ushort address)
        {
            ulong value = mem[address]                  |
                          (ulong)mem[address + 1] << 8  |
                          (ulong)mem[address + 2] << 16 |
                          (ulong)mem[address + 3] << 24 |
                          (ulong)mem[address + 4] << 32 |
                          (ulong)mem[address + 5] << 40 |
                          (ulong)mem[address + 6] << 48 |
                          (ulong)mem[address + 7] << 56;

            return value;
        }

        internal static void WriteUlong(byte[] mem, ushort address, ulong value)
        {
            mem[address]     = (byte)(value       & 0xFF);
            mem[address + 1] = (byte)(value >> 8  & 0xFF);
            mem[address + 2] = (byte)(value >> 16 & 0xFF);
            mem[address + 3] = (byte)(value >> 24 & 0xFF);
            mem[address + 4] = (byte)(value >> 32 & 0xFF);
            mem[address + 5] = (byte)(value >> 40 & 0xFF);
            mem[address + 6] = (byte)(value >> 48 & 0xFF);
            mem[address + 7] = (byte)(value >> 56 & 0xFF);
        }
    }
}