using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Confirm rebound to J, then every binding reset.</summary>
    internal sealed class WhenTheBindingsAreReset
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Confirm, Rebinding.KEYBOARD, m_Rebinding.Keyboard.jKey);

            m_Rebinding.Service.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            m_Rebinding.Destroy();
        }

        [Test]
        public void ThenTheGamesBindingsAreBack()
        {
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("Player/Confirm", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathOf("UI/Submit", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenNothingIsKept()
        {
            Assert.AreEqual("<Keyboard>/e", m_Rebinding.PathAfterLaunch("Player/Confirm", Rebinding.KEYBOARD));
        }
    }
}
