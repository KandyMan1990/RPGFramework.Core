using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenThePersistentBankIsCopiedAndRestored
    {
        private byte[]         m_Copy;
        private IMemoryService m_Restored;

        [SetUp]
        public void SetUp()
        {
            MemoryService  saved  = TestMemory.Make(64);
            IMemoryService memory = saved;

            memory.WriteByte(MemoryBank.Persistent, 0, 0xAB);
            memory.WriteUshort(MemoryBank.Persistent, 2, 0x1234);
            memory.WriteInt(MemoryBank.Persistent, 4, -123456);
            memory.WriteFloat(MemoryBank.Persistent, 8, 0.5f);
            memory.WriteUlong(MemoryBank.Persistent, 16, 0xDEADBEEFCAFEF00DUL);
            memory.WriteBool(MemoryBank.Persistent, 24, true);

            m_Copy = ((IMemoryBankAccess)saved).CopyPersistent();

            MemoryService restored = TestMemory.Make(64);

            ((IMemoryBankAccess)restored).RestorePersistent(m_Copy);

            m_Restored = restored;
        }

        [Test]
        public void ThenTheCopyIsTheBanksLength()
        {
            Assert.AreEqual(64, m_Copy.Length);
        }

        [Test]
        public void ThenItIsLittleEndian()
        {
            Assert.AreEqual(0x34, m_Copy[2]);
            Assert.AreEqual(0x12, m_Copy[3]);
        }

        [Test]
        public void ThenAByteReadsBackTheSame()
        {
            Assert.AreEqual(0xAB, m_Restored.ReadByte(MemoryBank.Persistent, 0));
        }

        [Test]
        public void ThenAUshortReadsBackTheSame()
        {
            Assert.AreEqual(0x1234, m_Restored.ReadUshort(MemoryBank.Persistent, 2));
        }

        [Test]
        public void ThenAnIntReadsBackTheSame()
        {
            Assert.AreEqual(-123456, m_Restored.ReadInt(MemoryBank.Persistent, 4));
        }

        [Test]
        public void ThenAFloatReadsBackTheSame()
        {
            Assert.AreEqual(0.5f, m_Restored.ReadFloat(MemoryBank.Persistent, 8));
        }

        [Test]
        public void ThenAUlongReadsBackTheSame()
        {
            Assert.AreEqual(0xDEADBEEFCAFEF00DUL, m_Restored.ReadUlong(MemoryBank.Persistent, 16));
        }

        [Test]
        public void ThenABoolReadsBackTheSame()
        {
            Assert.IsTrue(m_Restored.ReadBool(MemoryBank.Persistent, 24));
        }
    }
}
