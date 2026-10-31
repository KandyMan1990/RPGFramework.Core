namespace RPGFramework.Core.Data
{
    internal static class FrameworkSettingsSections
    {
        /// <summary>
        /// The whole of <see cref="RPGFramework.Core.SharedTypes.MemoryBank.Settings" />, written as a raw byte blob.
        /// Managed entirely by the settings service — a game neither reads nor writes this section itself, it just reads
        /// and writes variables.
        /// </summary>
        internal const string SETTINGS_MEMORY = "RPGFramework.SettingsMemory";

        /// <summary>
        /// Where each settings variable sat in <see cref="SETTINGS_MEMORY" /> when the file was written, so a build whose
        /// map has changed since can still find every value.
        /// </summary>
        internal const string SETTINGS_MEMORY_LAYOUT = "RPGFramework.SettingsMemoryLayout";
    }
}
