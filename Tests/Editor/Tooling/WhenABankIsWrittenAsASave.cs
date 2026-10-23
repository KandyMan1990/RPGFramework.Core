using System;
using NUnit.Framework;
using RPGFramework.Core.Editor;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Tests.Saves;

namespace RPGFramework.Core.Tests.Tooling
{
    internal sealed class WhenABankIsWrittenAsASave
    {
        private ISaveDataService m_Saves;
        private IMemoryService   m_Loaded;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap   map     = SaveFiles.Version2WithLoadedFromSave();
            ISaveStorage      storage = new FakeSaveStorage();
            IPersistentValues values  = new PersistentValues(map, new FakeMemoryArgs(64));
            byte[]            bank    = values.CreateDefaultBank();

            BitConverter.GetBytes(70000).CopyTo(bank, 32);

            storage.Write(SaveFiles.FILE_NAME, values.WriteSave(bank));

            m_Saves = SaveFiles.Service(map, storage, out m_Loaded);
            m_Saves.BeginSave(SaveFiles.FILE_NAME);
        }

        [Test]
        public void ThenTheGameLoadsItWithTheBanksValues()
        {
            Assert.AreEqual(70000, m_Loaded.ReadInt(MemoryBank.Persistent, 32));
        }

        [Test]
        public void ThenTheGameCanLoadIt()
        {
            Assert.IsTrue(m_Saves.ReadPreview(SaveFiles.FILE_NAME).CanLoad);
        }
    }
}
