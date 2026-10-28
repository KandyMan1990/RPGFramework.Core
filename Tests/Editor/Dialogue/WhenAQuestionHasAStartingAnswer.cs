using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAQuestionHasAStartingAnswer
    {
        [Test]
        public void ThenItsCursorStartsThere()
        {
            IDialogueFlow      flow   = new ChoiceDialogueFlow(new FakeAudioIntentPlayer(), AudioContext.Field, 2, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(new TaskCompletionSource<bool>().Task);

            // Left waiting on an answer that never comes, as the question stays open.
            flow.RunAsync(window, new[] { "Which?", "One", "Two", "Three" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None);

            Assert.AreEqual(2, window.StartingChoice);
        }
    }
}
