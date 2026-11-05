using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>The menu, rebound to E: Confirm, displaced in the Player map, takes its Q, and Delete, on Q in the UI map, takes the E Confirm left there.</summary>
    internal sealed class WhenASwapDisplacesAControlInAnotherMap
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/q");

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
        public void ThenTheDisplacedTakesItsKeyInEveryMap()
        {
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("Player/Confirm", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathOf("UI/Submit", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenTheOneItDisplacesInTurnTakesTheKeyLeft()
        {
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("UI/Delete", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenAllAreKept()
        {
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathAfterLaunch("UI/Delete", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/q", m_Rebinding.PathAfterLaunch("UI/Submit", Rebinding.KEYBOARD));
        }
    }
}
