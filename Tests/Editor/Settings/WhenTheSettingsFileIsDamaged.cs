using NUnit.Framework;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    internal sealed class WhenTheSettingsFileIsDamaged
    {
        private ISettingsStore m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map     = TestSettings.Map();
            ISaveStorage    storage = new FakeSaveStorage();

            ISettingsService settings = TestSettings.Launch(map, storage, out IMemoryService memory);

            ISettingsStore store = new SettingsStore(memory, map);

            store.SetLanguage("fr");
            settings.Commit();

            byte[] file = storage.Read(SettingsService.SETTINGS_FILE_NAME);

            file[file.Length - 1] ^= 0xFF;
            storage.Write(SettingsService.SETTINGS_FILE_NAME, file);

            TestSettings.Launch(map, storage, out IMemoryService next);

            m_Store = new SettingsStore(next, map);
        }

        [Test]
        public void ThenItIsAFirstLaunch()
        {
            Assert.IsFalse(m_Store.IsLanguageChosen);
        }

        [Test]
        public void ThenEachSettingIsAtItsDefault()
        {
            Assert.AreEqual(1f, m_Store.MusicVolume);
        }
    }
}
