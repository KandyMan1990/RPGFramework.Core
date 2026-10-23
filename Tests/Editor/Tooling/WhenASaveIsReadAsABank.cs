using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Tooling
{
    internal sealed class WhenASaveIsReadAsABank
    {
        private IPersistentValues m_Values;
        private byte[]            m_Written;

        [SetUp]
        public void SetUp()
        {
            m_Values  = new PersistentValues(SaveFiles.Version2WithLoadedFromSave(), new FakeMemoryArgs(64));
            m_Written = m_Values.CreateDefaultBank();

            m_Written[2] = 200;
        }

        [Test]
        public void ThenTheBankItWasWrittenFromComesBack()
        {
            m_Values.TryReadSave(m_Values.WriteSave(m_Written), out byte[] read);

            CollectionAssert.AreEqual(m_Written, read);
        }

        [Test]
        public void ThenADamagedSaveIsRefused()
        {
            Assert.IsFalse(m_Values.TryReadSave(new byte[] { 1, 2, 3 }, out _));
        }
    }
}
