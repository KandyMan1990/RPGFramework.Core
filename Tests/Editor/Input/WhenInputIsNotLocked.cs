using NUnit.Framework;
using RPGFramework.Core.Input;
using UnityEngine;

namespace RPGFramework.Core.Tests.Input
{
    internal sealed class WhenInputIsNotLocked
    {
        [Test]
        public void ThenItReachesTheTopContext()
        {
            IInputRouter          router  = new InputRouter();
            RecordingInputContext context = new RecordingInputContext();

            router.Push(context);
            router.Route(ControlSlot.Primary);
            router.RouteMovement(Vector2.up);

            Assert.AreEqual(1, context.Presses);
            Assert.AreEqual(1, context.Moves);
        }
    }
}
