using NUnit.Framework;
using RPGFramework.Core.Memory;
using UnityEngine;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenANewVariableIsAllocated
    {
        private VariableMapAsset m_Map;

        [SetUp]
        public void SetUp()
        {
            m_Map = MapWithAGap.Make();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Map);
        }

        [Test]
        public void ThenItFillsTheFirstGapItFits()
        {
            Assert.AreEqual(4, MapWithAGap.OffsetOf(m_Map, "s"));
            Assert.AreEqual(6, MapWithAGap.OffsetOf(m_Map, "t"));
        }

        [Test]
        public void ThenOneNoGapFitsGoesAfterTheLastAligned()
        {
            Assert.AreEqual(16, MapWithAGap.OffsetOf(m_Map, "l"));
        }

        [Test]
        public void ThenEachBanksGapsAreItsOwn()
        {
            Assert.AreEqual(0, MapWithAGap.OffsetOf(m_Map, "x"));
        }
    }
}
