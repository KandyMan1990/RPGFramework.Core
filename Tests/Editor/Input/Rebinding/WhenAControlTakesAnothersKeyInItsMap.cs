using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>The menu, rebound to E, which Confirm has in the Player map.</summary>
    internal sealed class WhenAControlTakesAnothersKeyInItsMap
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Menu, Rebinding.KEYBOARD, m_Rebinding.Keyboard.eKey);
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
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("Player/Menu", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenTheOtherTakesItsKeyInEveryMap()
        {
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("Player/Confirm", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("UI/Submit", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenBothAreKept()
        {
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathAfterLaunch("Player/Confirm", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathAfterLaunch("Player/Menu", Rebinding.KEYBOARD));
        }
    }
}
