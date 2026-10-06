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
#if UNITY_EDITOR
        public static ulong FromInteger(long value, VariableWidth width)
        {
            ulong bits = (ulong)value & Mask(width);

            return bits;
        }

        internal static ulong FromBool(bool value)
        {
            ulong bits = value ? 1UL : 0UL;

            return bits;
        }
#endif

        public static ulong FromFloat(float value)
        {
            ulong bits = (uint)BitConverter.SingleToInt32Bits(value);

            return bits;
        }

#if UNITY_EDITOR
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
#endif

        internal static bool ToBool(ulong bits)
        {
            bool value = (bits & 0xFF) != 0;

            return value;
        }

#if UNITY_EDITOR
        internal static float ToFloat(ulong bits)
        {
            float value = BitConverter.Int32BitsToSingle((int)(uint)bits);

            return value;
        }
#endif

        /// <summary>
        /// Write the default of every variable in <paramref name="bank" />, every element of an array and every field of
        /// a record included — what a new game starts with.
        /// </summary>
        internal static void Write(IMemoryService memory, IVariableMap map, MemoryBank bank)
        {
            ForEachDefault(map, bank, (offset, byteCount, value) =>
                                      {
                                          for (int i = 0; i < byteCount; i++)
                                          {
                                              memory.WriteByte(bank, (ushort)(offset + i), (byte)(value >> (8 * i)));
                                          }
                                      });
        }

        /// <summary>
        /// As <see cref="Write(IMemoryService, IVariableMap, MemoryBank)" />, into a copy of the bank rather than the bank.
        /// </summary>
        internal static void Write(byte[] image, IVariableMap map, MemoryBank bank)
        {
            ForEachDefault(map, bank, (offset, byteCount, value) =>
                                      {
                                          for (int i = 0; i < byteCount; i++)
                                          {
                                              image[offset + i] = (byte)(value >> (8 * i));
                                          }
                                      });
        }

        private static void ForEachDefault(IVariableMap map, MemoryBank bank, Action<int, int, ulong> write)
        {
            for (int i = 0; i < map.Variables.Count; i++)
            {
                VariableDefinition variable = map.Variables[i];

                if (variable.Bank != bank)
                {
                    continue;
                }

                for (int element = 0; element < variable.Count; element++)
                {
                    int offset = variable.GetElementOffset(element);

                    if (!variable.IsRecord)
                    {
                        write(offset, variable.Width.GetByteCount(), variable.GetDefault(element));
                        continue;
                    }

                    for (int j = 0; j < variable.Fields.Count; j++)
                    {
                        VariableRecordField field = variable.Fields[j];

                        int byteCount = field.Width.GetByteCount();

                        for (int fieldIndex = 0; fieldIndex < field.Count; fieldIndex++)
                        {
                            write(offset + fieldIndex * byteCount, byteCount, variable.GetDefault(element, field, fieldIndex));
                        }

                        offset += field.ByteCount;
                    }
                }
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
