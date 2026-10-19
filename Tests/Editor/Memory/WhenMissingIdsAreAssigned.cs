using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    /// <summary>
    /// A variable with no id, one hand-edited above the map's counter and copied in the list, and a record with no id
    /// whose first field was copied: the ids a map from before them, or a copy, leaves.
    /// </summary>
    internal sealed class WhenMissingIdsAreAssigned
    {
        private VariableMapAsset m_Map;
        private uint             m_IdBefore;
        private int              m_Assigned;

        [SetUp]
        public void SetUp()
        {
            m_Map      = MapAuthoring.Empty();
            m_IdBefore = ((IVariableMap)m_Map).LastVariableId;

            m_Map.Allocate("legacy", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 0);
            m_Map.Allocate("original", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 0);
            m_Map.AllocateRecord("record", MemoryBank.Persistent, new[] { new VariableRecordField("x", VariableWidth.Byte, 1, 0) }, 1, string.Empty);

            MapAuthoring.SetId(m_Map, "legacy", 0);
            MapAuthoring.SetId(m_Map, "original", m_IdBefore + 5);
            MapAuthoring.Duplicate(m_Map, "original", "copy");
            MapAuthoring.SetId(m_Map, "record", 0);
            MapAuthoring.DuplicateField(m_Map, "record", 0, "y");

            m_Map.RelocateOverlapping();

            m_Assigned = m_Map.AssignMissingIds();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenTheyAreGivenAboveTheHighestInUse()
        {
            Assert.AreEqual(m_IdBefore + 6, MapAuthoring.Named(m_Map, "legacy").Id);
            Assert.AreEqual(m_IdBefore + 7, MapAuthoring.Named(m_Map, "copy").Id);
            Assert.AreEqual(m_IdBefore + 8, MapAuthoring.Named(m_Map, "record").Id);
        }

        [Test]
        public void ThenAnIdInUseIsKept()
        {
            Assert.AreEqual(m_IdBefore + 5, MapAuthoring.Named(m_Map, "original").Id);
        }

        [Test]
        public void ThenACopiedFieldGetsItsOwn()
        {
            VariableDefinition record = MapAuthoring.Named(m_Map, "record");

            Assert.AreEqual(1, record.Fields[0].Id);
            Assert.AreEqual(2, record.Fields[1].Id);
        }

        [Test]
        public void ThenEveryOneGivenAnIdIsCounted()
        {
            Assert.AreEqual(4, m_Assigned);
        }

        [Test]
        public void ThenTheMapHasNoProblems()
        {
            CollectionAssert.IsEmpty(Validation.Problems(m_Map));
        }

        [Test]
        public void ThenAllocationCarriesOnAfterThem()
        {
            VariableDefinition next = m_Map.Allocate("next", MemoryBank.Persistent, VariableWidth.Byte, 1, string.Empty, 0);

            Assert.AreEqual(m_IdBefore + 9, next.Id);
        }
    }
}
