using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenThePersistentBankIsCleared
    {
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            MemoryService memory = TestMemory.Make(32);

            m_Memory = memory;
            m_Memory.WriteUlong(MemoryBank.Persistent, 16, 0xDEADBEEFCAFEF00DUL);
            m_Memory.WriteByte(MemoryBank.Session, 0, 0x77);

            ((IMemoryBankAccess)memory).ClearPersistent();
        }

        [Test]
        public void ThenItIsZero()
        {
            Assert.AreEqual(0UL, m_Memory.ReadUlong(MemoryBank.Persistent, 16));
        }

        [Test]
        public void ThenSessionIsLeftAlone()
        {
            Assert.AreEqual(0x77, m_Memory.ReadByte(MemoryBank.Session, 0));
        }
    }
}
