namespace RPGFramework.Core.Tests
{
    internal static class TestMemory
    {
        /// <summary>A memory service with a persistent bank of <paramref name="persistentBytes" /> and a small session one.</summary>
        internal static MemoryService Make(int persistentBytes)
        {
            MemoryService service = new MemoryService(new Args(persistentBytes, 16));

            return service;
        }

        private sealed class Args : IMemoryServiceArgs
        {
            private readonly int m_PersistentBytes;
            private readonly int m_SessionBytes;

            internal Args(int persistentBytes, int sessionBytes)
            {
                m_PersistentBytes = persistentBytes;
                m_SessionBytes    = sessionBytes;
            }

            int IMemoryServiceArgs.PersistentBytes => m_PersistentBytes;

            int IMemoryServiceArgs.SessionBytes => m_SessionBytes;
        }
    }
}
