using NUnit.Framework;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    internal sealed class WhenNoSettingsAreSaved
    {
        private FakeSaveStorage m_Storage;
        private ISettingsStore  m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = TestSettings.Map();

            m_Storage = new FakeSaveStorage();

            TestSettings.Launch(map, m_Storage, out IMemoryService memory);

            m_Store = new SettingsStore(memory, map);
        }

        [Test]
        public void ThenNoLanguageIsChosen()
        {
            Assert.IsFalse(m_Store.IsLanguageChosen);
        }

        [Test]
        public void ThenEachSettingIsAtItsDefault()
        {
            Assert.AreEqual(1f,   m_Store.MusicVolume);
            Assert.AreEqual(1f,   m_Store.SfxVolume);
            Assert.AreEqual(0.5f, m_Store.BattleMessageSpeed);
            Assert.AreEqual(0.5f, m_Store.FieldMessageSpeed);
        }

        [Test]
        public void ThenNothingIsWrittenUntilTheyAreCommitted()
        {
            Assert.AreEqual(0, m_Storage.Count);
        }
    }
}
