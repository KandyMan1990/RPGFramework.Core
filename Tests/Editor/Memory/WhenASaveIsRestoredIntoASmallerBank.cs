using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenASaveIsRestoredIntoASmallerBank
    {
        private byte[]        m_Saved;
        private MemoryService m_Shrunk;

        [SetUp]
        public void SetUp()
        {
            MemoryService saved = TestMemory.Make(64);

            ((IMemoryService)saved).WriteInt(MemoryBank.Persistent, 4, -123456);

            m_Saved  = ((IMemoryBankAccess)saved).CopyPersistent();
            m_Shrunk = TestMemory.Make(16);
        }

        [Test]
        public void ThenNothingIsThrown()
        {
            Assert.DoesNotThrow(() => ((IMemoryBankAccess)m_Shrunk).RestorePersistent(m_Saved));
        }

        [Test]
        public void ThenItKeepsWhatFits()
        {
            ((IMemoryBankAccess)m_Shrunk).RestorePersistent(m_Saved);

            Assert.AreEqual(-123456, ((IMemoryService)m_Shrunk).ReadInt(MemoryBank.Persistent, 4));
        }
    }
}
