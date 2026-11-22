using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    /// <summary>A playthrough begun from a save, its money changed since, previewed as a save made now would hold it.</summary>
    internal sealed class WhenThePlaythroughIsPreviewed
    {
        private IMemoryService m_Memory;
        private SavePreview    m_Preview;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap  map   = new TestVariableMap().Add("Gil", MemoryBank.Persistent, VariableWidth.UInt, 0, 300)
                                                       .Add(CoreVariables.LOADED_FROM_SAVE, MemoryBank.Session, VariableWidth.Bool, 0, 0);
            ISaveDataService saves = SaveFiles.Service(map, new FakeSaveStorage(), out m_Memory);

            saves.BeginSave(SaveFiles.FILE_NAME);
            m_Memory.WriteUint(MemoryBank.Persistent, 0, 750);

            m_Preview = saves.PreviewPlaythrough();
        }

        [Test]
        public void ThenItReadsTheValuesAsTheyAreNow()
        {
            Assert.AreEqual(750, m_Preview.Read<uint>("Gil"));
        }

        [Test]
        public void ThenItIsACopy()
        {
            m_Memory.WriteUint(MemoryBank.Persistent, 0, 900);

            Assert.AreEqual(750, m_Preview.Read<uint>("Gil"));
        }

        [Test]
        public void ThenItNamesTheCurrentSave()
        {
            Assert.AreEqual(SaveFiles.FILE_NAME, m_Preview.FileName);
        }

        [Test]
        public void ThenItCanBeLoaded()
        {
            Assert.IsTrue(m_Preview.CanLoad);
        }
    }
}
