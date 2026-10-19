using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenTheSessionBankIsCleared
    {
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            MemoryService memory = TestMemory.Make(64);

            m_Memory = memory;
            m_Memory.WriteByte(MemoryBank.Persistent, 0, 0x11);
            m_Memory.WriteByte(MemoryBank.Session, 0, 0x22);

            ((IMemoryBankAccess)memory).ClearSession();
        }

        [Test]
        public void ThenItIsZero()
        {
            Assert.AreEqual(0, m_Memory.ReadByte(MemoryBank.Session, 0));
        }

        [Test]
        public void ThenPersistentIsLeftAlone()
        {
            Assert.AreEqual(0x11, m_Memory.ReadByte(MemoryBank.Persistent, 0));
        }
    }
}
