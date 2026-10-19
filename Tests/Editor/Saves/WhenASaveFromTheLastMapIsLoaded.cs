using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveFromTheLastMapIsLoaded
    {
        // The move itself is PersistentMigration's, tested on its own; this is the service handing the save to it.
        [Test]
        public void ThenItsValuesMoveToWhereTheMapNowPutsThem()
        {
            ISaveStorage storage = new FakeSaveStorage();

            storage.Write(SaveFiles.FILE_NAME, SaveFiles.Holding(SaveLayouts.SavedUnderVersion1(), PersistentLayout.Write(SaveLayouts.Version1())));

            ISaveDataService saves = SaveFiles.Service(SaveFiles.Version2WithLoadedFromSave(), storage, out IMemoryService memory);

            saves.BeginSave(SaveFiles.FILE_NAME);

            Assert.AreEqual(70000, memory.ReadInt(MemoryBank.Persistent, 32));
        }
    }
}
