using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A game that gives a question's system sounds none.</summary>
    internal sealed class WhenAQuestionsSoundsAreNone
    {
        private FakeSfxPlayer m_Sfx;

        [SetUp]
        public void SetUp()
        {
            m_Sfx = new FakeSfxPlayer();

            IDialogueFlow      flow   = new ChoiceDialogueFlow(m_Sfx, FakeSystemSoundConfig.Silent(), 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(Task.CompletedTask);

            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ThenNothingIsAskedOfThePlayer()
        {
            Assert.AreEqual(0, m_Sfx.Played.Count);
        }
    }
}
