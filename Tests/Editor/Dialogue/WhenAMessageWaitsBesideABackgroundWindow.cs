using NUnit.Framework;
using RPGFramework.Core.Dialogue;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>Someone talking to the player while a conversation plays on in the background.</summary>
    internal sealed class WhenAMessageWaitsBesideABackgroundWindow
    {
        [Test]
        public void ThenItKeepsThePress()
        {
            DialogueInputContext context = Presses.ContextWith(Presses.NewWindow(), DialogueWindowKind.Background);
            context.Open(Presses.NewWindow(), DialogueWindowKind.Message);

            Assert.IsTrue(Presses.Press(context));
        }
    }
}
