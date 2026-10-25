using NUnit.Framework;
using RPGFramework.Core.Input;

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
            m_Context = new RecordingInputContext(true);

            ((IInputRouter)m_Router).Push(m_Context);
            ((IInputLock)m_Router).Lock();
        }

        [Test]
        public void ThenNoPressReachesTheContext()
        {
            ((IInputRouter)m_Router).Route(ControlSlot.Primary);

            Assert.AreEqual(0, m_Context.Presses);
        }

        // So whatever was moving stops as the lock begins, its release held back too.
        [Test]
        public void ThenMovementDoesNotReachIt()
        {
            Assert.IsFalse(((IInputRouter)m_Router).MovementReaches(m_Context));
        }
    }
}
