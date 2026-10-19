using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    internal sealed class WhenSettingsAreCommitted
    {
        private FakeSaveStorage  m_Storage;
        private ISettingsService m_Settings;

        [SetUp]
        public void SetUp()
        {
            m_Storage  = new FakeSaveStorage();
            m_Settings = new SettingsService(new FakeDefaultSettings(), m_Storage);

            m_Settings.SetSection(FakeDefaultSettings.A, new SaveSection<int>(1, 4));
            m_Settings.Commit();
        }

        [Test]
        public void ThenTheyAreSaved()
        {
            Assert.IsTrue(m_Settings.IsSaved);
        }

        [Test]
        public void ThenTheNextLaunchReadsThem()
        {
            ISettingsService next = new SettingsService(new FakeDefaultSettings(), m_Storage);

            next.TryGetSection(FakeDefaultSettings.A, out SaveSection<int> section);

            Assert.AreEqual(4, section.Data);
        }
    }
}
