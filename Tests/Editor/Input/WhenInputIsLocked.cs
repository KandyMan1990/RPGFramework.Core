using NUnit.Framework;
using RPGFramework.Core.Input;
using UnityEngine;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class WhenInputIsLocked
    {
        private InputRouter           m_Router;
        private RecordingInputContext m_Context;

        [SetUp]
        public void SetUp()
        {
            m_Router  = new InputRouter();
            m_Context = new RecordingInputContext();

            ((IInputRouter)m_Router).Push(m_Context);
            ((IInputRouter)m_Router).RouteMovement(Vector2.up);
            ((IInputLock)m_Router).Lock();
        }

        // Its release is held back too, so a move held as the lock begins would never stop.
        [Test]
        public void ThenAHeldMoveIsStopped()
        {
            Assert.AreEqual(2, m_Context.Moves);
            Assert.AreEqual(Vector2.zero, m_Context.LastMove);
        }

        [Test]
        public void ThenNothingRoutedReachesTheContext()
        {
            ((IInputRouter)m_Router).Route(ControlSlot.Primary);
            ((IInputRouter)m_Router).RouteMovement(Vector2.up);

            Assert.AreEqual(0, m_Context.Presses);
            Assert.AreEqual(2, m_Context.Moves);
        }
    }
}
