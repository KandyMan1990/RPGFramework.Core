using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SaveData;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Saves
{
    internal sealed class WhenANewGameBegins
    {
        private FakeSaveStorage m_Storage;
        private IMemoryService  m_Memory;

        [SetUp]
        public void SetUp()
        {
            TestVariableMap map = new TestVariableMap().Add("Gil", MemoryBank.Persistent, VariableWidth.UInt, 0, 300)
                                                       .Add(CoreVariables.LOADED_FROM_SAVE, MemoryBank.Session, VariableWidth.Bool, 0, 0);

            m_Storage = new FakeSaveStorage();

            ISaveDataService saves = SaveFiles.Service(map, m_Storage, out m_Memory);

            m_Memory.WriteInt(MemoryBank.Persistent, 0, 7);
            m_Memory.WriteBool(MemoryBank.Session, 0, true);
            m_Memory.WriteByte(MemoryBank.Settings, 0, 5);

            saves.BeginSave(SaveFiles.FILE_NAME);
        }

        [Test]
        public void ThenEveryVariableStartsAtItsDefault()
        {
            Assert.AreEqual(300, m_Memory.ReadInt(MemoryBank.Persistent, 0));
        }

        [Test]
        public void ThenItIsNotLoadedFromASave()
        {
            Assert.IsFalse(m_Memory.ReadBool(MemoryBank.Session, 0));
        }

        [Test]
        public void ThenTheSettingsAreKept()
        {
            Assert.AreEqual(5, m_Memory.ReadByte(MemoryBank.Settings, 0));
        }

        [Test]
        public void ThenNothingIsWrittenUntilItIsSaved()
        {
            Assert.AreEqual(0, m_Storage.Count);
        }
    }
}
