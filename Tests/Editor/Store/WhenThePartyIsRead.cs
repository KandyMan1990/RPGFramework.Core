using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using RPGFramework.Core.Store;

namespace RPGFramework.Core.Tests.Store
{
    /// <summary>
    /// A roster of four records — the last no one's, the third not joined — beside a game's own field, and a party of
    /// three holding the second record and then the first, its last slot empty.
    /// </summary>
    internal sealed class WhenThePartyIsRead
    {
        private const ushort PARTY  = 0;
        private const ushort ROSTER = 8;
        private const int    RECORD = 10;

        private const ulong FIRST  = 0x1111;
        private const ulong SECOND = 0x2222;
        private const ulong THIRD  = 0x3333;

        private IPartyStore m_Store;

        [SetUp]
        public void SetUp()
        {
            VariableRecordField[] fields =
            {
                new VariableRecordField("Level", VariableWidth.Byte, 1, 0),
                new VariableRecordField(CoreVariables.ROSTER_CHARACTER, VariableWidth.ULong, 1, 0),
                new VariableRecordField(CoreVariables.ROSTER_JOINED, VariableWidth.Bool, 1, 0)
            };

            TestVariableMap map = new TestVariableMap().Add(CoreVariables.PARTY, MemoryBank.Persistent, VariableWidth.Byte, PARTY, 255, 3)
                                                       .AddRecord(CoreVariables.ROSTER, MemoryBank.Persistent, ROSTER, 4, fields);

            IMemoryService memory = TestMemory.Make(64);

            Record(memory, 0, FIRST,  true);
            Record(memory, 1, SECOND, true);
            Record(memory, 2, THIRD,  false);

            memory.WriteByte(MemoryBank.Persistent, PARTY,     1);
            memory.WriteByte(MemoryBank.Persistent, PARTY + 1, 0);
            memory.WriteByte(MemoryBank.Persistent, PARTY + 2, 255);

            m_Store = new PartyStore(memory, map);
        }

        [Test]
        public void ThenItsSizesAreTheMaps()
        {
            Assert.AreEqual(3, m_Store.PartySize);
            Assert.AreEqual(4, m_Store.RosterSize);
        }

        [Test]
        public void ThenASlotHoldsItsRecord()
        {
            Assert.AreEqual(1, m_Store.GetRecordInSlot(0));
            Assert.AreEqual(0, m_Store.GetRecordInSlot(1));
        }

        [Test]
        public void ThenAnEmptySlotHoldsNone()
        {
            Assert.AreEqual(-1, m_Store.GetRecordInSlot(2));
        }

        [Test]
        public void ThenARecordNamesItsCharacter()
        {
            Assert.AreEqual(THIRD, m_Store.GetCharacter(2));
            Assert.AreEqual(0UL,   m_Store.GetCharacter(3));
        }

        [Test]
        public void ThenARecordSaysWhetherItHasJoined()
        {
            Assert.IsTrue(m_Store.HasJoined(0));
            Assert.IsFalse(m_Store.HasJoined(2));
        }

        [Test]
        public void ThenACharacterIsFoundByItsRecord()
        {
            Assert.AreEqual(2, m_Store.FindRecord(THIRD));
        }

        [Test]
        public void ThenACharacterNoRecordNamesIsNotFound()
        {
            Assert.AreEqual(-1, m_Store.FindRecord(0x4444));
            Assert.AreEqual(-1, m_Store.FindRecord(0));
        }

        [Test]
        public void ThenACharacterInThePartyIsFoundInItsSlot()
        {
            Assert.AreEqual(0, m_Store.FindSlot(SECOND));
            Assert.AreEqual(1, m_Store.FindSlot(FIRST));
        }

        [Test]
        public void ThenACharacterOutsideThePartyHasNoSlot()
        {
            Assert.AreEqual(-1, m_Store.FindSlot(THIRD));
            Assert.AreEqual(-1, m_Store.FindSlot(0x4444));
        }

        private static void Record(IMemoryService memory, int record, ulong character, bool joined)
        {
            ushort start = (ushort)(ROSTER + record * RECORD);

            memory.WriteByte(MemoryBank.Persistent, start, 5);
            memory.WriteUlong(MemoryBank.Persistent, (ushort)(start + 1), character);
            memory.WriteBool(MemoryBank.Persistent, (ushort)(start + 9), joined);
        }
    }
}
