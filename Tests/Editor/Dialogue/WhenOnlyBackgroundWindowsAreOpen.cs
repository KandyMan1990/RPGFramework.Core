using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>Conversations left up while the player walks: the press carries on to what is under them.</summary>
    internal sealed class WhenOnlyBackgroundWindowsAreOpen
    {
        [Test]
        public void ThenThePressCarriesOn()
        {
            DialogueInputContext context = Presses.ContextWith(Presses.NewWindow(), DialogueWindowKind.Background);
            context.Open(Presses.NewWindow(), DialogueWindowKind.Background);

            Assert.IsFalse(Presses.Press(context));
        }
    }
}
