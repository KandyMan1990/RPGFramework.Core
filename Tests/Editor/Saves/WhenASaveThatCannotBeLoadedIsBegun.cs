using System;
using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenASaveThatCannotBeLoadedIsBegun
    {
        [Test]
        public void ThenItIsRefused()
        {
            ISaveStorage storage = new FakeSaveStorage();

            storage.Write(SaveFiles.FILE_NAME, SaveFiles.Holding(new byte[2], PersistentLayout.Write(SaveLayouts.WithAVariableFromLater())));

            ISaveDataService saves = SaveFiles.Service(SaveFiles.Version2WithLoadedFromSave(), storage, out _);

            Assert.Throws<InvalidOperationException>(() => saves.BeginSave(SaveFiles.FILE_NAME));
        }
    }
}
