using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenVariablesAreAllocated
    {
        private VariableMapAsset m_Map;
        private uint             m_IdBefore;
        private uint             m_First;
        private uint             m_Deleted;
        private uint             m_AfterTheDeleted;

        [SetUp]
        public void SetUp()
        {
            m_Map      = MapAuthoring.Empty();
            m_IdBefore = ((IVariableMap)m_Map).LastVariableId;

            m_First   = m_Map.Allocate("a", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0).Id;
            m_Deleted = m_Map.Allocate("b", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0).Id;

            MapAuthoring.Delete(m_Map, "b");

            m_AfterTheDeleted = m_Map.Allocate("c", MemoryBank.Persistent, VariableWidth.Int, 1, string.Empty, 0).Id;

            VariableRecordField[] fields =
            {
                new VariableRecordField("hp", VariableWidth.UShort, 1, 0),
                new VariableRecordField("level", VariableWidth.Byte, 1, 0)
            };

            m_Map.AllocateRecord("party", MemoryBank.Persistent, fields, 3, string.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenEachIsGivenTheNextIdUp()
        {
            Assert.AreEqual(m_IdBefore + 1, m_First);
            Assert.AreEqual(m_IdBefore + 2, m_Deleted);
        }

        [Test]
        public void ThenADeletedVariablesIdIsNotGivenAgain()
        {
            Assert.AreEqual(m_IdBefore + 3, m_AfterTheDeleted);
        }

        [Test]
        public void ThenARecordsFieldsGetIdsFromOne()
        {
            VariableDefinition party = MapAuthoring.Named(m_Map, "party");

            Assert.AreEqual(1, party.Fields[0].Id);
            Assert.AreEqual(2, party.Fields[1].Id);
        }

        [Test]
        public void ThenTheMapHasNoProblems()
        {
            CollectionAssert.IsEmpty(Validation.Problems(m_Map));
        }
    }
}
