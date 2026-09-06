namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// The storage width of a variable in a <see cref="RPGFramework.Core.SharedTypes.MemoryBank" />.<br /><br />
    /// These map one to one onto the typed accessors on <see cref="IMemoryService" />, so a variable declared
    /// as <see cref="UShort" /> is read with <see cref="IMemoryService.ReadUshort" /> and no other accessor.
    /// </summary>
    public enum VariableWidth : byte
    {
        /// <summary>
        /// One byte, read and written as a bool. Any non-zero value reads as true.
        /// </summary>
        Bool = 0,

        /// <summary>
        /// One byte, unsigned.
        /// </summary>
        Byte = 1,

        /// <summary>
        /// Two bytes, unsigned, little endian.
        /// </summary>
        UShort = 2,

        /// <summary>
        /// Four bytes, signed, little endian.
        /// </summary>
        Int = 3,

        /// <summary>
        /// Four bytes, IEEE 754 single precision, little endian.
        /// </summary>
        Float = 4,

        /// <summary>
        /// Eight bytes, unsigned, little endian.
        /// </summary>
        ULong = 5
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
                                VariableWidth.Byte   => 1,
                                VariableWidth.UShort => 2,
                                VariableWidth.Int    => 4,
                                VariableWidth.Float  => 4,
                                VariableWidth.ULong  => 8,
                                _                    => 1
                            };

            return byteCount;
        }
    }
}