using NUnit.Framework;
using RPGFramework.Core.Memory;
using RPGFramework.Core.SharedTypes;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenAnArrayIsLaidOut
    {
        private VariableDefinition m_Counts;

        [SetUp]
        public void SetUp()
        {
            m_Counts = new TestVariableMap().Add("Counts", MemoryBank.Persistent, VariableWidth.UShort, 2, 7, 5)[0];
        }

        [Test]
        public void ThenItEndsAfterItsLastElement()
        {
            Assert.AreEqual(12, m_Counts.EndOffset);
        }

        [Test]
        public void ThenItsElementsStepByItsWidth()
        {
            Assert.AreEqual(2, m_Counts.GetElementOffset(0));
            Assert.AreEqual(10, m_Counts.GetElementOffset(4));
        }
    }
}
