using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class WhenInputIsNotLocked
    {
        private IInputRouter          m_Router;
        private RecordingInputContext m_Context;

        [SetUp]
        public void SetUp()
        {
            m_Router  = new InputRouter();
            m_Context = new RecordingInputContext(true);

            m_Router.Push(m_Context);
        }

        [Test]
        public void ThenAPressReachesTheTopContext()
        {
            m_Router.Route(ControlSlot.Primary);

            Assert.AreEqual(1, m_Context.Presses);
        }

        [Test]
        public void ThenMovementReachesIt()
        {
            Assert.IsTrue(m_Router.MovementReaches(m_Context));
        }
    }
}
