using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenTheCursorMovesBetweenAnswers
    {
        private FakeAudioIntentPlayer m_Audio;

        [SetUp]
        public void SetUp()
        {
            m_Audio = new FakeAudioIntentPlayer();

            IDialogueFlow      flow   = new ChoiceDialogueFlow(m_Audio, AudioContext.Field, 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(new TaskCompletionSource<bool>().Task);

            // Left waiting on an answer that never comes, as the question stays open.
            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None);

            window.MoveCursor();
            window.MoveCursor();
        }

        [Test]
        public void ThenNavigatePlaysForEachMove()
        {
            Assert.AreEqual(2, m_Audio.Played.Count);
            Assert.AreEqual(AudioIntent.Navigate, m_Audio.Played[0].Intent);
            Assert.AreEqual(AudioIntent.Navigate, m_Audio.Played[1].Intent);
        }
    }
}
