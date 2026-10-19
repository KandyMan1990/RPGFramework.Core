using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenASaveHasNoMemorySection
    {
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            MemoryService memory = TestMemory.Make(32);

            m_Memory = memory;
            m_Memory.WriteByte(MemoryBank.Persistent, 0, 0xFF);

            ((IMemoryBankAccess)memory).RestorePersistent(null);
        }

        [Test]
        public void ThenThePersistentBankIsCleared()
        {
            Assert.AreEqual(0, m_Memory.ReadByte(MemoryBank.Persistent, 0));
        }
    }
}
