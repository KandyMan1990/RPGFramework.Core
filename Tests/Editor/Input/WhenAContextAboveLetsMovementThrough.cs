using NUnit.Framework;
using RPGFramework.Core.Input;

namespace RPGFramework.Core.Tests.Input
{
    /// <summary>A script waiting on a button, over the field, with the player free to walk.</summary>
    internal sealed class WhenAContextAboveLetsMovementThrough
    {
        [Test]
        public void ThenMovementReachesTheOneBeneath()
        {
            IInputRouter          router  = new InputRouter();
            RecordingInputContext beneath = new RecordingInputContext(true);

            router.Push(beneath);
            router.Push(new RecordingInputContext(false));

            Assert.IsTrue(router.MovementReaches(beneath));
        }
    }
}
