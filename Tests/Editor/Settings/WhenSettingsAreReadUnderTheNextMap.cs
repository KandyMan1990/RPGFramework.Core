using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.Settings;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>
    /// Settings committed under one map and read under the next, which has moved the music volume and added a setting of
    /// the game's own.
    /// </summary>
    internal sealed class WhenSettingsAreReadUnderTheNextMap
    {
        private const ushort MOVED_MUSIC_VOLUME = 24;
        private const ushort SUBTITLES          = 28;

        private ISettingsStore m_Store;
        private IMemoryService m_Memory;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap first   = TestSettings.Map();
            FakeSaveStorage storage = new FakeSaveStorage();

            ISettingsService settings = TestSettings.Launch(first, storage, out IMemoryService memory);
            ISettingsStore   store    = new SettingsStore(memory, first);

            store.SetLanguage("de");
            store.SetMusicVolume(0.25f);
            settings.Commit();

            TestVariableMap next = new TestVariableMap().Add(CoreVariables.LANGUAGE,             TestSettings.S, VariableWidth.ULong, 0,                  0,                               id: 1)
                                                        .Add(CoreVariables.MUSIC_VOLUME,         TestSettings.S, VariableWidth.Float, MOVED_MUSIC_VOLUME, VariableDefaults.FromFloat(1f),   id: 2)
                                                        .Add(CoreVariables.SFX_VOLUME,           TestSettings.S, VariableWidth.Float, 12,                 VariableDefaults.FromFloat(1f),   id: 3)
                                                        .Add(CoreVariables.BATTLE_MESSAGE_SPEED, TestSettings.S, VariableWidth.Float, 16,                 VariableDefaults.FromFloat(0.5f), id: 4)
                                                        .Add(CoreVariables.FIELD_MESSAGE_SPEED,  TestSettings.S, VariableWidth.Float, 20,                 VariableDefaults.FromFloat(0.5f), id: 5)
                                                        .Add("Subtitles",                        TestSettings.S, VariableWidth.Bool,  SUBTITLES,          VariableDefaults.FromBool(true),  id: 6);

            TestSettings.Launch(next, storage, out m_Memory);

            m_Store = new SettingsStore(m_Memory, next);
        }

        [Test]
        public void ThenAMovedSettingKeepsItsValue()
        {
            Assert.AreEqual(0.25f, m_Store.MusicVolume);
        }

        [Test]
        public void ThenOneThatStayedKeepsItsValue()
        {
            Assert.IsTrue(m_Store.IsLanguage("de"));
        }

        [Test]
        public void ThenANewSettingStartsAtItsDefault()
        {
            Assert.IsTrue(m_Memory.ReadBool(TestSettings.S, SUBTITLES));
        }
    }
}
