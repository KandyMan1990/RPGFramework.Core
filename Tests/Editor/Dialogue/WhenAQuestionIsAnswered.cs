using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RPGFramework.Core.Audio;
using RPGFramework.Core.Dialogue;
using RPGFramework.Core.Dialogue.Flows;

namespace RPGFramework.Core.Tests.Dialogue
{
    internal sealed class WhenAQuestionIsAnswered
    {
        private FakeAudioIntentPlayer m_Audio;

        [SetUp]
        public void SetUp()
        {
            m_Audio = new FakeAudioIntentPlayer();

            IDialogueFlow      flow   = new ChoiceDialogueFlow(m_Audio, AudioContext.Field, 0, ChoiceDialogueFlow.NO_CANCEL_ANSWER);
            FakeDialogueWindow window = new FakeDialogueWindow(Task.CompletedTask);

            flow.RunAsync(window, new[] { "Stay or go?", "Stay", "Go" }, Presses.ContextWith(window, DialogueWindowKind.Question), CancellationToken.None).GetAwaiter().GetResult();
        }

        [Test]
        public void ThenConfirmPlaysWhereTheWindowIs()
        {
            Assert.AreEqual(1, m_Audio.Played.Count);
            Assert.AreEqual(AudioIntent.Confirm, m_Audio.Played[0].Intent);
            Assert.AreEqual(AudioContext.Field, m_Audio.Played[0].Context);
        }
    }
}
