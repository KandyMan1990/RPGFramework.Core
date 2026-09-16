namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Core-internal access to the raw bytes of the persistent memory bank, so the save system can persist it.
    /// <br /><br />
    /// This is deliberately not part of <see cref="IMemoryService" />. A game reads and writes memory one
    /// typed address at a time; handing it the backing array would let it resize or corrupt the bank, and
    /// nothing outside Core has a reason to.
    /// </summary>
    internal interface IMemoryBankAccess
    {
        /// <summary>
        /// The size of the persistent bank as currently allocated, which is whatever
        /// <see cref="IMemoryServiceArgs.PersistentBytes" /> reported at construction.
        /// </summary>
        int PersistentByteCount { get; }

        /// <summary>
        /// A copy of the persistent bank, for writing to a save.
        /// </summary>
        byte[] CopyPersistent();

        /// <summary>
        /// Overwrite the persistent bank from <paramref name="source" />, which is a bank captured by a
        /// possibly older build and so may be a different length.<br /><br />
        /// Bytes present in both are copied; bytes the source does not reach are zeroed, so variables added
        /// since the save was written start at zero rather than at whatever the bank last held. A source
        /// longer than the current bank is truncated, which loses the variables that were removed.
        /// </summary>
        void RestorePersistent(byte[] source);

        /// <summary>
        /// Zero the persistent bank. Used when starting a new game and when discarding loaded save data, so
        /// one playthrough's variables cannot leak into the next.
        /// </summary>
        void ClearPersistent();

        /// <summary>
        /// Zero the session bank. Session state is what should survive a module change but not a
        /// restart — where an NPC was left standing, which of its lines have already been heard.
        /// Beginning a new game and loading a save are both a restart as far as that state is
        /// concerned, so the save system clears it alongside the persistent bank.
        /// </summary>
        void ClearSession();
    }
}