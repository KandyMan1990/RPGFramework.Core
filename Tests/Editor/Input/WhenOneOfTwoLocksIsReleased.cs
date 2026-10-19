using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class WhenOneOfTwoLocksIsReleased
    {
        [Test]
        public void ThenInputIsStillHeldBack()
        {
            InputRouter           router  = new InputRouter();
            RecordingInputContext context = new RecordingInputContext();

            ((IInputRouter)router).Push(context);
            ((IInputLock)router).Lock();
            ((IInputLock)router).Lock();
            ((IInputLock)router).Unlock();
            ((IInputRouter)router).Route(ControlSlot.Primary);

            Assert.AreEqual(0, context.Presses);
        }
    }
}
