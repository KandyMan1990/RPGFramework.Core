using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Editor
{
    /// <summary>
    /// A variable's value as the bytes it occupies in a bank, lowest first — as a default and a picker hold it — and the
    /// number those bytes are in a width.
    /// </summary>
    public readonly struct VariableBits
    {
        public ulong Bits { get; }

        public VariableBits(ulong bits)
        {
            Bits = bits;
        }

        /// <summary>The bits of <paramref name="integer" /> in <paramref name="width" />, those beyond it dropped.</summary>
        public VariableBits(long integer, VariableWidth width)
        {
            Bits = VariableDefaults.FromInteger(integer, width);
        }

        /// <summary>The number the bits are in <paramref name="width" />, sign-extended for a signed one.</summary>
        public long ToInteger(VariableWidth width)
        {
            return VariableDefaults.ToInteger(Bits, width);
        }
    }
}
