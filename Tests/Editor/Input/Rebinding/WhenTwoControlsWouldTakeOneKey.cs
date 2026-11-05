using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Cancel, rebound to E, which Confirm and Delete both have in the UI map, a game's own clash: both would take Escape.</summary>
    internal sealed class WhenTwoControlsWouldTakeOneKey
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/e");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Cancel, Rebinding.KEYBOARD, m_Rebinding.Keyboard.eKey);
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenItIsRefused()
        {
            Assert.AreEqual(RebindOutcome.Refused, m_Outcome);
        }

        [Test]
        public void ThenNothingChanges()
        {
            Assert.AreEqual("<Keyboard>/escape", m_Rebinding.PathOf("UI/Cancel", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("UI/Submit", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("UI/Delete", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenNothingIsKept()
        {
            Assert.AreEqual("<Keyboard>/escape", m_Rebinding.PathAfterLaunch("Player/Cancel", Rebinding.KEYBOARD));
        }
    }
}
