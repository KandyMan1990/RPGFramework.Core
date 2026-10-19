using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveFromANewerBuildIsPreviewed
    {
        private SavePreview m_Preview;

        [SetUp]
        public void SetUp()
        {
            ISaveStorage storage = new FakeSaveStorage();

            storage.Write(SaveFiles.FILE_NAME, SaveFiles.Holding(new byte[2], PersistentLayout.Write(SaveLayouts.WithAVariableFromLater())));

            m_Preview = SaveFiles.Service(SaveLayouts.Version2(), storage, out _).ReadPreview(SaveFiles.FILE_NAME);
        }

        [Test]
        public void ThenItSaysSo()
        {
            Assert.IsTrue(m_Preview.IsFromNewerVersion);
        }

        [Test]
        public void ThenItCannotBeLoaded()
        {
            Assert.IsFalse(m_Preview.CanLoad);
        }
    }
}
