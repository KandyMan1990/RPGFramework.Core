using NUnit.Framework;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAScriptsTempMemoryIsWritten
    {
        private TempMemory m_Temp;
        private TempMemory m_Other;

        [SetUp]
        public void SetUp()
        {
            m_Temp  = new TempMemory(16);
            m_Other = new TempMemory(16);

            m_Temp.WriteByte(0, 0xAB);
            m_Temp.WriteBool(1, true);
            m_Temp.WriteUshort(2, 0xBEEF);
            m_Temp.WriteInt(4, -42);
            m_Temp.WriteFloat(8, 1.5f);
        }

        [Test]
        public void ThenEveryWidthReadsBackTheSame()
        {
            Assert.AreEqual(0xAB, m_Temp.ReadByte(0));
            Assert.IsTrue(m_Temp.ReadBool(1));
            Assert.AreEqual(0xBEEF, m_Temp.ReadUshort(2));
            Assert.AreEqual(-42, m_Temp.ReadInt(4));
            Assert.AreEqual(1.5f, m_Temp.ReadFloat(8));
        }

        [Test]
        public void ThenAUlongReadsBackTheSame()
        {
            m_Temp.WriteUlong(8, 0xDEADBEEFCAFEF00DUL);

            Assert.AreEqual(0xDEADBEEFCAFEF00DUL, m_Temp.ReadUlong(8));
        }

        [Test]
        public void ThenItIsLittleEndianLikeTheBanks()
        {
            Assert.AreEqual(0xEF, m_Temp.ReadByte(2));
            Assert.AreEqual(0xBE, m_Temp.ReadByte(3));
        }

        [Test]
        public void ThenAnotherScriptsTempMemoryDoesNotSeeIt()
        {
            Assert.AreEqual(0, m_Other.ReadUshort(2));
        }
    }
}
