using System;
using System.Buffers.Binary;

namespace RPGFramework.Core.Memory
{
    /// <summary>Values read from and written to a bank's bytes, lowest byte first, as every bank and save holds them.</summary>
    internal static class MemoryEncoding
    {
        internal static ushort ReadUshort(byte[] mem, ushort address)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(mem.AsSpan(address));
        }

        internal static void WriteUshort(byte[] mem, ushort address, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(mem.AsSpan(address), value);
        }

        internal static int ReadInt(byte[] mem, ushort address)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(mem.AsSpan(address));
        }

        internal static void WriteInt(byte[] mem, ushort address, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(mem.AsSpan(address), value);
        }

        internal static ulong ReadUlong(byte[] mem, ushort address)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(mem.AsSpan(address));
        }

        internal static void WriteUlong(byte[] mem, ushort address, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(mem.AsSpan(address), value);
        }
    }
}
