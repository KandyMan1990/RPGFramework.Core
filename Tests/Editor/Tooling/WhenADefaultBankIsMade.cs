using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Tooling
{
    internal sealed class WhenADefaultBankIsMade
    {
        private byte[] m_Bank;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add("Gil", MemoryBank.Persistent, VariableWidth.Byte, 0, 7)
                                                       .Add("Steps", MemoryBank.Session, VariableWidth.Byte, 0, 9);

            IPersistentValues values = new PersistentValues(map, new FakeMemoryArgs(4));

            m_Bank = values.CreateDefaultBank();
        }

        [Test]
        public void ThenAPersistentVariableIsAtItsDefault()
        {
            Assert.AreEqual(7, m_Bank[0]);
        }

        [Test]
        public void ThenItIsAsLongAsTheMemoryArgsSay()
        {
            Assert.AreEqual(4, m_Bank.Length);
        }
    }
}
