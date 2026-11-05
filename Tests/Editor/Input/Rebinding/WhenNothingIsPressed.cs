using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Confirm's rebinding waited on, with no key pressed.</summary>
    internal sealed class WhenNothingIsPressed
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Wait(m_Rebinding.Confirm, Rebinding.KEYBOARD);
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenItIsCancelled()
        {
            Assert.AreEqual(RebindOutcome.Cancelled, m_Outcome);
        }

        [Test]
        public void ThenNothingChanges()
        {
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("Player/Confirm", Rebinding.KEYBOARD));
        }
    }
}
