using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenARecordsDefaultsAreWritten
    {
        private VariableMapAsset   m_Map;
        private VariableDefinition m_Characters;
        private int                m_Stats;
        private IMemoryService     m_Memory;

        // Three characters of hp 90, level 1 and four stats at 5, the second's third stat given 12 of its own.
        [SetUp]
        public void SetUp()
        {
            m_Map = MapAuthoring.Empty();

            m_Map.AllocateRecord("Characters", MemoryBank.Persistent, Records.CharacterFields(), 3, string.Empty);
            m_Map.Allocate("After", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 3);

            MapAuthoring.AddElementDefault(m_Map, "Characters", 1, "stats", 2, 12);

            m_Characters = MapAuthoring.Named(m_Map, "Characters");
            m_Memory     = TestMemory.Make(64);

            m_Characters.TryGetField("stats", out VariableRecordField _, out m_Stats);

            VariableDefaults.Write(m_Memory, m_Map, MemoryBank.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenEveryRecordStartsAtItsFieldsDefaults()
        {
            Assert.AreEqual(90, m_Memory.ReadUshort(MemoryBank.Persistent, At(0, 0)));
            Assert.AreEqual(1, m_Memory.ReadByte(MemoryBank.Persistent, At(0, 2)));
            Assert.AreEqual(90, m_Memory.ReadUshort(MemoryBank.Persistent, At(2, 0)));
            Assert.AreEqual(5, m_Memory.ReadByte(MemoryBank.Persistent, At(2, m_Stats + 3)));
        }

        [Test]
        public void ThenOneRecordsOwnDefaultIsUsed()
        {
            Assert.AreEqual(12, m_Memory.ReadByte(MemoryBank.Persistent, At(1, m_Stats + 2)));
            Assert.AreEqual(5, m_Memory.ReadByte(MemoryBank.Persistent, At(1, m_Stats + 1)));
        }

        [Test]
        public void ThenTheVariableAfterItIsIntact()
        {
            Assert.AreEqual(3, m_Memory.ReadByte(MemoryBank.Persistent, (ushort)MapAuthoring.Named(m_Map, "After").Offset));
        }

        // A byte in one record, counted from the record's start.
        private ushort At(int record, int offset)
        {
            ushort address = (ushort)(m_Characters.GetElementOffset(record) + offset);

            return address;
        }
    }
}
