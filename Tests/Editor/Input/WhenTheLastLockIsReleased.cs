using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class WhenTheLastLockIsReleased
    {
        private IInputRouter          m_Router;
        private RecordingInputContext m_Context;

        [SetUp]
        public void SetUp()
        {
            InputRouter router = new InputRouter();

            m_Router  = router;
            m_Context = new RecordingInputContext(true);

            m_Router.Push(m_Context);
            ((IInputLock)router).Lock();
            ((IInputLock)router).Lock();
            ((IInputLock)router).Unlock();
            ((IInputLock)router).Unlock();
        }

        [Test]
        public void ThenPressesReachTheContext()
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
