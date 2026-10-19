using NUnit.Framework;
using RPGFramework.Core.Memory;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAVariableGrowsIntoItsNeighbours
    {
        private VariableMapAsset m_Map;
        private int              m_Moved;

        [SetUp]
        public void SetUp()
        {
            m_Map = MapWithAGap.Make();

            MapAuthoring.SetCount(m_Map, "a", 2);

            m_Moved = m_Map.RelocateOverlapping();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenItStays()
        {
            Assert.AreEqual(0, MapWithAGap.OffsetOf(m_Map, "a"));
        }

        [Test]
        public void ThenTheyMoveToTheFirstGapsTheyFit()
        {
            Assert.AreEqual(12, MapWithAGap.OffsetOf(m_Map, "s"));
            Assert.AreEqual(14, MapWithAGap.OffsetOf(m_Map, "t"));
        }

        [Test]
        public void ThenTheMovedAreCounted()
        {
            Assert.AreEqual(2, m_Moved);
        }
    }
}
