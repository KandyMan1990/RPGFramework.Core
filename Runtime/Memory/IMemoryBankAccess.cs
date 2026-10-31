using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Memory
{
    /// <summary>
    /// Core-internal access to the raw bytes of the persistent, session and settings banks, so the save and settings
    /// services can persist them.<br /><br />
    /// This is deliberately not part of <see cref="IMemoryService" />. A game reads and writes memory one
    /// typed address at a time; handing it the backing array would let it resize or corrupt the bank, and
    /// nothing outside Core has a reason to.
    /// </summary>
    internal interface IMemoryBankAccess
    {
        /// <summary>
        /// The size of the bank as allocated, which is whatever <see cref="IMemoryServiceArgs" /> reported for it at
        /// construction.
        /// </summary>
        int GetByteCount(MemoryBank bank);

        /// <summary>
        /// A copy of the bank, for writing to a file.
        /// </summary>
        byte[] Copy(MemoryBank bank);

        /// <summary>
        /// Overwrite the bank from <paramref name="source" />, which may be a different length.<br /><br />
        /// Bytes present in both are copied; bytes the source does not reach are zeroed, so variables the source lacks
        /// start at zero rather than at whatever the bank last held. A source longer than the bank is truncated.
        /// </summary>
        void Restore(MemoryBank bank, byte[] source);

        /// <summary>
        /// Zero the bank. The save system clears the persistent and session banks as a save begins, new game or load,
        /// so one playthrough's variables cannot leak into the next: session state is what should survive a module
        /// change but not a restart — where an NPC was left standing, which of its lines have already been heard.
        /// </summary>
        void Clear(MemoryBank bank);
    }
}
