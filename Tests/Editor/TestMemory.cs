namespace RPGFramework.Core.Tests
{
    internal static class TestMemory
    {
        /// <summary>
        /// A memory service with a persistent bank of <paramref name="persistentBytes" /> and small session and settings
        /// ones.
        /// </summary>
        internal static MemoryService Make(int persistentBytes)
        {
            MemoryService service = new MemoryService(new Args(persistentBytes, 16, 32));

            return service;
        }

        private sealed class Args : IMemoryServiceArgs
        {
            private readonly int m_PersistentBytes;
            private readonly int m_SessionBytes;
            private readonly int m_SettingsBytes;

            internal Args(int persistentBytes, int sessionBytes, int settingsBytes)
            {
                m_PersistentBytes = persistentBytes;
                m_SessionBytes    = sessionBytes;
                m_SettingsBytes   = settingsBytes;
            }

            int IMemoryServiceArgs.PersistentBytes => m_PersistentBytes;

            int IMemoryServiceArgs.SessionBytes => m_SessionBytes;

            int IMemoryServiceArgs.SettingsBytes => m_SettingsBytes;
        }
    }
}
