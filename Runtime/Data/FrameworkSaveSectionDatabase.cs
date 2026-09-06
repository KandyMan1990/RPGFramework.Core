namespace RPGFramework.Core.Data
{
    public static class FrameworkSaveSectionDatabase
    {
        public const string CONFIG_DATA = "RPGFramework.ConfigData";

        /// <summary>
        /// The whole of <see cref="RPGFramework.Core.SharedTypes.MemoryBank.Global" />, written as a raw
        /// byte blob. Managed entirely by <see cref="RPGFramework.Core.SaveData.ISaveDataService" /> — a
        /// game neither reads nor writes this section itself, it just reads and writes variables.
        /// </summary>
        public const string GLOBAL_MEMORY = "RPGFramework.GlobalMemory";
    }
}