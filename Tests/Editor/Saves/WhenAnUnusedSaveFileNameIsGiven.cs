using System;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenAnUnusedSaveFileNameIsGiven
    {
        [Test]
        public void ThenItIsTheLowestSlotNoSaveHas()
        {
            FakeSaveStorage  storage = new FakeSaveStorage("save000.sav", "save001.sav", "save003.sav", "save002.txt", "a.sav");
            ISaveDataService saves   = SaveFiles.Service(new TestVariableMap(), storage, out _);

            Assert.AreEqual("save002.sav", saves.GetUnusedSaveFileName());
        }

        // A name the slot search could not read back would be given again, so each is taken before the next is asked.
        [Test]
        public void ThenEverySlotIsGivenInTurn()
        {
            ISaveStorage     storage = new FakeSaveStorage();
            ISaveDataService saves   = SaveFiles.Service(new TestVariableMap(), storage, out _);

            for (int slot = 0; slot <= byte.MaxValue; slot++)
            {
                string fileName = saves.GetUnusedSaveFileName();

                Assert.AreEqual($"save{slot:D3}.sav", fileName);

                storage.Write(fileName, Array.Empty<byte>());
            }
        }

        [Test]
        public void ThenNoneIsGivenOnceEverySlotIsTaken()
        {
            ISaveStorage     storage = new FakeSaveStorage();
            ISaveDataService saves   = SaveFiles.Service(new TestVariableMap(), storage, out _);

            for (int slot = 0; slot <= byte.MaxValue; slot++)
            {
                storage.Write($"save{slot:D3}.sav", Array.Empty<byte>());
            }

            Assert.Throws<InvalidOperationException>(() => saves.GetUnusedSaveFileName());
        }
    }
}
