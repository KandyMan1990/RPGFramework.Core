using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Up, rebound on the keyboard to I.</summary>
    internal sealed class WhenADirectionIsRebound
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Up, Rebinding.KEYBOARD, m_Rebinding.Keyboard.iKey);
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenThatPartFollows()
        {
            Assert.AreEqual(RebindOutcome.Bound, m_Outcome);
            Assert.AreEqual("<Keyboard>/i", m_Rebinding.PathOf("Player/Movement", Rebinding.KEYBOARD, "up"));
        }

        [Test]
        public void ThenTheOtherPartsKeepTheirKeys()
        {
            Assert.AreEqual("<Keyboard>/s", m_Rebinding.PathOf("Player/Movement", Rebinding.KEYBOARD, "down"));
        }
    }
}
