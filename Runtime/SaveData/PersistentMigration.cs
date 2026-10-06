using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.SaveData
{
    /// <summary>
    /// Moves a save's persistent values from where its layout had them to where the current map puts them, matched on
    /// ids, so the map can change after saves exist. A rename or a move keeps a value; a variable, element or field added
    /// since takes its default; one deleted is dropped; a width that changed is converted.
    /// </summary>
    internal static class PersistentMigration
    {
        internal static byte[] Migrate(byte[] saved, PersistentLayout layout, IVariableMap map, int bankSize)
        {
            byte[] image = new byte[bankSize];

            VariableDefaults.Write(image, map, MemoryBank.Persistent);

            for (int i = 0; i < map.Variables.Count; i++)
            {
                VariableDefinition variable = map.Variables[i];

                if (variable.Bank != MemoryBank.Persistent || !layout.TryGetEntry(variable.Id, out PersistentLayout.Entry entry) || entry.IsRecord != variable.IsRecord)
                {
                    continue;
                }

                int count = Math.Min(entry.Count, variable.Count);

                for (int element = 0; element < count; element++)
                {
                    int from = entry.Offset + element * entry.ElementSize;
                    int to   = variable.GetElementOffset(element);

                    if (variable.IsRecord)
                    {
                        CopyRecord(saved, from, entry, image, to, variable);
                    }
                    else
                    {
                        Convert(saved, from, entry.Width, image, to, variable.Width);
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// A save holding a variable or record field id above any this map has given out, or a data version above this
        /// build's, was written by a newer build: loading it would drop what this build does not know, and saving over
        /// it would lose that for good.
        /// </summary>
        internal static bool IsFromNewerBuild(PersistentLayout layout, IVariableMap map)
        {
            if (layout.DataVersion > PersistentLayout.DATA_VERSION)
            {
                return true;
            }

            Dictionary<uint, VariableDefinition> byId = new Dictionary<uint, VariableDefinition>();

            for (int i = 0; i < map.Variables.Count; i++)
            {
                VariableDefinition variable = map.Variables[i];

                byId[variable.Id] = variable;
            }

            foreach (PersistentLayout.Entry entry in layout.Entries)
            {
                if (entry.Id > map.LastVariableId)
                {
                    return true;
                }

                if (!entry.IsRecord || !byId.TryGetValue(entry.Id, out VariableDefinition variable))
                {
                    continue;
                }

                for (int i = 0; i < entry.Fields.Length; i++)
                {
                    PersistentLayout.Field field = entry.Fields[i];

                    if (field.Id > variable.LastFieldId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void CopyRecord(byte[] saved, int from, PersistentLayout.Entry entry, byte[] image, int to, VariableDefinition variable)
        {
            for (int j = 0; j < variable.Fields.Count; j++)
            {
                VariableRecordField field = variable.Fields[j];

                if (entry.TryGetField(field.Id, out PersistentLayout.Field savedField, out int savedFieldOffset))
                {
                    int count = Math.Min(savedField.Count, field.Count);

                    for (int i = 0; i < count; i++)
                    {
                        Convert(saved, from + savedFieldOffset + i * savedField.Width.GetByteCount(), savedField.Width,
                                image, to                     + i * field.Width.GetByteCount(),      field.Width);
                    }
                }

                to += field.ByteCount;
            }
        }

        /// <summary>
        /// One value from its saved width to its current one, as a number: widening keeps it, narrowing clamps it to the
        /// new range, a float becomes an integer rounded toward zero, and anything non-zero becomes a true bool.
        /// </summary>
        private static void Convert(byte[] source, int sourceOffset, VariableWidth sourceWidth, byte[] target, int targetOffset, VariableWidth targetWidth)
        {
            if (sourceWidth == targetWidth)
            {
                Buffer.BlockCopy(source, sourceOffset, target, targetOffset, sourceWidth.GetByteCount());
                return;
            }

            ReadOnlySpan<byte> from = source.AsSpan(sourceOffset, sourceWidth.GetByteCount());
            Span<byte>         to   = target.AsSpan(targetOffset, targetWidth.GetByteCount());

            switch (sourceWidth)
            {
                case VariableWidth.Float:
                    WriteReal(BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(from)), to, targetWidth);
                    break;
                case VariableWidth.SByte:
                    WriteSigned((sbyte)from[0], to, targetWidth);
                    break;
                case VariableWidth.Short:
                    WriteSigned(BinaryPrimitives.ReadInt16LittleEndian(from), to, targetWidth);
                    break;
                case VariableWidth.Int:
                    WriteSigned(BinaryPrimitives.ReadInt32LittleEndian(from), to, targetWidth);
                    break;
                case VariableWidth.Long:
                    WriteSigned(BinaryPrimitives.ReadInt64LittleEndian(from), to, targetWidth);
                    break;
                case VariableWidth.Bool:
                    WriteUnsigned(from[0] != 0 ? 1UL : 0UL, to, targetWidth);
                    break;
                case VariableWidth.Byte:
                    WriteUnsigned(from[0], to, targetWidth);
                    break;
                case VariableWidth.UShort:
                    WriteUnsigned(BinaryPrimitives.ReadUInt16LittleEndian(from), to, targetWidth);
                    break;
                case VariableWidth.UInt:
                    WriteUnsigned(BinaryPrimitives.ReadUInt32LittleEndian(from), to, targetWidth);
                    break;
                case VariableWidth.ULong:
                    WriteUnsigned(BinaryPrimitives.ReadUInt64LittleEndian(from), to, targetWidth);
                    break;
            }
        }

        private static void WriteSigned(long value, Span<byte> to, VariableWidth width)
        {
            if (width == VariableWidth.Bool)
            {
                to[0] = value != 0 ? (byte)1 : (byte)0;
            }
            else if (width == VariableWidth.Float)
            {
                WriteFloat(value, to);
            }
            else if (width.IsSigned())
            {
                WriteBytes((ulong)Math.Max(SignedMin(to.Length), Math.Min(value, SignedMax(to.Length))), to);
            }
            else
            {
                WriteBytes(value < 0 ? 0UL : Math.Min((ulong)value, UnsignedMax(to.Length)), to);
            }
        }

        private static void WriteUnsigned(ulong value, Span<byte> to, VariableWidth width)
        {
            if (width == VariableWidth.Bool)
            {
                to[0] = value != 0 ? (byte)1 : (byte)0;
            }
            else if (width == VariableWidth.Float)
            {
                WriteFloat(value, to);
            }
            else if (width.IsSigned())
            {
                long max = SignedMax(to.Length);

                WriteBytes(value > (ulong)max ? (ulong)max : value, to);
            }
            else
            {
                WriteBytes(Math.Min(value, UnsignedMax(to.Length)), to);
            }
        }

        private static void WriteReal(float value, Span<byte> to, VariableWidth width)
        {
            if (width == VariableWidth.Bool)
            {
                to[0] = value != 0 && !float.IsNaN(value) ? (byte)1 : (byte)0;
                return;
            }

            double whole = float.IsNaN(value) ? 0 : Math.Truncate((double)value);

            if (width.IsSigned())
            {
                long min = SignedMin(to.Length);
                long max = SignedMax(to.Length);

                long clamped = whole <= min ? min
                             : whole >= max ? max
                                            : (long)whole;

                WriteBytes((ulong)clamped, to);
            }
            else
            {
                ulong max = UnsignedMax(to.Length);

                ulong clamped = whole <= 0   ? 0UL
                              : whole >= max ? max
                                             : (ulong)whole;

                WriteBytes(clamped, to);
            }
        }

        private static void WriteFloat(float value, Span<byte> to)
        {
            BinaryPrimitives.WriteInt32LittleEndian(to, BitConverter.SingleToInt32Bits(value));
        }

        private static void WriteBytes(ulong value, Span<byte> to)
        {
            for (int i = 0; i < to.Length; i++)
            {
                to[i] = (byte)(value >> (8 * i));
            }
        }

        private static long SignedMax(int byteCount)
        {
            long max = byteCount >= sizeof(long) ? long.MaxValue : (1L << (byteCount * 8 - 1)) - 1;

            return max;
        }

        private static long SignedMin(int byteCount)
        {
            long min = -SignedMax(byteCount) - 1;

            return min;
        }

        private static ulong UnsignedMax(int byteCount)
        {
            ulong max = byteCount >= sizeof(ulong) ? ulong.MaxValue : (1UL << (byteCount * 8)) - 1;

            return max;
        }
    }
}
