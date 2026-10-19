using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenASaveIsRestoredIntoALargerBank
    {
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            MemoryService saved = TestMemory.Make(64);

            ((IMemoryService)saved).WriteInt(MemoryBank.Persistent, 4, -123456);

            MemoryService grown = TestMemory.Make(128);

            m_Memory = grown;
            m_Memory.WriteInt(MemoryBank.Persistent, 100, 0x7FFFFFFF);

            ((IMemoryBankAccess)grown).RestorePersistent(((IMemoryBankAccess)saved).CopyPersistent());
        }

        [Test]
        public void ThenItKeepsWhatTheSaveHeld()
        {
            Assert.AreEqual(-123456, m_Memory.ReadInt(MemoryBank.Persistent, 4));
        }

        [Test]
        public void ThenTheNewAreaIsZero()
        {
            Assert.AreEqual(0, m_Memory.ReadInt(MemoryBank.Persistent, 100));
        }
    }
}
