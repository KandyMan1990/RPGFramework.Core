namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Core-internal access to the raw bytes of the global memory bank, so the save system can persist it.
    /// <br /><br />
    /// This is deliberately not part of <see cref="IMemoryService" />. A game reads and writes memory one
    /// typed address at a time; handing it the backing array would let it resize or corrupt the bank, and
    /// nothing outside Core has a reason to.
    /// </summary>
    internal interface IMemoryBankAccess
    {
        /// <summary>
        /// The size of the global bank as currently allocated, which is whatever
        /// <see cref="IMemoryServiceArgs.GlobalBytes" /> reported at construction.
        /// </summary>
        int GlobalByteCount { get; }

        /// <summary>
        /// A copy of the global bank, for writing to a save.
        /// </summary>
        byte[] CopyGlobal();

        /// <summary>
        /// Overwrite the global bank from <paramref name="source" />, which is a bank captured by a
        /// possibly older build and so may be a different length.<br /><br />
        /// Bytes present in both are copied; bytes the source does not reach are zeroed, so variables added
        /// since the save was written start at zero rather than at whatever the bank last held. A source
        /// longer than the current bank is truncated, which loses the variables that were removed.
        /// </summary>
        void RestoreGlobal(byte[] source);

        /// <summary>
        /// Zero the global bank. Used when starting a new game and when discarding loaded save data, so
        /// one playthrough's variables cannot leak into the next.
        /// </summary>
        void ClearGlobal();
    }
}