using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.Settings;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Settings
{
    /// <summary>
    /// The player's message speed committed under one build's map, then a new game begun under the next build's, whose
    /// range of message speeds is wider.
    /// </summary>
    internal sealed class WhenALaterBuildChangesTheRange
    {
        private ISettingsStore     m_Settings;
        private IMessageSpeedStore m_MessageSpeed;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap first   = WithRange(22.5f, 52.5f);
            FakeSaveStorage storage = new FakeSaveStorage();

            ISettingsService settings = TestSettings.Launch(first, storage, out IMemoryService firstMemory);
            ISettingsStore   store    = new SettingsStore(firstMemory, first);

            store.SetFieldMessageSpeed(0.25f);
            settings.Commit();

            TestVariableMap next = WithRange(10f, 90f);

            TestSettings.Launch(next, storage, out IMemoryService memory);

            ISaveDataService saves = new SaveDataService((IMemoryBankAccess)memory, memory, next, storage);

            saves.BeginSave(SaveFiles.FILE_NAME);

            m_Settings     = new SettingsStore(memory, next);
            m_MessageSpeed = new MessageSpeedStore(memory, next);
        }

        [Test]
        public void ThenThePlayersSettingIsKept()
        {
            Assert.AreEqual(0.25f, m_Settings.FieldMessageSpeed);
        }

        [Test]
        public void ThenTheNewRangeIsUsed()
        {
            Assert.AreEqual(30f, m_MessageSpeed.GetCharactersPerSecond(m_Settings.FieldMessageSpeed));
        }

        private static TestVariableMap WithRange(float slowest, float fastest)
        {
            TestVariableMap map = TestSettings.Map().Add(CoreVariables.LOADED_FROM_SAVE,      MemoryBank.Session, VariableWidth.Bool,  0, 0)
                                                    .Add(CoreVariables.MESSAGE_SPEED_SLOWEST, MemoryBank.Session, VariableWidth.Float, 4, VariableDefaults.FromFloat(slowest))
                                                    .Add(CoreVariables.MESSAGE_SPEED_FASTEST, MemoryBank.Session, VariableWidth.Float, 8, VariableDefaults.FromFloat(fastest));

            return map;
        }
    }
}
