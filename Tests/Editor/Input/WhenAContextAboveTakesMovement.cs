using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    /// <summary>A conversation holding the player, over the field.</summary>
    internal sealed class WhenAContextAboveTakesMovement
    {
        private IInputRouter          m_Router;
        private RecordingInputContext m_Beneath;
        private RecordingInputContext m_Above;

        [SetUp]
        public void SetUp()
        {
            m_Router  = new InputRouter();
            m_Beneath = new RecordingInputContext(true);
            m_Above   = new RecordingInputContext(true);

            m_Router.Push(m_Beneath);
            m_Router.Push(m_Above);
        }

        [Test]
        public void ThenMovementDoesNotReachTheOneBeneath()
        {
            Assert.IsFalse(m_Router.MovementReaches(m_Beneath));
        }

        [Test]
        public void ThenItReachesTheOneAbove()
        {
            Assert.IsTrue(m_Router.MovementReaches(m_Above));
        }

        [Test]
        public void ThenItReachesTheOneBeneathOnceTheOneAboveIsPopped()
        {
            m_Router.Pop(m_Above);

            Assert.IsTrue(m_Router.MovementReaches(m_Beneath));
        }
    }
}
