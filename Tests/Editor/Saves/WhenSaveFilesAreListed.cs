using NUnit.Framework;
using RPGFramework.Core.SaveData;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenSaveFilesAreListed
    {
        [TestCase("save000.sav")]
        [TestCase("save007.sav")]
        [TestCase("save255.sav")]
        public void ThenASaveIsListed(string fileName)
        {
            ISaveDataService saves = SaveFiles.Service(new TestVariableMap(), new FakeSaveStorage(fileName), out _);

            CollectionAssert.AreEqual(new[] { fileName }, saves.GetListOfSaveFiles());
        }

        // "a.sav" is the name that once threw.
        [TestCase("a.sav")]
        [TestCase("")]
        [TestCase("save999.sav")]
        [TestCase("saveabc.sav")]
        [TestCase("save+12.sav")]
        [TestCase("save 12.sav")]
        [TestCase("temp000.sav")]
        [TestCase("save000.txt")]
        [TestCase("save0000.sav")]
        [TestCase("settings.dat")]
        public void ThenAnyOtherFileIsLeftOut(string fileName)
        {
            ISaveDataService saves = SaveFiles.Service(new TestVariableMap(), new FakeSaveStorage(fileName), out _);

            CollectionAssert.IsEmpty(saves.GetListOfSaveFiles());
        }
    }
}
