using NUnit.Framework;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    internal sealed class WhenSettingsAreCommitted
    {
        private ISettingsStore m_NextLaunch;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map     = TestSettings.Map();
            FakeSaveStorage storage = new FakeSaveStorage();

            ISettingsService settings = TestSettings.Launch(map, storage, out IMemoryService memory);
            ISettingsStore   store    = new SettingsStore(memory, map);

            store.SetLanguage("fr");
            store.SetMusicVolume(0.25f);
            store.SetFieldMessageSpeed(0.75f);
            settings.Commit();

            TestSettings.Launch(map, storage, out IMemoryService next);

            m_NextLaunch = new SettingsStore(next, map);
        }

        [Test]
        public void ThenTheNextLaunchHasTheLanguage()
        {
            Assert.IsTrue(m_NextLaunch.IsLanguage("fr"));
        }

        [Test]
        public void ThenTheNextLaunchHasTheVolumesAndSpeeds()
        {
            Assert.AreEqual(0.25f, m_NextLaunch.MusicVolume);
            Assert.AreEqual(0.75f, m_NextLaunch.FieldMessageSpeed);
        }

        [Test]
        public void ThenWhatWasNotChangedKeepsItsDefault()
        {
            Assert.AreEqual(1f, m_NextLaunch.SfxVolume);
        }
    }
}
