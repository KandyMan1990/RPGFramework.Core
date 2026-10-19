using NUnit.Framework;
using RPGFramework.Core.Input;
using UnityEngine;

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
            m_Context = new RecordingInputContext();

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
        public void ThenMovesReachTheContext()
        {
            m_Router.RouteMovement(Vector2.up);

            Assert.AreEqual(2, m_Context.Moves);
            Assert.AreEqual(Vector2.up, m_Context.LastMove);
        }
    }
}
