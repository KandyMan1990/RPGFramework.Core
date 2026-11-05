using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input.Rebinding
{
    /// <summary>Confirm, rebound on the keyboard to J, in both maps it is in.</summary>
    internal sealed class WhenAControlIsRebound
    {
        private Rebinding     m_Rebinding;
        private RebindOutcome m_Outcome;

        [SetUp]
        public void SetUp()
        {
            m_Rebinding = new Rebinding("<Keyboard>/r");

            m_Outcome = m_Rebinding.Rebind(m_Rebinding.Confirm, Rebinding.KEYBOARD, m_Rebinding.Keyboard.jKey);
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
        public void ThenEveryActionOfItFollows()
        {
            Assert.AreEqual("<Keyboard>/j", m_Rebinding.PathOf("Player/Confirm", Rebinding.KEYBOARD));
            Assert.AreEqual("<Keyboard>/j", m_Rebinding.PathOf("UI/Submit", Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenThePadKeepsItsButton()
        {
            Assert.AreEqual("<Gamepad>/buttonSouth", m_Rebinding.PathOf("Player/Confirm", Rebinding.GAMEPAD));
        }

        [Test]
        public void ThenItIsNamedAsThePlayerReadsIt()
        {
            Assert.AreEqual("J", m_Rebinding.Service.ControlName(m_Rebinding.Confirm, Rebinding.KEYBOARD));
        }

        [Test]
        public void ThenItIsKeptForTheNextLaunch()
        {
            Assert.AreEqual("<Keyboard>/j", m_Rebinding.PathAfterLaunch("UI/Submit", Rebinding.KEYBOARD));
        }
    }
}
