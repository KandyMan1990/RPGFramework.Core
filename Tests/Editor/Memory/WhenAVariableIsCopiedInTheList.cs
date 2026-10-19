using NUnit.Framework;
using RPGFramework.Core.Memory;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAVariableIsCopiedInTheList
    {
        private VariableMapAsset m_Map;
        private int              m_Moved;

        // Grown first, so the gap after the first variable is filled and the copy has to go after the last.
        [SetUp]
        public void SetUp()
        {
            m_Map = MapWithAGap.Make();

            MapAuthoring.SetCount(m_Map, "a", 2);
            m_Map.RelocateOverlapping();

            MapAuthoring.Duplicate(m_Map, "c", "copy");

            m_Moved = m_Map.RelocateOverlapping();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenTheOriginalStays()
        {
            Assert.AreEqual(8, MapWithAGap.OffsetOf(m_Map, "c"));
        }

        [Test]
        public void ThenTheCopyMovesToTheFirstGapItFits()
        {
            Assert.AreEqual(24, MapWithAGap.OffsetOf(m_Map, "copy"));
        }

        [Test]
        public void ThenOnlyTheCopyIsCounted()
        {
            Assert.AreEqual(1, m_Moved);
        }
    }
}
