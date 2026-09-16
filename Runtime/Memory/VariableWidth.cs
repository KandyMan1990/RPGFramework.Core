namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The storage width of a variable in a <see cref="RPGFramework.Core.SharedTypes.MemoryBank" />, in order of size.
    /// Multi-byte widths are little endian; the signed and unsigned widths of a size share their bytes and
    /// differ only in how they are read.
    /// </summary>
    public enum VariableWidth : byte
    {
        /// <summary>
        /// One byte, read as a bool. Any non-zero value reads as true.
        /// </summary>
        Bool = 0,

        SByte = 1,

        Byte = 2,

        Short = 3,

        UShort = 4,

        Int = 5,

        UInt = 6,

        Long = 7,

        ULong = 8,

        /// <summary>
        /// Four bytes, IEEE 754 single precision.
        /// </summary>
        Float = 9
    }

    public static class VariableWidthExtensions
    {
        /// <summary>
        /// How many bytes a variable of this width occupies in a memory bank.
        /// </summary>
        public static int GetByteCount(this VariableWidth width)
        {
            int byteCount = width switch
                            {
                                VariableWidth.Bool   => 1,
                                VariableWidth.SByte  => 1,
                                VariableWidth.Byte   => 1,
                                VariableWidth.Short  => 2,
                                VariableWidth.UShort => 2,
                                VariableWidth.Int    => 4,
                                VariableWidth.UInt   => 4,
                                VariableWidth.Long   => 8,
                                VariableWidth.ULong  => 8,
                                VariableWidth.Float  => 4,
                                _                    => 1
                            };

            return byteCount;
        }

        public static bool IsInteger(this VariableWidth width)
        {
            bool isInteger = width != VariableWidth.Bool && width != VariableWidth.Float;

            return isInteger;
        }

        public static bool IsSigned(this VariableWidth width)
        {
            bool isSigned = width == VariableWidth.SByte ||
                            width == VariableWidth.Short ||
                            width == VariableWidth.Int   ||
                            width == VariableWidth.Long  ||
                            width == VariableWidth.Float;

            return isSigned;
        }
    }
}
