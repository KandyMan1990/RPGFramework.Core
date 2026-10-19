using NUnit.Framework;
using RPGFramework.Core.Memory;

namespace RPGFramework.Core.Tests.Memory
{
    internal sealed class WhenARecordIsLaidOut
    {
        private VariableDefinition m_Characters;

        [SetUp]
        public void SetUp()
        {
            m_Characters = Records.Characters()[0];
        }

        [Test]
        public void ThenItIsItsFieldsEndToEnd()
        {
            Assert.AreEqual(7, m_Characters.ElementSize);
        }

        [Test]
        public void ThenAnArrayOfThemEndsAfterTheLast()
        {
            Assert.AreEqual(25, m_Characters.EndOffset);
        }

        [Test]
        public void ThenAFieldStartsAfterTheOnesBeforeIt()
        {
            bool found = m_Characters.TryGetField("stats", out VariableRecordField stats, out int offset);

            Assert.IsTrue(found);
            Assert.AreEqual(4, stats.Count);
            Assert.AreEqual(3, offset);
        }

        [Test]
        public void ThenAMissingFieldIsNotFound()
        {
            Assert.IsFalse(m_Characters.TryGetField("mp", out VariableRecordField _, out int _));
        }
    }
}
