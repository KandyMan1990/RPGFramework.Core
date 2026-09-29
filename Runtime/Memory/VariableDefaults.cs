using System;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// A variable's default is held as the bytes it occupies in its bank, lowest byte first, so one field
    /// carries any width — a flag, a float or a field name's hash. These convert it to and from the value an
    /// author means, and put a map's defaults into memory when a game begins.
    /// </summary>
    public static class VariableDefaults
    {
        public static ulong FromInteger(long value, VariableWidth width)
        {
            ulong bits = (ulong)value & Mask(width);

            return bits;
        }

        public static ulong FromBool(bool value)
        {
            ulong bits = value ? 1UL : 0UL;

            return bits;
        }

        public static ulong FromFloat(float value)
        {
            ulong bits = (uint)BitConverter.SingleToInt32Bits(value);

            return bits;
        }

        /// <summary>
        /// The value of a signed or unsigned integer width, sign-extended for the signed ones.
        /// </summary>
        public static long ToInteger(ulong bits, VariableWidth width)
        {
            int   shift = 64 - width.GetByteCount() * 8;
            ulong value = bits & Mask(width);

            long integer = width.IsSigned() ? (long)(value << shift) >> shift : (long)value;

            return integer;
        }

        public static bool ToBool(ulong bits)
        {
            bool value = (bits & 0xFF) != 0;

            return value;
        }

        public static float ToFloat(ulong bits)
        {
            float value = BitConverter.Int32BitsToSingle((int)(uint)bits);

            return value;
        }

        /// <summary>
        /// Write the defaults of every variable in <paramref name="bank" /> that starts at or after
        /// <paramref name="fromOffset" />, every element of an array and every field of a record included. A new game
        /// writes them all; a load writes only those past the end of the saved bank, which were added since that save
        /// and would otherwise start at zero.
        /// </summary>
        public static void Write(IMemoryService memory, IVariableMap map, MemoryBank bank, int fromOffset)
        {
            foreach (VariableDefinition variable in map.Variables)
            {
                if (variable.Bank != bank || variable.Offset < fromOffset)
                {
                    continue;
                }

                for (int element = 0; element < variable.Count; element++)
                {
                    int offset = variable.GetElementOffset(element);

                    if (!variable.IsRecord)
                    {
                        WriteValue(memory, bank, offset, variable.Width, variable.GetDefault(element));
                        continue;
                    }

                    foreach (VariableRecordField field in variable.Fields)
                    {
                        for (int fieldIndex = 0; fieldIndex < field.Count; fieldIndex++)
                        {
                            WriteValue(memory, bank, offset + fieldIndex * field.Width.GetByteCount(), field.Width, variable.GetDefault(element, field, fieldIndex));
                        }

                        offset += field.ByteCount;
                    }
                }
            }
        }

        private static void WriteValue(IMemoryService memory, MemoryBank bank, int offset, VariableWidth width, ulong value)
        {
            int byteCount = width.GetByteCount();

            for (int i = 0; i < byteCount; i++)
            {
                memory.WriteByte(bank, (ushort)(offset + i), (byte)(value >> (8 * i)));
            }
        }

        private static ulong Mask(VariableWidth width)
        {
            int byteCount = width.GetByteCount();

            ulong mask = byteCount >= sizeof(ulong) ? ulong.MaxValue : (1UL << (byteCount * 8)) - 1;

            return mask;
        }
    }
}
