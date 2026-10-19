using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveIsCommittedAndLoaded
    {
        private IMemoryService m_Loaded;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map     = SaveFiles.Version2WithLoadedFromSave();
            ISaveStorage    storage = new FakeSaveStorage();

            ISaveDataService saved = SaveFiles.Service(map, storage, out IMemoryService memory);

            memory.WriteInt(MemoryBank.Persistent, 32, 70000);
            memory.WriteUshort(MemoryBank.Persistent, 2, 200);
            saved.CommitSave(SaveFiles.FILE_NAME);

            ISaveDataService loading = SaveFiles.Service(map, storage, out m_Loaded);

            loading.BeginSave(SaveFiles.FILE_NAME);
        }

        [Test]
        public void ThenTheBankComesBackAsItWasSaved()
        {
            Assert.AreEqual(70000, m_Loaded.ReadInt(MemoryBank.Persistent, 32));
            Assert.AreEqual(200, m_Loaded.ReadUshort(MemoryBank.Persistent, 2));
        }

        [Test]
        public void ThenItIsLoadedFromASave()
        {
            Assert.IsTrue(m_Loaded.ReadBool(MemoryBank.Session, 0));
        }
    }
}
