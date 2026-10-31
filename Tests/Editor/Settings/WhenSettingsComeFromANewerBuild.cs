using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>Settings committed by a build whose map has a setting this one never gave, read by this one.</summary>
    internal sealed class WhenSettingsComeFromANewerBuild
    {
        private ISettingsStore m_Store;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map     = TestSettings.Map();
            TestVariableMap newer   = TestSettings.Map().Add("Later", TestSettings.S, VariableWidth.Byte, 24, 0, id: 20);
            FakeSaveStorage storage = new FakeSaveStorage();

            ISettingsService settings = TestSettings.Launch(newer, storage, out IMemoryService memory);
            ISettingsStore   store    = new SettingsStore(memory, newer);

            store.SetLanguage("ja");
            store.SetSfxVolume(0.5f);
            settings.Commit();

            TestSettings.Launch(map, storage, out IMemoryService next);

            m_Store = new SettingsStore(next, map);
        }

        [Test]
        public void ThenWhatThisBuildKnowsIsKept()
        {
            Assert.IsTrue(m_Store.IsLanguage("ja"));
            Assert.AreEqual(0.5f, m_Store.SfxVolume);
        }
    }
}
