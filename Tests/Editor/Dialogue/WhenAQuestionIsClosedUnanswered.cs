using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    /// <summary>A script closing the window while its question waits.</summary>
    internal sealed class WhenAQuestionIsClosedUnanswered
    {
        private FakeAudioIntentPlayer m_Audio;

        [SetUp]
        public void SetUp()
        {
            m_Audio = new FakeAudioIntentPlayer();

            IDialogueFlow           flow   = new ChoiceDialogueFlow(m_Audio, AudioContext.Field, 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow      window = new FakeDialogueWindow(new TaskCompletionSource<bool>().Task);
            CancellationTokenSource close  = new CancellationTokenSource();

            close.Cancel();

            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), close.Token).GetAwaiter().GetResult();

            close.Dispose();
        }

        [Test]
        public void ThenNothingPlays()
        {
            Assert.AreEqual(0, m_Audio.Played.Count);
        }
    }
}
