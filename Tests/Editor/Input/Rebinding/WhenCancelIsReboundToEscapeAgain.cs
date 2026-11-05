using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Cancel, rebound to J, then back to Escape, which no longer cancels a rebinding.</summary>
    internal sealed class WhenCancelIsReboundToEscapeAgain
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Rebinding.Rebind(m_Rebinding.Cancel, Rebinding.KEYBOARD, m_Rebinding.Keyboard.jKey);

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Cancel, Rebinding.KEYBOARD, m_Rebinding.Keyboard.escapeKey);
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenItIsBound()
        {
            Assert.AreEqual(RebindOutcome.Bound, m_Outcome);
        }

        [Test]
        public void ThenEscapeCancelsAgainInEveryMap()
        {
            Assert.AreEqual("<Keyboard>/escape", m_Rebinding.PathOf("Player/Cancel", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/escape", m_Rebinding.PathOf("UI/Cancel", Rebinding.KEYBOARD));
        }
    }
}
