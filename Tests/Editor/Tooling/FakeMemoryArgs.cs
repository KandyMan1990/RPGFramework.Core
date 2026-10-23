namespace RPGFramework.Core.Tests.Tooling
{
    internal sealed class FakeMemoryArgs : IMemoryServiceArgs
    {
        private readonly int m_PersistentBytes;

        int IMemoryServiceArgs.PersistentBytes => m_PersistentBytes;
        int IMemoryServiceArgs.SessionBytes    => m_PersistentBytes;

        internal FakeMemoryArgs(int persistentBytes)
        {
            m_PersistentBytes = persistentBytes;
        }
    }
}
