using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    internal sealed class WhenNoSettingsAreSaved
    {
        private ISettingsService m_Settings;

        [SetUp]
        public void SetUp()
        {
            m_Settings = new SettingsService(new FakeDefaultSettings(), new FakeSaveStorage());
        }

        [Test]
        public void ThenItIsAFirstLaunch()
        {
            Assert.IsFalse(m_Settings.IsSaved);
        }

        [Test]
        public void ThenTheGamesDefaultsStand()
        {
            m_Settings.TryGetSection(FakeDefaultSettings.D, out SaveSection<byte> section);

            Assert.AreEqual(5, section.Data);
        }
    }
}
