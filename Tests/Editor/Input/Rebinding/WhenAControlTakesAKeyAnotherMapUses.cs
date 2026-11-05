using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Delete, in the UI map, rebound to Q, which the menu has in the Player map.</summary>
    internal sealed class WhenAControlTakesAKeyAnotherMapUses
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Delete, Rebinding.KEYBOARD, m_Rebinding.Keyboard.qKey);
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenBothHaveIt()
        {
            Assert.AreEqual(RebindOutcome.Bound, m_Outcome);
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("UI/Delete", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("Player/Menu", Rebinding.KEYBOARD));
        }
    }
}
