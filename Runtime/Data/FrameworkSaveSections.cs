namespace RPGFramework.Core.Data
{
    internal static class FrameworkSaveSections
    {
        /// <summary>
        /// The whole of <see cref="RPGFramework.Core.SharedTypes.MemoryBank.Persistent" />, written as a raw
        /// byte blob. Managed entirely by <see cref="RPGFramework.Core.SaveData.ISaveDataService" /> — a
        /// game neither reads nor writes this section itself, it just reads and writes variables.
        /// </summary>
        internal const string PERSISTENT_MEMORY = "RPGFramework.PersistentMemory";

        /// <summary>
        /// Where each persistent variable sat in <see cref="PERSISTENT_MEMORY" /> when the save was written, so a build
        /// whose map has changed since can still find every value.
        /// </summary>
        internal const string PERSISTENT_MEMORY_LAYOUT = "RPGFramework.PersistentMemoryLayout";
    }
}