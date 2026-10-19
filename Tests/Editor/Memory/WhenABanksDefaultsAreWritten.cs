using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenABanksDefaultsAreWritten
    {
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add("Flag", MemoryBank.Persistent, VariableWidth.Bool, 0, VariableDefaults.FromBool(true))
                                                       .Add("Count", MemoryBank.Persistent, VariableWidth.Int, 4, VariableDefaults.FromInteger(-7, VariableWidth.Int))
                                                       .Add("Speed", MemoryBank.Persistent, VariableWidth.Float, 8, VariableDefaults.FromFloat(2.5f))
                                                       .Add("Field", MemoryBank.Persistent, VariableWidth.ULong, 16, 0xDEADBEEFCAFEF00DUL)
                                                       .Add("Seen", MemoryBank.Session, VariableWidth.Byte, 0, 9);

            m_Memory = TestMemory.Make(32);

            VariableDefaults.Write(m_Memory, map, MemoryBank.Persistent);
        }

        [Test]
        public void ThenEachLandsAsATypedWriteWould()
        {
            Assert.IsTrue(m_Memory.ReadBool(MemoryBank.Persistent, 0));
            Assert.AreEqual(-7, m_Memory.ReadInt(MemoryBank.Persistent, 4));
            Assert.AreEqual(2.5f, m_Memory.ReadFloat(MemoryBank.Persistent, 8));
            Assert.AreEqual(0xDEADBEEFCAFEF00DUL, m_Memory.ReadUlong(MemoryBank.Persistent, 16));
        }

        [Test]
        public void ThenTheOtherBankIsLeftAlone()
        {
            Assert.AreEqual(0, m_Memory.ReadByte(MemoryBank.Session, 0));
        }
    }
}
