using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAnArraysDefaultsAreWritten
    {
        private VariableMapAsset   m_Map;
        private VariableDefinition m_Counts;
        private IMemoryService     m_Memory;

        // Five counts at 7, the fourth given 500 of its own, and a byte after them.
        [SetUp]
        public void SetUp()
        {
            m_Map = MapAuthoring.Empty();

            m_Map.Allocate("Counts", MemoryBank.Persistent, VariableWidth.UShort, 5, string.Empty, 7);
            m_Map.Allocate("After", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 3);

            MapAuthoring.AddElementDefault(m_Map, "Counts", 3, string.Empty, 0, 500);

            m_Counts = MapAuthoring.Named(m_Map, "Counts");
            m_Memory = TestMemory.Make(64);

            VariableDefaults.Write(m_Memory, m_Map, MemoryBank.Persistent);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenEveryElementStartsAtTheDefault()
        {
            Assert.AreEqual(7, m_Memory.ReadUshort(MemoryBank.Persistent, (ushort)m_Counts.GetElementOffset(0)));
            Assert.AreEqual(7, m_Memory.ReadUshort(MemoryBank.Persistent, (ushort)m_Counts.GetElementOffset(4)));
        }

        [Test]
        public void ThenAnElementsOwnDefaultIsUsed()
        {
            Assert.AreEqual(500, m_Memory.ReadUshort(MemoryBank.Persistent, (ushort)m_Counts.GetElementOffset(3)));
        }

        [Test]
        public void ThenTheVariableAfterItIsIntact()
        {
            Assert.AreEqual(3, m_Memory.ReadByte(MemoryBank.Persistent, (ushort)MapAuthoring.Named(m_Map, "After").Offset));
        }
    }
}
